using Clipper2Lib;

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

// for custom polygons defined by an IEnumerable of vertices
public record class PolygonCustom(IEnumerable<WPos> Vertices) : Shape
{
    public override List<WDir> Contour(WPos center) => Vertices.Select(v => v - center).ToList();
    public override string ToString() => $"{nameof(PolygonCustom)}:{string.Join(",", Vertices.Select(v => $"{v.X},{v.Z}"))}";
}

// for custom polygons defined by an IEnumerable of vertices with an offset, eg to account for hitbox radius
public record class PolygonCustomO(IEnumerable<WPos> Vertices, float Offset) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var originalPath = new Path64(Vertices.Select(v => new Point64((long)(v.X * PolygonClipper.Scale), (long)(v.Z * PolygonClipper.Scale))));
        ClipperOffset co = new();
        co.AddPath(originalPath, JoinType.Miter, EndType.Polygon);
        Paths64 solution = [];
        co.Execute(Offset * PolygonClipper.Scale, solution);
        var offsetPath = solution[0];
        var offsetContour = offsetPath.Select(p => new WDir((float)(p.X * PolygonClipper.InvScale - center.X), (float)(p.Y * PolygonClipper.InvScale - center.Z))).ToList();
        return offsetContour;
    }

    public override string ToString() => $"{nameof(PolygonCustomO)}:{string.Join(",", Vertices.Select(v => $"{v.X},{v.Z}"))},{Offset}";
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
        var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad);
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
        var dx = Rotation.ToDirection();
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
        var (sin, cos) = ((float, float))Math.SinCos(Rotation.Rad);
        var offset = Center - center;
        var halfSideCos = halfSideLength * cos;
        var halfSideSin = halfSideLength * sin;
        var halfHeightSin = halfHeight * sin;
        var halfHeightCos = halfHeight * cos;
        return
        [
            new WDir(halfSideCos - halfHeightSin, halfSideSin + halfHeightCos) + offset,
            new WDir(-halfSideCos - halfHeightSin, -halfSideSin + halfHeightCos) + offset,
            new WDir(halfHeightSin, -halfHeight * cos) + offset
        ];
    }
    public override string ToString() => $"{nameof(TriangleE)}:{Center.X},{Center.Z},{SideLength},{Rotation}";
}

// for polygons with edge count number of lines of symmetry, eg. pentagons, hexagons and octagons
public record class Polygon(WPos Center, float Radius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = Angle.DoublePI / Edges;
        var initialRotation = Rotation.Rad;
        var vertices = new List<WDir>(Edges);
        for (var i = Edges - 1; i >= 0; --i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);
            vertices.Add(new(Center.X + Radius * cos - center.X, Center.Z + Radius * sin - center.Z));
        }

        return vertices;
    }
    public override string ToString() => $"{nameof(Polygon)}:{Center.X},{Center.Z},{Radius},{Edges},{Rotation}";
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

// Approximates a cone with a customizable number of edges for the circle arc
public record class ConeV(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = 2 * HalfAngle.Rad / Edges;
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var vertices = new List<WDir>(Edges);

        for (var i = 0; i < Edges + 1; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            vertices.Add(new(Center.X + Radius * cos - center.X, Center.Z + Radius * sin - center.Z));
        }

        vertices.Add(new(Center.X - center.X, Center.Z - center.Z));
        return vertices;
    }

    public override string ToString() => $"{nameof(ConeV)}:{Center.X},{Center.Z},{Radius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut segment with a customizable number of edges per circle arc
public record class DonutSegmentV(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var angleIncrement = 2 * HalfAngle.Rad / Edges;
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var contourVertices = new List<WDir>();

        for (var i = Edges; i >= 0; --i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            contourVertices.Add(new(Center.X + OuterRadius * cos - center.X, Center.Z + OuterRadius * sin - center.Z));
        }

        for (var i = 0; i < Edges + 1; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            contourVertices.Add(new(Center.X + InnerRadius * cos - center.X, Center.Z + InnerRadius * sin - center.Z));
        }

        return contourVertices;
    }

    public override string ToString() => $"{nameof(DonutSegmentV)}:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{CenterDir},{HalfAngle},{Edges}";
}
