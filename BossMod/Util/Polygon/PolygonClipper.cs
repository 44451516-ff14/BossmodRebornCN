using Clipper2Lib;
using System.Buffers;

namespace BossMod;

public enum OperandType
{
    Union,
    Xor,
    Intersection,
    Difference
}

// utility for simplifying and performing boolean operations on complex polygons
public sealed class PolygonClipper
{
    public const float Scale = 1024f * 1024f; // note: we need at least 10 bits for integer part (-1024 to 1024 range); using 11 bits leaves 20 bits for fractional part; power-of-two scale should reduce rounding issues
    public const float InvScale = 1f / Scale;

    // reusable representation of the complex polygon ready for boolean operations
    public sealed class Operand
    {
        public Operand() { }
        public Operand(ReadOnlySpan<WDir> contour, bool isOpen = false) => AddContour(contour, isOpen);
        public Operand(RelPolygonWithHoles polygon) => AddPolygon(polygon);
        public Operand(RelSimplifiedComplexPolygon polygon) => AddPolygon(polygon);

        private readonly ReuseableDataContainer64 _data = new();

        public void Clear() => _data.Clear();

        public void AddContour(ReadOnlySpan<WDir> contour, bool isOpen = false)
        {
            var count = contour.Length;

            Point64[]? rented = null;
            var path = count <= 256 ? stackalloc Point64[count] : (rented = ArrayPool<Point64>.Shared.Rent(count)).AsSpan(0, count);
            try
            {
                for (var i = 0; i < count; ++i)
                {
                    path[i] = ConvertPoint(contour[i]);
                }
                _data.AddPath(path, PathType.Subject, isOpen);
            }
            finally
            {
                if (rented != null)
                {
                    ArrayPool<Point64>.Shared.Return(rented);
                }
            }
        }

        public void AddPolygon(RelPolygonWithHoles polygon)
        {
            AddContour(polygon.Exterior);
            var countH = polygon.HoleStarts.Count;
            for (var i = 0; i < countH; ++i)
            {
                AddContour(polygon.Interior(i));
            }
        }

        public void AddPolygon(RelSimplifiedComplexPolygon polygon)
        {
            var parts = polygon.Parts;
            var count = parts.Count;
            for (var i = 0; i < count; ++i)
            {
                AddPolygon(parts[i]);
            }
        }

        public void Assign(Clipper64 clipper, PathType role) => clipper.AddReuseableData(_data, role);
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
        var countP = parent.Count;
        for (var i = 0; i < countP; ++i)
        {
            var exterior = parent[i];
            if (exterior.Polygon == null || exterior.Polygon.Count == 0)
            {
                continue;
            }

            var extPolygon = exterior.Polygon;
            var pointCount = extPolygon.Count;
            var holeCount = 0;
            var countExt = exterior.Count;
            for (var j = 0; j < countExt; ++j)
            {
                var hole = exterior[j].Polygon;
                if (hole != null && hole.Count != 0)
                {
                    pointCount += hole.Count;
                    ++holeCount;
                }
            }

            var polygonPoints = new List<WDir>(pointCount);
            CollectionsMarshal.SetCount(polygonPoints, pointCount);
            var destination = CollectionsMarshal.AsSpan(polygonPoints);
            CopyPath(extPolygon, destination);
            var written = extPolygon.Count;
            var poly = new RelPolygonWithHoles(polygonPoints, [with(holeCount)]);
            result.Parts.Add(poly);
            for (var j = 0; j < countExt; ++j)
            {
                var interior = exterior[j];
                if (interior.Polygon == null || interior.Polygon.Count == 0)
                {
                    continue;
                }

                var intPolygon = interior.Polygon;
                poly.HoleStarts.Add(written);
                CopyPath(intPolygon, destination[written..]);
                written += intPolygon.Count;
                BuildResult(result, interior);
            }
        }
    }

    private static void CopyPath(Path64 path, Span<WDir> destination)
    {
        var source = CollectionsMarshal.AsSpan(path);
        for (var i = 0; i < source.Length; ++i)
        {
            destination[i] = ConvertPoint(source[i]);
        }
    }

    private static Point64 ConvertPoint(WDir pt) => new(pt.X * Scale, pt.Z * Scale);
    private static WDir ConvertPoint(Point64 pt) => new(pt.X * InvScale, pt.Y * InvScale);

    // shapes1 for unions, shapes 2 for shapes for XOR/intersection with shapes1, differences for shapes that get subtracted after previous operations
    // in most cases origin should be the actual arena center (Arena.Center) to translate the coords back to correct relative coordinates at the end
    public static RelSimplifiedComplexPolygon GetCombinedPolygon(WPos origin, IReadOnlyList<Shape> shapes1, IReadOnlyList<Shape>? differenceShapes = null, IReadOnlyList<Shape>? shapes2 = null,
    OperandType operandType = OperandType.Union)
    {
        var operand = CreateOperandFromShapes(shapes1, origin);
        RelSimplifiedComplexPolygon poly;
        var clipper = new PolygonClipper();
        if (shapes2 == null)
        {
            if (differenceShapes != null)
            {
                poly = clipper.Difference(operand, CreateOperandFromShapes(differenceShapes, origin));
                return poly;
            }
            else
            {
                poly = clipper.Simplify(operand);
                return poly;
            }
        }
        else
        {
            poly = clipper.Simplify(operand);
            if (operandType is OperandType.Intersection or OperandType.Xor)
            {
                var count = shapes2.Count;
                for (var i = 0; i < count; ++i)
                {
                    var shape = shapes2[i];
                    var singleShapeOperand = CreateOperandFromShape(shape, origin);
                    var operand2 = new Operand(poly);
                    switch (operandType)
                    {
                        case OperandType.Intersection:
                            poly = clipper.Intersect(operand2, singleShapeOperand);
                            break;
                        case OperandType.Xor:
                            poly = clipper.Xor(operand2, singleShapeOperand);
                            break;
                    }
                }
            }
            poly = differenceShapes != null ? clipper.Difference(new Operand(poly), CreateOperandFromShapes(differenceShapes, origin)) : poly;
            if (operandType == OperandType.Union)
            {
                poly = clipper.Union(CreateOperandFromShapes(shapes2, origin), new Operand(poly));
            }
            return poly;
        }
    }

    private static Operand CreateOperandFromShape(Shape shape, WPos origin)
    {
        var operand = new Operand();
        operand.AddPolygon(shape.ToPolygon(origin));
        return operand;
    }

    private static Operand CreateOperandFromShapes(IReadOnlyList<Shape>? shapes, WPos origin)
    {
        var operand = new Operand();
        if (shapes != null)
        {
            var count = shapes.Count;
            for (var i = 0; i < count; ++i)
            {
                operand.AddPolygon(shapes[i].ToPolygon(origin));
            }
        }
        return operand;
    }
}
