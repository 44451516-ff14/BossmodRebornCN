using Clipper2Lib;
using EarcutNet;
using System.Buffers;

// currently we use Clipper2 library (based on Vatti algorithm) for boolean operations and Earcut.net library (earcutting) for triangulating
// note: the major user of these primitives is bounds clipper; since they operate in 'local' coordinates, we use WDir everywhere (offsets from center) and call that 'relative polygons' - i'm not quite happy with that, it's not very intuitive
namespace BossMod;

// a triangle; as basic as it gets
public readonly record struct RelTriangle(WDir A, WDir B, WDir C);

// a complex polygon that is a single simple-polygon exterior minus 0 or more simple-polygon holes; all edges are assumed to be non intersecting
// hole-starts list contains starting index of each hole
public record class RelPolygonWithHoles(List<WDir> Vertices, List<int> HoleStarts)
{
    // constructor for simple polygon
    public RelPolygonWithHoles(List<WDir> simpleVertices) : this(simpleVertices, []) { }
    public ReadOnlySpan<WDir> AllVertices => Vertices.AsSpan();
    public ReadOnlySpan<WDir> Exterior => AllVertices[..ExteriorEnd];
    public ReadOnlySpan<WDir> Interior(int index) => AllVertices[HoleStarts[index]..HoleEnd(index)];
    public IEnumerable<int> Holes => Enumerable.Range(0, HoleStarts.Count);
    public IEnumerable<(WDir, WDir)> ExteriorEdges => PolygonUtil.EnumerateEdges(Vertices.Take(ExteriorEnd));
    public IEnumerable<(WDir, WDir)> InteriorEdges(int index) => PolygonUtil.EnumerateEdges(Vertices.Skip(HoleStarts[index]).Take(HoleEnd(index) - HoleStarts[index]));

    private static readonly ArrayPool<double> _doublePool = ArrayPool<double>.Shared;
    private ContourEdgeBuckets? _exteriorEdgeBuckets;
    private List<ContourEdgeBuckets> _holeEdgeBuckets = [];
    private const int BucketCount = 10;

    private int ExteriorEnd => HoleStarts.Count > 0 ? HoleStarts[0] : Vertices.Count;
    private int HoleEnd(int index) => index + 1 < HoleStarts.Count ? HoleStarts[index + 1] : Vertices.Count;

    // add new hole; input is assumed to be a simple polygon
    public void AddHole(ReadOnlySpan<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }
    public void AddHole(IEnumerable<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }

