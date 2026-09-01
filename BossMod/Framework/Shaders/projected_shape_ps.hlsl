#include "zone_wave.hlsli"

// Projection fix r21: character silhouettes terminate support rays instead of stretching holes.

cbuffer WorldRenderConstants : register(b4)
{
    row_major float4x4 ViewProj;
    row_major float4x4 InvViewProj;
    float4 NearPlane;
    float4 Viewport;
    float4 RasterScale;
    float4 SceneDepthParams;
    float4 SceneInfoParams; // x character-classification texture available, y near-black character threshold
};

cbuffer WorldProjectedSdfConstants : register(b5)
{
    float4 ShapeSdfMap; // xy world-space padded-domain minimum, zw inverse span
    float4 ArenaSdfMap;
    float4 ProjectedSdfFlags; // x shape SDF bound, y arena SDF bound
};

Texture2D<float> SceneDepthTexture : register(t4);
Texture2D<float4> SceneInfoTexture : register(t5);
Texture2D<float> ProjectedShapeSdf : register(t6);
Texture2D<float> ProjectedArenaSdf : register(t7);
SamplerState SdfSampler : register(s1);

struct PS_INPUT
{
    float4 pos                         : SV_POSITION;
    nointerpolation float4 col         : COLOR0;
    nointerpolation float3 origin      : TEXCOORD0;
    nointerpolation float4 directionAux: TEXCOORD1;
    nointerpolation float4 params0     : TEXCOORD2;
    nointerpolation float4 params1     : TEXCOORD3;
    nointerpolation uint packed        : TEXCOORD4;
    nointerpolation float outlineWidth : TEXCOORD5;
    nointerpolation float projectionHeight : TEXCOORD6;
    nointerpolation float4 outlineCol  : COLOR1;
    nointerpolation float2 waveOriginXZ : TEXCOORD7;
};

static const float PI = 3.14159265358979323846f;
static const float TWO_PI = 6.28318530717958647692f;
static const float HoleSupportMinHeightImprovement = 0.02f;
static const int HoleSupportBandSteps = 12;
static const int HoleSupportFineSteps = 32;
static const int HoleSupportCoarseSteps = 16;
static const int HoleSupportDenseSteps = HoleSupportFineSteps + HoleSupportCoarseSteps;
static const int HoleSupportDenseTexelReach = HoleSupportFineSteps + 2 * HoleSupportCoarseSteps;
static const int HoleSupportDistanceCount = 13;
// Absolute world distances form a nested search: increasing HoleFillRadius preserves the fixed taps
// already inside the old reach and moves only the terminal cap. Radius-scaled fractions resonated
// with regular grates and made larger values less reliable. Near distances are dense; far distances
// grow progressively and are normally skipped because a surrounding floor returns immediately.
static const float HoleSupportDistances[HoleSupportDistanceCount] =
{
    0.06f, 0.12f, 0.20f, 0.30f, 0.42f, 0.58f,
    0.80f, 1.10f, 1.50f, 2.00f, 2.80f, 4.00f, 5.00f
};
static const float2 HoleSupportAxes[4] =
{
    float2( 1.0f,  0.0f),
    float2( 0.0f,  1.0f),
    float2( 0.70710678f,  0.70710678f),
    float2(-0.70710678f,  0.70710678f)
};

float nativeUiVisibility(float2 pixelPos)
{
    return 1.0f;
}

float sdBox(float2 p, float2 halfExtents)
{
    float2 q = abs(p) - halfExtents;
    return length(max(q, 0.0f)) + min(max(q.x, q.y), 0.0f);
}

float sdTriangle(float2 p, float2 p0, float2 p1, float2 p2)
{
    float2 e0 = p1 - p0;
    float2 e1 = p2 - p1;
    float2 e2 = p0 - p2;
    float2 v0 = p - p0;
    float2 v1 = p - p1;
    float2 v2 = p - p2;

    float2 pq0 = v0 - e0 * clamp(dot(v0, e0) / max(dot(e0, e0), 1e-8f), 0.0f, 1.0f);
    float2 pq1 = v1 - e1 * clamp(dot(v1, e1) / max(dot(e1, e1), 1e-8f), 0.0f, 1.0f);
    float2 pq2 = v2 - e2 * clamp(dot(v2, e2) / max(dot(e2, e2), 1e-8f), 0.0f, 1.0f);

    float s = sign(e0.x * e2.y - e0.y * e2.x);
    float2 d0 = float2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x));
    float2 d1 = float2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x));
    float2 d2 = float2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x));
    float2 d = min(d0, min(d1, d2));
    return -sqrt(max(d.x, 0.0f)) * sign(d.y);
}

bool directionInArc(float2 startDirection, float2 radial, float2 endDirection, float angularLength)
{
    float absSweep = abs(angularLength);
    if (absSweep >= TWO_PI - 1e-5f)
        return true;

    // Coincident half-plane boundaries otherwise also accept the antipodal ray for a
    // zero-length arc; the authored geometry in that case is just its start endpoint.
    if (absSweep <= 1e-6f)
        return false;

    // BossMod's positive angle rotates clockwise in X/Z. Oriented half-plane tests classify the
    // directed interval without the per-pixel atan2 previously used here.
    float sweepSign = angularLength >= 0.0f ? 1.0f : -1.0f;
    float fromStart = -(startDirection.x * radial.y - startDirection.y * radial.x) * sweepSign;
    float toEnd = -(radial.x * endDirection.y - radial.y * endDirection.x) * sweepSign;
    const float sideEpsilon = -1e-6f;
    return absSweep <= PI
        ? fromStart >= sideEpsilon && toEnd >= sideEpsilon
        : fromStart >= sideEpsilon || toEnd >= sideEpsilon;
}

float sdArcCapsule(float2 p, float2 startDirection, float2 endDirection, float orbitRadius, float radius, float angularLength)
{
    float d = length(p);
    float2 radial = d > 1e-5f ? p / d : startDirection;

    float centerlineDistance;
    if (directionInArc(startDirection, radial, endDirection, angularLength))
    {
        centerlineDistance = abs(d - orbitRadius);
    }
    else
    {
        float2 startDelta = p - startDirection * orbitRadius;
        float2 endDelta = p - endDirection * orbitRadius;
        centerlineDistance = sqrt(min(dot(startDelta, startDelta), dot(endDelta, endDelta)));
    }
    return centerlineDistance - radius;
}

float sampleShapeWorldSdf(float4 map, float2 worldXZ)
{
    float2 uv = (worldXZ - map.xy) * map.zw;
    float2 clampedUv = saturate(uv);

    // World position derivatives are discontinuous at scene-depth edges (wall silhouettes, props,
    // actors, etc). Using those derivatives for mip selection can choose an extremely coarse mip and
    // move the apparent SDF zero contour by metres. The world SDF cache has a sufficiently dense base
    // level, so use it explicitly and let fieldCoverage provide the final edge antialiasing.
    float sampled = ProjectedShapeSdf.SampleLevel(SdfSampler, clampedUv, 0.0f).r;
    float result = sampled;
    [branch]
    if (any(uv != clampedUv))
    {
        float2 span = 1.0f / max(map.zw, float2(1e-7f, 1e-7f));
        float2 outsideWorld = (uv - clampedUv) * span;
        result += length(outsideWorld);
    }
    return result;
}

float sampleArenaWorldSdf(float4 map, float2 worldXZ)
{
    float2 uv = (worldXZ - map.xy) * map.zw;
    float2 clampedUv = saturate(uv);
    float sampled = ProjectedArenaSdf.SampleLevel(SdfSampler, clampedUv, 0.0f).r;
    float result = sampled;
    [branch]
    if (any(uv != clampedUv))
    {
        float2 span = 1.0f / max(map.zw, float2(1e-7f, 1e-7f));
        float2 outsideWorld = (uv - clampedUv) * span;
        result += length(outsideWorld);
    }
    return result;
}

