struct PS_INPUT
{
    float4 pos                    : SV_POSITION;
    float4 col                    : COLOR0;
    float3 bary                   : TEXCOORD0;
    nointerpolation uint boundary : TEXCOORD1;
};

float edgeCoverage(float coordinate)
{
    // fwidth converts barycentric distance into approximately one screen pixel.
    // Centering smoothstep around zero gives ~50% coverage on the geometric edge,
    // matching the derivative AA convention used by the analytic shape shaders.
    float aa = max(fwidth(coordinate), 1e-5f);
    return smoothstep(-aa, aa, coordinate);
}

float4 main(PS_INPUT input) : SV_Target
{
    float coverage = 1.0f;

    // X is zero on BC, Y on CA, Z on AB. Only true polygon-boundary edges are enabled;
    // triangulation-internal edges remain full coverage and therefore cannot create seams.
    if ((input.boundary & 1u) != 0u)
        coverage = min(coverage, edgeCoverage(input.bary.x));
    if ((input.boundary & 2u) != 0u)
        coverage = min(coverage, edgeCoverage(input.bary.y));
    if ((input.boundary & 4u) != 0u)
        coverage = min(coverage, edgeCoverage(input.bary.z));

    clip(coverage - 0.001f);
    float4 result = input.col;
    result.a *= coverage;
    return result;
}
