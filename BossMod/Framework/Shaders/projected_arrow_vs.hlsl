// Projection fix r14: zero height uses the authored plane and near-plane crossings stay bounded.

cbuffer WorldRenderConstants : register(b1)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport;
    float4 RasterScale;
    float4 SceneDepthParams;
    float4 SceneInfoParams;
};

struct VS_INPUT
{
    float3 origin             : POSITION0; // arrow tail/reference height
    float length              : TEXCOORD0;
    float2 directionXZ        : TEXCOORD1; // normalized +forward in world XZ
    float2 widths             : TEXCOORD2; // x shaft width, y head width
    float2 headProjection     : TEXCOORD3; // x head length, y +/- projection height
    float4 col                : COLOR0;
};

struct PS_INPUT
{
    float4 pos                         : SV_POSITION;
    nointerpolation float4 col         : COLOR0;
    nointerpolation float3 origin      : TEXCOORD0;
    nointerpolation float2 directionXZ : TEXCOORD1;
    nointerpolation float4 dimensions  : TEXCOORD2; // length, shaft half-width, head length, head half-width/length
    nointerpolation float projectionHeight : TEXCOORD3;
};

static const float2 Corner[4] =
{
    float2(-1.0f,  1.0f),
    float2( 1.0f,  1.0f),
    float2( 1.0f, -1.0f),
    float2(-1.0f, -1.0f)
};

void includeProjectedPoint(float4 world, inout float2 minNdc, inout float2 maxNdc,
    inout uint projectedPointCount)
{
    float4 clip = mul(world, ViewProj);
    if (abs(clip.w) > 1e-7f)
    {
        float2 ndc = clip.xy / clip.w;
        minNdc = min(minNdc, ndc);
        maxNdc = max(maxNdc, ndc);
        ++projectedPointCount;
    }
}

