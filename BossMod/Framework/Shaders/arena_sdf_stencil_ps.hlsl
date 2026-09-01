Texture2D<float> ArenaSdf : register(t1);
SamplerState SdfSampler : register(s1);

cbuffer ArenaSdfConstants : register(b1)
{
    float4 ArenaUvRow0;
    float4 ArenaUvRow1;
    float4 ArenaOutsideScale;
    float4 ArenaMipGrad;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
};

float arenaSdPx(float2 framebufferPx)
{
    float3 hp = float3(framebufferPx, 1.0f);
    float2 uv = float2(dot(hp, ArenaUvRow0.xyz), dot(hp, ArenaUvRow1.xyz));
    float2 clampedUv = saturate(uv);
    float sampledPx = ArenaSdf.SampleGrad(SdfSampler, clampedUv, ArenaMipGrad.xy, ArenaMipGrad.zw).r * ArenaUvRow1.w;
    float result = sampledPx;
    [branch]
    if (any(uv != clampedUv))
    {
        float2 outsideUv = uv - clampedUv;
        float2 outsidePxAxes = outsideUv * ArenaOutsideScale.xy;
        result += length(outsidePxAxes);
    }
    return result;
}

float4 main(PS_INPUT i) : SV_Target
{
    float d = arenaSdPx(i.pos.xy);
    clip(-d);
    return float4(0.0f, 0.0f, 0.0f, 0.0f);
}