float shapeSignedDistance(PS_INPUT input, float2 worldXZ)
{
    uint kind = input.packed & 0xFFu;
    float2 p = worldXZ - input.origin.xz;
    float2 dir = input.directionAux.xy;

    if (kind == 0u) // circle / donut
    {
        float d = length(p);
        float sd = d - input.params0.x; // outer
        if (input.params0.y > 0.0f)
            sd = max(sd, input.params0.y - d);
        return sd;
    }
    if (kind == 1u) // directional rect
    {
        float2 perp = float2(-dir.y, dir.x);
        float2 q = float2(dot(p, dir), dot(p, perp));
        return sdBox(q, input.params0.xy);
    }
    if (kind == 2u) // annular cone / sector
    {
        float d = length(p);
        float angularSd = -1e6f;
        if (d > 1e-5f && input.params0.z < PI - 1e-5f)
        {
            // Aux stores sin/cos(halfAngle), precomputed once on the CPU. This is algebraically
            // equivalent to d*sin(clamp(angle-halfAngle, -PI/2, PI/2)) without acos/sin.
            float sinHalf = input.directionAux.z;
            float cosHalf = input.directionAux.w;
            float forward = dot(p, dir);
            float side = abs(dir.x * p.y - dir.y * p.x);
            angularSd = side * cosHalf - forward * sinHalf;
            if (input.params0.z < 0.5f * PI && forward < -d * sinHalf)
                angularSd = d;
            else if (input.params0.z > 0.5f * PI && forward > d * sinHalf)
                angularSd = -d;
        }
        float sd = max(d - input.params0.x, angularSd);
        if (input.params0.y > 0.0f)
            sd = max(sd, input.params0.y - d);
        return sd;
    }
    if (kind == 3u) // straight capsule
    {
        float2 perp = float2(-dir.y, dir.x);
        float along = dot(p, dir);
        float across = dot(p, perp);
        float endExcess = max(abs(along) - input.params0.x, 0.0f);
        return length(float2(endExcess, across)) - input.params0.y;
    }
    if (kind == 4u) // arc capsule
        return sdArcCapsule(p, dir, input.directionAux.zw, input.params0.x, input.params0.y, input.params0.z);
    if (kind == 5u) // cross
    {
        float2 perp = float2(-dir.y, dir.x);
        float2 q = float2(dot(p, dir), dot(p, perp));
        return min(sdBox(q, input.params0.xy), sdBox(q, input.params0.yx));
    }
    if (kind == 6u) // arbitrary triangle; A/B in directionAux, C in params0.xy
        return sdTriangle(p, input.directionAux.xy, input.directionAux.zw, input.params0.xy);
    if (kind == 7u) // arbitrary polygon SDF
    {
        if (ProjectedSdfFlags.x < 0.5f)
            return 1e6f;
        return sampleShapeWorldSdf(ShapeSdfMap, worldXZ);
    }
    return 1e6f;
}

float stableWorldAaLimit(float2 pixelPos, float2 framebufferSize, float4 worldH, float3 world)
{
    // Estimate how much world X/Z one screen pixel covers without consulting neighbouring scene-depth
    // samples. Using the current pixel's depth for the offset rays makes this stable at wall/actor/depth
    // discontinuities, while still allowing more AA naturally as geometry gets farther from the camera.
    float xStep = pixelPos.x + 1.0f < framebufferSize.x ? 1.0f : -1.0f;
    float yStep = pixelPos.y + 1.0f < framebufferSize.y ? 1.0f : -1.0f;
    // At fixed depth, a one-pixel movement is a linear offset of the existing homogeneous world
    // position. Reusing it avoids two complete inverse-projection matrix multiplies per fragment.
    float4 worldXH = worldH + xStep * RasterScale.z * InvViewProj[0];
    float4 worldYH = worldH - yStep * RasterScale.w * InvViewProj[1];
    float3 worldX = worldXH.xyz / worldXH.w;
    float3 worldY = worldYH.xyz / worldYH.w;
    float pixelFootprint = length(worldX.xz - world.xz) + length(worldY.xz - world.xz);

    // This value is only a safety bound for real quad derivatives, not the final AA width. A somewhat
    // larger ceiling than v14 is useful at grazing angles where a genuine screen pixel covers a sizeable
    // patch of ground, while still preventing the multi-yalm derivative explosions seen at depth edges.
    return clamp(pixelFootprint * 4.0f, 0.02f, 1.25f);
}

float stableFieldPixelWidth(float field, float maxFootprint)
{
    // On a continuous receiving surface the hardware derivatives correctly follow that surface and give
    // the best screen-space measure of the SDF. At a depth discontinuity they can jump to unrelated
    // geometry, so clamp them by the same-depth estimate above.
    return max(min(abs(ddx(field)) + abs(ddy(field)), maxFootprint), 0.002f);
}

float fillFieldCoverage(float field, float maxFootprint)
{
    float pixelWidth = stableFieldPixelWidth(field, maxFootprint);
    return saturate(0.5f - field / pixelWidth);
}

float4 referenceRayBaseAtPixel(float2 pixelPos)
{
    float2 ndc = float2(pixelPos.x * RasterScale.z - 1.0f, 1.0f - pixelPos.y * RasterScale.w);
    return mul(float4(ndc, 0.0f, 1.0f), InvViewProj);
}

bool referencePlaneXZFromRayBase(float4 rayBase, float planeY, float2 fallbackXZ, out float2 xz)
{
    float4 depthRow = InvViewProj[2];
    float4 rayAH = rayBase + 0.25f * depthRow;
    float4 rayBH = rayBase + 0.75f * depthRow;
    if (abs(rayAH.w) <= 1e-7f || abs(rayBH.w) <= 1e-7f)
    {
        xz = fallbackXZ;
        return false;
    }

    float3 rayA = rayAH.xyz / rayAH.w;
    float3 rayB = rayBH.xyz / rayBH.w;
    float dy = rayB.y - rayA.y;
    if (abs(dy) < 1e-6f)
    {
        xz = fallbackXZ;
        return false;
    }

    float t = (planeY - rayA.y) / dy;
    xz = lerp(rayA.xz, rayB.xz, t);

    // Extremely close to the horizon the mathematical intersection can be arbitrarily far away.
    // Such a point is not a stable terrain-projection receiver for a thin outline; fail closed.
    float2 delta = xz - fallbackXZ;
    if (max(abs(delta.x), abs(delta.y)) > 10000.0f)
    {
        xz = fallbackXZ;
        return false;
    }
    return true;
}

bool referencePlaneReceiverAtPixel(PS_INPUT input, float2 pixelPos,
    out float3 receiverWorld, out float receiverDepth, out float aaLimit)
{
    receiverWorld = 0.0f;
    receiverDepth = 0.0f;
    aaLimit = 0.02f;

    float4 rayBase = referenceRayBaseAtPixel(pixelPos);
    float2 referenceXZ;
    if (!referencePlaneXZFromRayBase(rayBase, input.origin.y, input.origin.xz, referenceXZ))
        return false;

    receiverWorld = float3(referenceXZ.x, input.origin.y, referenceXZ.y);
    float4 receiverWorld4 = float4(receiverWorld, 1.0f);
    if (dot(receiverWorld4, NearPlane) >= 0.0f)
        return false;
    float4 receiverClip = mul(receiverWorld4, ViewProj);
    if (abs(receiverClip.w) <= 1e-7f)
        return false;
    receiverDepth = receiverClip.z / receiverClip.w;
    if (!(receiverDepth > 0.0f) || receiverDepth > 1.0001f)
        return false;

    float2 referenceX, referenceY;
    if (referencePlaneXZFromRayBase(rayBase + RasterScale.z * InvViewProj[0], input.origin.y, referenceXZ, referenceX)
        && referencePlaneXZFromRayBase(rayBase - RasterScale.w * InvViewProj[1], input.origin.y, referenceXZ, referenceY))
    {
        float worldPerPixelX = length(referenceX - referenceXZ);
        float worldPerPixelY = length(referenceY - referenceXZ);
        aaLimit = clamp((worldPerPixelX + worldPerPixelY) * 4.0f, 0.02f, 1.25f);
    }
    return true;
}

bool sceneOccludesReferencePlane(bool sceneDepthValid, float sceneDepth, float3 sceneWorld,
    float receiverDepth, float3 receiverWorld)
{
    // FFXIV reverse-Z: larger depth is closer. Ignore coplanar precision noise in world units.
    return sceneDepthValid && sceneDepth > receiverDepth
        && distance(sceneWorld, receiverWorld) > SceneDepthParams.z;
}