    // build a triangulation of the polygon
    public bool Triangulate(List<RelTriangle> result)
    {
        var vertexCount = Vertices.Count;
        var pts = _doublePool.Rent(vertexCount * 2);
        try
        {
            for (int i = 0, j = 0; i < vertexCount; i++, j += 2)
            {
                var v = Vertices[i];
                pts[j] = v.X;
                pts[j + 1] = v.Z;
            }

            var tess = Earcut.Tessellate(new Span<double>(pts, 0, vertexCount * 2), HoleStarts);
            for (var i = 0; i < tess.Count; i += 3)
            {
                result.Add(new(Vertices[tess[i]], Vertices[tess[i + 1]], Vertices[tess[i + 2]]));
            }
            return tess.Count > 0;
        }
        finally
        {
            _doublePool.Return(pts, true);
        }
    }
    public List<RelTriangle> Triangulate()
    {
        List<RelTriangle> result = [];
        Triangulate(result);
        return result;
    }

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p)
    {
        if (_exteriorEdgeBuckets == null)
        {
            _exteriorEdgeBuckets = BuildEdgeBucketsForContour(Exterior);
            if (HoleStarts.Count > 0)
            {
                _holeEdgeBuckets = new List<ContourEdgeBuckets>(HoleStarts.Count);
                for (var i = 0; i < HoleStarts.Count; i++)
                {
                    _holeEdgeBuckets.Add(BuildEdgeBucketsForContour(Interior(i)));
                }
            }
        }

        if (!InSimplePolygon(p, _exteriorEdgeBuckets!))
            return false;
        for (var i = 0; i < _holeEdgeBuckets.Count; i++)
        {
            if (InSimplePolygon(p, _holeEdgeBuckets[i]))
                return false;
        }
        return true;
    }

    private static bool InSimplePolygon(WDir p, ContourEdgeBuckets buckets)
    {
        float x = p.X, y = p.Z;
        var bucketIndex = (int)((y - buckets.MinY) * buckets.InvBucketHeight);
        if ((uint)bucketIndex >= BucketCount)
            return false;
        var edges = buckets.EdgeBuckets[bucketIndex];
        var inside = false;
        for (var i = 0; i < edges.Length; ++i)
        {
            var edge = edges[i];
            if ((edge.y0 > y) != (edge.y1 > y) && x < edge.x0 + edge.slopeX * (y - edge.y0))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private static ContourEdgeBuckets BuildEdgeBucketsForContour(ReadOnlySpan<WDir> contour)
    {
        float minY = float.MaxValue, maxY = float.MinValue;
        var count = contour.Length;

        for (var i = 0; i < count; i++)
        {
            var y = contour[i].Z;
            if (y < minY)
                minY = y;
            if (y > maxY)
                maxY = y;
        }

        var invBucketHeight = BucketCount / (maxY - minY + 1e-8f);

        var buckets = new List<Edges>[BucketCount];
        for (var i = 0; i < BucketCount; i++)
        {
            buckets[i] = [];
        }

        var prev = contour[^1];
        for (var i = 0; i < count; i++)
        {
            var curr = contour[i];
            var edge = new Edges(prev.X, prev.Z, curr.X, curr.Z);

            var edgeMinY = Math.Min(edge.y0, edge.y1);
            var edgeMaxY = Math.Max(edge.y0, edge.y1);

            var bucketStart = (int)((edgeMinY - minY) * invBucketHeight);
            var bucketEnd = (int)((edgeMaxY - minY) * invBucketHeight);

            bucketStart = Math.Clamp(bucketStart, 0, BucketCount - 1);
            bucketEnd = Math.Clamp(bucketEnd, 0, BucketCount - 1);

            for (var b = bucketStart; b <= bucketEnd; b++)
            {
                buckets[b].Add(edge);
            }

            prev = curr;
        }

        var edgeBuckets = new Edges[BucketCount][];
        for (var i = 0; i < BucketCount; i++)
        {
            edgeBuckets[i] = [.. buckets[i]];
        }

        return new ContourEdgeBuckets(edgeBuckets, minY, invBucketHeight);
    }

    private readonly struct Edges
    {
        public readonly float x0, y0, x1, y1, slopeX;

        public Edges(float ax, float ay, float bx, float by)
        {
            x0 = ax;
            y0 = ay;
            x1 = bx;
            y1 = by;
            var dy = by - ay;
            var invDy = dy != 0 ? 1 / dy : 0;
            slopeX = (x1 - x0) * invDy;
        }
    }

    private sealed class ContourEdgeBuckets(Edges[][] edgeBuckets, float minY, float invBucketHeight)
    {
        public readonly Edges[][] EdgeBuckets = edgeBuckets;
        public readonly float MinY = minY, InvBucketHeight = invBucketHeight;
    }
}

// generic 'simplified' complex polygon that consists of 0 or more non-intersecting polygons with holes (note however that some polygons could be fully inside other polygon's hole)
public record class RelSimplifiedComplexPolygon(List<RelPolygonWithHoles> Parts)
{
    public RelSimplifiedComplexPolygon() : this(new List<RelPolygonWithHoles>()) { }

    // constructors for simple polygon
    public RelSimplifiedComplexPolygon(List<WDir> simpleVertices) : this([new RelPolygonWithHoles(simpleVertices)]) { }
    public RelSimplifiedComplexPolygon(IEnumerable<WDir> simpleVertices) : this([new RelPolygonWithHoles([.. simpleVertices])]) { }

    // build a triangulation of the polygon
    public List<RelTriangle> Triangulate()
    {
        List<RelTriangle> result = [];
        for (var i = 0; i < Parts.Count; ++i)
            Parts[i].Triangulate(result);
        return result;
    }

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p)
    {
        for (var i = 0; i < Parts.Count; ++i)
            if (Parts[i].Contains(p))
                return true;
        return false;
    }

    // positive offsets inflate, negative shrink polygon
    public RelSimplifiedComplexPolygon Offset(float offset)
    {
        var clipperOffset = new ClipperOffset();
        var allPaths = new Paths64();

        foreach (var part in Parts)
        {
            allPaths.Add(ToPath64(part.Exterior));
            foreach (var i in part.Holes)
                allPaths.Add(ToPath64(part.Interior(i)));
        }

        var solution = new Paths64();
        clipperOffset.AddPaths(allPaths, JoinType.Miter, EndType.Polygon);
        clipperOffset.Execute(offset * PolygonClipper.Scale, solution);

        var result = new RelSimplifiedComplexPolygon();
        BuildResultFromPaths(result, solution);
        return result;
    }

    private void BuildResultFromPaths(RelSimplifiedComplexPolygon result, Paths64 paths)
    {
        var c = new Clipper64();
        c.AddPaths(paths, PathType.Subject);
        var tree = new PolyTree64();
        c.Execute(ClipType.Union, FillRule.NonZero, tree);

        PolygonClipper.BuildResult(result, tree);
    }

    private static Path64 ToPath64(ReadOnlySpan<WDir> vertices)
    {
        var count = vertices.Length;
        var path = new Path64(count);
        for (var i = 0; i < count; i++)
        {
            var vertex = vertices[i];
            path.Add(new(vertex.X * PolygonClipper.Scale, vertex.Z * PolygonClipper.Scale));
        }
        return path;
    }
}

