Texture2D spriteTexture : register(t3);
SamplerState spriteSampler : register(s3);

struct PS_INPUT
{
    float4 pos          : SV_POSITION;
    float2 uv           : TEXCOORD0;
    nointerpolation float4 col          : COLOR0;
    nointerpolation float4 outlineCol   : COLOR1;
    nointerpolation float  outlineWidth : TEXCOORD1;
};

float4 main(PS_INPUT input) : SV_Target
{
    return spriteTexture.Sample(spriteSampler, input.uv) * input.col;
}
