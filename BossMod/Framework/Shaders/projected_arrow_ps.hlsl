// Projection fix r14: zero height evaluates the authored plane with foreground depth occlusion.

cbuffer WorldRenderConstants : register(b4)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport;
    float4 RasterScale;
    float4 SceneDepthParams; // xy scene depth actual dimensions, z line occlusion tolerance, w availability
    float4 SceneInfoParams; // x character-classification texture available, y near-black character threshold
};

Texture2D<float> SceneDepthTexture : register(t4);
Texture2D<float4> SceneInfoTexture : register(t5);

struct PS_INPUT
{
    float4 pos                         : SV_POSITION;
    nointerpolation float4 col         : COLOR0;
    nointerpolation float3 origin      : TEXCOORD0;
    nointerpolation float2 directionXZ : TEXCOORD1;
    nointerpolation float4 dimensions  : TEXCOORD2; // length, shaft half-width, head length, head half-width/length
    nointerpolation float projectionHeight : TEXCOORD3;
};

float nativeUiVisibility(float2 pixelPos)
{
    return 1.0f;
}

float stableWorldAaLimit(float2 pixelPos, float2 framebufferSize, float4 worldH, float3 world)
{
    // Neighbouring pixels can belong to unrelated floor meshes. Estimate a conservative one-pixel
    // world footprint at the current depth without reading neighbouring depth values, then use it to
    // cap the real SDF derivatives below. This prevents depth discontinuities from widening the arrow
    // edge into coloured stripes along arbitrary floor-geometry edges.
    float xStep = pixelPos.x + 1.0f < framebufferSize.x ? 1.0f : -1.0f;
    float yStep = pixelPos.y + 1.0f < framebufferSize.y ? 1.0f : -1.0f;
    float4 worldXH = worldH + xStep * RasterScale.z * InvViewProj[0];
    float4 worldYH = worldH - yStep * RasterScale.w * InvViewProj[1];
    float3 worldX = worldXH.xyz / worldXH.w;
    float3 worldY = worldYH.xyz / worldYH.w;
    float pixelFootprint = length(worldX.xz - world.xz) + length(worldY.xz - world.xz);
    return clamp(pixelFootprint * 4.0f, 0.02f, 1.25f);
}

float fillFieldCoverage(float field, float maxFootprint)
{
    // Hardware derivatives retain the best AA on a continuous sloped receiver. At a depth edge they
    // can jump by many yalms, so never allow them to exceed the same-depth estimate above.
    float footprint = max(min(abs(ddx(field)) + abs(ddy(field)), maxFootprint), 0.002f);
    return saturate(0.5f - field / footprint);
}

float4 referenceRayBaseAtPixel(float2 pixelPos)
{
    float2 ndc = float2(pixelPos.x * RasterScale.z - 1.0f, 1.0f - pixelPos.y * RasterScale.w);
    return mul(float4(ndc, 0.0f, 1.0f), InvViewProj);
}

bool referencePlaneXZFromRayBase(float4 rayBase, float planeY, float2 fallbackXZ, out float2 xz)
{
    float4 depthRow = InvViewProj[2];
    float4 rayAH = rayBase + 0.25f * depthRow;
    float4 rayBH = rayBase + 0.75f * depthRow;
    if (abs(rayAH.w) <= 1e-7f || abs(rayBH.w) <= 1e-7f)
    {
        xz = fallbackXZ;
        return false;
    }

    float3 rayA = rayAH.xyz / rayAH.w;
    float3 rayB = rayBH.xyz / rayBH.w;
    float dy = rayB.y - rayA.y;
    if (abs(dy) < 1e-6f)
    {
        xz = fallbackXZ;
        return false;
    }

    float t = (planeY - rayA.y) / dy;
    xz = lerp(rayA.xz, rayB.xz, t);
    float2 delta = xz - fallbackXZ;
    if (max(abs(delta.x), abs(delta.y)) > 10000.0f)
    {
        xz = fallbackXZ;
        return false;
    }
    return true;
}

bool referencePlaneReceiver(float2 pixelPos, float planeY, float2 fallbackXZ,
    out float3 world, out float receiverDepth, out float aaLimit)
{
    world = 0.0f;
    receiverDepth = 0.0f;
    aaLimit = 0.02f;

    float4 rayBase = referenceRayBaseAtPixel(pixelPos);
    float2 xz;
    if (!referencePlaneXZFromRayBase(rayBase, planeY, fallbackXZ, xz))
    {
        return false;
    }

    world = float3(xz.x, planeY, xz.y);
    float4 world4 = float4(world, 1.0f);
    if (dot(world4, NearPlane) >= 0.0f)
        return false;
    float4 clip = mul(world4, ViewProj);
    if (abs(clip.w) <= 1e-7f)
        return false;
    receiverDepth = clip.z / clip.w;
    if (!(receiverDepth > 0.0f) || receiverDepth > 1.0001f)
        return false;

    float2 framebufferSize = max(Viewport.xy, float2(1.0f, 1.0f));
    float xStep = pixelPos.x + 1.0f < framebufferSize.x ? 1.0f : -1.0f;
    float yStep = pixelPos.y + 1.0f < framebufferSize.y ? 1.0f : -1.0f;
    // Initialize explicitly for FXC's conservative definite-assignment analysis around short-circuit
    // expressions with out parameters.
    float2 referenceX = xz;
    float2 referenceY = xz;
    if (referencePlaneXZFromRayBase(rayBase + xStep * RasterScale.z * InvViewProj[0], planeY, xz, referenceX)
        && referencePlaneXZFromRayBase(rayBase - yStep * RasterScale.w * InvViewProj[1], planeY, xz, referenceY))
    {
        float pixelFootprint = length(referenceX - xz) + length(referenceY - xz);
        aaLimit = clamp(pixelFootprint * 4.0f, 0.02f, 1.25f);
    }
    return true;
}

