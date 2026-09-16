Texture2D<float> ArenaSdf : register(t1);
SamplerState ArenaSdfSampler : register(s1);
static const float OutlineCoverageScale = 1.5f;
static const float ShadowCoverageScale = 1.75f;
static const float PI = 3.14159265358979323846f;
static const float TWO_PI = 6.28318530717958647692f;

cbuffer ArenaSdfConstants : register(b1)
{
    float4 ArenaUvRow0;
    float4 ArenaUvRow1;
    float4 ArenaOutsideScale;
    float4 ArenaMipGrad;
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

float sdBox(float2 p, float2 h)
{
    float2 q = abs(p) - h;
    return length(max(q, 0.0f)) + min(max(q.x, q.y), 0.0f);
}

float distSegment(float2 p, float2 a, float2 b)
{
    float2 ab = b - a;
    float t = saturate(dot(p - a, ab) / max(dot(ab, ab), 1e-6f));
    return length(p - (a + t * ab));
}

float sdCapsule(float2 p, float2 d, float halfSeg, float r)
{
    float a = clamp(dot(p, d), -halfSeg, halfSeg);
    return length(p - d * a) - r;
}

bool directionInArc(float2 startDir, float2 radial, float2 endDir, float sweep)
{
    float absSweep = abs(sweep);
    if (absSweep >= TWO_PI - 1e-5f)
        return true;

    if (absSweep <= 1e-6f)
        return false;

    float sweepSign = sweep >= 0.0f ? 1.0f : -1.0f;
    float fromStart = -(startDir.x * radial.y - startDir.y * radial.x) * sweepSign;
    float toEnd = -(radial.x * endDir.y - radial.y * endDir.x) * sweepSign;
    const float sideEpsilon = -1e-6f;
    return absSweep <= PI
        ? fromStart >= sideEpsilon && toEnd >= sideEpsilon
        : fromStart >= sideEpsilon || toEnd >= sideEpsilon;
}

float sdArcCapsule(float2 p, float2 startDir, float orbitR, float r, float sweep, float2 endDir)
{
    float d = length(p);
    float2 radial = d > 1e-5f ? p / d : startDir;
    float2 startDelta = p - startDir * orbitR;
    float2 endDelta = p - endDir * orbitR;
    float dc = directionInArc(startDir, radial, endDir, sweep)
        ? abs(d - orbitR)
        : sqrt(min(dot(startDelta, startDelta), dot(endDelta, endDelta)));
    return dc - r;
}

float edgeDistanceCone(float2 p, float2 dir, float outerR, float innerR, float c, float sn, float halfAngle)
{
    if (halfAngle >= 3.141591f)
    {
        float d = length(p);
        float e = abs(d - outerR);
        if (innerR > 0.0f)
            e = min(e, abs(d - innerR));
        return e;
    }

    float2 sideA = float2(dir.x * c + dir.y * sn, dir.y * c - dir.x * sn);
    float2 sideB = float2(dir.x * c - dir.y * sn, dir.y * c + dir.x * sn);
    float d = length(p);
    float2 radial = d > 1e-5f ? p / d : dir;
    float e = 1e20f;
    if (dot(radial, dir) >= c)
    {
        e = abs(d - outerR);
        if (innerR > 0.0f)
            e = min(e, abs(d - innerR));
    }
    e = min(e, distSegment(p, sideA * innerR, sideA * outerR));
    e = min(e, distSegment(p, sideB * innerR, sideB * outerR));
    return e;
}

float sdCone(float2 p, float2 dir, float outerR, float innerR, float c, float sn, float halfAngle)
{
    float d = length(p);
    bool radialInside = d <= outerR && d >= innerR;
    bool angularInside = halfAngle >= 3.141591f || d <= 1e-6f || dot(p / d, dir) >= c;
    float edge = edgeDistanceCone(p, dir, outerR, innerR, c, sn, halfAngle);
    return radialInside && angularInside ? -edge : edge;
}

float sdTriangle(float2 p, float2 p0, float2 p1, float2 p2)
{
    float2 e0 = p1 - p0;
    float2 e1 = p2 - p1;
    float2 e2 = p0 - p2;
    float2 v0 = p - p0;
    float2 v1 = p - p1;
    float2 v2 = p - p2;

    float d0 = max(dot(e0, e0), 1e-8f);
    float d1 = max(dot(e1, e1), 1e-8f);
    float d2 = max(dot(e2, e2), 1e-8f);
    float2 pq0 = v0 - e0 * saturate(dot(v0, e0) / d0);
    float2 pq1 = v1 - e1 * saturate(dot(v1, e1) / d1);
    float2 pq2 = v2 - e2 * saturate(dot(v2, e2) / d2);

    float orient = sign(e0.x * e2.y - e0.y * e2.x);
    if (orient == 0.0f)
        orient = 1.0f;

    float2 d = min(
        min(float2(dot(pq0, pq0), orient * (v0.x * e0.y - v0.y * e0.x)),
            float2(dot(pq1, pq1), orient * (v1.x * e1.y - v1.y * e1.x))),
        float2(dot(pq2, pq2), orient * (v2.x * e2.y - v2.y * e2.x)));

    return -sqrt(max(d.x, 0.0f)) * sign(d.y);
}

float sdShape(float2 p, float2 dir, float4 par, float2 extra)
{
    float kind = par.w;
    if (kind < 0.5f)
    {
        float d = length(p);
        return par.y > 0.0f ? max(d - par.x, par.y - d) : d - par.x;
    }
    if (kind < 1.5f)
    {
        float2 perp = float2(-dir.y, dir.x);
        return sdBox(float2(dot(p, dir), dot(p, perp)), float2(par.x, par.y));
    }
    if (kind < 2.5f)
        return sdCone(p, dir, par.x, par.y, par.z, extra.x, extra.y);
    if (kind < 3.5f)
        return sdCapsule(p, dir, par.x, par.y);
    if (kind < 4.5f)
        return sdArcCapsule(p, dir, par.x, par.y, par.z, extra);

    if (kind < 5.5f)
    {
        float2 perp = float2(-dir.y, dir.x);
        float2 local = float2(dot(p, dir), dot(p, perp));
        return min(sdBox(local, float2(par.x, par.y)), sdBox(local, float2(par.y, par.x)));
    }

    // Triangle payload:
    //   dir      = vertex A in screen-local framebuffer pixels
    //   par.xy   = vertex B
    //   par.z    = vertex C.x
    //   extra.x  = vertex C.y
    return sdTriangle(p, dir, par.xy, float2(par.z, extra.x));
}

float arenaSdUv(float2 uv)
{
    // Clamp to the padded SDF edge, then continuously extend its positive distance
    // outside the texture. The SDF texture has guaranteed positive padding around the
    // arena, so this is a conservative continuation of the arena distance field.
    float2 clampedUv = saturate(uv);
    float sampledPx = ArenaSdf.SampleGrad(ArenaSdfSampler, clampedUv, ArenaMipGrad.xy, ArenaMipGrad.zw).r * ArenaUvRow1.w;
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

float positiveSquare(float x)
{
    x = max(x, 0.0f);
    return x * x;
}

float halfPlaneCoverage(float edgeDistance, float2 gradientAxes, float coverageScale)
{
    // Exact box-filter coverage of a locally straight boundary over the unit pixel square.
    // Keep only gradient direction: the distance fields are already expressed in framebuffer
    // pixels, and ignoring derivative magnitude avoids the corner inflation of raw fwidth().
    float a = gradientAxes.x * coverageScale;
    float b = gradientAxes.y * coverageScale;

    // Axis-aligned limit of the convolution below.
    if (b <= 1e-4f)
        return saturate(0.5f - edgeDistance / max(a, 1e-5f));

    // Projection of a uniform pixel square onto the edge normal is the convolution of
    // two uniforms of widths a and b. Its exact CDF gives the covered pixel area.
    float h = 0.5f * (a + b);
    float q = h - edgeDistance;
    float area = positiveSquare(q)
               - positiveSquare(q - a)
               - positiveSquare(q - b)
               + positiveSquare(q - a - b);
    return saturate(area / (2.0f * a * b));
}

float bandCoverage(float signedDistance, float halfWidth, float2 gradientAxes, float coverageScale)
{
    // Area between the two locally parallel boundaries d=-w and d=+w. Keeping the signed
    // distance avoids the abs() cusp at the centerline and is smoother under rotation.
    return saturate(halfPlaneCoverage(signedDistance - halfWidth, gradientAxes, coverageScale)
                  - halfPlaneCoverage(signedDistance + halfWidth, gradientAxes, coverageScale));
}

float outlineSdAt(float2 localPx, float2 arenaUv, float2 direction, float4 params, float2 extra)
{
    float shapeSd = sdShape(localPx, direction, params, extra);
#ifdef UNCLIPPED_OUTLINE
    return shapeSd;
#else
    return max(shapeSd, arenaSdUv(arenaUv));
#endif
}

float4 composeExclusiveHalo(float4 shadowCol, float shadowCoverage, float4 col, float colorCoverage)
{
    // The shadow is a coverage ring around the colored stroke, not another opaque stroke
    // underneath it. Partition coverage so black cannot leak through partially covered
    // colored AA pixels; that leak exaggerates temporal aliasing on high-contrast outlines.
    float exclusiveShadow = shadowCoverage * (1.0f - colorCoverage);
    float sa = shadowCol.a * exclusiveShadow;
    float ca = col.a * colorCoverage;
    float a = ca + sa;
    if (a <= 1e-5f)
        return float4(0.0f, 0.0f, 0.0f, 0.0f);
    float3 rgb = (col.rgb * ca + shadowCol.rgb * sa) / a;
    return float4(rgb, a);
}

float4 main(PS_INPUT i) : SV_Target
{
    // Derivatives are only used for the affine local-coordinate basis. Evaluate them before
    // any data-dependent discard so they remain well-defined across the pixel quad.
    float2 localDx = ddx(i.localPx);
    float2 localDy = ddy(i.localPx);
    float2 p = i.pos.xy;
    float2 arenaUv = 0.0f;
    float2 arenaUvDx = 0.0f;
    float2 arenaUvDy = 0.0f;
#ifndef UNCLIPPED_OUTLINE
    float3 hp = float3(p, 1.0f);
    arenaUv = float2(dot(hp, ArenaUvRow0.xyz), dot(hp, ArenaUvRow1.xyz));
    arenaUvDx = 0.5f * float2(ArenaUvRow0.x, ArenaUvRow1.x);
    arenaUvDy = 0.5f * float2(ArenaUvRow0.y, ArenaUvRow1.y);
    float arenaSd = arenaSdUv(arenaUv);
#ifdef CLIP_EDGE_ONLY
    float replayBand = max(i.widthsPx.y + 2.5f, 4.0f);
    if (abs(arenaSd) > replayBand)
        discard;
#endif
#endif

    // Reuse the center arena-SDF sample above instead of sampling t1 a second time in
    // outlineSdAt. Offset samples below still evaluate both fields independently.
    float shapeSd = sdShape(i.localPx, i.direction, i.params, i.extra);
#ifdef UNCLIPPED_OUTLINE
    float finalSd = shapeSd;
#else
    float finalSd = max(shapeSd, arenaSd);
#endif
    // Exact box coverage has no support farther than sqrt(2)/2 pixels beyond the band.
    // Skip the four normal-estimation evaluations for the large empty part of the quad.
    if (abs(finalSd) > i.widthsPx.y + 1.25f)
        discard;

    // ddx/ddy of localPx are safe here because localPx is an affine VS interpolant. We do
    // not use derivatives of the nonlinear distance field itself; instead evaluate that field
    // symmetrically at +/- half a framebuffer pixel to avoid 2x2-quad normal popping.
    float dx = outlineSdAt(i.localPx + 0.5f * localDx, arenaUv + arenaUvDx, i.direction, i.params, i.extra)
             - outlineSdAt(i.localPx - 0.5f * localDx, arenaUv - arenaUvDx, i.direction, i.params, i.extra);
    float dy = outlineSdAt(i.localPx + 0.5f * localDy, arenaUv + arenaUvDy, i.direction, i.params, i.extra)
             - outlineSdAt(i.localPx - 0.5f * localDy, arenaUv - arenaUvDy, i.direction, i.params, i.extra);
    float2 g = float2(dx, dy);

    float2 n = abs(g) * rsqrt(max(dot(g, g), 1e-10f));
    float2 gradientAxes = float2(max(n.x, n.y), min(n.x, n.y));
    float c = bandCoverage(finalSd, i.widthsPx.x, gradientAxes, OutlineCoverageScale);
    float s = bandCoverage(finalSd, i.widthsPx.y, gradientAxes, ShadowCoverageScale);
    float4 o = composeExclusiveHalo(i.shadowCol, s, i.col, c);
    clip(o.a - 0.001f);
    return o;
}
