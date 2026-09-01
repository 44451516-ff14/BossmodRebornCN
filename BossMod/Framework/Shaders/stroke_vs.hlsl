struct VS_INPUT
{
    float2 prevNdc   : POSITION0;
    float2 aNdc      : POSITION1;
    float2 bNdc      : POSITION2;
    float2 nextNdc   : POSITION3;
    float2 ndcToPx      : TEXCOORD0;
    float2 widthsPx    : TEXCOORD1;
    float4 col         : COLOR0;
    float4 shadowCol   : COLOR1;
    uint flags         : TEXCOORD3;
};

struct PS_INPUT
{
    float4 pos                         : SV_POSITION;
    nointerpolation float4 col         : COLOR0;
    nointerpolation float4 shadowCol   : COLOR1;
    noperspective float acrossPx       : TEXCOORD0;
    noperspective float alongPx        : TEXCOORD1;
    nointerpolation float3 params      : TEXCOORD2; // x/y widths, z segment length
    nointerpolation uint flags         : TEXCOORD3;
};

static const float2 Corner[4] =
{
    float2(0.0f,  1.0f),
    float2(1.0f,  1.0f),
    float2(1.0f, -1.0f),
    float2(0.0f, -1.0f)
};

float2 normalizeOr(float2 v, float2 fallback)
{
    float lenSq = dot(v, v);
    return lenSq > 1e-8f ? v * rsqrt(lenSq) : fallback;
}

float2 joinOffset(float2 incoming, float2 outgoing)
{
    float2 n0 = float2(-incoming.y, incoming.x);
    float2 n1 = float2(-outgoing.y, outgoing.x);
    float2 sum = n0 + n1;
    float sumLenSq = dot(sum, sum);
    if (!(sumLenSq > 1e-8f))
        return n1;

    float2 miter = sum * rsqrt(sumLenSq);
    float denom = dot(miter, n1);
    if (abs(denom) < 0.2f)
        denom = denom < 0.0f ? -0.2f : 0.2f;

    float2 result = miter / denom;
    float resultLenSq = dot(result, result);
    if (resultLenSq > 16.0f)
        result *= 4.0f * rsqrt(resultLenSq);
    return result;
}

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    PS_INPUT output;
    output.col = input.col;
    output.shadowCol = input.shadowCol;
    output.flags = input.flags;

    float2 deltaPx = (input.bNdc - input.aNdc) * input.ndcToPx;
    float segmentLenSq = dot(deltaPx, deltaPx);
    if (!(segmentLenSq > 1e-8f))
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.acrossPx = 0.0f;
        output.alongPx = 0.0f;
        output.params = float3(0.0f, 0.0f, 0.0f);
        output.col.a = 0.0f;
        output.shadowCol.a = 0.0f;
        return output;
    }

    float segmentLengthPx = sqrt(segmentLenSq);
    float2 direction = deltaPx / segmentLengthPx;
    float2 normal = float2(-direction.y, direction.x);

    bool startCap = (input.flags & 0x1u) != 0u;
    bool endCap = (input.flags & 0x2u) != 0u;
    float2 incoming = startCap ? direction : normalizeOr((input.aNdc - input.prevNdc) * input.ndcToPx, direction);
    float2 outgoing = endCap ? direction : normalizeOr((input.nextNdc - input.bNdc) * input.ndcToPx, direction);

    float outerHalfWidthPx = max(0.0f, max(input.widthsPx.x, input.widthsPx.y)) + 2.0f;
    float2 joinA = (startCap ? normal : joinOffset(incoming, direction)) * outerHalfWidthPx;
    float2 joinB = (endCap ? normal : joinOffset(direction, outgoing)) * outerHalfWidthPx;

    float2 corner = Corner[vertexId];
    bool atB = corner.x > 0.5f;
    float side = corner.y;
    const float aaPadPx = 2.0f;
    float2 localPx;
    float2 ndc;
    if (atB)
    {
        localPx = deltaPx + side * joinB + (endCap ? direction * aaPadPx : float2(0.0f, 0.0f));
        ndc = input.aNdc + localPx / input.ndcToPx;
    }
    else
    {
        localPx = side * joinA - (startCap ? direction * aaPadPx : float2(0.0f, 0.0f));
        ndc = input.aNdc + localPx / input.ndcToPx;
    }

    output.pos = float4(ndc, 0.0f, 1.0f);
    output.acrossPx = dot(localPx, normal);
    output.alongPx = dot(localPx, direction);
    output.params = float3(input.widthsPx, segmentLengthPx);
    return output;
}
