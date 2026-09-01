struct VS_INPUT
{
    float2 centerNdc : TEXCOORD0;
    float2 extentNdc : TEXCOORD1;
    float2 extentPx  : TEXCOORD2;
    float2 direction : TEXCOORD3;
    float4 params    : TEXCOORD4;
    float2 widthsPx  : TEXCOORD6;
    float2 extra     : TEXCOORD7;
    float4 col       : COLOR0;
    float4 shadowCol : COLOR1;
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
static const float2 Quad[4] =
{
    float2(-1,-1), float2(1,-1), float2(1,1), float2(-1,1)
};
PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    float2 corner = Quad[vertexId];
    PS_INPUT o;
    o.pos = float4(input.centerNdc.x + corner.x * input.extentNdc.x,
                   input.centerNdc.y - corner.y * input.extentNdc.y, 0, 1);
    o.localPx = corner * input.extentPx;
    o.direction = input.direction;
    o.params = input.params;
    o.widthsPx = input.widthsPx;
    o.extra = input.extra;
    o.col = input.col;
    o.shadowCol = input.shadowCol;
    return o;
}
