namespace BossMod;

[SkipLocalsInit]
public abstract class Shape
{
    public const float MaxApproxError = CurveApprox.ScreenError;
    public const float Half = 0.5f;

    public abstract List<WDir> Contour(WPos center);

    public RelSimplifiedComplexPolygon ToPolygon(WPos center) => new((List<RelPolygonWithHoles>)[new(Contour(center))]);
}

[SkipLocalsInit]
public sealed class Circle(WPos center, float radius) : Shape
{
    public WPos Center = center;
    public float Radius = radius;

    public override List<WDir> Contour(WPos center)
    {
        return CurveApprox.CircleL(Center - center, Radius, MaxApproxError);
    }

    public override string ToString() => $"Circle:{Center},{Radius}";
}

// for custom polygons defined by an IReadOnlyList of vertices
[SkipLocalsInit]
public sealed class PolygonCustom(WPos[] vertices) : Shape
{
    public readonly WPos[] Vertices = vertices;

    public override List<WDir> Contour(WPos center)
    {
        var len = Vertices.Length;
        var result = new List<WDir>(len);
        CollectionsMarshal.SetCount(result, len);
        var vertices = CollectionsMarshal.AsSpan(result);

        for (var i = 0; i < len; ++i)
        {
            vertices[i] = Vertices[i] - center;
        }
        return result;
    }

