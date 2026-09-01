cbuffer WorldRenderConstants : register(b4)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport;
    float4 RasterScale; // xy framebuffer->scene-depth scale, zw framebuffer->NDC scale
    float4 SceneDepthParams;
    float4 SceneInfoParams;
};

Texture2D fontTexture : register(t3);
SamplerState fontSampler : register(s3);
Texture2D<float> SceneDepthTexture : register(t4);

struct PS_INPUT
{
    float4 pos          : SV_POSITION;
    float2 uv           : TEXCOORD0;
    nointerpolation float4 col          : COLOR0;
    nointerpolation float4 outlineCol   : COLOR1;
    nointerpolation float  outlineWidth : TEXCOORD1;
};

static const float MSDF_PX_RANGE = 8.0f;

float median3(float a, float b, float c)
{
    return max(min(a, b), min(max(a, b), c));
}

float screenPxRange(float2 uv)
{
    uint width, height;
    fontTexture.GetDimensions(width, height);
    float2 unitRange = float2(MSDF_PX_RANGE / max((float)width, 1.0f), MSDF_PX_RANGE / max((float)height, 1.0f));
    float2 uvPerScreenPixel = max(fwidth(uv), float2(1e-7f, 1e-7f));
    float2 screenTexSize = 1.0f / uvPerScreenPixel;
    return max(0.5f * dot(unitRange, screenTexSize), 1.0f);
}

bool worldPixelOccluded(PS_INPUT input)
{
    if (SceneDepthParams.w < 0.5f)
        return false;

    int2 depthSize = max(int2(SceneDepthParams.xy), int2(1, 1));
    int2 depthCoord = int2(input.pos.xy * RasterScale.xy);
    depthCoord = clamp(depthCoord, int2(0, 0), depthSize - int2(1, 1));
    float sceneDepth = SceneDepthTexture.Load(int3(depthCoord, 0));

    // FFXIV reverse-Z: zero is cleared/far. The billboard keeps one anchor depth across the quad.
    if (!(sceneDepth > 0.0f))
        return false;

    float primitiveDepth = input.pos.z;
    if (primitiveDepth >= sceneDepth)
        return false;

    float2 ndc = float2(input.pos.x * RasterScale.z - 1.0f, 1.0f - input.pos.y * RasterScale.w);
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
    if (uiVisibility <= 0.0f || worldPixelOccluded(input))
        clip(-1.0f);

    float3 msd = fontTexture.Sample(fontSampler, input.uv).rgb;
    float pxRange = screenPxRange(input.uv);
    float pxDistance = (median3(msd.r, msd.g, msd.b) - 0.5f) * pxRange;

    float fillCoverage = saturate(pxDistance + 0.5f);
    float maxOutlineWidth = max(0.0f, 0.5f * pxRange - 0.5f);
    float outlineWidth = min(max(input.outlineWidth, 0.0f), maxOutlineWidth);
    float outlineCoverage = saturate(pxDistance + outlineWidth + 0.5f);

    float fillAlpha = input.col.a * fillCoverage;
    float outlineAlpha = input.outlineCol.a * max(outlineCoverage - fillCoverage, 0.0f);
    float outAlpha = (fillAlpha + outlineAlpha * (1.0f - fillAlpha)) * uiVisibility;
    clip(outAlpha - 0.001f);

    float3 premul = input.col.rgb * fillAlpha + input.outlineCol.rgb * outlineAlpha * (1.0f - fillAlpha);
    float localAlpha = fillAlpha + outlineAlpha * (1.0f - fillAlpha);
    float3 rgb = localAlpha > 1e-6f ? premul / localAlpha : 0.0f;
    return float4(rgb, outAlpha);
}
