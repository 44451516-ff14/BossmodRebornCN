struct VS_OUTPUT
{
    float4 pos : SV_POSITION;
};

VS_OUTPUT main(uint vertexId : SV_VertexID)
{
    float2 uv = float2((vertexId << 1u) & 2u, vertexId & 2u);
    VS_OUTPUT output;
    output.pos = float4(uv.x * 2.0f - 1.0f, 1.0f - uv.y * 2.0f, 0.0f, 1.0f);
    return output;
}