float4 main(PS_INPUT input) : SV_Target
{
    // A projected arrow has no sensible depth fallback: scene depth is the surface it is painted on.
    if (SceneDepthParams.w < 0.5f)
        clip(-1.0f);

    float uiVisibility = nativeUiVisibility(input.pos.xy);
    if (uiVisibility <= 0.0f)
        clip(-1.0f);

    int2 depthSize = max(int2(SceneDepthParams.xy), int2(1, 1));
    int2 depthCoord = int2(input.pos.xy * RasterScale.xy);
    depthCoord = clamp(depthCoord, int2(0, 0), depthSize - int2(1, 1));
    float sceneDepth = SceneDepthTexture.Load(int3(depthCoord, 0));
    bool sceneDepthValid = sceneDepth > 0.0f;

    if (sceneDepthValid && SceneInfoParams.x > 0.5f)
    {
        float3 sceneInfo = SceneInfoTexture.Load(int3(depthCoord, 0)).rgb;
        clip(max(sceneInfo.r, max(sceneInfo.g, sceneInfo.b)) - SceneInfoParams.y);
    }

    // Defaults also keep FXC's conservative definite-assignment pass quiet across clip paths.
    float3 world = 0.0f;
    float aaLimit = 0.02f;
    if (input.projectionHeight > 0.0f)
    {
        if (!sceneDepthValid)
            clip(-1.0f);
        float2 ndc = float2(input.pos.x * RasterScale.z - 1.0f, 1.0f - input.pos.y * RasterScale.w);
        float4 worldH = mul(float4(ndc, sceneDepth, 1.0f), InvViewProj);
        if (abs(worldH.w) <= 1e-7f)
            clip(-1.0f);
        world = worldH.xyz / worldH.w;
        clip(input.projectionHeight - abs(world.y - input.origin.y));
        aaLimit = stableWorldAaLimit(input.pos.xy, max(Viewport.xy, float2(1.0f, 1.0f)), worldH, world);
    }
    else
    {
        float receiverDepth;
        if (!referencePlaneReceiver(input.pos.xy, input.origin.y, input.origin.xz, world, receiverDepth, aaLimit))
            clip(-1.0f);

        // Reverse-Z: a larger scene depth is closer to the camera. Keep coplanar depth noise within
        // the normal world-space tolerance, but reject actors/props and other real foreground geometry.
        if (sceneDepthValid && sceneDepth > receiverDepth)
        {
            float2 ndc = float2(input.pos.x * RasterScale.z - 1.0f, 1.0f - input.pos.y * RasterScale.w);
            float4 sceneWorldH = mul(float4(ndc, sceneDepth, 1.0f), InvViewProj);
            if (abs(sceneWorldH.w) > 1e-7f)
            {
                float3 sceneWorld = sceneWorldH.xyz / sceneWorldH.w;
                if (distance(sceneWorld, world) > SceneDepthParams.z)
                    clip(-1.0f);
            }
        }
    }

    // Do not infer a surface normal from ddx/ddy(world) here. At floor/wall and stair-riser
    // boundaries a 2x2 pixel derivative quad can straddle unrelated surfaces, making the result
    // camera-angle dependent and causing projected arrows to pop or tear. For positive height, the
    // reference-height band is deliberately the only surface-selection rule.

    float2 delta = world.xz - input.origin.xz;
    float2 dir = input.directionXZ;
    float2 right = float2(-dir.y, dir.x);
    float forward = dot(delta, dir);
    float side = abs(dot(delta, right));

    float length = input.dimensions.x;
    float shaftHalf = input.dimensions.y;
    float headLength = input.dimensions.z;
    float headBase = length - headLength;

    float shaftField = max(max(-forward, forward - headBase), side - shaftHalf);
    float headField = max(max(headBase - forward, forward - length), side - (length - forward) * input.dimensions.w);
    float coverage = fillFieldCoverage(min(shaftField, headField), aaLimit);

    float alpha = input.col.a * coverage * uiVisibility;
    clip(alpha - 0.001f);
    return float4(input.col.rgb, alpha);
}
