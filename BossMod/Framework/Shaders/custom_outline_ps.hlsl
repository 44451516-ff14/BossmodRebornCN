Texture2D<float> ArenaSdf : register(t1);
Texture2D<float> CustomSdf : register(t2);
SamplerState SdfSampler : register(s1);
static const float OutlineCoverageScale = 1.5f;
static const float ShadowCoverageScale = 1.75f;

cbuffer ArenaSdfConstants : register(b1)
{
    float4 ArenaUvRow0;
    float4 ArenaUvRow1;
    float4 ArenaOutsideScale;
    float4 ArenaMipGrad;
};

cbuffer CustomSdfConstants : register(b2)
{
    float4 CustomUvRow0;
    float4 CustomUvRow1;
    float4 CustomOutsideScale;
    float4 CustomMipGrad;
};

struct PS_INPUT
{
    float4 pos       : SV_POSITION;
    float2 localPx   : TEXCOORD0;
    nointerpolation float2 direction : TEXCOORD1;
    nointerpolation float4 params    : TEXCOORD2;
    nointerpolation float2 widthsPx  : TEXCOORD3;
    nointerpolation float2 extra     : TEXCOORD4;
    nointerpolation float4 col       : COLOR0;
    nointerpolation float4 shadowCol : COLOR1;
};

float extendOutside(float sampledPx, float2 uv, float2 clampedUv, float2 outsideScale)
{
    float result = sampledPx;
    [branch]
    if (any(uv != clampedUv))
    {
        float2 outsideUv = uv - clampedUv;
        float2 outsidePxAxes = outsideUv * outsideScale;
        result += length(outsidePxAxes);
    }
    return result;
}

float arenaSdUv(float2 uv)
{
    float2 clampedUv = saturate(uv);
    float sampledPx = ArenaSdf.SampleGrad(SdfSampler, clampedUv, ArenaMipGrad.xy, ArenaMipGrad.zw).r * ArenaUvRow1.w;
    return extendOutside(sampledPx, uv, clampedUv, ArenaOutsideScale.xy);
}

float customSdUv(float2 uv)
{
    float2 clampedUv = saturate(uv);
    float sampledPx = CustomSdf.SampleGrad(SdfSampler, clampedUv, CustomMipGrad.xy, CustomMipGrad.zw).r * CustomUvRow1.w;
    return extendOutside(sampledPx, uv, clampedUv, CustomOutsideScale.xy);
}

float positiveSquare(float x)
{
    x = max(x, 0.0f);
    return x * x;
}

float halfPlaneCoverage(float edgeDistance, float2 gradientAxes, float coverageScale)
{
    // Exact box-filter coverage of a locally straight boundary over the unit pixel square.
    // Keep only gradient direction: the distance fields are already expressed in framebuffer
    // pixels, and ignoring derivative magnitude avoids the corner inflation of raw fwidth().
    float a = gradientAxes.x * coverageScale;
    float b = gradientAxes.y * coverageScale;

    // Axis-aligned limit of the convolution below.
    if (b <= 1e-4f)
        return saturate(0.5f - edgeDistance / max(a, 1e-5f));

    // Projection of a uniform pixel square onto the edge normal is the convolution of
    // two uniforms of widths a and b. Its exact CDF gives the covered pixel area.
    float h = 0.5f * (a + b);
    float q = h - edgeDistance;
    float area = positiveSquare(q)
               - positiveSquare(q - a)
               - positiveSquare(q - b)
               + positiveSquare(q - a - b);
    return saturate(area / (2.0f * a * b));
}

float bandCoverage(float signedDistance, float halfWidth, float2 gradientAxes, float coverageScale)
{
    // Area between the two locally parallel boundaries d=-w and d=+w. Keeping the signed
    // distance avoids the abs() cusp at the centerline and is smoother under rotation.
    return saturate(halfPlaneCoverage(signedDistance - halfWidth, gradientAxes, coverageScale)
                  - halfPlaneCoverage(signedDistance + halfWidth, gradientAxes, coverageScale));
}

float clippedSdAt(float2 arenaUv, float2 customUv)
{
    return max(customSdUv(customUv), arenaSdUv(arenaUv));
}

float4 composeExclusiveHalo(float4 shadowCol, float shadowCoverage, float4 col, float colorCoverage)
{
    // The shadow is a coverage ring around the colored stroke, not another opaque stroke
    // underneath it. Partition coverage so black cannot leak through partially covered
    // colored AA pixels; that leak exaggerates temporal aliasing on high-contrast outlines.
    float exclusiveShadow = shadowCoverage * (1.0f - colorCoverage);
    float sa = shadowCol.a * exclusiveShadow;
    float ca = col.a * colorCoverage;
    float a = ca + sa;
    if (a <= 1e-5f)
        return float4(0.0f, 0.0f, 0.0f, 0.0f);
    float3 rgb = (col.rgb * ca + shadowCol.rgb * sa) / a;
    return float4(rgb, a);
}

float4 main(PS_INPUT i) : SV_Target
{
    float2 p = i.pos.xy;
    float3 hp = float3(p, 1.0f);
    float2 arenaUv = float2(dot(hp, ArenaUvRow0.xyz), dot(hp, ArenaUvRow1.xyz));
    float2 customUv = float2(dot(hp, CustomUvRow0.xyz), dot(hp, CustomUvRow1.xyz));
#ifdef CLIP_EDGE_ONLY
    float arenaSd = arenaSdUv(arenaUv);
    float replayBand = max(i.widthsPx.y + 2.5f, 4.0f);
    if (abs(arenaSd) > replayBand)
        discard;
    // Reuse the center arena sample below; only the four offset evaluations need both fields.
    float clippedSd = max(customSdUv(customUv), arenaSd);
#else
    float clippedSd = clippedSdAt(arenaUv, customUv);
#endif
    if (abs(clippedSd) > i.widthsPx.y + 1.25f)
        discard;

    // Sample the complete Boolean field at symmetric half-pixel offsets. This makes the
    // estimated edge normal continuous instead of depending on pixel-quad derivatives.
    float2 arenaUvDx = 0.5f * float2(ArenaUvRow0.x, ArenaUvRow1.x);
    float2 arenaUvDy = 0.5f * float2(ArenaUvRow0.y, ArenaUvRow1.y);
    float2 customUvDx = 0.5f * float2(CustomUvRow0.x, CustomUvRow1.x);
    float2 customUvDy = 0.5f * float2(CustomUvRow0.y, CustomUvRow1.y);
    float2 g = float2(
        clippedSdAt(arenaUv + arenaUvDx, customUv + customUvDx) - clippedSdAt(arenaUv - arenaUvDx, customUv - customUvDx),
        clippedSdAt(arenaUv + arenaUvDy, customUv + customUvDy) - clippedSdAt(arenaUv - arenaUvDy, customUv - customUvDy));
    float2 n = abs(g) * rsqrt(max(dot(g, g), 1e-10f));
    float2 gradientAxes = float2(max(n.x, n.y), min(n.x, n.y));
    float c = bandCoverage(clippedSd, i.widthsPx.x, gradientAxes, OutlineCoverageScale);
    float s = bandCoverage(clippedSd, i.widthsPx.y, gradientAxes, ShadowCoverageScale);
    float4 o = composeExclusiveHalo(i.shadowCol, s, i.col, c);
    clip(o.a - 0.001f);
    return o;
}
