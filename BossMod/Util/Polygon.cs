using Clipper2Lib;
using EarcutNet;
using System.Threading;

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
    public ReadOnlySpan<int> Holes
    {
        get
        {
            var count = HoleStarts.Count;
            List<int> result = new(count);
            for (var i = 0; i < count; ++i)
                result.Add(i);
            return result.AsSpan();
        }
    }

    public List<(WDir, WDir)> ExteriorEdges => PolygonUtil.EnumerateEdges(Exterior);
    public List<(WDir, WDir)> InteriorEdges(int index) => PolygonUtil.EnumerateEdges(Interior(index));

    private EdgeBuckets? _edgeBuckets;
    private const int BucketCount = 20;
    private const float Epsilon = 1e-8f;

    private int ExteriorEnd => HoleStarts.Count > 0 ? HoleStarts[0] : Vertices.Count;
    private int HoleEnd(int index) => index + 1 < HoleStarts.Count ? HoleStarts[index + 1] : Vertices.Count;

    // add new hole; input is assumed to be a simple polygon
    public void AddHole(List<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }

    // build a triangulation of the polygon
    public bool Triangulate(List<RelTriangle> result)
    {
        var vertexCount = Vertices.Count;
        Span<double> pts = vertexCount <= 256 ? stackalloc double[vertexCount * 2] : new double[vertexCount * 2];
        for (int i = 0, j = 0; i < vertexCount; ++i, j += 2)
        {
            var v = Vertices[i];
            pts[j] = v.X;
            pts[j + 1] = v.Z;
        }
        var tess = Earcut.Tessellate(pts[..(vertexCount * 2)], HoleStarts);
        for (var i = 0; i < tess.Count; i += 3)
        {
            result.Add(new(Vertices[tess[i]], Vertices[tess[i + 1]], Vertices[tess[i + 2]]));
        }

        return tess.Count > 0;
    }
    public List<RelTriangle> Triangulate()
    {
        var result = new List<RelTriangle>(Vertices.Count);
        Triangulate(result);
        return result;
    }

    // build a new polygon by transformation
    public RelPolygonWithHoles Transform(WDir offset, WDir rotation)
    {
        var count = Vertices.Count;
        var newVerts = new List<WDir>(count);
        for (var i = 0; i < count; ++i)
            newVerts.Add(Vertices[i].Rotate(rotation) + offset);
        return new RelPolygonWithHoles(newVerts, [.. HoleStarts]);
    }

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p)
    {
        ref var edgeBuckets = ref _edgeBuckets;
        if (edgeBuckets == null)
        {
            var holecount = HoleStarts.Count;
            ContourEdgeBuckets[] holeEdgeBuckets;
            var exteriorTask = Task.Run(() => BuildEdgeBucketsForContour(Exterior));
            switch (holecount)
            {
                case 0:
                    holeEdgeBuckets = [];
                    break;
                case 1:
                    holeEdgeBuckets = new ContourEdgeBuckets[1];
                    holeEdgeBuckets[0] = Task.Run(() => BuildEdgeBucketsForContour(Interior(0))).Result;
                    break;
                default:
                    holeEdgeBuckets = new ContourEdgeBuckets[holecount];
                    var holeTasks = new Task[holecount];
                    for (var i = 0; i < holecount; ++i)
                    {
                        var index = i;
                        holeTasks[i] = Task.Run(() =>
                        {
                            holeEdgeBuckets[index] = BuildEdgeBucketsForContour(Interior(index));
                        });
                    }
                    Task.WaitAll(holeTasks);
                    break;
            }

            var newEdgeBuckets = new EdgeBuckets(exteriorTask.Result, holeEdgeBuckets);
            var original = Interlocked.CompareExchange(ref _edgeBuckets, newEdgeBuckets, null);

            edgeBuckets = original ?? newEdgeBuckets;
        }

        if (!InSimplePolygon(p, edgeBuckets.ExteriorEdgeBuckets))
            return false;
        var len = edgeBuckets.HoleEdgeBuckets.Length;
        for (var i = 0; i < len; ++i)
        {
            if (InSimplePolygon(p, edgeBuckets.HoleEdgeBuckets[i]))
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
        ref readonly var edges = ref buckets.EdgeBuckets[bucketIndex];
        var inside = false;
        var len = edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var edge = ref edges[i];
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

        for (var i = 0; i < count; ++i)
        {
            var y = contour[i].Z;
            if (y < minY)
                minY = y;
            if (y > maxY)
                maxY = y;
        }

        var invBucketHeight = BucketCount / (maxY - minY + Epsilon);

        var edgeBucketsArray = new List<Edges>[BucketCount];
        for (var b = 0; b < BucketCount; ++b)
        {
            edgeBucketsArray[b] = [];
        }

        var prev = contour[^1];
        for (var i = 0; i < count; ++i)
        {
            var curr = contour[i];
            var edge = new Edges(prev.X, prev.Z, curr.X, curr.Z);

            var bucketStart = (int)((Math.Min(edge.y0, edge.y1) - minY) * invBucketHeight);
            var bucketEnd = (int)((Math.Max(edge.y0, edge.y1) - minY) * invBucketHeight);

            bucketStart = Math.Clamp(bucketStart, 0, BucketCount - 1);
            bucketEnd = Math.Clamp(bucketEnd, 0, BucketCount - 1);

            for (var b = bucketStart; b <= bucketEnd; ++b)
            {
                edgeBucketsArray[b].Add(edge);
            }

            prev = curr;
        }

        var edgeBuckets = new Edges[BucketCount][];
        for (var b = 0; b < BucketCount; ++b)
        {
            edgeBuckets[b] = [.. edgeBucketsArray[b]];
        }

        return new(edgeBuckets, minY, invBucketHeight);
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
            var invDy = dy != 0 ? 1f / dy : 0;
            slopeX = (x1 - x0) * invDy;
        }
    }

    private sealed class EdgeBuckets(ContourEdgeBuckets exteriorEdgeBuckets, ContourEdgeBuckets[] holeEdgeBuckets)
    {
        public readonly ContourEdgeBuckets ExteriorEdgeBuckets = exteriorEdgeBuckets;
        public readonly ContourEdgeBuckets[] HoleEdgeBuckets = holeEdgeBuckets;
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

    // build a new polygon by transformation
    public RelSimplifiedComplexPolygon Transform(WDir offset, WDir rotation) => new([.. Parts.Select(p => p.Transform(offset, rotation))]);

    // point-in-polygon test; point is defined as offset from shape center
    public bool Contains(WDir p)
    {
        for (var i = 0; i < Parts.Count; ++i)
            if (Parts[i].Contains(p))
                return true;
        return false;
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
            var count = contour.Length;
            Path64 path = new(count);
            for (var i = 0; i < count; ++i)
                path.Add(ConvertPoint(contour[i]));
            AddContour(path, isOpen);
        }

        public void AddContour(IEnumerable<WDir> contour, bool isOpen = false) => AddContour([.. contour.Select(ConvertPoint)], isOpen);

        public void AddPolygon(RelPolygonWithHoles polygon)
        {
            AddContour(polygon.Exterior);
            var len = polygon.Holes.Length;
            for (var i = 0; i < len; ++i)
                AddContour(polygon.Interior(polygon.Holes[i]));
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
            for (var j = 0; j < extPolygon.Count; ++j)
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
    public static List<(T, T)> EnumerateEdges<T>(ReadOnlySpan<T> contour) where T : struct, IEquatable<T>
    {
        var count = contour.Length;
        var result = new List<(T, T)>(count);
        if (count == 0)
            return result;

        var prev = contour[count - 1];
        for (var i = 0; i < count; ++i)
        {
            result.Add((prev, contour[i]));
            prev = contour[i];
        }
        return result;
    }
}

public readonly struct Edge(float ax, float ay, float dx, float dy)
{
    private const float Epsilon = 1e-8f;

    public readonly float Ax = ax, Ay = ay, Dx = dx, Dy = dy, InvLengthSq = 1f / (dx * dx + dy * dy + Epsilon);
}

public class SpatialIndex
{
    private int[][] _grid = [];
    private readonly Edge[] _edges;
    private readonly int _minX, _minY, _gridWidth, _gridHeight;
    private const float InvGridSize = 1 / 5f;

    public SpatialIndex(Edge[] edges)
    {
        _edges = edges;
        ComputeGridBounds(out _minX, out _minY, out _gridWidth, out _gridHeight);
        BuildIndex();
    }

    private void ComputeGridBounds(out int minX, out int minY, out int gridWidth, out int gridHeight)
    {
        minX = minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        var len = _edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[i];
            var edgeAx = edge.Ax * InvGridSize;
            var edgeAy = edge.Ay * InvGridSize;
            var edgeAxDx = (edgeAx + edge.Dx) * InvGridSize;
            var edgeAydy = (edgeAy + edge.Dy) * InvGridSize;
            var ex0 = (int)MathF.Floor(Math.Min(edgeAx, edgeAxDx));
            var ex1 = (int)MathF.Floor(Math.Max(edgeAx, edgeAxDx));
            var ey0 = (int)MathF.Floor(Math.Min(edgeAy, edgeAydy));
            var ey1 = (int)MathF.Floor(Math.Max(edgeAy, edgeAydy));

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
        var cellCount = _gridWidth * _gridHeight;
        var grid = new List<int>[cellCount];
        for (var i = 0; i < cellCount; ++i)
        {
            grid[i] = [];
        }
        var len = _edges.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[i];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeAxDx = edgeAx + edge.Dx;
            var edgeAydy = edgeAy + edge.Dy;
            var minX = Math.Min(edgeAx, edgeAxDx);
            var maxX = Math.Max(edgeAx, edgeAxDx);
            var minY = Math.Min(edgeAy, edgeAydy);
            var maxY = Math.Max(edgeAy, edgeAydy);

            var x0 = (int)MathF.Floor(minX * InvGridSize) - _minX;
            var x1 = (int)MathF.Floor(maxX * InvGridSize) - _minX;
            var y0 = (int)MathF.Floor(minY * InvGridSize) - _minY;
            var y1 = (int)MathF.Floor(maxY * InvGridSize) - _minY;

            for (var y = y0; y <= y1; ++y)
            {
                var rowIndex = y * _gridWidth;
                for (var x = x0; x <= x1; ++x)
                {
                    grid[rowIndex + x].Add(i);
                }
            }
        }

        _grid = new int[cellCount][];
        for (var i = 0; i < cellCount; ++i)
        {
            _grid[i] = [.. grid[i]];
        }
    }

    public int[] Query(float px, float py)
    {
        var cellX = (int)MathF.Floor(px * InvGridSize) - _minX;
        var cellY = (int)MathF.Floor(py * InvGridSize) - _minY;

        return (uint)cellX >= _gridWidth || (uint)cellY >= _gridHeight ? [] : _grid[cellY * _gridWidth + cellX];
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
        var edgeCount = 0;
        var countPolygonParts = polygon.Parts.Count;
        for (var i = 0; i < countPolygonParts; ++i)
        {
            var part = polygon.Parts[i];
            edgeCount += part.ExteriorEdges.Count;
            var lenPolygonHoles = part.Holes.Length;
            for (var j = 0; j < lenPolygonHoles; ++j)
                edgeCount += part.InteriorEdges(j).Count;
        }
        _edges = new Edge[edgeCount];
        var edgeIndex = 0;
        for (var i = 0; i < countPolygonParts; ++i)
        {
            var part = polygon.Parts[i];
            var exteriorEdges = GetEdges(part.Exterior, origin);
            var exteriorCount = exteriorEdges.Length;
            Array.Copy(exteriorEdges, 0, _edges, edgeIndex, exteriorCount);
            edgeIndex += exteriorCount;
            var lenPolygonHoles = part.Holes.Length;
            for (var j = 0; j < lenPolygonHoles; ++j)
            {
                var holeEdges = GetEdges(part.Interior(j), origin);
                var holeEdgesCount = holeEdges.Length;
                Array.Copy(holeEdges, 0, _edges, edgeIndex, holeEdgesCount);
                edgeIndex += holeEdgesCount;
            }
        }
        _spatialIndex = new(_edges);

        static Edge[] GetEdges(ReadOnlySpan<WDir> vertices, WPos origin)
        {
            var count = vertices.Length;

            if (count == 0)
                return [];

            var edges = new Edge[count];

            var prev = vertices[count - 1];
            var originX = origin.X;
            var originZ = origin.Z;

            for (var i = 0; i < count; ++i)
            {
                var curr = vertices[i];
                var prevX = prev.X;
                var prevZ = prev.Z;
                edges[i] = new(originX + prevX, originZ + prevZ, curr.X - prevX, curr.Z - prevZ);
                prev = curr;
            }

            return edges;
        }
    }

    public readonly float Distance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var localPoint = new WDir(pX - _origin.X, pZ - _origin.Z);
        if (_polygon.Contains(localPoint)) // NOTE: our usecase doesn't care about distance inside of the polygon, so we can short circuit here
            return 0f;
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, 0f, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }

    public readonly float InvertedDistance(WPos p)
    {
        var pX = p.X;
        var pZ = p.Z;
        var localPoint = new WDir(pX - _origin.X, pZ - _origin.Z);
        if (!_polygon.Contains(localPoint)) // NOTE: our usecase doesn't care about distance outside of the polygon, so we can short circuit here
            return 0f;
        var minDistanceSq = float.MaxValue;

        var indices = _spatialIndex.Query(pX, pZ);
        var len = indices.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var edge = ref _edges[indices[i]];
            var edgeAx = edge.Ax;
            var edgeAy = edge.Ay;
            var edgeDx = edge.Dx;
            var edgeDy = edge.Dy;
            var t = Math.Clamp(((pX - edgeAx) * edgeDx + (pZ - edgeAy) * edgeDy) * edge.InvLengthSq, 0f, 1f);
            var distX = pX - (edgeAx + t * edgeDx);
            var distY = pZ - (edgeAy + t * edgeDy);

            minDistanceSq = Math.Min(minDistanceSq, distX * distX + distY * distY);
        }
        return MathF.Sqrt(minDistanceSq);
    }
}
