struct VS_INPUT
{
    float2 centerNdc : TEXCOORD0;
    float2 extentNdc : TEXCOORD1;
    float2 extentPx  : TEXCOORD2;
    float2 direction : TEXCOORD3;
    float4 params    : TEXCOORD4;
    float4 col       : COLOR0;
};

struct PS_INPUT
{
    float4 pos       : SV_POSITION;
    float2 localPx   : TEXCOORD0;
    nointerpolation float2 direction : TEXCOORD1;
    nointerpolation float4 params    : TEXCOORD2;
    nointerpolation float2 arcEndDirection : TEXCOORD3;
    nointerpolation float4 col       : COLOR0;
};

static const float2 Quad[4] =
{
    float2(-1.0f, -1.0f),
    float2( 1.0f, -1.0f),
    float2( 1.0f,  1.0f),
    float2(-1.0f,  1.0f)
};

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    float2 corner = Quad[vertexId];
    PS_INPUT output;
    output.pos = float4(
        input.centerNdc.x + corner.x * input.extentNdc.x,
        input.centerNdc.y - corner.y * input.extentNdc.y,
        0.0f,
        1.0f);
    output.localPx = corner * input.extentPx;
    output.direction = input.direction;
    output.params = input.params;
    output.arcEndDirection = 0.0f;
    if (input.params.w >= 3.5f && input.params.w < 4.5f)
    {
        float sinSweep, cosSweep;
        sincos(input.params.z, sinSweep, cosSweep);
        output.arcEndDirection = float2(
            input.direction.x * cosSweep + input.direction.y * sinSweep,
            input.direction.y * cosSweep - input.direction.x * sinSweep);
    }
    output.col = input.col;
    return output;
}
