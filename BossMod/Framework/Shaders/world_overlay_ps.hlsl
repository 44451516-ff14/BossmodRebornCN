Texture2D<float4> PremultipliedOverlay : register(t0);

float4 main(float4 pos : SV_POSITION) : SV_Target
{
    float4 color = PremultipliedOverlay.Load(int3(int2(pos.xy), 0));
    if (color.a <= 1e-6f)
        return 0.0f;

    // Normal source-alpha rendering accumulates premultiplied RGB in a transparent target. The
    // native TextureImageNode applies alpha when it composites, so convert back to straight alpha
    // here to avoid multiplying edge alpha twice.
    color.rgb = saturate(color.rgb / color.a);
    return color;
}
