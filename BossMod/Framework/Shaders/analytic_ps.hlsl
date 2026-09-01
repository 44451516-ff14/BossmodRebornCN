struct PS_INPUT
{
    float4 pos       : SV_POSITION;
    float2 localPx   : TEXCOORD0;
    nointerpolation float2 direction : TEXCOORD1;
    nointerpolation float4 params    : TEXCOORD2;
    nointerpolation float2 arcEndDirection : TEXCOORD3;
    nointerpolation float4 col       : COLOR0;
};

// Coarse rejects stay well outside the analytic AA footprint. Apart from reducing
// fragment work, keeping this margin larger than a 2x2 derivative quad means the
// fwidth evaluations at a visible edge never straddle one of these early returns.
static const float CoarseRejectPad = 2.5f;

float coverageCircleDonut(float2 p, float outerRadius, float innerRadius)
{
    float d2 = dot(p, p);
    float outerReject = outerRadius + CoarseRejectPad;
    if (d2 > outerReject * outerReject)
        return 0.0f;
    if (innerRadius > CoarseRejectPad)
    {
        float innerReject = innerRadius - CoarseRejectPad;
        if (d2 < innerReject * innerReject)
            return 0.0f;
    }

    float d = sqrt(d2);
    float aa = max(fwidth(d), 0.75f);
    float outerCoverage = 1.0f - smoothstep(outerRadius - aa, outerRadius + aa, d);
    float innerCoverage = innerRadius > 0.0f
        ? smoothstep(innerRadius - aa, innerRadius + aa, d)
        : 1.0f;
    return outerCoverage * innerCoverage;
}

float coverageRect(float2 p, float2 direction, float halfLength, float halfWidth)
{
    float2 perp = float2(-direction.y, direction.x);
    float2 local = float2(dot(p, direction), dot(p, perp));
    float2 a = abs(local);
    if (a.x > halfLength + CoarseRejectPad || a.y > halfWidth + CoarseRejectPad)
        return 0.0f;

    float2 q = a - float2(halfLength, halfWidth);
    float sd = length(max(q, 0.0f)) + min(max(q.x, q.y), 0.0f);
    float aa = max(fwidth(sd), 0.75f);
    return 1.0f - smoothstep(-aa, aa, sd);
}

float sdBox(float2 p, float2 halfExtents)
{
    float2 q = abs(p) - halfExtents;
    return length(max(q, 0.0f)) + min(max(q.x, q.y), 0.0f);
}

float coverageCross(float2 p, float2 direction, float range, float halfWidth)
{
    float2 perp = float2(-direction.y, direction.x);
    float2 local = float2(dot(p, direction), dot(p, perp));
    float2 a = abs(local);
    float outerExtent = max(range, halfWidth);
    float innerExtent = min(range, halfWidth);

    // Outside the enclosing square, or in one of the four corner regions farther than
    // the AA pad from both arms, the union cannot contribute visible coverage.
    if (a.x > outerExtent + CoarseRejectPad || a.y > outerExtent + CoarseRejectPad ||
        (a.x > innerExtent + CoarseRejectPad && a.y > innerExtent + CoarseRejectPad))
        return 0.0f;

    // Union of the horizontal and vertical arms. Taking the minimum signed distance
    // evaluates the union in one pixel-shader invocation, avoiding double alpha blend
    // in the center that two separate rectangle draws would cause.
    float sdA = sdBox(local, float2(range, halfWidth));
    float sdB = sdBox(local, float2(halfWidth, range));
    float sd = min(sdA, sdB);
    float aa = max(fwidth(sd), 0.75f);
    return 1.0f - smoothstep(-aa, aa, sd);
}

float coverageCone(float2 p, float2 direction, float outerRadius, float innerRadius, float cosHalfAngle)
{
    float d2 = dot(p, p);
    float outerReject = outerRadius + CoarseRejectPad;
    if (d2 > outerReject * outerReject)
        return 0.0f;
    if (innerRadius > CoarseRejectPad)
    {
        float innerReject = innerRadius - CoarseRejectPad;
        if (d2 < innerReject * innerReject)
            return 0.0f;
    }

    float d = sqrt(d2);
    float aa = max(fwidth(d), 0.75f);
    float radial = (1.0f - smoothstep(outerRadius - aa, outerRadius + aa, d)) *
        (innerRadius > 0.0f ? smoothstep(innerRadius - aa, innerRadius + aa, d) : 1.0f);

    if (d < 1e-4f || cosHalfAngle <= -0.999999f)
        return radial;

    float angularMetric = dot(p / d, direction) - cosHalfAngle;
    float angularAA = max(fwidth(angularMetric), 1e-4f);
    float angular = smoothstep(-angularAA, angularAA, angularMetric);
    return radial * angular;
}

