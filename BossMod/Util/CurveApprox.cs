namespace BossMod;

// a bunch of utilities for approximating curves with line segments
// we need them, since clipping and rendering works with polygons
[SkipLocalsInit]
public static class CurveApprox
{
    public const float ScreenError = 0.05f;
    // for angles, we use standard FF convention: 0 is 'south'/down/(0, -r), and then increases clockwise

    public static int CalculateCircleSegments(float radius, Angle angularLength, float maxError)
    {
        // select max angle such that tesselation error is smaller than desired
        // error = R * (1 - cos(phi/2)) => cos(phi/2) = 1 - error/R
        var tessAngle = 2 * MathF.Acos(1 - Math.Min(maxError / radius, 1));
        var tessNumSegments = (int)MathF.Ceiling(angularLength.Rad / tessAngle);
        tessNumSegments = (tessNumSegments + 1) & ~1;
        return Math.Clamp(tessNumSegments, 4, 512);
    }

    // return polygon points approximating full circle; implicitly closed path - last point is not included
    // winding: points are in CCW order
    public static List<WDir> Circle(float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, 360f.Degrees(), maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();

        var result = new List<WDir>(numSegments);
        CollectionsMarshal.SetCount(result, numSegments);
        var vertices = CollectionsMarshal.AsSpan(result);

        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            vertices[i] = radius * (i * angleIncrement).ToDirection();
        }
        return result;
    }

    public static WDir[] Circle(WDir centerOffset, float Radius, float maxError)
    {
        var radius = Radius;
        var numSegments = CalculateCircleSegments(radius, 360f.Degrees(), maxError);
        var angleIncrement = (Angle.DoublePI / numSegments).Radians();
        var points = new WDir[numSegments];
        var centerO = centerOffset;
        for (var i = 0; i < numSegments; ++i) // note: do not include last point
        {
            points[i] = radius * (i * angleIncrement).ToDirection() + centerO;
        }
        return points;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<WDir> CircleL(WDir centerOffset, float radius, float maxError)
    {
        return ArrayListWrapper<WDir>.Wrap(Circle(centerOffset, radius, maxError));
    }

    public static WDir[] CircleArc(float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, length.Abs(), maxError);
        var angleIncrement = length / numSegments;
        var points = new WDir[numSegments + 1];
        for (var i = 0; i <= numSegments; ++i)
        {
            var angle = angleStart + i * angleIncrement;
            points[i] = PolarToCartesian(radius, angle);
        }
        return points;
    }

    // return polygon points approximating circle sector; implicitly closed path - center + arc
    public static WDir[] CircleSector(WDir centerOffset, float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var _radius = radius;
        var numSegments = CalculateCircleSegments(_radius, length.Abs(), maxError);
        var angleIncrement = length / numSegments;
        var points = new WDir[numSegments + 2];
        var centerO = centerOffset;

        for (var i = 0; i <= numSegments; ++i)
        {
            points[i + 1] = _radius * (angleStart + i * angleIncrement).ToDirection() + centerO;
        }

        points[0] = centerO;
        return points;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<WDir> CircleSectorL(WDir centerOffset, float radius, Angle angleStart, Angle angleEnd, float maxError)
    {
        return ArrayListWrapper<WDir>.Wrap(CircleSector(centerOffset, radius, angleStart, angleEnd, maxError));
    }

    // return polygon points approximating full donut; implicitly closed path - outer arc + inner arc
    public static WDir[] Donut(WDir centerOffset, float innerRadius, float outerRadius, float maxError)
    {
        var radiusO = outerRadius;
        var radiusI = innerRadius;
        var a360 = 360f.Degrees();
        var numSegmentsO = CalculateCircleSegments(radiusO, a360, maxError);
        var numSegmentsI = CalculateCircleSegments(radiusI, a360, maxError);
        var angleIncrementO = (Angle.DoublePI / numSegmentsO).Radians();
        var points = new WDir[numSegmentsO + numSegmentsI + 2];
        var centerO = centerOffset;

        for (var i = 0; i < numSegmentsO; ++i) // note: do not include last point
        {
            points[i] = radiusO * (i * angleIncrementO).ToDirection() + centerO;
        }

        var v1 = new WDir(0f, 1f);
        points[numSegmentsO] = radiusO * v1 + centerO;
        points[numSegmentsO + 1] = radiusI * v1 + centerO;

        var index = numSegmentsO + 2;
        var innerAdj = numSegmentsI - 1;
        var angleIncrementI = (Angle.DoublePI / numSegmentsI).Radians();
        for (var i = innerAdj; i >= 0; --i)
        {
            points[index++] = radiusI * (i * angleIncrementI).ToDirection() + centerO;
        }

        return points;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<WDir> DonutL(WDir centerOffset, float innerRadius, float outerRadius, float maxError)
    {
        return ArrayListWrapper<WDir>.Wrap(Donut(centerOffset, innerRadius, outerRadius, maxError));
    }

    // return polygon points approximating donut sector; implicitly closed path - outer arc + inner arc
    public static WDir[] DonutSector(WDir centerOffset, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        var length = angleEnd - angleStart;
        var radiusO = outerRadius;
        var radiusI = innerRadius;
        var lenAbs = length.Abs();
        var numSegmentsO = CalculateCircleSegments(radiusO, lenAbs, maxError);
        var numSegmentsI = CalculateCircleSegments(radiusI, lenAbs, maxError);
        var angleIncrementO = length / numSegmentsO;
        var angleIncrementI = length / numSegmentsI;
        var points = new WDir[numSegmentsO + numSegmentsI + 2];
        var centerO = centerOffset;

        for (var i = 0; i <= numSegmentsO; ++i)
        {
            points[i] = radiusO * (angleStart + i * angleIncrementO).ToDirection() + centerO;
        }

        var adj = numSegmentsO + 1;
        for (var i = 0; i <= numSegmentsI; ++i)
        {
            points[adj + i] = radiusI * (angleEnd - i * angleIncrementI).ToDirection() + centerO;
        }
        return points;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<WDir> DonutSectorL(WDir centerOffset, float innerRadius, float outerRadius, Angle angleStart, Angle angleEnd, float maxError)
    {
        return ArrayListWrapper<WDir>.Wrap(DonutSector(centerOffset, innerRadius, outerRadius, angleStart, angleEnd, maxError));
    }

    private static WDir PolarToCartesian(float r, Angle phi) => r * phi.ToDirection();
}
