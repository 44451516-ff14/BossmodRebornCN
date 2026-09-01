cbuffer WorldLineConstants : register(b1)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport;
    float4 RasterScale; // zw = framebuffer pixel -> NDC scale
    float4 SceneDepthParams;
    float4 SceneInfoParams;
};

struct VS_INPUT
{
    float3 center       : POSITION0;
    float4 rectPx       : TEXCOORD0;
    float4 uvRect       : TEXCOORD1;
    float4 col          : COLOR0;
    float4 outlineCol   : COLOR1;
    float  outlineWidth : TEXCOORD2;
};

struct PS_INPUT
{
    float4 pos          : SV_POSITION;
    float2 uv           : TEXCOORD0;
    nointerpolation float4 col          : COLOR0;
    nointerpolation float4 outlineCol   : COLOR1;
    nointerpolation float  outlineWidth : TEXCOORD1;
};

static const float2 Quad[4] =
{
    float2(0.0f, 0.0f), float2(1.0f, 0.0f),
    float2(1.0f, 1.0f), float2(0.0f, 1.0f)
};

PS_INPUT main(VS_INPUT input, uint vertexId : SV_VertexID)
{
    float2 corner = Quad[vertexId];
    PS_INPUT output;
    output.col = input.col;
    output.outlineCol = input.outlineCol;
    output.outlineWidth = input.outlineWidth;
    output.uv = float2(
        corner.x < 0.5f ? input.uvRect.x : input.uvRect.z,
        corner.y < 0.5f ? input.uvRect.y : input.uvRect.w);

    float4 clip = mul(float4(input.center, 1.0f), ViewProj);
    if (!(clip.w > 1e-5f))
    {
        // Keep the instance degenerate/invisible when the anchor is behind the camera.
        output.pos = float4(2.0f, 2.0f, 0.0f, 1.0f);
        output.col.a = 0.0f;
        output.outlineCol.a = 0.0f;
        return output;
    }

    float2 px = float2(
        corner.x < 0.5f ? input.rectPx.x : input.rectPx.z,
        corner.y < 0.5f ? input.rectPx.y : input.rectPx.w);

    // RectPx is in framebuffer pixels. Apply the offset in NDC and multiply by clip.w so the
    // billboard remains fixed-size on screen while retaining the anchor's world depth.
    float2 ndcOffset = float2(px.x * RasterScale.z, -px.y * RasterScale.w);
    clip.xy += ndcOffset * clip.w;
    output.pos = clip;
    return output;
}
