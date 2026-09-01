struct VS_INPUT
{
    float2 pos      : POSITION;
    float4 col      : COLOR0;
    uint boundary   : TEXCOORD0;
};

struct PS_INPUT
{
    float4 pos                  : SV_POSITION;
    float4 col                  : COLOR0;
    float3 bary                 : TEXCOORD0;
    nointerpolation uint boundary : TEXCOORD1;
};

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    PS_INPUT output;
    output.pos = float4(input.pos, 0.0f, 1.0f);
    output.col = input.col;
    output.boundary = input.boundary;

    uint corner = vertexId % 3;
    output.bary = corner == 0
        ? float3(1.0f, 0.0f, 0.0f)
        : (corner == 1 ? float3(0.0f, 1.0f, 0.0f) : float3(0.0f, 0.0f, 1.0f));
    return output;
}