float stableOutlinePixelWidth(PS_INPUT input, float2 pixelPos, float2 fallbackXZ,
    out float2 referenceXZ, out bool referenceValid)
{
    // Build the reference ray directly from SV_POSITION. The previous implementation first
    // reconstructed a scene-depth world point and then subtracted its depth contribution again.
    // That algebra is exact on paper but loses precision in FP32 at depth discontinuities, making
    // the outline width jump on every floor-mesh edge. This path never reads scene depth.
    float4 rayBase = referenceRayBaseAtPixel(pixelPos);
    referenceValid = referencePlaneXZFromRayBase(rayBase, input.origin.y, fallbackXZ, referenceXZ);
    if (!referenceValid)
        return 0.002f;

    float2 framebufferSize = max(Viewport.xy, float2(1.0f, 1.0f));
    float xStep = pixelPos.x + 1.0f < framebufferSize.x ? 1.0f : -1.0f;
    float yStep = pixelPos.y + 1.0f < framebufferSize.y ? 1.0f : -1.0f;
    float4 rayBaseX = rayBase + xStep * RasterScale.z * InvViewProj[0];
    float4 rayBaseY = rayBase - yStep * RasterScale.w * InvViewProj[1];

    float2 referenceX, referenceY;
    bool validX = referencePlaneXZFromRayBase(rayBaseX, input.origin.y, referenceXZ, referenceX);
    bool validY = referencePlaneXZFromRayBase(rayBaseY, input.origin.y, referenceXZ, referenceY);
    referenceValid = validX && validY;
    if (!referenceValid)
        return 0.002f;

    // Explicit one-pixel finite differences avoid quad derivatives entirely. Derivatives after clip
    // are implementation-dependent and were another route for geometry-edge shimmer to leak into the
    // minimum-width outline. The reference-plane field is smooth and independent of the receiver.
    float planeField = shapeSignedDistance(input, referenceXZ);
    float planeFieldX = shapeSignedDistance(input, referenceX);
    float planeFieldY = shapeSignedDistance(input, referenceY);
    return clamp(abs(planeFieldX - planeField) + abs(planeFieldY - planeField), 0.002f, 20.0f);
}

float outlineFieldCoverage(PS_INPUT input, float signedDistance, float outlineWidth, float2 pixelPos,
    out float2 referenceXZ, out bool referenceValid)
{
    // Convert both the centre-point SDF and the requested world-space outline width into approximate
    // screen pixels using a depth-independent reference-plane finite difference. Performing the final
    // test in pixels keeps the requested minimum thickness stable at low camera angles.
    const float minOutlinePixels = 1.5f;
    const float edgeAaPixels = 0.65f;
    float worldPerPixel = stableOutlinePixelWidth(input, pixelPos, input.origin.xz, referenceXZ, referenceValid);
    if (!referenceValid)
        return 0.0f;

    float distancePixels = abs(signedDistance) / worldPerPixel;
    float requestedHalfPixels = 0.5f * max(outlineWidth, 0.0f) / worldPerPixel;
    float effectiveHalfPixels = max(requestedHalfPixels, 0.5f * minOutlinePixels);

    return 1.0f - smoothstep(
        max(effectiveHalfPixels - edgeAaPixels, 0.0f),
        effectiveHalfPixels + edgeAaPixels,
        distancePixels);
}


bool reconstructWorldAtDepthTexel(int2 depthCoord, int2 depthSize, float2 depthTexelToFramebuffer,
    float2 centerPixel, float centerDepth, float4 centerWorldH, out float3 world)
{
    if (any(depthCoord < int2(0, 0)) || any(depthCoord >= depthSize))
    {
        world = 0.0f;
        return false;
    }

    float depth = SceneDepthTexture.Load(int3(depthCoord, 0));
    if (!(depth > 0.0f))
    {
        world = 0.0f;
        return false;
    }

    // Reconstruct at the centre of the depth texel. Scene depth can be a different resolution from
    // the framebuffer, so convert its texel centre back into framebuffer-pixel coordinates. The
    // neighbour clip vector differs from the already reconstructed centre by only x/y/depth deltas;
    // applying those inverse-matrix rows avoids another complete 4x4 multiply.
    float2 pixelPos = (float2(depthCoord) + 0.5f) * depthTexelToFramebuffer;
    float2 pixelDelta = pixelPos - centerPixel;
    float2 ndcDelta = pixelDelta * float2(RasterScale.z, -RasterScale.w);
    float4 worldH = centerWorldH
        + ndcDelta.x * InvViewProj[0]
        + ndcDelta.y * InvViewProj[1]
        + (depth - centerDepth) * InvViewProj[2];
    if (abs(worldH.w) <= 1e-7f)
    {
        world = 0.0f;
        return false;
    }

    world = worldH.xyz / worldH.w;
    return true;
}

bool receiverWithinProjectionHeight(float3 world, float referenceY, float projectionHeight)
{
    return abs(world.y - referenceY) <= projectionHeight;
}

bool sceneDepthTexelIsCharacter(int2 depthCoord)
{
    if (SceneInfoParams.x <= 0.5f)
        return false;

    float3 sceneInfo = SceneInfoTexture.Load(int3(depthCoord, 0)).rgb;
    return max(sceneInfo.r, max(sceneInfo.g, sceneInfo.b)) <= SceneInfoParams.y;
}

bool reconstructHoleSupportWorld(int2 depthCoord, int2 depthSize, float2 depthTexelToFramebuffer,
    float2 centerPixel, float centerDepth, float4 centerWorldH, bool centerDepthValid,
    out float3 world, out bool characterOccluded)
{
    characterOccluded = false;
    if (any(depthCoord < int2(0, 0)) || any(depthCoord >= depthSize))
    {
        world = 0.0f;
        return false;
    }

    float depth = SceneDepthTexture.Load(int3(depthCoord, 0));
    // Character pixels are occluders, never evidence that a floor surrounds a hole. In particular,
    // this prevents feet and character silhouettes from changing the result when ProjectionHeight is
    // large enough that their reconstructed Y would otherwise fall inside the receiver band.
    if (!(depth > 0.0f))
    {
        world = 0.0f;
        return false;
    }
    if (sceneDepthTexelIsCharacter(depthCoord))
    {
        world = 0.0f;
        characterOccluded = true;
        return false;
    }

    float2 pixelPos = (float2(depthCoord) + 0.5f) * depthTexelToFramebuffer;
    float4 worldH;
    if (centerDepthValid)
    {
        float2 pixelDelta = pixelPos - centerPixel;
        float2 ndcDelta = pixelDelta * float2(RasterScale.z, -RasterScale.w);
        worldH = centerWorldH
            + ndcDelta.x * InvViewProj[0]
            + ndcDelta.y * InvViewProj[1]
            + (depth - centerDepth) * InvViewProj[2];
    }
    else
    {
        float2 ndc = float2(pixelPos.x * RasterScale.z - 1.0f, 1.0f - pixelPos.y * RasterScale.w);
        worldH = mul(float4(ndc, depth, 1.0f), InvViewProj);
    }

    if (abs(worldH.w) <= 1e-7f)
    {
        world = 0.0f;
        return false;
    }

    world = worldH.xyz / worldH.w;
    return true;
}

bool findHoleSupportInTexelBand(int2 sampleCoord, int2 sideOffset, bool widenBand,
    int2 centerCoord, int2 depthSize,
    float2 depthTexelToFramebuffer, float2 centerPixel, float centerDepth, float4 centerWorldH,
    bool centerDepthValid, float referenceY, float supportHeightTolerance, float centerHeightError,
    out float worldY, out bool characterOccluded, out bool viewportClipped)
{
    worldY = 0.0f;
    characterOccluded = false;
    viewportClipped = false;
    bool found = false;
    float bestHeightError = 1e30f;

    // A one-texel half-width turns the discrete ray into a narrow coverage band. The centre is tried
    // first, so ordinary floor hits retain the one-sample fast path. The antipodal side texels matter
    // only at a rasterized grate edge, where point samples otherwise flip as the camera rotates by a
    // fraction of a depth texel.
    [unroll]
    for (int footprint = 0; footprint < 3; ++footprint)
    {
        if (!widenBand && footprint > 0)
            continue;
        int2 offset = footprint == 0 ? int2(0, 0) : (footprint == 1 ? sideOffset : -sideOffset);
        int2 candidateCoord = sampleCoord + offset;
        if (any(candidateCoord < int2(0, 0)) || any(candidateCoord >= depthSize))
        {
            viewportClipped = true;
            continue;
        }
        if (all(candidateCoord == centerCoord))
            continue;

        // FXC does not reliably propagate definite assignment from the out parameters through this
        // bool-returning call after unrolling, so initialize the receivers explicitly as well.
        float3 candidateWorld = 0.0f;
        bool candidateCharacterOccluded = false;
        if (reconstructHoleSupportWorld(candidateCoord, depthSize,
            depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH, centerDepthValid,
            candidateWorld, candidateCharacterOccluded))
        {
            float candidateHeightError = abs(candidateWorld.y - referenceY);
            if (candidateHeightError <= supportHeightTolerance
                && candidateHeightError + HoleSupportMinHeightImprovement < centerHeightError
                && candidateHeightError < bestHeightError)
            {
                bestHeightError = candidateHeightError;
                worldY = candidateWorld.y;
                found = true;
                if (candidateHeightError <= 0.5f * HoleSupportMinHeightImprovement)
                    break;
            }
        }
        else
        {
            characterOccluded = characterOccluded || candidateCharacterOccluded;
        }
    }
    return found;
}

