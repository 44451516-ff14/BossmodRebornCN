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
    float3 center            : POSITION0;
    float radius             : TEXCOORD0;
    float4 curveParams       : TEXCOORD1;
    float4 col               : COLOR0;
    float thickness          : TEXCOORD2;
    uint transformIndex      : TEXCOORD3;
    uint packed              : TEXCOORD4;
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

static const float PI = 3.14159265358979323846f;
static const float TWO_PI = 6.28318530717958647692f;
static const float2 Corner[4] =
{
    float2(-1.0f,  1.0f),
    float2( 1.0f,  1.0f),
    float2( 1.0f, -1.0f),
    float2(-1.0f, -1.0f)
};

void RingPointPair(float radius, uint segment, float invSegments, float sinStep, float cosStep, out float3 a, out float3 b)
{
    float angle = TWO_PI * (float)segment * invSegments;
    float sa, ca;
    sincos(angle, sa, ca);
    float sb = sa * cosStep + ca * sinStep;
    float cb = ca * cosStep - sa * sinStep;
    a = float3(sa * radius, 0.0f, ca * radius);
    b = float3(sb * radius, 0.0f, cb * radius);
}

void ArcPointPair(float radius, float angleA, float sinStep, float cosStep, out float3 a, out float3 b)
{
    float sa, ca;
    sincos(angleA, sa, ca);
    float sb = sa * cosStep + ca * sinStep;
    float cb = ca * cosStep - sa * sinStep;
    a = float3(sa * radius, 0.0f, ca * radius);
    b = float3(sb * radius, 0.0f, cb * radius);
}

bool BuildCurveEndpoints(VS_INPUT input, uint lineIndex, out float3 a, out float3 b)
{
    a = input.center;
    b = input.center;
    uint kind = input.packed & 0x7u;
    uint segments = input.packed >> 3;
    if (segments == 0u || !(input.radius >= 0.0f) || !(input.thickness > 0.0f))
        return false;

    if (kind == 1u) // horizontal XZ circle
    {
        if (lineIndex >= segments)
            return false;
        float3 pa, pb;
        RingPointPair(input.radius, lineIndex, input.curveParams.z, input.curveParams.x, input.curveParams.y, pa, pb);
        a += pa;
        b += pb;
        return true;
    }

    if (kind == 2u) // three orthogonal great-circle rings
    {
        // XZ/YZ/XY for segment 0, then segment 1...
        uint segment = lineIndex / 3u;
        uint ring = lineIndex - segment * 3u;
        if (segment >= segments)
            return false;
        float3 pa, pb;
        RingPointPair(input.radius, segment, input.curveParams.z, input.curveParams.x, input.curveParams.y, pa, pb);
        if (ring == 0u)
        {
            a += pa;
            b += pb;
        }
        else if (ring == 1u)
        {
            a += float3(0.0f, pa.z, pa.x);
            b += float3(0.0f, pb.z, pb.x);
        }
        else
        {
            a += float3(pa.z, pa.x, 0.0f);
            b += float3(pb.z, pb.x, 0.0f);
        }
        return true;
    }

    if (kind == 3u) // cylinder: top ring, bottom ring, and one vertical per angular segment
    {
        uint segment = lineIndex / 3u;
        uint edgeKind = lineIndex - segment * 3u;
        if (segment >= segments)
            return false;
        float3 pa, pb;
        RingPointPair(input.radius, segment, input.curveParams.w, input.curveParams.y, input.curveParams.z, pa, pb);
        float halfHeight = input.curveParams.x;
        if (edgeKind == 0u)
        {
            a += float3(pa.x, halfHeight, pa.z);
            b += float3(pb.x, halfHeight, pb.z);
        }
        else if (edgeKind == 1u)
        {
            a += float3(pa.x, -halfHeight, pa.z);
            b += float3(pb.x, -halfHeight, pb.z);
        }
        else
        {
            // verticals are attached to each segment's end point.
            a += float3(pb.x, halfHeight, pb.z);
            b += float3(pb.x, -halfHeight, pb.z);
        }
        return true;
    }

    if (kind == 4u) // horizontal arc sector (the existing DrawWorldCone wire outline)
    {
        if (lineIndex >= segments + 2u)
            return false;
        float startAngle = input.curveParams.x;
        float step = input.curveParams.y;
        float sinStep = input.curveParams.z;
        float cosStep = input.curveParams.w;

        if (lineIndex == 0u)
        {
            float sn, cs;
            sincos(startAngle, sn, cs);
            b += float3(sn * input.radius, 0.0f, cs * input.radius);
            return true;
        }
        if (lineIndex == segments + 1u)
        {
            float endAngle = startAngle + step * (float)segments;
            float sn, cs;
            sincos(endAngle, sn, cs);
            a += float3(sn * input.radius, 0.0f, cs * input.radius);
            return true;
        }

        uint segment = lineIndex - 1u;
        float3 pa, pb;
        ArcPointPair(input.radius, startAngle + step * (float)segment, sinStep, cosStep, pa, pb);
        a += pa;
        b += pb;
        return true;
    }

    return false;
}

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    PS_INPUT output;
    output.col = input.col;
    output.shadowCol = float4(0.0f, 0.0f, 0.0f, 0.0f);
    output.flags = 0x7u; // start/end caps + scene-depth occlusion

    // Indexed draw assigns four unique vertex ids to each generated line; the shared
    // index buffer repeats ids 0 and 2 to form the same two triangles
    uint lineIndex = vertexId / 4u;
    uint cornerIndex = vertexId - lineIndex * 4u;
    float3 localA, localB;
    bool rejected = !BuildCurveEndpoints(input, lineIndex, localA, localB);

    float4 wa = mul(float4(localA, 1.0f), WorldTransform[input.transformIndex]);
    float4 wb = mul(float4(localB, 1.0f), WorldTransform[input.transformIndex]);
    float an = dot(wa, NearPlane);
    float bn = dot(wb, NearPlane);
    float4 ca = mul(wa, ViewProj);
    float4 cb = mul(wb, ViewProj);

    rejected = rejected || (an >= 0.0f && bn >= 0.0f);
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

    float2 corner = Corner[cornerIndex];
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
