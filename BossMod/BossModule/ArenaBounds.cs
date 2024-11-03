namespace BossMod;
// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
// note: if arena bounds are changed, new instance is recreated; max approx error can change without recreating the instance
public abstract record class ArenaBounds(float Radius, float MapResolution, float ScaleFactor = 1)
{
    // fields below are used for clipping & drawing borders
    public const float Half = 0.5f;
    public readonly PolygonClipper Clipper = new();
    public float MaxApproxError { get; private set; }
    public RelSimplifiedComplexPolygon ShapeSimplified { get; private set; } = new();
    public List<RelTriangle> ShapeTriangulation { get; private set; } = [];
    private readonly PolygonClipper.Operand _clipOperand = new();
    public readonly Dictionary<object, object> Cache = [];
#pragma warning disable IDE0032
    private float _screenHalfSize;
#pragma warning restore IDE0032
    public float ScreenHalfSize
    {
        get => _screenHalfSize;
        set
        {
            if (_screenHalfSize != value)
            {
                _screenHalfSize = value;
                MaxApproxError = CurveApprox.ScreenError / value * Radius;
                ShapeSimplified = Clipper.Simplify(BuildClipPoly());
                ShapeTriangulation = ShapeSimplified.Triangulate();
                _clipOperand.Clear();
                _clipOperand.AddPolygon(ShapeSimplified); // note: I assume using simplified shape as an operand is better than raw one
            }
        }
    }

    protected abstract PolygonClipper.Operand BuildClipPoly();
    public abstract void PathfindMap(Pathfinding.Map map, WPos center);
    public abstract bool Contains(WDir offset);
    public abstract float IntersectRay(WDir originOffset, WDir dir);
    public abstract WDir ClampToBounds(WDir offset);

    // functions for clipping various shapes to bounds; all shapes are expected to be defined relative to bounds center
    public List<RelTriangle> ClipAndTriangulate(IEnumerable<WDir> poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();
    public List<RelTriangle> ClipAndTriangulate(RelSimplifiedComplexPolygon poly) => Clipper.Intersect(new(poly), _clipOperand).Triangulate();

    public List<RelTriangle> ClipAndTriangulateCone(WDir centerOffset, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle)
    {
        // TODO: think of a better way to do that (analytical clipping?)
        if (innerRadius >= outerRadius || innerRadius < 0 || halfAngle.Rad <= 0)
            return [];

        var fullCircle = halfAngle.Rad >= MathF.PI;
        var donut = innerRadius > 0;
        var points = (donut, fullCircle) switch
        {
            (false, false) => CurveApprox.CircleSector(outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (false, true) => CurveApprox.Circle(outerRadius, MaxApproxError),
            (true, false) => CurveApprox.DonutSector(innerRadius, outerRadius, centerDirection - halfAngle, centerDirection + halfAngle, MaxApproxError),
            (true, true) => CurveApprox.Donut(innerRadius, outerRadius, MaxApproxError),
        };
        return ClipAndTriangulate(points.Select(p => p + centerOffset));
    }

    public List<RelTriangle> ClipAndTriangulateCircle(WDir centerOffset, float radius)
        => ClipAndTriangulate(CurveApprox.Circle(radius, MaxApproxError).Select(p => p + centerOffset));
    public List<RelTriangle> ClipAndTriangulateCapsule(WDir centerOffset, WDir direction, float radius, float length)
        => ClipAndTriangulate(CurveApprox.Capsule(direction, length, radius, CurveApprox.ScreenError).Select(p => p + centerOffset));
    public List<RelTriangle> ClipAndTriangulateDonut(WDir centerOffset, float innerRadius, float outerRadius)
        => innerRadius < outerRadius && innerRadius >= 0
            ? ClipAndTriangulate(CurveApprox.Donut(innerRadius, outerRadius, MaxApproxError).Select(p => p + centerOffset))
            : [];

    public List<RelTriangle> ClipAndTriangulateTri(WDir oa, WDir ob, WDir oc)
        => ClipAndTriangulate([oa, ob, oc]);

    public List<RelTriangle> ClipAndTriangulateIsoscelesTri(WDir apexOffset, WDir height, WDir halfBase)
        => ClipAndTriangulateTri(apexOffset, apexOffset + height + halfBase, apexOffset + height - halfBase);

    public List<RelTriangle> ClipAndTriangulateIsoscelesTri(WDir apexOffset, Angle direction, Angle halfAngle, float height)
    {
        var dir = direction.ToDirection();
        var normal = dir.OrthoL();
        return ClipAndTriangulateIsoscelesTri(apexOffset, height * dir, height * halfAngle.Tan() * normal);
    }

    public List<RelTriangle> ClipAndTriangulateRect(WDir originOffset, WDir direction, float lenFront, float lenBack, float halfWidth)
    {
        var side = halfWidth * direction.OrthoR();
        var front = originOffset + lenFront * direction;
        var back = originOffset - lenBack * direction;
        return ClipAndTriangulate([front + side, front - side, back - side, back + side]);
    }

    public List<RelTriangle> ClipAndTriangulateRect(WDir originOffset, Angle direction, float lenFront, float lenBack, float halfWidth)
        => ClipAndTriangulateRect(originOffset, direction.ToDirection(), lenFront, lenBack, halfWidth);

    public List<RelTriangle> ClipAndTriangulateRect(WDir startOffset, WDir endOffset, float halfWidth)
    {
        var dir = (endOffset - startOffset).Normalized();
        var side = halfWidth * dir.OrthoR();
        return ClipAndTriangulate([startOffset + side, startOffset - side, endOffset - side, endOffset + side]);
    }

    public void AddToInstanceCache(object key, object value)
    {
        if (Cache.Count > 2500)
            Cache.Clear();
        Cache[key] = value;
    }
}

public record class ArenaBoundsCircle(float Radius, float MapResolution = ArenaBounds.Half) : ArenaBounds(Radius, MapResolution)
{
    private Pathfinding.Map? _cachedMap;

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Circle(Radius, MaxApproxError));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);
    public override bool Contains(WDir offset) => offset.LengthSq() <= Radius * Radius;
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayCircle(originOffset, dir, Radius);