bool findWorldSpaceHoleSupport(float4 referenceClip, float centerNearDistance,
    float2 direction, float searchRadius, int2 centerCoord, int2 depthSize,
    float2 depthTexelToFramebuffer, float2 centerPixel, float centerDepth, float4 centerWorldH,
    bool centerDepthValid, float referenceY, float supportHeightTolerance, float centerHeightError,
    out float worldY, out float supportDistance, out bool supportUnavailable)
{
    worldY = 0.0f;
    supportDistance = 0.0f;
    supportUnavailable = false;
    int2 previousCoord = int2(-2147483647, -2147483647);
    float previousTestedDistance = 0.0f;
    float4 directionClipPerWorld = direction.x * ViewProj[0] + direction.y * ViewProj[2];
    float directionNearPerWorld = direction.x * NearPlane.x + direction.y * NearPlane.z;
    float2 referenceNdc = referenceClip.xy / referenceClip.w;
    float2 referenceDepthPosition = 0.5f * float2(referenceNdc.x + 1.0f, 1.0f - referenceNdc.y) * float2(depthSize);

    // Traverse the first part of the projected world ray at approximately one scene-depth texel per
    // step. This is the part that can contain a small grate opening. The analytic NDC derivative is
    // evaluated at the rejected pixel; every sampled position is still projected exactly below.
    float inverseReferenceWSq = 1.0f / max(referenceClip.w * referenceClip.w, 1e-14f);
    float2 ndcDerivative = float2(
        (directionClipPerWorld.x * referenceClip.w - referenceClip.x * directionClipPerWorld.w) * inverseReferenceWSq,
        (directionClipPerWorld.y * referenceClip.w - referenceClip.y * directionClipPerWorld.w) * inverseReferenceWSq);
    float2 depthTexelDerivative = 0.5f * float2(ndcDerivative.x, -ndcDerivative.y) * float2(depthSize);
    float depthTexelsPerWorld = max(abs(depthTexelDerivative.x), abs(depthTexelDerivative.y));
    float denseStepDistance = depthTexelsPerWorld > 1e-5f ? 1.0f / depthTexelsPerWorld : searchRadius;
    denseStepDistance = max(denseStepDistance, 1e-4f);
    float denseCoveredDistance = min(HoleSupportDenseTexelReach * denseStepDistance, searchRadius);

    // After the dense raster traversal, retain the nested world-distance caps for openings that span
    // more than 64 pixels. The first 32 texels are visited individually; the next 32 use two-texel
    // spacing because a floor bar at that scale is itself several texels wide. Only the first twelve
    // steps use the three-texel anti-aliasing band, keeping the larger-radius path bounded.
    [loop]
    for (int tap = 0; tap < HoleSupportDenseSteps + HoleSupportDistanceCount; ++tap)
    {
        bool denseTap = tap < HoleSupportDenseSteps;
        float sampleDistance;
        bool finalTap;
        if (denseTap)
        {
            float denseTexelOffset = tap < HoleSupportFineSteps
                ? tap + 1
                : HoleSupportFineSteps + 2 * (tap - HoleSupportFineSteps + 1);
            sampleDistance = min(denseTexelOffset * denseStepDistance, searchRadius);
            finalTap = sampleDistance >= searchRadius;
        }
        else
        {
            int fixedTap = tap - HoleSupportDenseSteps;
            sampleDistance = min(HoleSupportDistances[fixedTap], searchRadius);
            finalTap = HoleSupportDistances[fixedTap] >= searchRadius;
            if (sampleDistance <= denseCoveredDistance + 1e-4f)
                continue;
        }
        if (centerNearDistance + sampleDistance * directionNearPerWorld >= 0.0f)
        {
            // This endpoint is clipped by the camera, rather than disproving floor support. Its
            // antipodal partner may still provide enough visible evidence for an enclosed hole.
            supportUnavailable = true;
            break;
        }

        // A line on the authored world plane is also a line in homogeneous clip space. Advancing the
        // centre clip coordinate this way avoids a matrix multiply for every tap while still applying
        // perspective before the depth texel is selected.
        float4 sampleClip = referenceClip + sampleDistance * directionClipPerWorld;
        if (abs(sampleClip.w) <= 1e-7f)
        {
            supportUnavailable = true;
            break;
        }
        float2 sampleNdc = sampleClip.xy / sampleClip.w;
        if (any(sampleNdc < float2(-1.0f, -1.0f)) || any(sampleNdc >= float2(1.0f, 1.0f)))
        {
            supportUnavailable = true;
            break;
        }

        float2 sampleDepthPosition = 0.5f * float2(sampleNdc.x + 1.0f, 1.0f - sampleNdc.y) * float2(depthSize);
        int2 sampleCoord = int2(sampleDepthPosition);
        if (all(sampleCoord == centerCoord))
        {
            // If the configured world radius is sub-pixel, the raster can still select a lower
            // surface for the centre texel. On the final tap, move to the nearest texel in the
            // projected world direction so that such a one-pixel opening remains closable.
            if (!finalTap)
                continue;
            float2 projectedDelta = sampleDepthPosition - referenceDepthPosition;
            if (abs(projectedDelta.x) >= abs(projectedDelta.y) && abs(projectedDelta.x) > 1e-5f)
                sampleCoord.x += projectedDelta.x > 0.0f ? 1 : -1;
            else if (abs(projectedDelta.y) > 1e-5f)
                sampleCoord.y += projectedDelta.y > 0.0f ? 1 : -1;
            else
                continue;
        }
        if (all(sampleCoord == centerCoord) || all(sampleCoord == previousCoord))
        {
            if (finalTap)
                break;
            continue;
        }
        previousCoord = sampleCoord;

        float2 projectedDelta = sampleDepthPosition - referenceDepthPosition;
        int2 sideOffset = abs(projectedDelta.x) >= abs(projectedDelta.y)
            ? int2(0, 1)
            : int2(1, 0);
        float candidateY;
        bool tapCharacterOccluded, tapViewportClipped;
        bool widenBand = denseTap && tap < HoleSupportBandSteps;
        if (findHoleSupportInTexelBand(sampleCoord, sideOffset, widenBand, centerCoord, depthSize,
            depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH, centerDepthValid,
            referenceY, supportHeightTolerance, centerHeightError,
            candidateY, tapCharacterOccluded, tapViewportClipped))
        {
            worldY = candidateY;
            // The physical floor edge is bracketed by the preceding rejected texel and this first
            // supported texel. Using the successful tap itself rounds both ends of a hole outward;
            // that made a radius near the true size fail by up to two complete sample intervals.
            supportDistance = 0.5f * (previousTestedDistance + sampleDistance);
            return true;
        }
        supportUnavailable = supportUnavailable || tapCharacterOccluded || tapViewportClipped;
        if (tapCharacterOccluded)
        {
            // Scene depth behind a character is unavailable. Continuing to the far side of a large
            // first-person silhouette makes its screen-space width part of the measured hole and can
            // reject an otherwise valid close. Stop this half-ray as unknown instead; the antipodal
            // floor evidence below decides whether the crossing can be inferred safely.
            break;
        }
        previousTestedDistance = sampleDistance;
        if (finalTap)
            break;
    }
    return false;
}

void considerHoleSupportPair(bool firstValid, float firstY, float firstDistance, bool firstUnavailable,
    bool secondValid, float secondY, float secondDistance, bool secondUnavailable,
    float maxHeightDifference, float maxSupportSpan,
    inout int coherentPairCount, inout int observedPairCount, inout int inferredPairCount,
    inout float minPairHeight, inout float maxPairHeight)
{
    if (firstValid && secondValid
        && firstDistance + secondDistance <= maxSupportSpan
        && abs(firstY - secondY) <= maxHeightDifference)
    {
        float pairHeight = 0.5f * (firstY + secondY);
        minPairHeight = min(minPairHeight, pairHeight);
        maxPairHeight = max(maxPairHeight, pairHeight);
        ++coherentPairCount;
        ++observedPairCount;
    }
    else if ((firstValid && !secondValid && secondUnavailable)
        || (secondValid && !firstValid && firstUnavailable))
    {
        // The output pixel still performs normal scene-depth occlusion, so this never paints over
        // a character or beyond the viewport. This only prevents a clipped kernel endpoint (or a
        // character silhouette) from erasing visible floor evidence on the antipodal endpoint.
        float pairHeight = firstValid ? firstY : secondY;
        minPairHeight = min(minPairHeight, pairHeight);
        maxPairHeight = max(maxPairHeight, pairHeight);
        ++coherentPairCount;
        ++inferredPairCount;
    }
}