    public override string ToString()
    {
        var len = Vertices.Length;
        var sb = new StringBuilder("PolygonCustom:", 14 + len * 9);
        for (var i = 0; i < len; ++i)
        {
            var vertex = Vertices[i];
            sb.Append(vertex).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

[SkipLocalsInit]
public sealed class PolygonCustomRel(WDir[] vertices) : Shape
{
    public readonly WDir[] Vertices = vertices;
    public override List<WDir> Contour(WPos center) => ArrayListWrapper<WDir>.Wrap(Vertices);

    public override string ToString()
    {
        var len = Vertices.Length;
        var sb = new StringBuilder("PolygonCustomRel:", 17 + len * 9);
        for (var i = 0; i < len; ++i)
        {
            var vertex = Vertices[i];
            sb.Append(vertex).Append(';');
        }
        --sb.Length;
        return sb.ToString();
    }
}

[SkipLocalsInit]
public sealed class Donut(WPos center, float innerRadius, float outerRadius) : Shape
{
    public WPos Center = center;
    public float InnerRadius = innerRadius;
    public float OuterRadius = outerRadius;

    public override List<WDir> Contour(WPos center)
    {
        return CurveApprox.DonutL(Center - center, InnerRadius, OuterRadius, MaxApproxError);
    }

    public override string ToString() => $"Donut:{Center},{InnerRadius},{OuterRadius}";
}

// for rectangles defined by a center, halfwidth, halfheight and optionally rotation
[SkipLocalsInit]
public class Rectangle(WPos center, float halfWidth, float halfHeight, Angle rotation = default) : Shape
{
    public WPos Center = center;
    public float HalfWidth = halfWidth;
    public float HalfHeight = halfHeight;
    public Angle Rotation = rotation;

    public override List<WDir> Contour(WPos center)
    {
        var dir = Rotation != default ? Rotation.ToDirection() : new(default, 1f);
        var dx = dir.OrthoL() * HalfWidth;
        var dz = dir * HalfHeight;

        var result = new List<WDir>(4);
        CollectionsMarshal.SetCount(result, 4);
        var vertices = CollectionsMarshal.AsSpan(result);
        var offset = Center - center;

        vertices[0] = dx - dz + offset;
        vertices[1] = -dx - dz + offset;
        vertices[2] = -dx + dz + offset;
        vertices[3] = dx + dz + offset;
        return result;
    }

    public override string ToString() => $"Rectangle:{Center},{HalfWidth},{HalfHeight},{Rotation}";
}

// for rectangles defined by a start point, end point and halfwidth
[SkipLocalsInit]
public sealed class RectangleSE(WPos Start, WPos End, float HalfWidth) : Rectangle(
    center: new((Start.X + End.X) * Half, (Start.Z + End.Z) * Half),
    halfWidth: HalfWidth,
    halfHeight: (End - Start).Length() * Half,
    rotation: new Angle(MathF.Atan2(End.X - Start.X, End.Z - Start.Z))
);

[SkipLocalsInit]
public sealed class Square(WPos Center, float HalfSize, Angle Rotation = default) : Rectangle(Center, HalfSize, HalfSize, Rotation);

[SkipLocalsInit]
public sealed class Cross(WPos center, float length, float halfWidth, Angle rotation = default) : Shape
{
    public WPos Center = center;
    public float Length = length;
    public float HalfWidth = halfWidth;
    public Angle Rotation = rotation;

    public override List<WDir> Contour(WPos center)
    {
        var dx = Rotation.ToDirection();
        var dy = dx.OrthoL();
        var dx1 = dx * Length;
        var dx2 = dx * HalfWidth;
        var dy1 = dy * Length;
        var dy2 = dy * HalfWidth;

        var result = new List<WDir>(12);
        CollectionsMarshal.SetCount(result, 12);
        var vertices = CollectionsMarshal.AsSpan(result);
        var offset = Center - center;

        vertices[0] = dx1 + dy2 + offset;
        vertices[1] = dx2 + dy2 + offset;
        vertices[2] = dx2 + dy1 + offset;
        vertices[3] = -dx2 + dy1 + offset;
        vertices[4] = -dx2 + dy2 + offset;
        vertices[5] = -dx1 + dy2 + offset;
        vertices[6] = -dx1 - dy2 + offset;
        vertices[7] = -dx2 - dy2 + offset;
        vertices[8] = -dx2 - dy1 + offset;
        vertices[9] = dx2 - dy1 + offset;
        vertices[10] = dx2 - dy2 + offset;
        vertices[11] = dx1 - dy2 + offset;

        return result;
    }

    public override string ToString() => $"Cross:{Center},{Length},{HalfWidth},{Rotation}";
}

// for polygons with edge count number of lines of symmetry, eg. pentagons, hexagons and octagons
[SkipLocalsInit]
public sealed class Polygon(WPos center, float radius, int edges, Angle rotation = default) : Shape
{
    public WPos Center = center;
    public float Radius = radius;
    public int Edges = edges;
    public Angle Rotation = rotation;

    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = Angle.DoublePI / edges;
        var initialRotation = Rotation.Rad;
        var radius = Radius;

        var result = new List<WDir>(edges);
        CollectionsMarshal.SetCount(result, edges);
        var vertices = CollectionsMarshal.AsSpan(result);

        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;

        for (var i = 0; i < edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);
            vertices[i] = new(radius * sin + offsetX, radius * cos + offsetZ);
        }
        return result;
    }

    public override string ToString() => $"Polygon:{Center},{Radius},{Edges},{Rotation}";
}

// for cones defined by radius, start angle and end angle
[SkipLocalsInit]
public class Cone(WPos center, float radius, Angle startAngle, Angle endAngle) : Shape
{
    public WPos Center = center;
    public float Radius = radius;
    public Angle StartAngle = startAngle;
    public Angle EndAngle = endAngle;

    public override List<WDir> Contour(WPos center)
    {
        return CurveApprox.CircleSectorL(Center - center, Radius, StartAngle, EndAngle, MaxApproxError);
    }

    public override string ToString() => $"Cone:{Center},{Radius},{StartAngle},{EndAngle}";
}

// for cones defined by radius, direction and half angle
[SkipLocalsInit]
public sealed class ConeHA(WPos Center, float Radius, Angle CenterDir, Angle HalfAngle) : Cone(Center, Radius, CenterDir - HalfAngle, CenterDir + HalfAngle);

// for donut segments defined by inner and outer radius, direction, start angle and end angle
[SkipLocalsInit]
public class DonutSegment(WPos center, float innerRadius, float outerRadius, Angle startAngle, Angle endAngle) : Shape
{
    public WPos Center = center;
    public float InnerRadius = innerRadius;
    public float OuterRadius = outerRadius;
    public Angle StartAngle = startAngle;
    public Angle EndAngle = endAngle;

