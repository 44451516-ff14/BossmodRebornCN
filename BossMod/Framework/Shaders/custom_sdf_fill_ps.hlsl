Texture2D<float> CustomSdf : register(t2);
SamplerState SdfSampler : register(s1);
static const float FillCoverageScale = 1.5f;

cbuffer CustomSdfConstants : register(b2)
{
    float4 CustomUvRow0;
    float4 CustomUvRow1;
    float4 CustomOutsideScale;
    float4 CustomMipGrad;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
};

float customSdPx(float2 framebufferPx)
{
    // The fill mesh is exactly the padded SDF domain, so rasterized samples are already
    // inside its UV rectangle; unlike outline overlays, no outside-domain reconstruction
    // is needed here.
    float3 hp = float3(framebufferPx, 1.0f);
    float2 uv = float2(dot(hp, CustomUvRow0.xyz), dot(hp, CustomUvRow1.xyz));
    return CustomSdf.SampleGrad(SdfSampler, uv, CustomMipGrad.xy, CustomMipGrad.zw).r * CustomUvRow1.w;
}

float positiveSquare(float x)
{
    x = max(x, 0.0f);
    return x * x;
}

float halfPlaneCoverage(float edgeDistance, float2 gradient)
{
    // Exact box-filter coverage of a locally straight boundary over the unit pixel square.
    // Keep only gradient direction: the distance fields are already expressed in framebuffer
    // pixels, and ignoring derivative magnitude avoids the corner inflation of raw fwidth().
    float gl = length(gradient);
    if (gl <= 1e-5f)
        return edgeDistance <= 0.0f ? 1.0f : 0.0f;

    float2 n = abs(gradient / gl);
    float a = max(n.x, n.y) * FillCoverageScale;
    float b = min(n.x, n.y) * FillCoverageScale;

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

float bandCoverage(float signedDistance, float halfWidth, float2 gradient)
{
    // Area between the two locally parallel boundaries d=-w and d=+w. Keeping the signed
    // distance avoids the abs() cusp at the centerline and is smoother under rotation.
    return saturate(halfPlaneCoverage(signedDistance - halfWidth, gradient)
                  - halfPlaneCoverage(signedDistance + halfWidth, gradient));
}

float4 main(PS_INPUT i) : SV_Target
{
    float d = customSdPx(i.pos.xy);
    float2 g = float2(ddx(d), ddy(d));
    float coverage = halfPlaneCoverage(d, g);
    clip(coverage - (1.0f / 255.0f));
    float4 o = i.col;
    o.a *= coverage;
    return o;
}
