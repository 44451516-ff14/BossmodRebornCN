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
    float3 origin          : POSITION0; // shape center/reference height
    float projectionHeight : TEXCOORD0;
    float4 boundsXZ        : TEXCOORD1; // minX, minZ, maxX, maxZ in world space
    float4 directionAux    : TEXCOORD2; // xy primary direction / triangle A, zw aux / triangle B
    float4 params0         : TEXCOORD3;
    float4 params1         : TEXCOORD4;
    float4 col             : COLOR0;
    uint packed            : TEXCOORD5;
    float outlineWidth     : TEXCOORD6;
    float4 outlineCol      : COLOR1;
    float2 waveOriginXZ    : TEXCOORD7;
};

struct PS_INPUT
{
    float4 pos                         : SV_POSITION;
    nointerpolation float4 col         : COLOR0;
    nointerpolation float3 origin      : TEXCOORD0;
    nointerpolation float4 directionAux: TEXCOORD1;
    nointerpolation float4 params0     : TEXCOORD2;
    nointerpolation float4 params1     : TEXCOORD3;
    nointerpolation uint packed        : TEXCOORD4;
    nointerpolation float outlineWidth : TEXCOORD5;
    nointerpolation float projectionHeight : TEXCOORD6;
    nointerpolation float4 outlineCol  : COLOR1;
    nointerpolation float2 waveOriginXZ : TEXCOORD7;
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
    output.directionAux = input.directionAux;
    output.params0 = input.params0;
    output.params1 = input.params1;
    output.packed = input.packed;
    output.outlineWidth = input.outlineWidth;
    output.projectionHeight = input.projectionHeight;
    output.outlineCol = input.outlineCol;
    output.waveOriginXZ = input.waveOriginXZ;

    // params1.x optionally carries a larger conservative height used only to build the screen-space
    // draw bounds. Actor markers use this so their pixel acceptance band can remain shallow while the
    // conservative screen footprint still covers their configured floor range.
    bool referencePlaneProjection = !(input.projectionHeight > 0.0f);
    float boundsProjectionHeight = referencePlaneProjection
        ? 0.0f
        : (input.params1.x > 0.0f ? max(input.projectionHeight, input.params1.x) : input.projectionHeight);
    float minY = input.origin.y - boundsProjectionHeight;
    float maxY = input.origin.y + boundsProjectionHeight;
    float2 minXZ = input.boundsXZ.xy;
    float2 maxXZ = input.boundsXZ.zw;

    if (maxXZ.x <= minXZ.x || maxXZ.y <= minXZ.y)
    {
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.col.a = 0.0f;
        return output;
    }

    // Zero height draws the authored horizontal plane. That plane has a finite footprint, so it uses
    // the same tight screen rectangle as an ordinary projected receiver.

    float2 minNdc = float2(1e20f, 1e20f);
    float2 maxNdc = float2(-1e20f, -1e20f);
    uint behind = 0u;
    uint projectedPointCount = 0u;

    [unroll]
    for (uint cornerY = 0u; cornerY < 2u; ++cornerY)
    {
        float y = cornerY == 0u ? minY : maxY;
        [unroll]
        for (uint cornerX = 0u; cornerX < 2u; ++cornerX)
        {
            float x = cornerX == 0u ? minXZ.x : maxXZ.x;
            [unroll]
            for (uint cornerZ = 0u; cornerZ < 2u; ++cornerZ)
            {
                float z = cornerZ == 0u ? minXZ.y : maxXZ.y;
                float4 world = float4(x, y, z, 1.0f);
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

    // Corners plus intersections of the twelve box edges with the camera near plane are the vertices
    // of the clipped convex volume. Their NDC extrema avoid the old all-screen first-person fallback.
    if (behind != 0u)
    {
        [unroll]
        for (uint xEdgeY = 0u; xEdgeY < 2u; ++xEdgeY)
        {
            float y = xEdgeY == 0u ? minY : maxY;
            [unroll]
            for (uint xEdgeZ = 0u; xEdgeZ < 2u; ++xEdgeZ)
            {
                float z = xEdgeZ == 0u ? minXZ.y : maxXZ.y;
                includeNearPlaneEdge(float4(minXZ.x, y, z, 1.0f), float4(maxXZ.x, y, z, 1.0f),
                    minNdc, maxNdc, projectedPointCount);
            }
        }
        [unroll]
        for (uint yEdgeX = 0u; yEdgeX < 2u; ++yEdgeX)
        {
            float x = yEdgeX == 0u ? minXZ.x : maxXZ.x;
            [unroll]
            for (uint yEdgeZ = 0u; yEdgeZ < 2u; ++yEdgeZ)
            {
                float z = yEdgeZ == 0u ? minXZ.y : maxXZ.y;
                includeNearPlaneEdge(float4(x, minY, z, 1.0f), float4(x, maxY, z, 1.0f),
                    minNdc, maxNdc, projectedPointCount);
            }
        }
        [unroll]
        for (uint zEdgeX = 0u; zEdgeX < 2u; ++zEdgeX)
        {
            float x = zEdgeX == 0u ? minXZ.x : maxXZ.x;
            [unroll]
            for (uint zEdgeY = 0u; zEdgeY < 2u; ++zEdgeY)
            {
                float y = zEdgeY == 0u ? minY : maxY;
                includeNearPlaneEdge(float4(x, y, minXZ.y, 1.0f), float4(x, y, maxXZ.y, 1.0f),
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

    float2 uv = 0.5f * (Corner[vertexId] + 1.0f);
    float2 ndcOut = lerp(minNdc, maxNdc, uv);
    output.pos = float4(ndcOut, 0.0f, 1.0f);
    return output;
}
