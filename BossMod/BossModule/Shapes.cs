namespace BossMod;

public abstract record class Shape
{
    public const float MaxApproxError = CurveApprox.ScreenError;
    public const float Half = 0.5f;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new((List<RelPolygonWithHoles>)[new(Contour(center))]);
}

public record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.Circle(Radius, MaxApproxError).Select(p => p + (Center - center)).ToList();
    public override string ToString() => $"{nameof(Circle)}:{Center.X},{Center.Z},{Radius}";
}

// for custom polygons, automatically checking if convex or concave
public record class PolygonCustom(IEnumerable<WPos> Vertices) : Shape
{
    public override List<WDir> Contour(WPos center) => Vertices.Select(v => v - center).ToList();
    public override string ToString() => $"{nameof(PolygonCustom)}:{string.Join(",", Vertices.Select(v => $"{v.X},{v.Z}"))}";
}

public record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError).Select(p => p + (Center - center)).ToList();
    public override string ToString() => $"{nameof(Donut)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius}";
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad); // calculating as double and converting to float on purpose, otherwise error gets visibly magnified by polygon operations
        var rotatedHalfWidth = new WDir(HalfWidth * cos, HalfWidth * sin);
        var rotatedHalfHeight = new WDir(-HalfHeight * sin, HalfHeight * cos);
        var offset = Center - center;

        return
        [
            rotatedHalfWidth + rotatedHalfHeight + offset,
            rotatedHalfWidth - rotatedHalfHeight + offset,
            -rotatedHalfWidth - rotatedHalfHeight + offset,
            -rotatedHalfWidth + rotatedHalfHeight + offset
        ];
    }
    public override string ToString() => $"{nameof(Rectangle)}:{Center.X},{Center.Z},{HalfWidth},{HalfHeight},{Rotation}";
}

// for rectangles defined by a start point, end point and halfwidth
public record class RectangleSE(WPos Start, WPos End, float HalfWidth) : Rectangle(
    Center: new((Start.X + End.X) * Half, (Start.Z + End.Z) * Half),
    HalfWidth: HalfWidth,
    HalfHeight: (End - Start).Length() * Half,
    Rotation: new Angle(MathF.Atan2(End.Z - Start.Z, End.X - Start.X)) + 90.Degrees()
);

public record class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

public record class Cross(WPos Center, float Length, float HalfWidth, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad); // calculating as double and converting to float on purpose, otherwise error gets visibly magnified by polygon operations
        var dx = new WDir(sin, cos);
        var dy = dx.OrthoL();
        var dx1 = dx * Length;
        var dx2 = dx * HalfWidth;
        var dy1 = dy * Length;
        var dy2 = dy * HalfWidth;
        var offset = Center - center;

        return
        [
            dx1 + dy2 + offset,
            dx2 + dy2 + offset,
            dx2 + dy1 + offset,
            -dx2 + dy1 + offset,
            -dx2 + dy2 + offset,
            -dx1 + dy2 + offset,
            -dx1 - dy2 + offset,
            -dx2 - dy2 + offset,
            -dx2 - dy1 + offset,
            dx2 - dy1 + offset,
            dx2 - dy2 + offset,
            dx1 - dy2 + offset
        ];
    }
    public override string ToString() => $"{nameof(Cross)}:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation}";
}

// Equilateral triangle defined by center, sidelength and rotation
public record class TriangleE(WPos Center, float SideLength, Angle Rotation = default) : Shape
{
    private static readonly float heightFactor = MathF.Sqrt(3) * Half;
    public override List<WDir> Contour(WPos center)
    {
        var height = SideLength * heightFactor;
        var halfSideLength = SideLength * Half;
        var halfHeight = height * Half;
        var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad); // calculating as double and converting to float on purpose, otherwise error gets visibly magnified by polygon operations
        var offset = Center - center;
        return
        [
            new WDir(halfSideLength * cos - halfHeight * sin, halfSideLength * sin + halfHeight * cos) + offset,
            new WDir(-halfSideLength * cos - halfHeight * sin, -halfSideLength * sin + halfHeight * cos) + offset,
            new WDir(halfHeight * sin, -halfHeight * cos) + offset
        ];
    }
    public override string ToString() => $"{nameof(TriangleE)}:{Center.X},{Center.Z},{SideLength},{Rotation}";
}

// for polygons defined by a radius and n amount of vertices
public record class Polygon(WPos Center, float Radius, int Vertices, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = 2 * MathF.PI / Vertices;
        var initialRotation = Rotation.Rad;
        var vertices = new List<WDir>(Vertices);
        for (var i = Vertices - 1; i >= 0; i--)
        {
            var angle = i * angleIncrement + initialRotation;
            var (sin, cos) = ((float, float))Math.SinCos(angle); // calculating as double and converting to float on purpose, otherwise error gets visibly magnified by polygon operations
            var x = Center.X + Radius * cos;
            var z = Center.Z + Radius * sin;
            vertices.Add(new(x - center.X, z - center.Z));
        }

        return vertices;
    }
    public override string ToString() => $"{nameof(Polygon)}:{Center.X},{Center.Z},{Radius},{Vertices},{Rotation}";
}

// for cones defined by radius, start angle and end angle
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError).Select(p => p - center).ToList();
    public override string ToString() => $"{nameof(Cone)}:{Center.X},{Center.Z},{Radius},{StartAngle},{EndAngle}";
}

// for cones defined by radius, direction and half angle
public record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center) => CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError).Select(p => p + (Center - center)).ToList();
    public override string ToString() => $"{nameof(DonutSegment)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle},{EndAngle}";
}

// for donut segments defined by inner and outer radius, direction and half angle
public record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);

// Approximates a cone with a customizable number of vertices
public record class ConeV(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle, int Vertices) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = 2 * HalfAngle.Rad / (Vertices - 1);
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var vertices = new List<WDir>(Vertices);

        for (var i = 0; i < Vertices; i++)
        {
            var angle = startAngle + i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            var x = Center.X + Radius * cos;
            var z = Center.Z + Radius * sin;
            vertices.Add(new(x - center.X, z - center.Z));
        }

        vertices.Add(new(Center.X - center.X, Center.Z - center.Z));
        return vertices;
    }

    public override string ToString() => $"{nameof(ConeV)}:{Center.X},{Center.Z},{Radius},{CenterDir},{HalfAngle},{Vertices}";
}

// Approximates a donut segment with a customizable number of vertices
public record class DonutSegmentV(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle, int Vertices) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = 2 * HalfAngle.Rad / (Vertices - 1);
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var contourVertices = new List<WDir>();

        for (var i = Vertices - 1; i >= 0; i--)
        {
            var angle = startAngle + i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            var x = Center.X + OuterRadius * cos;
            var z = Center.Z + OuterRadius * sin;
            contourVertices.Add(new(x - center.X, z - center.Z));
        }

        for (var i = 0; i < Vertices; i++)
        {
            var angle = startAngle + i * angleIncrement;
            var (sin, cos) = ((float, float))Math.SinCos(angle);
            var x = Center.X + InnerRadius * cos;
            var z = Center.Z + InnerRadius * sin;
            contourVertices.Add(new(x - center.X, z - center.Z));
        }

        return contourVertices;
    }

    public override string ToString() => $"{nameof(DonutSegmentV)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{CenterDir},{HalfAngle},{Vertices}";
}