float coverageCapsule(float2 p, float2 direction, float halfSegment, float radius)
{
    float2 perp = float2(-direction.y, direction.x);
    float along = dot(p, direction);
    float across = dot(p, perp);
    if (abs(along) > halfSegment + radius + CoarseRejectPad || abs(across) > radius + CoarseRejectPad)
        return 0.0f;

    // Exact segment distance in its local basis: outside an end cap only the excess
    // longitudinal distance contributes; inside the segment it collapses to |across|.
    float endExcess = max(abs(along) - halfSegment, 0.0f);
    float sd = length(float2(endExcess, across)) - radius;
    float aa = max(fwidth(sd), 0.75f);
    return 1.0f - smoothstep(-aa, aa, sd);
}

bool directionInArc(float2 startDirection, float2 radial, float2 endDirection, float angularLength)
{
    const float PI = 3.14159265358979323846f;
    const float TWO_PI = 6.28318530717958647692f;

    float absSweep = abs(angularLength);
    if (absSweep >= TWO_PI - 1e-5f)
        return true;

    // With coincident start/end rays, the two half-plane tests alone also accept the
    // antipodal ray. Treat a degenerate sweep as the single authored endpoint instead.
    if (absSweep <= 1e-6f)
        return false;

    float sweepSign = angularLength >= 0.0f ? 1.0f : -1.0f;
    float fromStart = -(startDirection.x * radial.y - startDirection.y * radial.x) * sweepSign;
    float toEnd = -(radial.x * endDirection.y - radial.y * endDirection.x) * sweepSign;
    const float sideEpsilon = -1e-6f;
    return absSweep <= PI
        ? fromStart >= sideEpsilon && toEnd >= sideEpsilon
        : fromStart >= sideEpsilon || toEnd >= sideEpsilon;
}

float coverageArcCapsule(float2 p, float2 startDirection, float2 endDirection, float orbitRadius, float radius, float angularLength)
{

    float d2 = dot(p, p);
    float outerReject = orbitRadius + radius + CoarseRejectPad;
    if (d2 > outerReject * outerReject)
        return 0.0f;
    float innerReject = orbitRadius - radius - CoarseRejectPad;
    if (innerReject > 0.0f && d2 < innerReject * innerReject)
        return 0.0f;

    float d = sqrt(d2);
    float2 radial = d > 1e-5f ? p / d : startDirection;

    float distanceToCenterline;
    if (directionInArc(startDirection, radial, endDirection, angularLength))
    {
        // Angular projection lands on the arc interior: nearest centerline point is at the
        // same polar angle, so distance is simply radial distance to the orbit circle.
        distanceToCenterline = abs(d - orbitRadius);
    }
    else
    {
        // Outside the angular interval: nearest point on the centerline is one of the arc ends.
        // sqrt(min(d2)) is exactly min(length(...)) and saves one square root.
        float2 startDelta = p - startDirection * orbitRadius;
        float2 endDelta = p - endDirection * orbitRadius;
        distanceToCenterline = sqrt(min(dot(startDelta, startDelta), dot(endDelta, endDelta)));
    }

    float sd = distanceToCenterline - radius;
    float aa = max(fwidth(sd), 0.75f);
    return 1.0f - smoothstep(-aa, aa, sd);
}

float coverageEye(float2 p, float radius, float centerOffset)
{
    // Intersection of equal-radius circles. Because sqrt is monotonic,
    // max(length(a), length(b)) == sqrt(max(dot(a,a), dot(b,b))), allowing one
    // square root instead of two without changing the signed-distance result.
    float2 topDelta = p - float2(0.0f, centerOffset);
    float2 bottomDelta = p + float2(0.0f, centerOffset);
    float topD2 = dot(topDelta, topDelta);
    float bottomD2 = dot(bottomDelta, bottomDelta);
    float rejectRadius = radius + CoarseRejectPad;
    float rejectD2 = rejectRadius * rejectRadius;
    if (topD2 > rejectD2 || bottomD2 > rejectD2)
        return 0.0f;

    float sd = sqrt(max(topD2, bottomD2)) - radius;
    float aa = max(length(float2(ddx(sd), ddy(sd))), 0.75f);
    return 1.0f - smoothstep(-aa, aa, sd);
}

float4 main(PS_INPUT input) : SV_Target
{
    float shape = input.params.w;
    float coverage;

    if (shape < 0.5f)
        coverage = coverageCircleDonut(input.localPx, input.params.x, input.params.y);
    else if (shape < 1.5f)
        coverage = coverageRect(input.localPx, input.direction, input.params.x, input.params.y);
    else if (shape < 2.5f)
        coverage = coverageCone(input.localPx, input.direction, input.params.x, input.params.y, input.params.z);
    else if (shape < 3.5f)
        coverage = coverageCapsule(input.localPx, input.direction, input.params.x, input.params.y);
    else if (shape < 4.5f)
        coverage = coverageArcCapsule(input.localPx, input.direction, input.arcEndDirection, input.params.x, input.params.y, input.params.z);
    else if (shape < 5.5f)
        coverage = coverageCross(input.localPx, input.direction, input.params.x, input.params.y);
    else
        coverage = coverageEye(input.localPx, input.params.x, input.params.y);

    clip(coverage - 0.001f);
    float4 result = input.col;
    result.a *= coverage;
    return result;
}