// utility for simplifying and performing boolean operations on complex polygons
public class PolygonClipper
{
    public const float Scale = 1024 * 1024; // note: we need at least 10 bits for integer part (-1024 to 1024 range); using 11 bits leaves 20 bits for fractional part; power-of-two scale should reduce rounding issues
    public const float InvScale = 1 / Scale;

    // reusable representation of the complex polygon ready for boolean operations
    public record class Operand
    {
        public Operand() { }
        public Operand(ReadOnlySpan<WDir> contour, bool isOpen = false) => AddContour(contour, isOpen);
        public Operand(IEnumerable<WDir> contour, bool isOpen = false) => AddContour(contour, isOpen);
        public Operand(RelPolygonWithHoles polygon) => AddPolygon(polygon);
        public Operand(RelSimplifiedComplexPolygon polygon) => AddPolygon(polygon);

        private readonly ReuseableDataContainer64 _data = new();

        public void Clear() => _data.Clear();

        public void AddContour(ReadOnlySpan<WDir> contour, bool isOpen = false)
        {
            Path64 path = new(contour.Length);
            foreach (var p in contour)
                path.Add(ConvertPoint(p));
            AddContour(path, isOpen);
        }

        public void AddContour(IEnumerable<WDir> contour, bool isOpen = false) => AddContour([.. contour.Select(ConvertPoint)], isOpen);

        public void AddPolygon(RelPolygonWithHoles polygon)
        {
            AddContour(polygon.Exterior);
            foreach (var i in polygon.Holes)
                AddContour(polygon.Interior(i));
        }

        public void AddPolygon(RelSimplifiedComplexPolygon polygon) => polygon.Parts.ForEach(AddPolygon);

        public void Assign(Clipper64 clipper, PathType role) => clipper.AddReuseableData(_data, role);

        private void AddContour(Path64 contour, bool isOpen) => _data.AddPaths([contour], PathType.Subject, isOpen);
    }

    private readonly Clipper64 _clipper = new() { PreserveCollinear = false };

    public RelSimplifiedComplexPolygon Simplify(Operand poly, FillRule fillRule = FillRule.NonZero)
    {
        poly.Assign(_clipper, PathType.Subject);
        return Execute(ClipType.Union, fillRule);
    }

