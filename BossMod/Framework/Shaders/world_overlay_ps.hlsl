Texture2D<float4> PremultipliedOverlay : register(t0);

cbuffer WorldOverlayConstants : register(b0)
{
    float4 RiskColor;
    float4 RiskParams; // xy = framebuffer size, z = pulse phase
};

float ScreenRiskAlpha(float2 pixel)
{
    if (RiskColor.a <= 0.0f)
        return 0.0f;

    // Use the shorter screen dimension so the glow stays equally wide on all four edges,
    // including ultrawide displays. The center of the screen remains fully untouched.
    float2 size = max(RiskParams.xy, 1.0f);
    float2 edgePixels = min(pixel, size - pixel);
    float edge = max(min(edgePixels.x, edgePixels.y), 0.0f) / min(size.x, size.y);
    if (edge >= 0.065f)
        return 0.0f;

    float glow = 1.0f - smoothstep(0.0f, 0.065f, edge);
    glow *= glow;
    float rim = 1.0f - smoothstep(0.0f, 0.004f, edge);
    float pulse = 0.5f + 0.5f * cos(RiskParams.z);
    float shimmer = 0.92f + 0.08f * cos(edge * 160.0f - RiskParams.z * 2.0f);
    float alpha = 0.34f * glow * (0.4f + 0.6f * pulse * pulse) * shimmer
        + 0.14f * rim * (0.5f + 0.5f * pulse);
    return saturate(RiskColor.a * alpha);
}

float4 main(float4 pos : SV_POSITION) : SV_Target
{
    float4 color = PremultipliedOverlay.Load(int3(int2(pos.xy), 0));
    float riskAlpha = ScreenRiskAlpha(pos.xy);
    // Composite into premultiplied RGB before the existing straight-alpha resolve. This keeps
    // the border visible by itself and preserves world shapes underneath it without dark halos.
    color.rgb = color.rgb * (1.0f - riskAlpha) + RiskColor.rgb * riskAlpha;
    color.a = color.a * (1.0f - riskAlpha) + riskAlpha;
    if (color.a <= 1e-6f)
        return 0.0f;

    // Normal source-alpha rendering accumulates premultiplied RGB in a transparent target. The
    // native TextureImageNode applies alpha when it composites, so convert back to straight alpha
    // here to avoid multiplying edge alpha twice.
    color.rgb = saturate(color.rgb / color.a);
    return color;
}