    public override WDir ClampToBounds(WDir offset)
    {
        if (offset.LengthSq() > Radius * Radius)
            offset *= Radius / offset.Length();
        return offset;
    }

    private Pathfinding.Map BuildMap()
    {
        var map = new Pathfinding.Map(MapResolution, default, Radius, Radius);
        map.BlockPixelsInsideConvex(p => -ShapeDistance.Circle(default, Radius)(p), 0, 0);
        return map;
    }
}

// if rotation is 0, half-width is along X and half-height is along Z
public record class ArenaBoundsRect(float HalfWidth, float HalfHeight, Angle Rotation = default, float MapResolution = ArenaBounds.Half) : ArenaBounds(Math.Max(HalfWidth, HalfHeight), MapResolution, Rotation != default ? CalculateScaleFactor(Rotation) : 1)
{
    private Pathfinding.Map? _cachedMap;
    public readonly WDir Orientation = Rotation.ToDirection();
    private static float CalculateScaleFactor(Angle Rotation)
    {
        var (sin, cos) = MathF.SinCos(Rotation.Rad);
        return Math.Abs(cos) + Math.Abs(sin);
    }

    protected override PolygonClipper.Operand BuildClipPoly() => new(CurveApprox.Rect(Orientation, HalfWidth, HalfHeight));
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);
    private Pathfinding.Map BuildMap()
    {
        var map = new Pathfinding.Map(MapResolution, default, HalfWidth, HalfHeight, Rotation);
        map.BlockPixelsInsideConvex(p => -ShapeDistance.Rect(default, Rotation, HalfHeight, HalfHeight, HalfWidth)(p), 0, 0);
        return map;
    }

    public override bool Contains(WDir offset) => offset.InRect(Orientation, HalfHeight, HalfHeight, HalfWidth);
    public override float IntersectRay(WDir originOffset, WDir dir) => Intersect.RayRect(originOffset, dir, Orientation, HalfWidth, HalfHeight);

    public override WDir ClampToBounds(WDir offset)
    {
        if (offset.X == default) // if actor is almost in the center of the arena, do nothing
            return offset;
        var dx = Math.Abs(offset.Dot(Orientation.OrthoL()));
        if (dx > HalfWidth)
            offset *= HalfWidth / dx;
        var dy = Math.Abs(offset.Dot(Orientation));
        if (dy > HalfHeight)
            offset *= HalfHeight / dy;
        return offset;
    }
}

public record class ArenaBoundsSquare(float Radius, Angle Rotation = default, float MapResolution = ArenaBounds.Half) : ArenaBoundsRect(Radius, Radius, Rotation, MapResolution) { }

// custom complex polygon bounds
public record class ArenaBoundsCustom : ArenaBounds
{
    private const float Epsilon = 1e-5f;
    private Pathfinding.Map? _cachedMap;
    private readonly RelSimplifiedComplexPolygon poly;
    private readonly (WDir, WDir)[] edges;
    private readonly float offset;
    public float HalfWidth, HalfHeight;