    public RelSimplifiedComplexPolygon Intersect(Operand p1, Operand p2, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Intersection, fillRule, p1, p2);
    public RelSimplifiedComplexPolygon Union(Operand p1, Operand p2, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Union, fillRule, p1, p2);
    public RelSimplifiedComplexPolygon Difference(Operand starting, Operand remove, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Difference, fillRule, starting, remove);
    public RelSimplifiedComplexPolygon Xor(Operand p1, Operand p2, FillRule fillRule = FillRule.NonZero) => Execute(ClipType.Xor, fillRule, p1, p2);

    private RelSimplifiedComplexPolygon Execute(ClipType operation, FillRule fillRule, Operand subject, Operand clip)
    {
        subject.Assign(_clipper, PathType.Subject);
        clip.Assign(_clipper, PathType.Clip);
        return Execute(operation, fillRule);
    }

    private RelSimplifiedComplexPolygon Execute(ClipType operation, FillRule fillRule)
    {
        var solution = new PolyTree64();
        _clipper.Execute(operation, fillRule, solution);
        _clipper.Clear();

        var result = new RelSimplifiedComplexPolygon();
        BuildResult(result, solution);
        return result;
    }

    public static void BuildResult(RelSimplifiedComplexPolygon result, PolyPath64 parent)
    {
        for (var i = 0; i < parent.Count; ++i)
        {
            var exterior = parent[i];
            if (exterior.Polygon == null || exterior.Polygon.Count == 0)
                continue;
            var polygonPoints = new List<WDir>(exterior.Polygon.Count);
            var extPolygon = exterior.Polygon;
            for (var j = 0; j < extPolygon.Count; j++)
                polygonPoints.Add(ConvertPoint(extPolygon[j]));

            var poly = new RelPolygonWithHoles(polygonPoints);
            result.Parts.Add(poly);

            for (var j = 0; j < exterior.Count; ++j)
            {
                var interior = exterior[j];
                if (interior.Polygon == null || interior.Polygon.Count == 0)
                    continue;
                var holePoints = new List<WDir>(interior.Polygon.Count);
                var intPolygon = interior.Polygon;
                for (var k = 0; k < intPolygon.Count; k++)
                    holePoints.Add(ConvertPoint(intPolygon[k]));

                poly.AddHole(holePoints);
                BuildResult(result, interior);
            }
        }
    }

    private static Point64 ConvertPoint(WDir pt) => new(pt.X * Scale, pt.Z * Scale);
    private static WDir ConvertPoint(Point64 pt) => new(pt.X * InvScale, pt.Y * InvScale);
}

public static class PolygonUtil
{
    public static IEnumerable<(T, T)> EnumerateEdges<T>(IEnumerable<T> contour) where T : struct, IEquatable<T>
    {
        var contourList = contour as IList<T> ?? contour.ToArray();
        var count = contourList.Count;
        if (count == 0)
            yield break;

        var prevPoint = contourList[count - 1];
        foreach (var currentPoint in contourList)
        {
            yield return (prevPoint, currentPoint);
            prevPoint = currentPoint;
        }
    }
}

public readonly struct Edge(float ax, float ay, float dx, float dy)
{
    public readonly float Ax = ax, Ay = ay, Dx = dx, Dy = dy, InvLengthSq = 1 / (dx * dx + dy * dy + 1e-8f);
}

public class SpatialIndex
{
    private readonly List<int>[] _grid;
    private readonly Edge[] _edges;
    private readonly int _minX, _minY, _gridWidth, _gridHeight;
    public SpatialIndex(Edge[] edges)
    {
        _edges = edges;
        ComputeGridBounds(out _minX, out _minY, out _gridWidth, out _gridHeight);
        _grid = new List<int>[_gridWidth * _gridHeight];
        BuildIndex();
    }

    private void ComputeGridBounds(out int minX, out int minY, out int gridWidth, out int gridHeight)
    {
        minX = minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (var edge in _edges)
        {
            var ex0 = (int)MathF.Floor(Math.Min(edge.Ax, edge.Ax + edge.Dx));
            var ex1 = (int)MathF.Floor(Math.Max(edge.Ax, edge.Ax + edge.Dx));
            var ey0 = (int)MathF.Floor(Math.Min(edge.Ay, edge.Ay + edge.Dy));
            var ey1 = (int)MathF.Floor(Math.Max(edge.Ay, edge.Ay + edge.Dy));

            minX = Math.Min(minX, ex0);
            minY = Math.Min(minY, ey0);
            maxX = Math.Max(maxX, ex1);
            maxY = Math.Max(maxY, ey1);
        }

        gridWidth = maxX - minX + 1;
        gridHeight = maxY - minY + 1;
    }

