// Signature intentionally matches AnalyticInstance/_analyticInputLayout exactly.
// Kept deliberately simple: each of the fixed 18 output vertices
// selects one of the six precomputed NDC corners directly.
struct VS_INPUT
{
    float2 aL   : TEXCOORD0;
    float2 aR   : TEXCOORD1;
    float2 bL   : TEXCOORD2;
    float2 bR   : TEXCOORD3;
    float4 cLR  : TEXCOORD4; // xy = cL, zw = cR
    float4 col  : COLOR0;
};

struct PS_INPUT
{
    float4 pos                      : SV_POSITION;
    float4 col                      : COLOR0;
    float3 bary                     : TEXCOORD0;
    nointerpolation uint boundary   : TEXCOORD1;
};

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    float2 cL = input.cLR.xy;
    float2 cR = input.cLR.zw;
    float2 p;
    float3 bary;
    uint boundary;

    // AB: aL,bL,bR / aL,bR,aR
    // BC: bL,cL,cR / bL,cR,bR
    // CA: cL,aL,aR / cL,aR,cR
    if (vertexId == 0)       { p = input.aL; bary = float3(1,0,0); boundary = 4; }
    else if (vertexId == 1)  { p = input.bL; bary = float3(0,1,0); boundary = 4; }
    else if (vertexId == 2)  { p = input.bR; bary = float3(0,0,1); boundary = 4; }
    else if (vertexId == 3)  { p = input.aL; bary = float3(1,0,0); boundary = 1; }
    else if (vertexId == 4)  { p = input.bR; bary = float3(0,1,0); boundary = 1; }
    else if (vertexId == 5)  { p = input.aR; bary = float3(0,0,1); boundary = 1; }
    else if (vertexId == 6)  { p = input.bL; bary = float3(1,0,0); boundary = 4; }
    else if (vertexId == 7)  { p = cL;       bary = float3(0,1,0); boundary = 4; }
    else if (vertexId == 8)  { p = cR;       bary = float3(0,0,1); boundary = 4; }
    else if (vertexId == 9)  { p = input.bL; bary = float3(1,0,0); boundary = 1; }
    else if (vertexId == 10) { p = cR;       bary = float3(0,1,0); boundary = 1; }
    else if (vertexId == 11) { p = input.bR; bary = float3(0,0,1); boundary = 1; }
    else if (vertexId == 12) { p = cL;       bary = float3(1,0,0); boundary = 4; }
    else if (vertexId == 13) { p = input.aL; bary = float3(0,1,0); boundary = 4; }
    else if (vertexId == 14) { p = input.aR; bary = float3(0,0,1); boundary = 4; }
    else if (vertexId == 15) { p = cL;       bary = float3(1,0,0); boundary = 1; }
    else if (vertexId == 16) { p = input.aR; bary = float3(0,1,0); boundary = 1; }
    else                     { p = cR;       bary = float3(0,0,1); boundary = 1; }

    PS_INPUT output;
    output.pos = float4(p, 0, 1);
    output.col = input.col;
    output.bary = bary;
    output.boundary = boundary;
    return output;
}
