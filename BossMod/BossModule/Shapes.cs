using Clipper2Lib;

namespace BossMod;

public abstract record class Shape
{
    public const float MaxApproxError = CurveApprox.ScreenError;
    public const float Half = 0.5f;
    protected WDir[]? Points;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new((List<RelPolygonWithHoles>)[new(Contour(center))]);
}

public sealed record class Circle(WPos Center, float Radius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        Points ??= CurveApprox.Circle(Radius, MaxApproxError);
        var len = Points.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] + offset);
        return result;
    }

    public override string ToString() => $"Circle:{Center.X},{Center.Z},{Radius}";
}

// for custom polygons defined by an IReadOnlyList of vertices
public sealed record class PolygonCustom(IReadOnlyList<WPos> Vertices) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        var vertices = Vertices;
        var count = vertices.Count;
        var result = new List<WDir>(count);
        for (var i = 0; i < count; ++i)
            result.Add(vertices[i] - center);
        return result;
    }

    public override string ToString()
    {
        var vertices = Vertices;
        var count = vertices.Count;
        var sb = new StringBuilder("PolygonCustom:", 14 + count * 9);
        for (var i = 0; i < count; ++i)
        {
            var vertex = vertices[i];
            sb.Append(vertex.X).Append(',').Append(vertex.Z).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

// for custom polygons defined by an IReadOnlyList of vertices with an offset, eg to account for hitbox radius
public sealed record class PolygonCustomO(IReadOnlyList<WPos> Vertices, float Offset) : Shape
{
    private Path64? path;

    public override List<WDir> Contour(WPos center)
    {
        if (path == null)
        {
            var originalPath = new Path64();
            var vertices = Vertices;
            var countV = vertices.Count;
            for (var i = 0; i < countV; ++i)
            {
                var v = vertices[i];
                originalPath.Add(new Point64((long)(v.X * PolygonClipper.Scale), (long)(v.Z * PolygonClipper.Scale)));
            }

            ClipperOffset co = new();
            co.AddPath(originalPath, JoinType.Miter, EndType.Polygon);
            var solution = new Paths64();
            co.Execute(Offset * PolygonClipper.Scale, solution);
            path = solution[0];
        }

        var count = path.Count;
        var offsetContour = new List<WDir>(count);

        for (var i = 0; i < count; ++i)
        {
            var p = path[i];
            offsetContour.Add(new WDir((float)(p.X * PolygonClipper.InvScale - center.X), (float)(p.Y * PolygonClipper.InvScale - center.Z)));
        }

        return offsetContour;
    }

    public override string ToString()
    {
        var vertices = Vertices;
        var count = vertices.Count;
        var sb = new StringBuilder("PolygonCustomO:", 15 + count * 9);
        for (var i = 0; i < count; ++i)
        {
            var vertex = vertices[i];
            sb.Append(vertex.X).Append(',').Append(vertex.Z).Append(';');
        }
        sb.Append("Offset:").Append(Offset);
        return sb.ToString();
    }
}

public sealed record class Donut(WPos Center, float InnerRadius, float OuterRadius) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        Points ??= CurveApprox.Donut(InnerRadius, OuterRadius, MaxApproxError);
        var len = Points.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Donut:{Center.X},{Center.Z},{InnerRadius},{OuterRadius}";
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
public record class Rectangle(WPos Center, float HalfWidth, float HalfHeight, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var dir = Rotation != default ? Rotation.ToDirection() : new(0, 1);
            var dx = dir.OrthoL() * HalfWidth;
            var dz = dir * HalfHeight;
            Points =
            [
                dx - dz,
                -dx - dz,
                -dx + dz,
                dx + dz
            ];
        }
        var offset = Center - center;
        var result = new List<WDir>(4);
        for (var i = 0; i < 4; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Rectangle:{Center.X},{Center.Z},{HalfWidth},{HalfHeight},{Rotation}";
}

// for rectangles defined by a start point, end point and halfwidth
public sealed record class RectangleSE(WPos Start, WPos End, float HalfWidth) : Rectangle(
    Center: new((Start.X + End.X) * Half, (Start.Z + End.Z) * Half),
    HalfWidth: HalfWidth,
    HalfHeight: (End - Start).Length() * Half,
    Rotation: new Angle(MathF.Atan2(End.X - Start.X, End.Z - Start.Z))
);

public sealed record class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

public sealed record class Cross(WPos Center, float Length, float HalfWidth, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var dx = Rotation.ToDirection();
            var dy = dx.OrthoL();
            var dx1 = dx * Length;
            var dx2 = dx * HalfWidth;
            var dy1 = dy * Length;
            var dy2 = dy * HalfWidth;

            Points =
            [
                dx1 + dy2,
                dx2 + dy2,
                dx2 + dy1,
                -dx2 + dy1,
                -dx2 + dy2,
                -dx1 + dy2,
                -dx1 - dy2,
                -dx2 - dy2,
                -dx2 - dy1,
                dx2 - dy1,
                dx2 - dy2,
                dx1 - dy2
            ];
        }
        var offset = Center - center;
        var result = new List<WDir>(12);
        for (var i = 0; i < 12; ++i)
            result.Add(Points[i] + offset);
        return result;
    }
    public override string ToString() => $"Cross:{Center.X},{Center.Z},{Length},{HalfWidth},{Rotation}";
}

