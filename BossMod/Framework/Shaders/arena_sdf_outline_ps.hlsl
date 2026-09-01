Texture2D<float> ArenaSdf : register(t1);
SamplerState ArenaSdfSampler : register(s1);
static const float OutlineCoverageScale = 1.5f;
static const float ShadowCoverageScale = 1.75f;

cbuffer ArenaSdfConstants : register(b1)
{
    float4 ArenaUvRow0;
    float4 ArenaUvRow1;
    float4 ArenaOutsideScale;
    float4 ArenaMipGrad;
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

float arenaSdUv(float2 uv)
{
    float2 clampedUv = saturate(uv);
    float sampledPx = ArenaSdf.SampleGrad(ArenaSdfSampler, clampedUv, ArenaMipGrad.xy, ArenaMipGrad.zw).r * ArenaUvRow1.w;
    float result = sampledPx;

    [branch]
    if (any(uv != clampedUv))
    {
        float2 outsideUv = uv - clampedUv;
        float2 outsidePxAxes = outsideUv * ArenaOutsideScale.xy;
        result += length(outsidePxAxes);
    }
    return result;
}

float positiveSquare(float x)
{
    x = max(x, 0.0f);
    return x * x;
}

float halfPlaneCoverage(float edgeDistance, float2 gradientAxes, float coverageScale)
{
    // Exact box-filter coverage of a locally straight boundary over the unit pixel square.
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
    float2 uv = float2(dot(hp, ArenaUvRow0.xyz), dot(hp, ArenaUvRow1.xyz));
    float d = arenaSdUv(uv);
    if (abs(d) > i.widthsPx.y + 1.25f)
        discard;

    // Symmetric half-pixel finite differences are continuous with motion/rotation and do
    // not inherit the 2x2-quad phase changes of ddx/ddy on the sampled distance field.
    float2 uvDx = 0.5f * float2(ArenaUvRow0.x, ArenaUvRow1.x);
    float2 uvDy = 0.5f * float2(ArenaUvRow0.y, ArenaUvRow1.y);
    float2 g = float2(
        arenaSdUv(uv + uvDx) - arenaSdUv(uv - uvDx),
        arenaSdUv(uv + uvDy) - arenaSdUv(uv - uvDy));
    // The same normalized gradient is used by both sides of both color bands; compute it once.
    float2 n = abs(g) * rsqrt(max(dot(g, g), 1e-10f));
    float2 gradientAxes = float2(max(n.x, n.y), min(n.x, n.y));
    float c = bandCoverage(d, i.widthsPx.x, gradientAxes, OutlineCoverageScale);
    float s = bandCoverage(d, i.widthsPx.y, gradientAxes, ShadowCoverageScale);
    float4 o = composeExclusiveHalo(i.shadowCol, s, i.col, c);
    clip(o.a - 0.001f);
    return o;
}
