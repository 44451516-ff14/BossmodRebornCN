// Lightweight animated treatment for terrain-projected filled danger zones.
// One tiny constant buffer is shared by the projected-zone pixel shader; no extra geometry or draws.
cbuffer ZoneWaveConstants : register(b7)
{
    float4 ZoneWaveParams; // x = elapsed seconds
};

// Cheap radial-distance approximation (octagonal isolines). Max error is small enough for a
// decorative wave and avoids a sqrt in every covered pixel.
float zoneWaveDistance(float2 delta)
{
    float2 a = abs(delta);
    float hi = max(a.x, a.y);
    float lo = min(a.x, a.y);
    return hi + 0.375f * lo;
}

float zoneWavePulse(float distance, float spacing, float speed)
{
    // phase==0 is a wave front. Subtracting time makes each front move toward larger radii.
    float phase = frac(distance / spacing - ZoneWaveParams.x * speed / spacing);
    float edge = min(phase, 1.0f - phase);
    return 1.0f - smoothstep(0.0f, 0.115f, edge);
}

float4 applyZoneWaveWorld(float4 color, float2 deltaWorld, float strength)
{
    if (strength <= 0.0f)
        return color;

    // Keep the same 4.5-unit spacing and 2.9-unit/sec speed as before. Squaring the smooth pulse
    // tightens the visible crest without adding rings; the stronger luminance/alpha contrast makes
    // each front easier to read while keeping the underlying zone colour dominant.
    float pulse = zoneWavePulse(zoneWaveDistance(deltaWorld), 4.5f, 2.9f) * saturate(strength);
    pulse *= pulse;
    color.rgb *= 1.0f + 0.075f * pulse;
    color.a *= 0.92f + 0.08f * pulse;
    return color;
}