// for polygons with edge count number of lines of symmetry, eg. pentagons, hexagons and octagons
public sealed record class Polygon(WPos Center, float Radius, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = Angle.DoublePI / Edges;
            var initialRotation = Rotation.Rad;
            var vertices = new WDir[Edges];
            for (var i = 0; i < Edges; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);
                vertices[i] = new(Center.X + Radius * sin, Center.Z + Radius * cos);
            }
            Points = vertices;
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }
    public override string ToString() => $"Polygon:{Center.X},{Center.Z},{Radius},{Edges},{Rotation}";
}

// for cones defined by radius, start angle and end angle
public record class Cone(WPos Center, float Radius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var points = CurveApprox.CircleSector(Center, Radius, StartAngle, EndAngle, MaxApproxError);
            var length = points.Length;
            var vertices = new WDir[length];
            for (var i = 0; i < length; ++i)
                vertices[i] = points[i] - new WPos();
            Points = vertices;
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }
    public override string ToString() => $"Cone:{Center.X},{Center.Z},{Radius},{StartAngle},{EndAngle}";
}

// for cones defined by radius, direction and half angle
public sealed record class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
public record class DonutSegment(WPos Center, float InnerRadius, float OuterRadius, Angle StartAngle, Angle EndAngle) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        Points ??= CurveApprox.DonutSector(InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError);
        var len = Points.Length;
        var result = new List<WDir>(len);
        var offset = Center - center;
        for (var i = 0; i < len; ++i)
        {
            result.Add(Points[i] + offset);
        }
        return result;
    }
    public override string ToString() => $"DonutSegment:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{StartAngle},{EndAngle}";
}

// for donut segments defined by inner and outer radius, direction and half angle
public sealed record class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);

// Approximates a cone with a customizable number of edges for the circle arc - with 1 edge this turns into a triangle, 2 edges result in a parallelogram
public sealed record class ConeV(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = 2 * HalfAngle.Rad / Edges;
            var startAngle = CenterDir.Rad - HalfAngle.Rad;
            var vertices = new WDir[Edges + 2];
            var centerX = Center.X;
            var CenterZ = Center.Z;
            var radius = Radius;
            for (var i = 0; i < Edges + 1; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
                vertices[i] = new(centerX + radius * sin, CenterZ + radius * cos);
            }
            vertices[Edges + 1] = Center - new WPos();
            Points = vertices;
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }

    public override string ToString() => $"ConeV:{Center.X},{Center.Z},{Radius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut segment with a customizable number of edges per circle arc
public sealed record class DonutSegmentV(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = 2 * HalfAngle.Rad / Edges;
            var startAngle = CenterDir.Rad - HalfAngle.Rad;
            var n = Edges + 1;
            var vertices = new WDir[2 * n];
            var centerX = Center.X;
            var CenterZ = Center.Z;
            var innerRadius = InnerRadius;
            var outerRadius = OuterRadius;
            for (var i = 0; i < n; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
                vertices[i] = new(centerX + outerRadius * sin, CenterZ + outerRadius * cos);
                vertices[2 * n - 1 - i] = new WDir(centerX + innerRadius * sin, CenterZ + innerRadius * cos);
            }

            Points = vertices;
        }
        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }

    public override string ToString() => $"DonutSegmentV:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut with a customizable number of edges per circle arc
