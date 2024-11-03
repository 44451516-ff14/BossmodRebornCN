namespace BossMod;

// a bunch of utilities for approximating curves with line segments
// we need them, since clipping and rendering works with polygons
public static class CurveApprox
{
    public const float ScreenError = 0.05f; // typical maximal screen-space error; tradeoff between performance and fidelity
    private static readonly Angle a0 = 0.Degrees(), a90 = 90.Degrees(), a270 = 270.Degrees(), a360 = 360.Degrees();
    private const float DoublePI = 2 * MathF.PI;

    public static int CalculateCircleSegments(float radius, Angle angularLength, float maxError)
    {
        // select max angle such that tesselation error is smaller than desired
        // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R
        var tessAngle = 2 * MathF.Acos(1 - Math.Min(maxError / radius, 1));
        var tessNumSegments = (int)MathF.Ceiling(angularLength.Rad / tessAngle);
        tessNumSegments = (tessNumSegments + 1) & ~1; // round up to even for symmetry
        return Math.Clamp(tessNumSegments, 4, 512);
    }

    // return polygon points approximating full circle; implicitly closed path - last point is not included
    // winding: points are in CCW order
    public static IEnumerable<WDir> Circle(float radius, float maxError)
    {
        var numSegments = CalculateCircleSegments(radius, a360, maxError);
        var angle = (DoublePI / numSegments).Radians();
        for (var i = 0; i < numSegments; ++i) // note: do not include last point
            yield return PolarToCartesian(radius, i * angle);
    }
    public static IEnumerable<WPos> Circle(WPos center, float radius, float maxError) => Circle(radius, maxError).Select(off => center + off);

    // return polygon points approximating circle arc; both start and end points are included
    // winding: points are either in CCW order (if length is positive) or CW order (if length is negative)
    public static IEnumerable<WDir> CircleArc(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var numSegments = CalculateCircleSegments(radius, length.Abs(), maxError);
        var angle = length / numSegments;
        for (var i = 0; i <= numSegments; ++i)
            yield return PolarToCartesian(radius, angleStart + i * angle);
    }
    public static IEnumerable<WPos> CircleArc(WPos center, float radius, Angle angleStart, Angle angleEnd, float maxError) => CircleArc(radius, angleStart, angleEnd, maxError).Select(off => center + off);
    public static IEnumerable<WDir> CircleArc(WDir dirZ, float radius, Angle angleStart, Angle angleEnd, float maxError) => CircleArc(radius, angleStart, angleEnd, maxError);

    // return polygon points approximating circle sector; implicitly closed path - center + arc
    public static IEnumerable<WDir> CircleSector(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        yield return default;
        foreach (var v in CircleArc(radius, angleStart, angleEnd, maxError))
            yield return v;
    }
    public static IEnumerable<WPos> CircleSector(WPos center, float radius, Angle angleStart, Angle angleEnd, float maxError) => CircleSector(radius, angleStart, angleEnd, maxError).Select(off => center + off);

    // return polygon points approximating full donut; implicitly closed path - outer arc + inner arc
    public static IEnumerable<WDir> Donut(float innerRadius, float outerRadius, float maxError)
    {
        foreach (var v in Circle(outerRadius, maxError))
            yield return v;
        yield return PolarToCartesian(outerRadius, a0);
        yield return PolarToCartesian(innerRadius, a0);
        foreach (var v in Circle(innerRadius, maxError).Reverse())
            yield return v;
    }
    public static IEnumerable<WPos> Donut(WPos center, float innerRadius, float outerRadius, float maxError) => Donut(innerRadius, outerRadius, maxError).Select(off => center + off);

    // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
    public static IEnumerable<WDir> DonutSector(float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        foreach (var v in CircleArc(outerRadius, angleStart, angleEnd, maxError))
            yield return v;
        foreach (var v in CircleArc(innerRadius, angleEnd, angleStart, maxError))
            yield return v;
    }
    public static IEnumerable<WPos> DonutSector(WPos center, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError) => DonutSector(innerRadius, outerRadius, angleStart, angleEnd, maxError).Select(off => center + off);

    // return polygon points for rectangle - it's not really a curve, but whatever...
    public static IEnumerable<WDir> Rect(WDir dx, WDir dz)
    {
        yield return dx - dz;
        yield return dx + dz;
        yield return -dx + dz;
        yield return -dx - dz;
    }
    public static IEnumerable<WDir> Rect(WDir dirZ, float halfWidth, float halfHeight) => Rect(dirZ.OrthoL() * halfWidth, dirZ * halfHeight);
    public static IEnumerable<WDir> Rect(WDir center, WDir dx, WDir dz) => Rect(dx, dz).Select(off => center + off);
    public static IEnumerable<WDir> Rect(WDir center, WDir dirZ, float halfWidth, float halfHeight) => Rect(center, dirZ.OrthoL() * halfWidth, dirZ * halfHeight);
    public static IEnumerable<WPos> Rect(WPos center, WDir dx, WDir dz) => Rect(dx, dz).Select(off => center + off);
    public static IEnumerable<WPos> Rect(WPos center, WDir dirZ, float halfWidth, float halfHeight) => Rect(center, dirZ.OrthoL() * halfWidth, dirZ * halfHeight);

    // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise
    private static WDir PolarToCartesian(float r, Angle phi) => r * phi.ToDirection();

    public static IEnumerable<WDir> Capsule(WDir dir, float length, float radius, float maxError)
    {
        var p0 = default(WDir);
        var p1 = length * dir;

        var dirPerp = dir.OrthoL();
        var angleDir = Angle.FromDirection(dir);

        var angleStartP1 = angleDir - a90;
        var angleEnd = angleDir + a90;
        var angleEndP0 = angleDir + a270;
        var radiusDirPerp = radius * dirPerp;

        yield return p0 + radiusDirPerp;
        yield return p1 + radiusDirPerp;
        foreach (var v in CircleArc(radius, angleStartP1, angleEnd, maxError).Select(off => p1 + off))
            yield return v;
        yield return p1 - radiusDirPerp;
        yield return p0 - radiusDirPerp;
        foreach (var v in CircleArc(radius, angleEnd, angleEndP0, maxError).Select(off => p0 + off))
            yield return v;
    }

    public static IEnumerable<WDir> Capsule(WDir origin, WDir dir, float length, float radius, float maxError)
        => Capsule(dir, length, radius, maxError).Select(off => origin + off);
}