bool tryCloseSmallReceiverHole(PS_INPUT input, int2 depthCoord, int2 depthSize,
    float2 depthTexelToFramebuffer, float2 centerPixel, float centerDepth, float4 centerWorldH,
    bool centerDepthValid, float3 centerWorld, out float3 receiverWorld, out float aaLimit)
{
    receiverWorld = 0.0f;
    aaLimit = 0.02f;

    float holeRadius = input.params1.y;
    if (!(holeRadius > 0.0f))
        return false;
    float centerHeightError = centerDepthValid ? abs(centerWorld.y - input.origin.y) : 1e30f;
    if (centerDepthValid && centerHeightError <= HoleSupportMinHeightImprovement)
        return false;

    // Derive only the edge-AA scale from neighbouring reference-plane rays. The support kernel itself
    // is projected from explicit world offsets below, so a near-horizon finite-difference failure must
    // not turn hole closing off at a particular camera angle.
    float4 rayBase = referenceRayBaseAtPixel(centerPixel);
    float2 referenceXZ, referenceX, referenceY;
    if (!referencePlaneXZFromRayBase(rayBase, input.origin.y, input.origin.xz, referenceXZ))
        return false;

    bool referenceXValid = referencePlaneXZFromRayBase(
        rayBase + RasterScale.z * InvViewProj[0], input.origin.y, referenceXZ, referenceX);
    bool referenceYValid = referencePlaneXZFromRayBase(
        rayBase - RasterScale.w * InvViewProj[1], input.origin.y, referenceXZ, referenceY);
    float fallbackWorldPerPixel = max(min(holeRadius, 1.25f), 0.02f);
    float worldPerPixelX = referenceXValid
        ? max(length(referenceX - referenceXZ), 1e-4f)
        : (referenceYValid ? max(length(referenceY - referenceXZ), 1e-4f) : fallbackWorldPerPixel);
    float worldPerPixelY = referenceYValid
        ? max(length(referenceY - referenceXZ), 1e-4f)
        : worldPerPixelX;

    // Do not spend depth taps where the authored-plane version of the primitive has no coverage.
    float bridgeAa = clamp((worldPerPixelX + worldPerPixelY) * 2.0f, 0.02f, 1.25f);
    float referenceShapeSd = shapeSignedDistance(input, referenceXZ);
    bool outlineOnly = (input.packed & 0x100u) != 0u;
    if ((outlineOnly && abs(referenceShapeSd) > 0.5f * max(input.outlineWidth, 0.0f) + bridgeAa)
        || (!outlineOnly && referenceShapeSd > bridgeAa))
        return false;
    if (ProjectedSdfFlags.y > 0.5f && sampleArenaWorldSdf(ArenaSdfMap, referenceXZ) > bridgeAa)
        return false;

    float4 referenceWorld4 = float4(referenceXZ.x, input.origin.y, referenceXZ.y, 1.0f);
    float centerNearDistance = dot(referenceWorld4, NearPlane);
    if (centerNearDistance >= 0.0f)
        return false;
    float4 referenceClip = mul(referenceWorld4, ViewProj);
    if (abs(referenceClip.w) <= 1e-7f)
        return false;

    // Hole support is intentionally much closer to the authored plane than the general receiver band.
    // ProjectionHeight can legitimately be very large, but a remote lower floor (or a prop) must not
    // become evidence that a small opening in this floor is surrounded.
    float supportHeightTolerance = min(input.projectionHeight, max(0.25f, 2.0f * holeRadius));
    // A closable diameter can be almost 2R away from a pixel at the opposite edge. The first raster
    // texel that proves surrounding floor can lie just beyond that geometric boundary, so the ray
    // must search through the same quantization allowance used by the final span test. The measured
    // antipodal span remains bounded and therefore still rejects genuinely larger openings.
    float maxSupportSpan = 2.25f * holeRadius
        + min(max(worldPerPixelX, worldPerPixelY), 0.25f * holeRadius);
    float searchRadius = maxSupportSpan;
    float maxHeightDifference = max(0.25f, 2.0f * holeRadius);
    int coherentPairCount = 0;
    int observedPairCount = 0;
    int inferredPairCount = 0;
    float minPairHeight = 1e30f;
    float maxPairHeight = -1e30f;

    // At least two coherent antipodal axes must cross the same floor. Try cardinal axes first and
    // stop once the decision is proven; diagonals remain available for rotated grids and occlusion.
    [loop]
    for (int axis = 0; axis < 4; ++axis)
    {
        float2 axisDirection = HoleSupportAxes[axis];
        float firstY, secondY, firstDistance, secondDistance;
        bool firstUnavailable, secondUnavailable;
        bool firstValid = findWorldSpaceHoleSupport(referenceClip, centerNearDistance, axisDirection,
            searchRadius, depthCoord, depthSize, depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH,
            centerDepthValid, input.origin.y, supportHeightTolerance, centerHeightError,
            firstY, firstDistance, firstUnavailable);
        bool secondValid = findWorldSpaceHoleSupport(referenceClip, centerNearDistance, -axisDirection,
            searchRadius, depthCoord, depthSize, depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH,
            centerDepthValid, input.origin.y, supportHeightTolerance, centerHeightError,
            secondY, secondDistance, secondUnavailable);
        considerHoleSupportPair(firstValid, firstY, firstDistance, firstUnavailable,
            secondValid, secondY, secondDistance, secondUnavailable, maxHeightDifference, maxSupportSpan,
            coherentPairCount, observedPairCount, inferredPairCount, minPairHeight, maxPairHeight);

        if (coherentPairCount >= 2 && (observedPairCount > 0 || inferredPairCount >= 3)
            && maxPairHeight - minPairHeight <= maxHeightDifference)
            break;
    }

    // Ordinarily require one completely observed crossing. If a character or screen boundary hides
    // an entire half of the kernel, three inferred crossings still provide enough visible evidence.
    if (coherentPairCount < 2 || (observedPairCount == 0 && inferredPairCount < 3)
        || maxPairHeight - minPairHeight > maxHeightDifference)
        return false;

    receiverWorld = referenceWorld4.xyz;
    float receiverDepth = referenceClip.z / referenceClip.w;
    if (!(receiverDepth > 0.0f) || receiverDepth > 1.0001f
        || sceneOccludesReferencePlane(centerDepthValid, centerDepth, centerWorld, receiverDepth, receiverWorld))
        return false;

    aaLimit = clamp((worldPerPixelX + worldPerPixelY) * 4.0f, 0.02f, 1.25f);
    return true;
}

void considerReceiverTriangle(float3 center, float3 a, bool validA, float3 b, bool validB,
    inout float bestScore, inout float bestUpnessSq, inout float secondScore, inout float secondUpnessSq)
{
    if (!validA || !validB)
        return;

    float3 da = a - center;
    float3 db = b - center;
    float3 n = cross(da, db);
    float normalLenSq = dot(n, n);
    if (normalLenSq <= 1e-10f)
        return;

    // At a depth discontinuity, neighbours from an unrelated surface are usually much farther away in
    // reconstructed world space. Rank the local triangles by compactness, but keep the two best rather
    // than trusting a single lucky floor-looking pair at a wall/prop silhouette.
    float score = dot(da, da) + dot(db, db);
    float upnessSq = n.y * n.y / normalLenSq;
    if (score < bestScore)
    {
        secondScore = bestScore;
        secondUpnessSq = bestUpnessSq;
        bestScore = score;
        bestUpnessSq = upnessSq;
    }
    else if (score < secondScore)
    {
        secondScore = score;
        secondUpnessSq = upnessSq;
    }
}

bool outlineReceiverIsFloorLike(int2 depthCoord, int2 depthSize, float2 depthTexelToFramebuffer,
    float2 centerPixel, float centerDepth, float4 centerWorldH, float3 centerWorld)
{
    float3 l, r, u, d;
    bool vl = reconstructWorldAtDepthTexel(depthCoord + int2(-1, 0), depthSize, depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH, l);
    bool vr = reconstructWorldAtDepthTexel(depthCoord + int2( 1, 0), depthSize, depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH, r);
    bool vu = reconstructWorldAtDepthTexel(depthCoord + int2(0, -1), depthSize, depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH, u);
    bool vd = reconstructWorldAtDepthTexel(depthCoord + int2(0,  1), depthSize, depthTexelToFramebuffer, centerPixel, centerDepth, centerWorldH, d);

    float bestScore = 1e30f;
    float secondScore = 1e30f;
    float bestUpnessSq = -1.0f;
    float secondUpnessSq = -1.0f;
    considerReceiverTriangle(centerWorld, r, vr, d, vd, bestScore, bestUpnessSq, secondScore, secondUpnessSq);
    considerReceiverTriangle(centerWorld, d, vd, l, vl, bestScore, bestUpnessSq, secondScore, secondUpnessSq);
    considerReceiverTriangle(centerWorld, l, vl, u, vu, bestScore, bestUpnessSq, secondScore, secondUpnessSq);
    considerReceiverTriangle(centerWorld, u, vu, r, vr, bestScore, bestUpnessSq, secondScore, secondUpnessSq);

    // Require two independently compact, upward-facing local triangles. This intentionally fails closed
    // on silhouettes and one-pixel props, where the previous "best one wins" test could accidentally
    // select a floor neighbour while the centre depth actually belonged to a wall. A 0.55 up component
    // still accepts ordinary ramps up to about 57 degrees from horizontal.
    const float minReceiverUpness = 0.55f;
    float minUpnessSq = minReceiverUpness * minReceiverUpness;
    return secondScore < 1e29f && bestUpnessSq >= minUpnessSq && secondUpnessSq >= minUpnessSq;
}


