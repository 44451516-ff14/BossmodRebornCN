cbuffer WorldRenderConstants : register(b4)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport; // xy framebuffer dimensions, z logical->framebuffer pixel scale
    float4 RasterScale; // xy framebuffer->scene-depth scale, zw framebuffer->NDC scale
    float4 SceneDepthParams; // xy scene-depth actual dimensions, z world-occlusion tolerance (m), w availability
    float4 SceneInfoParams;
};

Texture2D<float> SceneDepthTexture : register(t4);

struct PS_INPUT
{
    float4 pos                         : SV_POSITION;
    nointerpolation float4 col         : COLOR0;
    nointerpolation float4 shadowCol   : COLOR1;
    noperspective float acrossPx       : TEXCOORD0;
    noperspective float alongPx        : TEXCOORD1;
    nointerpolation float3 params      : TEXCOORD2;
    nointerpolation uint flags         : TEXCOORD3;
};

float normalizedPixelCoverage(float edgeDistance)
{
    float2 g = float2(ddx(edgeDistance), ddy(edgeDistance));
    float gl = length(g);
    float footprint = gl > 1e-4f ? (abs(g.x) + abs(g.y)) / gl : 1.0f;
    footprint = clamp(footprint, 1.0f, 1.41421356f);
    return saturate(0.5f - edgeDistance / footprint);
}

float strokeCoverage(PS_INPUT input, float halfWidthPx)
{
    float edgeDistance = abs(input.acrossPx) - halfWidthPx;
    if ((input.flags & 0x1u) != 0u)
        edgeDistance = max(edgeDistance, -input.alongPx);
    if ((input.flags & 0x2u) != 0u)
        edgeDistance = max(edgeDistance, input.alongPx - input.params.z);
    return normalizedPixelCoverage(edgeDistance);
}

bool worldPixelOccluded(PS_INPUT input)
{
    // bit 2 is emitted only by world_line_vs/world_curve_vs. Ordinary arena strokes retain their
    // legacy screen-space behavior and never touch the scene-depth resource.
    if ((input.flags & 0x4u) == 0u || SceneDepthParams.w < 0.5f)
        return false;

    int2 depthSize = max(int2(SceneDepthParams.xy), int2(1, 1));
    int2 depthCoord = int2(input.pos.xy * RasterScale.xy);
    depthCoord = clamp(depthCoord, int2(0, 0), depthSize - int2(1, 1));
    float sceneDepth = SceneDepthTexture.Load(int3(depthCoord, 0));

    // FFXIV uses reverse-Z; zero is the cleared/far value. Treat sky/no-depth as unobstructed.
    if (!(sceneDepth > 0.0f))
        return false;

    float primitiveDepth = input.pos.z;
    if (primitiveDepth >= sceneDepth)
        return false;

    float2 ndc = float2(input.pos.x * RasterScale.z - 1.0f, 1.0f - input.pos.y * RasterScale.w);
    // Both points lie on the same camera ray. Evaluate the depth-independent part once and
    // advance along the inverse-projection depth row for each depth value.
    float4 rayBase = mul(float4(ndc, 0.0f, 1.0f), InvViewProj);
    float4 depthRow = InvViewProj[2];
    float4 sceneWorldH = rayBase + sceneDepth * depthRow;
    float4 primitiveWorldH = rayBase + primitiveDepth * depthRow;
    if (abs(sceneWorldH.w) <= 1e-7f || abs(primitiveWorldH.w) <= 1e-7f)
        return false;

    float3 sceneWorld = sceneWorldH.xyz / sceneWorldH.w;
    float3 primitiveWorld = primitiveWorldH.xyz / primitiveWorldH.w;
    return distance(primitiveWorld, sceneWorld) > SceneDepthParams.z;
}

float nativeUiVisibility(PS_INPUT input)
{
    return 1.0f;
}

float4 main(PS_INPUT input) : SV_Target
{
    float uiVisibility = nativeUiVisibility(input);
    if (uiVisibility <= 0.0f)
        clip(-1.0f);

    // World lines/curves are fully hidden by scene geometry. The old diagnostic occluded-alpha path
    // is intentionally gone now that the depth comparison is stable at render-time ViewProj.
    if (worldPixelOccluded(input))
        clip(-1.0f);

    input.col.a *= uiVisibility;
    input.shadowCol.a *= uiVisibility;

    float colorCoverage = strokeCoverage(input, input.params.x);
    float shadowCoverage = strokeCoverage(input, input.params.y);

    float ca = input.col.a * colorCoverage;
    float sa = input.shadowCol.a * shadowCoverage;
    float shadowBehind = sa * (1.0f - ca);
    float a = ca + shadowBehind;
    clip(a - 0.001f);
    float3 rgb = (input.col.rgb * ca + input.shadowCol.rgb * shadowBehind) / max(a, 1e-6f);
    return float4(rgb, a);
}