    public override List<WDir> Contour(WPos center)
    {
        return CurveApprox.DonutSectorL(Center - center, InnerRadius, OuterRadius, StartAngle, EndAngle, MaxApproxError);
    }

    public override string ToString() => $"DonutSegment:{Center},{InnerRadius},{OuterRadius},{StartAngle},{EndAngle}";
}

// for donut segments defined by inner and outer radius, direction and half angle
[SkipLocalsInit]
public sealed class DonutSegmentHA(WPos Center, float InnerRadius, float OuterRadius, Angle CenterDir, Angle HalfAngle) : DonutSegment(Center, InnerRadius, OuterRadius,
CenterDir - HalfAngle, CenterDir + HalfAngle);

// Approximates a cone with a customizable number of edges for the circle arc - with 1 edge this turns into a triangle, 2 edges result in a parallelogram
[SkipLocalsInit]
public sealed class ConeV(WPos center, float radius, Angle centerDir, Angle halfAngle, int edges) : Shape
{
    public WPos Center = center;
    public float Radius = radius;
    public Angle CenterDir = centerDir;
    public Angle HalfAngle = halfAngle;
    public int Edges = edges;

    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = 2f * HalfAngle.Rad / edges;
        var startAngle = CenterDir.Rad - HalfAngle.Rad;

        var count = edges + 2;
        var result = new List<WDir>(count);
        CollectionsMarshal.SetCount(result, count);
        var vertices = CollectionsMarshal.AsSpan(result);

        var radius = Radius;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var e1 = edges + 1;

        for (var i = 0; i < e1; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            vertices[i] = new(radius * sin + offsetX, radius * cos + offsetZ);
        }
        vertices[e1] = offset;
        return result;
    }

    public override string ToString() => $"ConeV:{Center},{Radius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut segment with a customizable number of edges per circle arc
[SkipLocalsInit]
public sealed class DonutSegmentV(WPos center, float innerRadius, float outerRadius, Angle centerDir, Angle halfAngle, int edges) : Shape
{
    public WPos Center = center;
    public float InnerRadius = innerRadius;
    public float OuterRadius = outerRadius;
    public Angle CenterDir = centerDir;
    public Angle HalfAngle = halfAngle;
    public int Edges = edges;

    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = 2f * HalfAngle.Rad / edges;
        var startAngle = CenterDir.Rad - HalfAngle.Rad;
        var n = Edges + 1;
        var count = 2 * edges + 2;

        var result = new List<WDir>(count);
        CollectionsMarshal.SetCount(result, count);
        var vertices = CollectionsMarshal.AsSpan(result);
        var innerRadius = InnerRadius;
        var outerRadius = OuterRadius;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var indexInner = 2 * edges + 1;

        for (var i = 0; i < n; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(startAngle + i * angleIncrement);
            vertices[i] = new(outerRadius * sin + offsetX, outerRadius * cos + offsetZ);
            vertices[indexInner - i] = new(innerRadius * sin + offsetX, innerRadius * cos + offsetZ);
        }

        return result;
    }

    public override string ToString() => $"DonutSegmentV:{Center},{InnerRadius},{OuterRadius},{CenterDir},{HalfAngle},{Edges}";
}

// Approximates a donut with a customizable number of edges per circle arc
[SkipLocalsInit]
public sealed class DonutV(WPos center, float innerRadius, float outerRadius, int edges, Angle rotation = default) : Shape
{
    public WPos Center = center;
    public float InnerRadius = innerRadius;
    public float OuterRadius = outerRadius;
    public int Edges = edges;
    public Angle Rotation = rotation;

    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = Angle.DoublePI / edges;

