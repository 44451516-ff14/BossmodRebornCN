cbuffer WorldLineConstants : register(b1)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport; // x/y framebuffer dimensions, z logical->framebuffer pixel scale
    float4 RasterScale;
    float4 SceneDepthParams;
    float4 SceneInfoParams;
};

cbuffer WorldLineTransforms : register(b2)
{
    row_major float4x4 WorldTransform[1024];
};

struct VS_INPUT
{
    float3 from              : POSITION0;
    float thickness          : TEXCOORD0;
    float3 to                : POSITION1;
    float4 col               : COLOR0;
    uint transformIndex      : TEXCOORD1;
};

struct PS_INPUT
{
    float4 pos                       : SV_POSITION;
    nointerpolation float4 col       : COLOR0;
    nointerpolation float4 shadowCol : COLOR1;
    noperspective float acrossPx     : TEXCOORD0;
    noperspective float alongPx      : TEXCOORD1;
    nointerpolation float3 params    : TEXCOORD2;
    nointerpolation uint flags       : TEXCOORD3;
};

static const float2 Corner[4] =
{
    float2(-1.0f,  1.0f),
    float2( 1.0f,  1.0f),
    float2( 1.0f, -1.0f),
    float2(-1.0f, -1.0f)
};

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    PS_INPUT output;
    output.col = input.col;
    output.shadowCol = float4(0.0f, 0.0f, 0.0f, 0.0f);
    output.flags = 0x7u; // start/end caps + scene-depth occlusion

    float4 wa = mul(float4(input.from, 1.0f), WorldTransform[input.transformIndex]);
    float4 wb = mul(float4(input.to, 1.0f), WorldTransform[input.transformIndex]);
    float an = dot(wa, NearPlane);
    float bn = dot(wb, NearPlane);
    float4 ca = mul(wa, ViewProj);
    float4 cb = mul(wb, ViewProj);

    bool rejected = an >= 0.0f && bn >= 0.0f;
    if (!rejected && (an >= 0.0f || bn >= 0.0f))
    {
        float denom = bn - an;
        if (abs(denom) <= 1e-12f)
            rejected = true;
        else
        {
            float t = saturate(-an / denom);
            float4 clipped = lerp(ca, cb, t);
            float4 clippedWorld = lerp(wa, wb, t);
            if (an >= 0.0f)
            {
                ca = clipped;
                wa = clippedWorld;
            }
            else
            {
                cb = clipped;
                wb = clippedWorld;
            }
        }
    }

    if (!rejected)
    {
        rejected = (ca.x < -ca.w && cb.x < -cb.w)
            || (ca.x > ca.w && cb.x > cb.w)
            || (ca.y < -ca.w && cb.y < -cb.w)
            || (ca.y > ca.w && cb.y > cb.w);
    }

    if (rejected)
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.acrossPx = 0.0f;
        output.alongPx = 0.0f;
        output.params = float3(0.0f, 0.0f, 0.0f);
        output.col.a = 0.0f;
        return output;
    }

    float2 aNdc = ca.xy / ca.w;
    float2 bNdc = cb.xy / cb.w;
    float2 deltaPx = float2(
        (bNdc.x - aNdc.x) * 0.5f * Viewport.x,
        (bNdc.y - aNdc.y) * -0.5f * Viewport.y);
    float segmentLengthPx = length(deltaPx);
    if (!(segmentLengthPx > 1e-5f) || !(input.thickness > 0.0f))
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.acrossPx = 0.0f;
        output.alongPx = 0.0f;
        output.params = float3(0.0f, 0.0f, 0.0f);
        output.col.a = 0.0f;
        return output;
    }

    float2 directionPx = deltaPx / segmentLengthPx;
    float2 normalPx = float2(-directionPx.y, directionPx.x);
    float halfWidthPx = 0.5f * input.thickness * Viewport.z;
    float outerHalfWidthPx = halfWidthPx + 2.0f;

    float2 corner = Corner[vertexId];
    float alongPx = corner.x < 0.0f ? -2.0f : segmentLengthPx + 2.0f;
    float acrossPx = corner.y * outerHalfWidthPx;
    float2 offsetPx = directionPx * alongPx + normalPx * acrossPx;
    float2 offsetNdc = offsetPx * float2(RasterScale.z, -RasterScale.w);

    // Preserve endpoint clip-space Z/W instead of flattening the expanded quad to z=0,w=1.
    // stroke_ps uses the resulting rasterized SV_POSITION.z to reconstruct this fragment's
    // primitive position on the same camera ray as the sampled FFXIV scene depth.
    bool atStart = corner.x < 0.0f;
    float endpointW = atStart ? ca.w : cb.w;
    float endpointZ = atStart ? ca.z : cb.z;
    float2 outNdc = aNdc + offsetNdc;
    output.pos = float4(outNdc * endpointW, endpointZ, endpointW);
    output.acrossPx = acrossPx;
    output.alongPx = alongPx;
    output.params = float3(halfWidthPx, 0.0f, segmentLengthPx);
    return output;
}