public sealed record class DonutV(WPos Center, float InnerRadius, float OuterRadius, int Edges) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = Angle.DoublePI / Edges;
            var n = Edges + 1;
            var vertices = new WDir[2 * n];
            var centerX = Center.X;
            var CenterZ = Center.Z;
            var innerRadius = InnerRadius;
            var outerRadius = OuterRadius;
            for (var i = 0; i < n; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);
                vertices[i] = new(centerX + outerRadius * sin, CenterZ + outerRadius * cos);
                vertices[2 * n - 1 - i] = new(centerX + innerRadius * sin, CenterZ + innerRadius * cos);
            }
            Points = vertices;
        }

        var len = Points.Length;
        var result = new List<WDir>(len);
        for (var i = 0; i < len; ++i)
            result.Add(Points[i] - center);
        return result;
    }

    public override string ToString() => $"DonutV:{Center.X},{Center.Z},{InnerRadius},{OuterRadius},{Edges}";
}

// Approximates an ellipse with a customizable number of edges
public sealed record class Ellipse(WPos Center, float HalfWidth, float HalfHeight, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var angleIncrement = Angle.DoublePI / Edges;
            var (sinRotation, cosRotation) = ((float, float))Math.SinCos(Rotation.Rad);
            var vertices = new WDir[Edges];
            var halfWidth = HalfWidth;
            var halfHeight = HalfHeight;
            for (var i = 0; i < Edges; ++i)
            {
                var currentAngle = i * angleIncrement;
                var (sin, cos) = ((float, float))Math.SinCos(currentAngle);
                var x = halfWidth * cos;
                var y = halfHeight * sin;
                var rotatedX = x * cosRotation - y * sinRotation;
                var rotatedY = x * sinRotation + y * cosRotation;

                vertices[i] = new(rotatedX, rotatedY);
            }
            Points = vertices;
        }

        var len = Points.Length;
        var offset = Center - center;
        var result = new List<WDir>(len);

        for (var i = 0; i < len; ++i)
            result.Add(Points[i] + offset);

        return result;
    }

    public override string ToString() => $"Ellipse:{Center.X},{Center.Z},{HalfWidth},{HalfHeight},{Edges},{Rotation}";
}

// Capsule shape defined by center, halfheight, halfwidth (radius), rotation, and number of edges. in this case the halfheight is the distance from capsule center to semicircle centers,
// the edges are per semicircle
public sealed record class Capsule(WPos Center, float HalfHeight, float HalfWidth, int Edges, Angle Rotation = default) : Shape
{
    public override List<WDir> Contour(WPos center)
    {
        if (Points == null)
        {
            var vertices = new WDir[2 * Edges];
            var angleIncrement = MathF.PI / Edges;
            var (sinRot, cosRot) = ((float, float))Math.SinCos(Rotation.Rad);
            var halfWidth = HalfWidth;
            var halfHeight = HalfHeight;
            for (var i = 0; i < Edges; ++i)
            {
                var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);
                var halfWidthCos = halfWidth * cos;
                var halfWidthSin = halfWidth * sin + halfHeight;
                var rxTop = halfWidthCos * cosRot - halfWidthSin * sinRot;
                var ryTop = halfWidthCos * sinRot + halfWidthSin * cosRot;
                vertices[i] = new(rxTop, ryTop);
                var rxBot = -rxTop;
                var ryBot = -ryTop;
                vertices[Edges + i] = new(rxBot, ryBot);
            }
            Points = vertices;
        }

        var offset = Center - center;
        var result = new List<WDir>(Points.Length);
        for (var i = 0; i < Points.Length; ++i)
            result.Add(Points[i] + offset);

        return result;
    }

    public override string ToString() => $"Capsule:{Center.X},{Center.Z},{HalfHeight},{HalfWidth},{Rotation},{Edges}";
}