// Analytic camera-facing 3D eye ---------------------------------------------------------------
// The 2D radar eye is an exact intersection of two equal circles. The world version lifts the same
// construction into 3D by intersecting two ellipsoids, producing a true biconvex lens rather than a
// billboard. A second, expanded lens is integrated as translucent animated mist around the solid eye.
bool eyeRaySphereInterval(float3 ro, float3 rd, float3 sphereCenter, float radius, out float t0, out float t1)
{
    float3 oc = ro - sphereCenter;
    float a = dot(rd, rd);
    float b = dot(oc, rd);
    float c = dot(oc, oc) - radius * radius;
    float h = b * b - a * c;
    if (!(a > 1e-10f) || h < 0.0f)
    {
        t0 = t1 = 0.0f;
        return false;
    }

    float root = sqrt(max(h, 0.0f));
    float invA = 1.0f / a;
    t0 = (-b - root) * invA;
    t1 = (-b + root) * invA;
    return true;
}

bool eyeLensInterval(float3 roLocal, float3 rdLocal, float halfWidth, float halfHeight, float halfDepth, out float t0, out float t1)
{
    halfWidth = max(halfWidth, 1e-4f);
    halfHeight = clamp(halfHeight, 1e-4f, halfWidth);
    halfDepth = max(halfDepth, 1e-4f);

    // Scale depth into the same metric as horizontal width; the original 2D circle-intersection
    // equation can then be reused unchanged for two ellipsoids.
    float depthScale = halfWidth / halfDepth;
    float3 ro = float3(roLocal.xy, roLocal.z * depthScale);
    float3 rd = float3(rdLocal.xy, rdLocal.z * depthScale);
    float sphereRadius = (halfWidth * halfWidth + halfHeight * halfHeight) / (2.0f * halfHeight);
    float sphereOffset = sphereRadius - halfHeight;

    float a0, a1, b0, b1;
    if (!eyeRaySphereInterval(ro, rd, float3(0.0f, sphereOffset, 0.0f), sphereRadius, a0, a1)
        || !eyeRaySphereInterval(ro, rd, float3(0.0f, -sphereOffset, 0.0f), sphereRadius, b0, b1))
    {
        t0 = t1 = 0.0f;
        return false;
    }

    t0 = max(a0, b0);
    t1 = min(a1, b1);
    return t1 >= t0;
}

void eyeCameraBasis(out float3 right, out float3 up, out float3 forward)
{
    // For row-vector unprojection, the first two inverse-VP rows are proportional to camera-right
    // and camera-up screen displacements. Re-orthogonalize once to keep the lens stable under jitter.
    right = normalize(InvViewProj[0].xyz);
    up = InvViewProj[1].xyz;
    up -= right * dot(up, right);
    up = normalize(up);
    forward = normalize(cross(right, up));
}

bool eyeViewRay(float4 sceneWorldH, float sceneDepth, float3 sceneWorld, out float3 rayOrigin, out float3 rayDirection, out float sceneT)
{
    // FFXIV uses reverse-Z. Sample the same pixel close to z=1 (near) and z=0 (far) to obtain a
    // stable world ray without needing an explicit camera-position constant.
    float4 depthRow = InvViewProj[2];
    float4 rayBase = sceneWorldH - sceneDepth * depthRow;
    float4 nearH = rayBase + 0.9999f * depthRow;
    float4 farH = rayBase + 0.0001f * depthRow;
    if (abs(nearH.w) <= 1e-7f || abs(farH.w) <= 1e-7f)
    {
        rayOrigin = rayDirection = 0.0f;
        sceneT = 0.0f;
        return false;
    }

    rayOrigin = nearH.xyz / nearH.w;
    float3 farPoint = farH.xyz / farH.w;
    float3 delta = farPoint - rayOrigin;
    float len = length(delta);
    if (!(len > 1e-6f))
    {
        rayDirection = 0.0f;
        sceneT = 0.0f;
        return false;
    }

    rayDirection = delta / len;
    sceneT = dot(sceneWorld - rayOrigin, rayDirection);
    if (sceneT < 0.0f)
    {
        rayDirection = -rayDirection;
        sceneT = -sceneT;
    }
    return sceneT > 0.0f;
}

float eyeMistPattern(float3 localPos, float timeSeconds)
{
    // Deliberately smooth, low-frequency motion: two crossing wave fields make the fog curl instead
    // of reading as concentric zone-wave rings. This is decorative only and costs no texture sample.
    float a = sin(localPos.x * 2.35f + localPos.y * 1.15f + timeSeconds * 1.85f);
    float b = sin(localPos.y * 2.75f - localPos.z * 3.10f - timeSeconds * 1.35f);
    float c = sin((localPos.x - localPos.y + localPos.z) * 1.65f + timeSeconds * 2.20f);
    return saturate(0.5f + (a + b + c) * (1.0f / 6.0f));
}

