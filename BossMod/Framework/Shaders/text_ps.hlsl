Texture2D fontTexture : register(t3);
SamplerState fontSampler : register(s3);

struct PS_INPUT
{
    float4 pos          : SV_POSITION;
    float2 uv           : TEXCOORD0;
    nointerpolation float4 col          : COLOR0;
    nointerpolation float4 outlineCol   : COLOR1;
    nointerpolation float  outlineWidth : TEXCOORD1;
};

// Must match BuildArenaFont.bat (-pxrange 8). The loader rejects mismatched atlas metadata.
static const float MSDF_PX_RANGE = 8.0f;

float median3(float a, float b, float c)
{
    return max(min(a, b), min(max(a, b), c));
}

float screenPxRange(float2 uv)
{
    uint width, height;
    fontTexture.GetDimensions(width, height);
    float2 unitRange = float2(MSDF_PX_RANGE / max((float)width, 1.0f), MSDF_PX_RANGE / max((float)height, 1.0f));
    float2 uvPerScreenPixel = max(fwidth(uv), float2(1e-7f, 1e-7f));
    float2 screenTexSize = 1.0f / uvPerScreenPixel;
    return max(0.5f * dot(unitRange, screenTexSize), 1.0f);
}

float4 main(PS_INPUT input) : SV_Target
{
    float3 msd = fontTexture.Sample(fontSampler, input.uv).rgb;
    float pxRange = screenPxRange(input.uv);
    float pxDistance = (median3(msd.r, msd.g, msd.b) - 0.5f) * pxRange;

    float fillCoverage = saturate(pxDistance + 0.5f);

    // The MSDF only stores signed distances in roughly [-pxRange/2, +pxRange/2].
    // If an outside outline reaches that negative-distance limit, coverage stays non-zero
    // at the glyph UV-rect boundary and the entire atlas rectangle becomes visible. Reserve
    // the 0.5px AA ramp and clamp the requested outline to the representable outside range.
    float maxOutlineWidth = max(0.0f, 0.5f * pxRange - 0.5f);
    float outlineWidth = min(max(input.outlineWidth, 0.0f), maxOutlineWidth);
    float outlineCoverage = saturate(pxDistance + outlineWidth + 0.5f);

    float fillAlpha = input.col.a * fillCoverage;
    float outlineAlpha = input.outlineCol.a * max(outlineCoverage - fillCoverage, 0.0f);
    float outAlpha = fillAlpha + outlineAlpha * (1.0f - fillAlpha);

    // Renderer inherits straight-alpha ImGui blending, so convert the local premultiplied
    // fill-over-outline composition back to straight RGB before returning.
    float3 premul = input.col.rgb * fillAlpha + input.outlineCol.rgb * outlineAlpha * (1.0f - fillAlpha);
    float3 rgb = outAlpha > 1e-6f ? premul / outAlpha : 0.0f;
    return float4(rgb, outAlpha);
}