    private void BuildIndex()
    {
        for (var i = 0; i < _grid.Length; i++)
        {
            _grid[i] = [];
        }

        for (var i = 0; i < _edges.Length; i++)
        {
            var edge = _edges[i];
            var minX = Math.Min(edge.Ax, edge.Ax + edge.Dx);
            var maxX = Math.Max(edge.Ax, edge.Ax + edge.Dx);
            var minY = Math.Min(edge.Ay, edge.Ay + edge.Dy);
            var maxY = Math.Max(edge.Ay, edge.Ay + edge.Dy);

            var x0 = (int)MathF.Floor(minX) - _minX;
            var x1 = (int)MathF.Floor(maxX) - _minX;
            var y0 = (int)MathF.Floor(minY) - _minY;
            var y1 = (int)MathF.Floor(maxY) - _minY;

            for (var x = x0; x <= x1; x++)
            {
                for (var y = y0; y <= y1; y++)
                {
                    var index = y * _gridWidth + x;
                    if (_grid[index] == null)
                    {
                        _grid[index] = [];
                    }
                    _grid[index].Add(i);
                }
            }
        }
    }

    public List<int> Query(float px, float py)
    {
        var cellX = (int)MathF.Floor(px) - _minX;
        var cellY = (int)MathF.Floor(py) - _minY;

        return (uint)cellX >= _gridWidth || (uint)cellY >= _gridHeight ? [] : _grid[cellY * _gridWidth + cellX] ?? [];
    }
}

public readonly struct PolygonWithHolesDistanceFunction
{
    private readonly RelSimplifiedComplexPolygon _polygon;
    private readonly WPos _origin;
    private readonly Edge[] _edges;
    private readonly SpatialIndex _spatialIndex;

    public PolygonWithHolesDistanceFunction(WPos origin, RelSimplifiedComplexPolygon polygon)
    {
        _origin = origin;
        _polygon = polygon;

        List<Edge> edges = [];
        foreach (var part in polygon.Parts)
        {
            AddEdgesFromPart(part, origin, edges);
        }
        _edges = [.. edges];
        _spatialIndex = new(_edges);
    }

    public readonly float Distance(WPos p)
    {
        var localPoint = new WDir(p.X - _origin.X, p.Z - _origin.Z);
        var isInside = _polygon.Contains(localPoint);

        var minDistanceSq = float.MaxValue;
        foreach (var i in _spatialIndex.Query(p.X, p.Z))
        {
            var edge = _edges[i];
            var t = Math.Clamp(((p.X - edge.Ax) * edge.Dx + (p.Z - edge.Ay) * edge.Dy) * edge.InvLengthSq, 0, 1);
            var distX = p.X - (edge.Ax + t * edge.Dx);
            var distY = p.Z - (edge.Ay + t * edge.Dy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }

        var minDistance = MathF.Sqrt(minDistanceSq);
        return isInside ? -minDistance : minDistance;
    }

    private static void AddEdgesFromPart(RelPolygonWithHoles part, WPos origin, List<Edge> edges)
    {
        edges.AddRange(GetEdges(part.Exterior, origin));

        foreach (var holeIndex in part.Holes)
        {
            edges.AddRange(GetEdges(part.Interior(holeIndex), origin));
        }
    }

    private static Edge[] GetEdges(ReadOnlySpan<WDir> vertices, WPos origin)
    {
        var count = vertices.Length;

        if (count == 0)
            return [];

        var edges = new Edge[count];

        var prev = vertices[count - 1];
        for (var i = 0; i < count; i++)
        {
            var curr = vertices[i];
            edges[i] = new(origin.X + prev.X, origin.Z + prev.Z, curr.X - prev.X, curr.Z - prev.Z);
            prev = curr;
        }

        return edges;
    }
}