float4 shadeWorldEye(PS_INPUT input, float4 sceneWorldH, float sceneDepth, float3 sceneWorld, float uiVisibility)
{
    float halfWidth = input.params0.x;
    float halfHeight = input.params0.y;
    float halfDepth = input.params0.z;
    float mistRadius = input.params0.w;
    bool invertedGaze = (input.packed & 0x800u) != 0u;
    float eyeTime = invertedGaze ? -ZoneWaveParams.x : ZoneWaveParams.x;

    float3 rayOrigin, rayDirection;
    float sceneT;
    if (!eyeViewRay(sceneWorldH, sceneDepth, sceneWorld, rayOrigin, rayDirection, sceneT))
        clip(-1.0f);

    float3 right, up, forward;
    eyeCameraBasis(right, up, forward);
    float3 roDelta = rayOrigin - input.origin;
    float3 roLocal = float3(dot(roDelta, right), dot(roDelta, up), dot(roDelta, forward));
    float3 rdLocal = float3(dot(rayDirection, right), dot(rayDirection, up), dot(rayDirection, forward));

    // Expanded lens volume for the mist. Clip it against the first native scene surface so the fog
    // can never bleed through walls/actors even though it is translucent.
    float mistWidth = halfWidth + mistRadius;
    float mistHeight = halfHeight + 0.70f * mistRadius;
    float mistDepth = halfDepth + 0.55f * mistRadius;
    float mistNear, mistFar;
    float mistAlpha = 0.0f;
    float mistPattern = 0.0f;
    float3 mistRgb = input.col.rgb;
    if (eyeLensInterval(roLocal, rdLocal, mistWidth, mistHeight, mistDepth, mistNear, mistFar))
    {
        float visibleNear = max(mistNear, 0.0f);
        float visibleFar = min(mistFar, sceneT - 0.015f);
        float visibleThickness = max(visibleFar - visibleNear, 0.0f);
        if (visibleThickness > 0.0f)
        {
            float mistMidT = 0.5f * (visibleNear + visibleFar);
            float3 mistMidWorld = rayOrigin + rayDirection * mistMidT;
            float3 md = mistMidWorld - input.origin;
            float3 mistLocal = float3(dot(md, right), dot(md, up), dot(md, forward));
            mistPattern = eyeMistPattern(mistLocal, eyeTime);
            float breathing = 0.78f + 0.22f * sin(eyeTime * 2.15f + length(mistLocal.xy) * 2.4f);
            float density = saturate(visibleThickness / max(2.0f * mistDepth, 0.1f));
            mistAlpha = input.col.a * density * (0.16f + 0.34f * mistPattern) * breathing;
            mistRgb = input.col.rgb * (1.04f + 0.24f * mistPattern + 0.10f * breathing);
        }
    }

    // Solid biconvex lens. Only the front intersection is needed; the alpha blend plus fog provides
    // depth while preserving the familiar icon readability of the 2D version.
    float lensNear, lensFar;
    bool solidVisible = eyeLensInterval(roLocal, rdLocal, halfWidth, halfHeight, halfDepth, lensNear, lensFar);
    float solidT = max(lensNear, 0.0f);
    solidVisible = solidVisible && solidT <= lensFar && solidT < sceneT + 0.015f;

    if (!solidVisible)
    {
        float alpha = mistAlpha * uiVisibility;
        clip(alpha - 0.001f);
        return float4(mistRgb, alpha);
    }

    float3 hitWorld = rayOrigin + rayDirection * solidT;
    float3 hd = hitWorld - input.origin;
    float3 hitLocal = float3(dot(hd, right), dot(hd, up), dot(hd, forward));

    float sphereRadius = (halfWidth * halfWidth + halfHeight * halfHeight) / (2.0f * halfHeight);
    float sphereOffset = sphereRadius - halfHeight;
    float2 q = hitLocal.xy;
    float lens2dSd = max(
        length(q - float2(0.0f, sphereOffset)) - sphereRadius,
        length(q - float2(0.0f, -sphereOffset)) - sphereRadius);
    float lensAa = max(abs(ddx(lens2dSd)) + abs(ddy(lens2dSd)), 0.003f);
    float solidCoverage = saturate(0.5f - lens2dSd / lensAa);

    // Surface normal from whichever of the two ellipsoids is the active intersection boundary.
    float depthScale = halfWidth / max(halfDepth, 1e-4f);
    float3 pScaled = float3(hitLocal.xy, hitLocal.z * depthScale);
    float3 topDelta = pScaled - float3(0.0f, sphereOffset, 0.0f);
    float3 bottomDelta = pScaled - float3(0.0f, -sphereOffset, 0.0f);
    float topField = length(topDelta) - sphereRadius;
    float bottomField = length(bottomDelta) - sphereRadius;
    float3 nScaled = normalize(topField >= bottomField ? topDelta : bottomDelta);
    float3 nLocal = normalize(float3(nScaled.xy, nScaled.z * depthScale));
    float3 nWorld = normalize(right * nLocal.x + up * nLocal.y + forward * nLocal.z);
    float facing = saturate(dot(nWorld, -rayDirection));
    float fresnel = (1.0f - facing) * (1.0f - facing);

    float4 border = input.outlineCol.a > 0.0f ? input.outlineCol : float4(input.col.rgb * 0.22f, input.col.a);
    float edgeMask = smoothstep(-0.18f * halfHeight, -0.025f * halfHeight, lens2dSd);

    float irisRadius = 0.325f * halfWidth;
    float pupilRadius = 0.165f * halfWidth;
    float highlightRadius = 0.072f * halfWidth;
    float irisDistance = length(q);
    float irisAa = max(abs(ddx(irisDistance)) + abs(ddy(irisDistance)), 0.003f);
    float irisMask = 1.0f - smoothstep(irisRadius - irisAa, irisRadius + irisAa, irisDistance);
    float pupilMask = 1.0f - smoothstep(pupilRadius - irisAa, pupilRadius + irisAa, irisDistance);
    float2 highlightCenter = float2(-0.09f * halfWidth, 0.15f * halfHeight);
    float highlightDistance = length(q - highlightCenter);
    float highlightAa = max(abs(ddx(highlightDistance)) + abs(ddy(highlightDistance)), 0.003f);
    float highlightMask = 1.0f - smoothstep(highlightRadius - highlightAa, highlightRadius + highlightAa, highlightDistance);

    // Motion communicates the instruction independently of colour: ordinary gazes push a bright
    // pulse away from the pupil, while inverted gazes pull it inward. Inverted eyes also retain a
    // static target reticle so the distinction remains legible in a still frame.
    float2 eyeUv = q / max(float2(halfWidth, halfHeight), float2(1e-4f, 1e-4f));
    float eyeRadial = length(eyeUv);
    float eyeUvAa = max(fwidth(eyeUv.x) + fwidth(eyeUv.y), 0.002f);
    float eyeRadialAa = max(fwidth(eyeRadial), 0.002f);
    float pulseCycle = frac(ZoneWaveParams.x * 0.72f);
    float pulseDirection = invertedGaze ? 1.0f - pulseCycle : pulseCycle;
    float pulseRadius = lerp(0.24f, 0.86f, pulseDirection);
    float pulseDistance = abs(eyeRadial - pulseRadius);
    float pulseMask = 1.0f - smoothstep(0.028f, 0.028f + eyeRadialAa, pulseDistance);
    pulseMask *= sin(pulseCycle * 3.14159265f);

    float reticleDistance = abs(eyeRadial - 0.48f);
    float reticleRing = 1.0f - smoothstep(0.022f, 0.022f + eyeRadialAa, reticleDistance);
    float2 absEyeUv = abs(eyeUv);
    float horizontalTick = (1.0f - smoothstep(0.026f, 0.026f + eyeUvAa, absEyeUv.y))
        * smoothstep(0.52f - eyeUvAa, 0.52f + eyeUvAa, absEyeUv.x)
        * (1.0f - smoothstep(0.72f - eyeUvAa, 0.72f + eyeUvAa, absEyeUv.x));
    float verticalTick = (1.0f - smoothstep(0.026f, 0.026f + eyeUvAa, absEyeUv.x))
        * smoothstep(0.52f - eyeUvAa, 0.52f + eyeUvAa, absEyeUv.y)
        * (1.0f - smoothstep(0.72f - eyeUvAa, 0.72f + eyeUvAa, absEyeUv.y));
    float reticleMask = invertedGaze ? saturate(reticleRing + horizontalTick + verticalTick) : 0.0f;

    float localMist = eyeMistPattern(hitLocal, eyeTime);
    float4 solid = input.col;
    solid.rgb *= 0.72f + 0.30f * facing;
    solid.rgb += input.col.rgb * (0.08f + 0.16f * localMist + 0.12f * fresnel);
    solid.rgb = lerp(solid.rgb, border.rgb, edgeMask);
    solid.rgb = lerp(solid.rgb, border.rgb * 1.08f, irisMask);
    solid.rgb = lerp(solid.rgb, float3(0.035f, 0.035f, 0.045f), pupilMask);
    solid.rgb = lerp(solid.rgb, float3(1.0f, 1.0f, 1.0f), highlightMask);
    float3 pulseColor = lerp(input.col.rgb, float3(1.0f, 1.0f, 1.0f), 0.78f);
    // Green correctness feedback is much brighter than red, so give bright/custom colours a little
    // more whitening while leaving the red danger pulse essentially unchanged.
    float pulseLuma = dot(input.col.rgb, float3(0.2126f, 0.7152f, 0.0722f));
    float pulseStrength = lerp(0.56f, 0.70f, smoothstep(0.25f, 0.70f, pulseLuma));
    solid.rgb = lerp(solid.rgb, pulseColor, pulseStrength * pulseMask);
    solid.rgb = lerp(solid.rgb, float3(0.86f, 0.96f, 1.0f), 0.92f * reticleMask);
    solid.a *= solidCoverage * (0.86f + 0.14f * facing);

    // Straight-alpha compositing inside the primitive: the solid eye sits in front of its own mist.
    float mistBehindWeight = mistAlpha * (1.0f - solid.a);
    float alpha = (solid.a + mistBehindWeight) * uiVisibility;
    clip(alpha - 0.001f);
    float3 rgb = (solid.rgb * solid.a + mistRgb * mistBehindWeight) / max(solid.a + mistBehindWeight, 1e-4f);
    return float4(rgb, alpha);
}