    public ArenaBoundsCustom(float Radius, RelSimplifiedComplexPolygon Poly, float MapResolution = Half, float Offset = 0)
        : base(Radius, MapResolution)
    {
        offset = Offset;
        poly = Poly;

        var edgeList = new List<(WDir, WDir)>();
        for (var i = 0; i < Poly.Parts.Count; ++i)
        {
            var part = Poly.Parts[i];
            edgeList.AddRange(part.ExteriorEdges);
            for (var j = 0; j < part.Holes.Count(); ++j)
            {
                edgeList.AddRange(part.InteriorEdges(j));
            }
        }
        edges = [.. edgeList];
    }

    protected override PolygonClipper.Operand BuildClipPoly() => new(poly);
    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    public override bool Contains(WDir offset) => poly.Contains(offset);

    public override float IntersectRay(WDir originOffset, WDir dir)
    {
        var cacheKey = (poly, originOffset, dir);
        if (Cache.TryGetValue(cacheKey, out var cachedResult))
            return (float)cachedResult;
        var result = Intersect.RayPolygon(originOffset, dir, poly);
        AddToInstanceCache(cacheKey, result);
        return result;
    }

    public override WDir ClampToBounds(WDir offset)
    {
        var cacheKey = (poly, offset);
        if (Cache.TryGetValue(cacheKey, out var cachedResult))
            return (WDir)cachedResult;
        if (Contains(offset) || offset.AlmostEqual(default, 1) || offset.X == default) // if actor is almost in the center of the arena, do nothing (eg donut arena)
        {
            Cache[(poly, offset)] = offset;
            return offset;
        }
        var minDistance = float.MaxValue;
        var nearestPoint = offset;
        for (var i = 0; i < edges.Length; ++i)
        {
            var edge = edges[i];
            var nearest = NearestPointOnSegment(offset, edge.Item1, edge.Item2);
            var distance = (nearest - offset).LengthSq();

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestPoint = nearest;
            }
        }
        AddToInstanceCache(cacheKey, nearestPoint);
        return nearestPoint;
    }

    private static WDir NearestPointOnSegment(WDir point, WDir segmentStart, WDir segmentEnd)
    {
        var segmentVector = segmentEnd - segmentStart;
        var segmentLengthSq = segmentVector.LengthSq();
        var t = Math.Max(0, Math.Min(1, (point - segmentStart).Dot(segmentVector) / segmentLengthSq));
        return segmentStart + t * segmentVector;
    }

    private Pathfinding.Map BuildMap()
    {
        // faster than using the polygonwithholes distance method directly
        var polygon = offset != 0 ? poly.Offset(offset) : poly;
        if (HalfHeight == default) // calculate bounding box if not already done by ArenaBoundsComplex to reduce amount of point in polygon tests
        {
            float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;

            for (var i = 0; i < polygon.Parts.Count; i++)
            {
                var part = polygon.Parts[i];
                for (var j = 0; j < part.Exterior.Length; j++)
                {
                    var vertex = part.Exterior[j];
                    if (vertex.X < minX)
                        minX = vertex.X;
                    if (vertex.X > maxX)
                        maxX = vertex.X;
                    if (vertex.Z < minZ)
                        minZ = vertex.Z;
                    if (vertex.Z > maxZ)
                        maxZ = vertex.Z;
                }
            }
            HalfWidth = (maxX - minX) * Half;
            HalfHeight = (maxZ - minZ) * Half;
        }
        var map = new Pathfinding.Map(MapResolution, default, HalfWidth, HalfHeight);
        var halfSample = MapResolution * Half - Epsilon; // tiny offset to account for floating point inaccuracies
        WDir[] sampleOffsets =
        [
        new(-halfSample, -halfSample),
        new(-halfSample,  0),
        new(-halfSample,  halfSample),
        new(0, -halfSample),
        new(0, 0),
        new(0, halfSample),
        new(halfSample, -halfSample),
        new(halfSample, 0),
        new(halfSample, halfSample)
        ];

        var pixels = map.Pixels;
        var width = map.Width;

        Parallel.For(0, map.Height, y =>
        {
            var rowOffset = y * width;
            for (var x = 0; x < width; x++)
            {
                var pos = map.GridToWorld(x, y, Half, Half);
                var relativeCenter = new WDir(pos.X, pos.Z);
                var allInside = true;
                for (var i = 0; i < 9; i++)
                {
                    var samplePoint = relativeCenter + sampleOffsets[i];
                    if (!polygon.Contains(samplePoint))
                    {
                        allInside = false;
                        break;
                    }
                }

                ref var pixel = ref pixels[rowOffset + x];
                pixel.MaxG = allInside ? float.MaxValue : 0;
            }
        });

        return map;
    }
}