void includeNearPlaneEdge(float4 a, float4 b, inout float2 minNdc, inout float2 maxNdc,
    inout uint projectedPointCount)
{
    float an = dot(a, NearPlane);
    float bn = dot(b, NearPlane);
    if ((an < 0.0f) == (bn < 0.0f))
        return;

    float denominator = bn - an;
    if (abs(denominator) <= 1e-12f)
        return;
    float t = saturate(-an / denominator);
    includeProjectedPoint(lerp(a, b, t), minNdc, maxNdc, projectedPointCount);
}

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    PS_INPUT output;
    output.col = input.col;
    output.origin = input.origin;
    output.directionXZ = input.directionXZ;
    float headLength = clamp(input.headProjection.x, 1e-4f, max(input.length, 1e-4f));
    output.dimensions = float4(input.length, 0.5f * input.widths.x, headLength, 0.5f * input.widths.y / headLength);
    output.projectionHeight = input.headProjection.y;

    float halfWidth = 0.5f * max(input.widths.x, input.widths.y);
    float2 right = float2(-input.directionXZ.y, input.directionXZ.x);
    float2 tailXZ = input.origin.xz;
    float2 tipXZ = tailXZ + input.directionXZ * input.length;
    bool referencePlaneProjection = !(input.headProjection.y > 0.0f);
    float boundsProjectionHeight = referencePlaneProjection ? 0.0f : input.headProjection.y;
    float minY = input.origin.y - boundsProjectionHeight;
    float maxY = input.origin.y + boundsProjectionHeight;

    if (!(input.length > 1e-4f) || !(input.widths.x > 0.0f) || !(input.widths.y > 0.0f))
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.col.a = 0.0f;
        return output;
    }

    float2 minNdc = float2(1e20f, 1e20f);
    float2 maxNdc = float2(-1e20f, -1e20f);
    uint behind = 0u;
    uint projectedPointCount = 0u;

    [unroll]
    for (uint cornerY = 0u; cornerY < 2u; ++cornerY)
    {
        float y = cornerY == 0u ? minY : maxY;
        [unroll]
        for (uint cornerEnd = 0u; cornerEnd < 2u; ++cornerEnd)
        {
            float2 baseXZ = cornerEnd == 0u ? tailXZ : tipXZ;
            [unroll]
            for (uint cornerSide = 0u; cornerSide < 2u; ++cornerSide)
            {
                float side = cornerSide == 0u ? -halfWidth : halfWidth;
                float4 world = float4(baseXZ.x + right.x * side, y, baseXZ.y + right.y * side, 1.0f);
                float nearD = dot(world, NearPlane);
                if (nearD >= 0.0f)
                {
                    ++behind;
                    continue;
                }
                includeProjectedPoint(world, minNdc, maxNdc, projectedPointCount);
            }
        }
    }

    // Clip the twelve oriented-box edges instead of using an all-screen fallback near the camera.
    if (behind != 0u)
    {
        [unroll]
        for (uint longEdgeY = 0u; longEdgeY < 2u; ++longEdgeY)
        {
            float y = longEdgeY == 0u ? minY : maxY;
            [unroll]
            for (uint longEdgeSide = 0u; longEdgeSide < 2u; ++longEdgeSide)
            {
                float side = longEdgeSide == 0u ? -halfWidth : halfWidth;
                float2 sideOffset = right * side;
                includeNearPlaneEdge(
                    float4(tailXZ.x + sideOffset.x, y, tailXZ.y + sideOffset.y, 1.0f),
                    float4(tipXZ.x + sideOffset.x, y, tipXZ.y + sideOffset.y, 1.0f),
                    minNdc, maxNdc, projectedPointCount);
            }
        }
        [unroll]
        for (uint verticalEdgeEnd = 0u; verticalEdgeEnd < 2u; ++verticalEdgeEnd)
        {
            float2 baseXZ = verticalEdgeEnd == 0u ? tailXZ : tipXZ;
            [unroll]
            for (uint verticalEdgeSide = 0u; verticalEdgeSide < 2u; ++verticalEdgeSide)
            {
                float side = verticalEdgeSide == 0u ? -halfWidth : halfWidth;
                float2 sideXZ = baseXZ + right * side;
                includeNearPlaneEdge(
                    float4(sideXZ.x, minY, sideXZ.y, 1.0f),
                    float4(sideXZ.x, maxY, sideXZ.y, 1.0f),
                    minNdc, maxNdc, projectedPointCount);
            }
        }
        [unroll]
        for (uint widthEdgeY = 0u; widthEdgeY < 2u; ++widthEdgeY)
        {
            float y = widthEdgeY == 0u ? minY : maxY;
            [unroll]
            for (uint widthEdgeEnd = 0u; widthEdgeEnd < 2u; ++widthEdgeEnd)
            {
                float2 baseXZ = widthEdgeEnd == 0u ? tailXZ : tipXZ;
                includeNearPlaneEdge(
                    float4(baseXZ.x - right.x * halfWidth, y, baseXZ.y - right.y * halfWidth, 1.0f),
                    float4(baseXZ.x + right.x * halfWidth, y, baseXZ.y + right.y * halfWidth, 1.0f),
                    minNdc, maxNdc, projectedPointCount);
            }
        }
    }

    if (projectedPointCount == 0u)
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.col.a = 0.0f;
        return output;
    }

    if (maxNdc.x < -1.0f || minNdc.x > 1.0f || maxNdc.y < -1.0f || minNdc.y > 1.0f)
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.col.a = 0.0f;
        return output;
    }
    float2 pad = 2.0f * RasterScale.zw;
    minNdc = max(minNdc - pad, float2(-1.0f, -1.0f));
    maxNdc = min(maxNdc + pad, float2(1.0f, 1.0f));

    float2 corner = Corner[vertexId];
    float2 uv = 0.5f * (corner + 1.0f);
    float2 ndcOut = lerp(minNdc, maxNdc, uv);
    output.pos = float4(ndcOut, 0.0f, 1.0f);
    return output;
}