        var count = 2 * edges + 2;
        var result = new List<WDir>(count);
        CollectionsMarshal.SetCount(result, count);
        var vertices = CollectionsMarshal.AsSpan(result);
        var initialRotation = Rotation.Rad;
        var innerRadius = InnerRadius;
        var outerRadius = OuterRadius;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;
        var indexInner = 2 * edges + 1;

        for (var i = 0; i < edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement + initialRotation);

            vertices[i] = new(outerRadius * sin + offsetX, outerRadius * cos + offsetZ);
            vertices[indexInner - i] = new(innerRadius * sin + offsetX, innerRadius * cos + offsetZ);
        }
        // ensure closed polygons, copy first vertices of each ring
        vertices[edges] = vertices[0];
        vertices[edges + 1] = vertices[indexInner];
        return result;
    }

    public override string ToString() => $"DonutV:{Center},{InnerRadius},{OuterRadius},{Edges}";
}

// Approximates an ellipse with a customizable number of edges
[SkipLocalsInit]
public sealed class Ellipse(WPos center, float halfWidth, float halfHeight, int edges, Angle rotation = default) : Shape
{
    public WPos Center = center;
    public float HalfWidth = halfWidth;
    public float HalfHeight = halfHeight;
    public int Edges = edges;
    public Angle Rotation = rotation;

    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var angleIncrement = Angle.DoublePI / edges;
        var (sinRotation, cosRotation) = ((float, float))Math.SinCos(Rotation.Rad);
        var result = new List<WDir>(edges);
        CollectionsMarshal.SetCount(result, edges);
        var vertices = CollectionsMarshal.AsSpan(result);
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;

        for (var i = 0; i < edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);
            var x = halfWidth * cos;
            var y = halfHeight * sin;
            vertices[i] = new(x * cosRotation - y * sinRotation + offsetX, x * sinRotation + y * cosRotation + offsetZ);
        }
        return result;
    }

    public override string ToString() => $"Ellipse:{Center},{HalfWidth},{HalfHeight},{Edges},{Rotation}";
}

// Capsule shape defined by center, halfheight, halfwidth (radius), rotation, and number of edges. in this case the halfheight is the distance from capsule center to semicircle centers,
// the edges are per semicircle
[SkipLocalsInit]
public sealed class Capsule(WPos center, float halfHeight, float halfWidth, int edges, Angle rotation = default) : Shape
{
    public WPos Center = center;
    public float HalfWidth = halfWidth;
    public float HalfHeight = halfHeight;
    public int Edges = edges;
    public Angle Rotation = rotation;

    public override List<WDir> Contour(WPos center)
    {
        var edges = Edges;
        var edgeAdj = edges + 1;
        var count = 2 * (edges + 1);
        var result = new List<WDir>(count);

        CollectionsMarshal.SetCount(result, count);
        var vertices = CollectionsMarshal.AsSpan(result);

        var angleIncrement = MathF.PI / edges;
        var (sinRot, cosRot) = ((float, float))Math.SinCos(Rotation.Rad);
        var offset = Center - center;
        var offsetX = offset.X;
        var offsetZ = offset.Z;

        for (var i = 0; i <= edges; ++i)
        {
            var (sin, cos) = ((float, float))Math.SinCos(i * angleIncrement);

            var x = HalfWidth * cos;
            var z = HalfWidth * sin + HalfHeight;

            var rx = x * cosRot - z * sinRot;
            var rz = x * sinRot + z * cosRot;

            // Upper semicircle
            vertices[i] = new(rx + offsetX, rz + offsetZ);

            // Lower semicircle
            vertices[edgeAdj + i] = new(-rx + offsetX, -rz + offsetZ);
        }

        return result;
    }

    public override string ToString() => $"Capsule:{Center},{HalfHeight},{HalfWidth},{Rotation},{Edges}";
}