// for creating complex bounds by using two IEnumerable of shapes
// first IEnumerable contains platforms that will be united, second optional IEnumberale contains shapes that will be subtracted
// for convenience third list will optionally perform additional unions at the end
public record class ArenaBoundsComplex : ArenaBoundsCustom
{
    public readonly WPos Center;
    public ArenaBoundsComplex(Shape[] UnionShapes, Shape[]? DifferenceShapes = null, Shape[]? AdditionalShapes = null, float MapResolution = Half, float Offset = 0)
        : base(BuildBounds(UnionShapes, DifferenceShapes, AdditionalShapes, MapResolution, Offset, out var center, out var halfWidth, out var halfHeight))
    {
        Center = center;
        HalfWidth = halfWidth + Offset;
        HalfHeight = halfHeight + Offset;
    }

    private static ArenaBoundsCustom BuildBounds(Shape[] unionShapes, Shape[]? differenceShapes, Shape[]? additionalShapes, float mapResolution, float offset, out WPos center, out float halfWidth, out float halfHeight)
    {
        var properties = CalculatePolygonProperties(unionShapes, differenceShapes ?? [], additionalShapes ?? []);
        center = properties.Center;
        halfWidth = properties.HalfWidth;
        halfHeight = properties.HalfHeight;
        return new(properties.Radius, properties.Poly, mapResolution, offset);
    }

    private static (WPos Center, float HalfWidth, float HalfHeight, float Radius, RelSimplifiedComplexPolygon Poly) CalculatePolygonProperties(Shape[] unionShapes, Shape[] differenceShapes, Shape[] additionalShapes)
    {
        var unionPolygons = ParseShapes(unionShapes);
        var differencePolygons = ParseShapes(differenceShapes);
        var additionalPolygons = ParseShapes(additionalShapes);

        var combinedPoly = CombinePolygons(unionPolygons, differencePolygons, additionalPolygons);

        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        var count = combinedPoly.Parts.Count;
        for (var i = 0; i < count; i++)
        {
            var part = combinedPoly.Parts[i];
            for (var j = 0; j < part.Exterior.Length; j++)
            {
                var vertex = part.Exterior[j];
                if (vertex.X < minX)
                    minX = vertex.X;
                if (vertex.X > maxX)
                    maxX = vertex.X;
                if (vertex.Z < minZ)
                    minZ = vertex.Z;
                if (vertex.Z > maxZ)
                    maxZ = vertex.Z;
            }
        }

        var center = new WPos((minX + maxX) * Half, (minZ + maxZ) * Half);
        var maxDistX = Math.Max(Math.Abs(maxX - center.X), Math.Abs(minX - center.X));
        var maxDistZ = Math.Max(Math.Abs(maxZ - center.Z), Math.Abs(minZ - center.Z));
        var halfWidth = (maxX - minX) * Half;
        var halfHeight = (maxZ - minZ) * Half;

        var combinedPolyCentered = CombinePolygons(ParseShapes(unionShapes, center), ParseShapes(differenceShapes, center), ParseShapes(additionalShapes, center));
        return (center, halfWidth, halfHeight, Math.Max(maxDistX, maxDistZ), combinedPolyCentered);
    }

    private static RelSimplifiedComplexPolygon CombinePolygons(RelSimplifiedComplexPolygon[] unionPolygons, RelSimplifiedComplexPolygon[] differencePolygons, RelSimplifiedComplexPolygon[] secondUnionPolygons)
    {
        var clipper = new PolygonClipper();
        var operandUnion = new PolygonClipper.Operand();
        var operandDifference = new PolygonClipper.Operand();
        var operandSecondUnion = new PolygonClipper.Operand();

        foreach (var polygon in unionPolygons)
            operandUnion.AddPolygon(polygon);
        foreach (var polygon in differencePolygons)
            operandDifference.AddPolygon(polygon);
        foreach (var polygon in secondUnionPolygons)
            operandSecondUnion.AddPolygon(polygon);

        var combinedShape = clipper.Difference(operandUnion, operandDifference);
        if (secondUnionPolygons.Length != 0)
            combinedShape = clipper.Union(new PolygonClipper.Operand(combinedShape), operandSecondUnion);

        return combinedShape;
    }

    private static RelSimplifiedComplexPolygon[] ParseShapes(Shape[] shapes, WPos center = default)
    {
        var polygons = new RelSimplifiedComplexPolygon[shapes.Length];
        for (var i = 0; i < shapes.Length; i++)
        {
            polygons[i] = shapes[i].ToPolygon(center);
        }
        return polygons;
    }
}