float4 main(PS_INPUT input) : SV_Target
{
    // Terrain depth is a defining property of this primitive family. Native UI ordering is supplied
    // by the background overlay layer rather than a sampled framebuffer mask.
    if (SceneDepthParams.w < 0.5f)
        clip(-1.0f);

    float uiVisibility = nativeUiVisibility(input.pos.xy);
    if (uiVisibility <= 0.0f)
        clip(-1.0f);

    float2 framebufferSize = max(Viewport.xy, float2(1.0f, 1.0f));
    int2 depthSize = max(int2(SceneDepthParams.xy), int2(1, 1));
    int2 depthCoord = int2(input.pos.xy * RasterScale.xy);
    depthCoord = clamp(depthCoord, int2(0, 0), depthSize - int2(1, 1));
    uint shapeKind = input.packed & 0xFFu;

    float sceneDepth = SceneDepthTexture.Load(int3(depthCoord, 0));
    bool sceneDepthValid = sceneDepth > 0.0f;

    // GBuffer[3] is FFXIV's scene classification target. Character pixels are black (with a
    // little edge noise), while opaque terrain and scene models carry non-zero channels. Sky is
    // black too, so require valid scene depth before treating a black texel as a character. That
    // leaves depthless grate openings eligible for the bounded receiver-hole closer below.
    // Eye3D is real foreground volume geometry and retains ordinary scene-depth occlusion.
    if (shapeKind != 8u && sceneDepthValid && SceneInfoParams.x > 0.5f)
    {
        float3 sceneInfo = SceneInfoTexture.Load(int3(depthCoord, 0)).rgb;
        clip(max(sceneInfo.r, max(sceneInfo.g, sceneInfo.b)) - SceneInfoParams.y);
    }

    float4 worldH = 0.0f;
    float3 world = 0.0f;
    if (sceneDepthValid)
    {
        float2 ndc = float2(input.pos.x * RasterScale.z - 1.0f, 1.0f - input.pos.y * RasterScale.w);
        worldH = mul(float4(ndc, sceneDepth, 1.0f), InvViewProj);
        sceneDepthValid = abs(worldH.w) > 1e-7f;
        if (sceneDepthValid)
            world = worldH.xyz / worldH.w;
    }

    if (shapeKind == 8u) // analytic camera-facing 3D eye volume
    {
        if (!sceneDepthValid)
            clip(-1.0f);
        return shadeWorldEye(input, worldH, sceneDepth, world, uiVisibility);
    }

    bool outlineOnly = (input.packed & 0x100u) != 0u;
    bool filledWithOutline = (input.packed & 0x200u) != 0u;
    bool suppressZoneWave = (input.packed & 0x400u) != 0u;
    bool referencePlaneProjection = !(input.projectionHeight > 0.0f);
    bool arenaClipped = ProjectedSdfFlags.y > 0.5f;

    bool receiverAccepted = false;
    bool holeFilled = false;
    float holeAaLimit = 0.02f;

    if (referencePlaneProjection)
    {
        // Height-disabled projection draws the authored horizontal plane. This gives true grate gaps
        // and depthless holes a receiver while retaining ordinary scene-depth occlusion in front.
        float3 referenceWorld;
        float referenceDepth;
        if (!referencePlaneReceiverAtPixel(input, input.pos.xy,
            referenceWorld, referenceDepth, holeAaLimit))
            clip(-1.0f);
        if (sceneOccludesReferencePlane(sceneDepthValid, sceneDepth, world, referenceDepth, referenceWorld))
            clip(-1.0f);
        world = referenceWorld;
        receiverAccepted = true;
        holeFilled = true;
    }
    else
    {
        receiverAccepted = sceneDepthValid
            && receiverWithinProjectionHeight(world, input.origin.y, input.projectionHeight);
        float2 depthTexelToFramebuffer = 1.0f / max(RasterScale.xy, float2(1e-7f, 1e-7f));

        // A lower surface seen through a grate can still fall inside the ordinary height band. Small
        // downward floor variation remains a real receiver; only a material drop launches the search.
        float minimumBridgeDrop = max(0.08f, min(0.20f, 0.25f * input.params1.y));
        if (receiverAccepted && input.params1.y > 0.0f
            && world.y < input.origin.y - minimumBridgeDrop)
        {
            float3 bridgedWorld;
            if (tryCloseSmallReceiverHole(input, depthCoord, depthSize,
                depthTexelToFramebuffer, input.pos.xy, sceneDepth, worldH, true, world,
                bridgedWorld, holeAaLimit))
            {
                world = bridgedWorld;
                holeFilled = true;
            }
        }

        if (!receiverAccepted)
        {
            // Only depthless pixels or surfaces below the authored band are hole candidates. Never
            // bridge a foreground surface above the reference floor.
            bool canCloseHole = input.params1.y > 0.0f
                && (!sceneDepthValid || world.y < input.origin.y - input.projectionHeight);
            if (!canCloseHole)
                clip(-1.0f);
            float3 bridgedWorld;
            if (!tryCloseSmallReceiverHole(input, depthCoord, depthSize,
                depthTexelToFramebuffer, input.pos.xy, sceneDepth, worldH, sceneDepthValid, world,
                bridgedWorld, holeAaLimit))
                clip(-1.0f);
            world = bridgedWorld;
            receiverAccepted = true;
            holeFilled = true;
        }
    }

    // The depth-safe filled-edge AA estimate reuses the current homogeneous reconstruction. Do it
    // only after receiver rejection, and skip it for an unclipped outline-only primitive.
    float aaLimit = 0.0f;
    if (!outlineOnly || arenaClipped)
        aaLimit = holeFilled ? holeAaLimit : stableWorldAaLimit(input.pos.xy, framebufferSize, worldH, world);

    // Arena clipping is independent of the primitive SDF. Reject fully outside pixels first so custom
    // polygon sampling and outline calculations are avoided when the arena already excludes them.
    float arenaCoverage = 1.0f;
    if (arenaClipped)
    {
        float arenaSd = sampleArenaWorldSdf(ArenaSdfMap, world.xz);
        arenaCoverage = fillFieldCoverage(arenaSd, aaLimit);
        if (arenaCoverage <= 0.0f)
            clip(-1.0f);
    }

    float sd = shapeSignedDistance(input, world.xz);

    // Filled-edge AA keeps the depth-discontinuity-safe path. True outlines use a separate
    // depth-independent reference-plane scale and are evaluated directly in screen pixels.
    float fillCoverage = outlineOnly ? 0.0f : fillFieldCoverage(sd, aaLimit);
    float2 outlineReferenceXZ = world.xz;
    bool outlineReferenceValid = true;
    float outlineCoverage = input.outlineWidth > 0.0f
        ? outlineFieldCoverage(input, sd, input.outlineWidth, input.pos.xy, outlineReferenceXZ, outlineReferenceValid)
        : 0.0f;
    float coverage;
    float4 shapeColor = input.col;
    if (outlineOnly)
    {
        coverage = outlineCoverage;
        if (coverage <= 0.0f)
            clip(-1.0f);

        // ProjectionHeight remains the authored vertical receiver limit, but near a horizontal camera
        // ray even a small Y difference maps to a huge X/Z displacement. That makes unrelated scene
        // surfaces sample the same thin SDF contour as repeated stripes. Bound that displacement for
        // outlines only: normal camera angles still get the full authored height, while grazing views
        // automatically narrow to receivers close to the primitive's stable reference plane. Reuse the
        // exact reference intersection already produced by the depth-independent outline-width path.
        const float maxStableOutlineProjectionXZSq = 16.0f; // 4 yalms squared
        if (!outlineReferenceValid)
            clip(-1.0f);
        float2 receiverShiftXZ = world.xz - outlineReferenceXZ;
        clip(maxStableOutlineProjectionXZSq - dot(receiverShiftXZ, receiverShiftXZ));

        // Keep terrain projection and its authored +/- ProjectionHeight receiver band, but validate the
        // receiver with explicit neighbouring depth texels rather than ddx/ddy(world). Require two
        // coherent floor-like local triangles so a single neighbour across a silhouette cannot make a
        // wall/prop look like a valid floor receiver. A closed grate hole already required four valid
        // surrounding receivers and deliberately has no centre surface to validate here.
        float2 depthTexelToFramebuffer = 1.0f / max(RasterScale.xy, float2(1e-7f, 1e-7f));
        if (!holeFilled && !outlineReceiverIsFloorLike(depthCoord, depthSize, depthTexelToFramebuffer, input.pos.xy, sceneDepth, worldH, world))
        {
            // A narrow coplanar grate bar can fail the one-texel normal test. Validate its authored
            // plane directly; for a displaced receiver, require the bounded surrounding-floor search.
            if (abs(world.y - input.origin.y) <= SceneDepthParams.z)
            {
                float3 referenceWorld;
                float referenceDepth;
                float referenceAaLimit;
                if (!referencePlaneReceiverAtPixel(input, input.pos.xy,
                    referenceWorld, referenceDepth, referenceAaLimit)
                    || sceneOccludesReferencePlane(sceneDepthValid, sceneDepth, world,
                        referenceDepth, referenceWorld))
                    clip(-1.0f);
            }
            else
            {
                float3 supportedWorld;
                float supportedAaLimit;
                if (!tryCloseSmallReceiverHole(input, depthCoord, depthSize,
                    depthTexelToFramebuffer, input.pos.xy, sceneDepth, worldH, sceneDepthValid,
                    world, supportedWorld, supportedAaLimit))
                    clip(-1.0f);
            }
        }
    }
    else if (filledWithOutline)
    {
        // Composite the edge colour inside this one projected primitive. This avoids a second scene
        // depth reconstruction and classification pass for actor marker outlines.
        coverage = max(fillCoverage, outlineCoverage);
        shapeColor = lerp(input.col, input.outlineCol, saturate(outlineCoverage));
    }
    else
    {
        coverage = fillCoverage;
    }

    coverage *= arenaCoverage;

    // Terrain-projected filled zones get the expanding wave. Outline-only primitives and the
    // combined fill+outline actor-marker path intentionally remain static.
    if (!outlineOnly && !suppressZoneWave)
        shapeColor = applyZoneWaveWorld(shapeColor, world.xz - input.waveOriginXZ, 1.0f);

    float alpha = shapeColor.a * coverage * uiVisibility;
    clip(alpha - 0.001f);
    return float4(shapeColor.rgb, alpha);
}
