namespace BossMod;

public abstract record class AOEShape
{
    public abstract bool Check(WPos position, WPos origin, Angle rotation);
    public abstract void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default);
    public abstract void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f);
    public abstract Func<WPos, float> Distance(WPos origin, Angle rotation);
    public abstract Func<WPos, float> InvertedDistance(WPos origin, Angle rotation);

    public bool Check(WPos position, Actor? origin) => origin != null && Check(position, origin.Position, origin.Rotation);

    public void Draw(MiniArena arena, Actor? origin, uint color = default)
    {
        if (origin != null)
            Draw(arena, origin.Position, origin.Rotation, color);
    }

    public void Outline(MiniArena arena, Actor? origin, uint color = default)
    {
        if (origin != null)
            Outline(arena, origin.Position, origin.Rotation, color);
    }
}

public sealed record class AOEShapeCone(float Radius, Angle HalfAngle, Angle DirectionOffset = default, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Cone: r={Radius:f3}, angle={HalfAngle * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCircleCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCone(origin, default, Radius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddCone(origin, Radius, rotation + DirectionOffset, HalfAngle, color, thickness);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? ShapeDistance.Cone(origin, Radius, rotation + DirectionOffset, HalfAngle)
            : ShapeDistance.InvertedCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedCone(origin, Radius, rotation + DirectionOffset, HalfAngle);
}

public sealed record class AOEShapeCircle(float Radius, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Circle: r={Radius:f3}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation = default) => position.InCircle(origin, Radius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = default, uint color = default) => arena.ZoneCircle(origin, Radius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = default, uint color = default, float thickness = 1f) => arena.AddCircle(origin, Radius, color, thickness);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? ShapeDistance.Circle(origin, Radius)
            : ShapeDistance.InvertedCircle(origin, Radius);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedCircle(origin, Radius);
}

public sealed record class AOEShapeDonut(float InnerRadius, float OuterRadius, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Donut: r={InnerRadius:f3}-{OuterRadius:f3}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation = default) => position.InDonut(origin, InnerRadius, OuterRadius);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation = default, uint color = default) => arena.ZoneDonut(origin, InnerRadius, OuterRadius, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation = default, uint color = default, float thickness = 1f)
    {
        arena.AddCircle(origin, InnerRadius, color, thickness);
        arena.AddCircle(origin, OuterRadius, color, thickness);
    }
    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? ShapeDistance.Donut(origin, InnerRadius, OuterRadius)
            : ShapeDistance.InvertedDonut(origin, InnerRadius, OuterRadius);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedDonut(origin, InnerRadius, OuterRadius);
}

public sealed record class AOEShapeDonutSector(float InnerRadius, float OuterRadius, Angle HalfAngle, Angle DirectionOffset = default, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Donut sector: r={InnerRadius:f3}-{OuterRadius:f3}, angle={HalfAngle * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddDonutCone(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle, color, thickness);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? ShapeDistance.DonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle)
            : ShapeDistance.InvertedDonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedDonutSector(origin, InnerRadius, OuterRadius, rotation + DirectionOffset, HalfAngle);
}

public sealed record class AOEShapeRect(float LengthFront, float HalfWidth, float LengthBack = default, Angle DirectionOffset = default, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Rect: l={LengthFront:f3}+{LengthBack:f3}, w={HalfWidth * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddRect(origin, (rotation + DirectionOffset).ToDirection(), LengthFront, LengthBack, HalfWidth, color, thickness);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? ShapeDistance.Rect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth)
            : ShapeDistance.InvertedRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedRect(origin, rotation + DirectionOffset, LengthFront, LengthBack, HalfWidth);

}

public sealed record class AOEShapeCross(float Length, float HalfWidth, Angle DirectionOffset = default, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Cross: l={Length:f3}, w={HalfWidth * 2}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCross(origin, rotation + DirectionOffset, Length, HalfWidth);
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZonePoly(((byte)0x01, origin, rotation + DirectionOffset, Length, HalfWidth), ContourPoints(origin, rotation), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f)
    {
        var points = ContourPoints(origin, rotation);
        for (var i = 0; i < 12; ++i)
            arena.PathLineTo(points[i]);
        MiniArena.PathStroke(true, color, thickness);
    }

    private WPos[] ContourPoints(WPos origin, Angle rotation, float offset = default)
    {
        var dx = (rotation + DirectionOffset).ToDirection();
        var dy = dx.OrthoL();

        var lengthOffset = Length + offset;
        var halfWidthOffset = HalfWidth + offset;

        var dxLength = dx * lengthOffset;
        var dxWidth = dx * halfWidthOffset;
        var dyLength = dy * lengthOffset;
        var dyWidth = dy * halfWidthOffset;

        return
        [
            origin + dxLength - dyWidth,
            origin + dxWidth - dyWidth,
            origin + dxWidth - dyLength,
            origin - dxWidth - dyLength,
            origin - dxWidth - dyWidth,
            origin - dxLength - dyWidth,
            origin - dxLength + dyWidth,
            origin - dxWidth + dyWidth,
            origin - dxWidth + dyLength,
            origin + dxWidth + dyLength,
            origin + dxWidth + dyWidth,
            origin + dxLength + dyWidth
        ];
    }

    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone
            ? ShapeDistance.Cross(origin, rotation + DirectionOffset, Length, HalfWidth)
            : ShapeDistance.InvertedCross(origin, rotation + DirectionOffset, Length, HalfWidth);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedCross(origin, rotation + DirectionOffset, Length, HalfWidth);
}

public sealed record class AOEShapeTriCone(float SideLength, Angle HalfAngle, Angle DirectionOffset = default, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"TriCone: side={SideLength:f3}, angle={HalfAngle * 2f}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InTri(origin, origin + SideLength * (rotation + DirectionOffset + HalfAngle).ToDirection(), origin + SideLength * (rotation + DirectionOffset - HalfAngle).ToDirection());
    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneTri(origin, origin + SideLength * (rotation + DirectionOffset + HalfAngle).ToDirection(), origin + SideLength * (rotation + DirectionOffset - HalfAngle).ToDirection(), color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f) => arena.AddTriangle(origin, origin + SideLength * (rotation + DirectionOffset + HalfAngle).ToDirection(), origin + SideLength * (rotation + DirectionOffset - HalfAngle).ToDirection(), color, thickness);

    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        var rotoffset = rotation + DirectionOffset;
        var direction1 = SideLength * (rotoffset + HalfAngle).ToDirection();
        var direction2 = SideLength * (rotoffset - HalfAngle).ToDirection();
        var shape = new RelTriangle(default, direction1, direction2);
        return !InvertForbiddenZone ? ShapeDistance.Tri(origin, shape) : ShapeDistance.InvertedTri(origin, shape);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation)
    {
        var rotoffset = rotation + DirectionOffset;
        var direction1 = SideLength * (rotoffset + HalfAngle).ToDirection();
        var direction2 = SideLength * (rotoffset - HalfAngle).ToDirection();
        var shape = new RelTriangle(default, direction1, direction2);
        return ShapeDistance.InvertedTri(origin, shape);
    }
}

public sealed record class AOEShapeCapsule(float Radius, float Length, Angle DirectionOffset = default, bool InvertForbiddenZone = false) : AOEShape
{
    public override string ToString() => $"Capsule: radius={Radius:f3}, length={Length}, off={DirectionOffset}, ifz={InvertForbiddenZone}";
    public override bool Check(WPos position, WPos origin, Angle rotation) => position.InCapsule(origin, (rotation + DirectionOffset).ToDirection(), Radius, Length);

    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default) => arena.ZoneCapsule(origin, (rotation + DirectionOffset).ToDirection(), Radius, Length, color);
    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f)
    => arena.AddCapsule(origin, (rotation + DirectionOffset).ToDirection(), Radius, Length, color, thickness);
    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        return !InvertForbiddenZone ? ShapeDistance.Capsule(origin, rotation, Length, Radius) : ShapeDistance.InvertedCapsule(origin, rotation, Length, Radius);
    }
    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation) => ShapeDistance.InvertedCapsule(origin, rotation, Length, Radius);
}

public enum OperandType
{
    Union,
    Xor,
    Intersection,
    Difference
}

// shapes1 for unions, shapes 2 for shapes for XOR/intersection with shapes1, differences for shapes that get subtracted after previous operations
// always create a new instance of AOEShapeCustom if something other than the invertforbiddenzone changes
// if the origin of the AOE can change, edit the origin default value to prevent cache issues
public sealed record class AOEShapeCustom(IReadOnlyList<Shape> Shapes1, IReadOnlyList<Shape>? DifferenceShapes = null, IReadOnlyList<Shape>? Shapes2 = null, bool InvertForbiddenZone = false, OperandType Operand = OperandType.Union, WPos Origin = default) : AOEShape
{
    public RelSimplifiedComplexPolygon? Polygon;
    private PolygonWithHolesDistanceFunction shapeDistance;
    private bool isShapeDistanceInitialized;
    private readonly int hashkey = CreateCacheKey(Shapes1, Shapes2 ?? [], DifferenceShapes ?? [], Operand, Origin);
    private static readonly Dictionary<int, RelSimplifiedComplexPolygon> cache = [];
    private static readonly LinkedList<int> cacheOrder = [];

    private void AddToCache(RelSimplifiedComplexPolygon value)
    {
        if (cache.Count >= 50)
        {
            var lruKey = cacheOrder.Last?.Value;
            if (lruKey != null)
            {
                cache.Remove(lruKey.Value);
                cacheOrder.RemoveLast();
            }
        }
        cache[hashkey] = value;
        cacheOrder.Remove(hashkey);
        cacheOrder.AddFirst(hashkey);
    }

    public override string ToString() => $"Custom AOE shape: hashkey={hashkey}, ifz={InvertForbiddenZone}";

    public RelSimplifiedComplexPolygon GetCombinedPolygon(WPos origin)
    {
        if (cache.TryGetValue(hashkey, out var cachedResult))
        {
            cacheOrder.Remove(hashkey);
            cacheOrder.AddFirst(hashkey);
            return Polygon = cachedResult;
        }

        var shapes1 = CreateOperandFromShapes(Shapes1, origin);

        var clipper = new PolygonClipper();
        if (Shapes2 == null)
        {
            if (DifferenceShapes != null)
            {
                Polygon = clipper.Difference(shapes1, CreateOperandFromShapes(DifferenceShapes, origin));
                AddToCache(Polygon);
                return Polygon;
            }
            else
            {
                Polygon = clipper.Simplify(shapes1);
                AddToCache(Polygon);
                return Polygon;
            }
        }
        else
        {
            Polygon = clipper.Simplify(shapes1);
            if (Operand is OperandType.Intersection or OperandType.Xor)
            {
                var count = Shapes2.Count;
                for (var i = 0; i < count; ++i)
                {
                    var shape = Shapes2[i];
                    var singleShapeOperand = CreateOperandFromShape(shape, origin);
                    var operand = new PolygonClipper.Operand(Polygon);
                    switch (Operand)
                    {
                        case OperandType.Intersection:
                            Polygon = clipper.Intersect(operand, singleShapeOperand);
                            break;
                        case OperandType.Xor:
                            Polygon = clipper.Xor(operand, singleShapeOperand);
                            break;
                    }
                }
            }
            Polygon = DifferenceShapes != null ? clipper.Difference(new PolygonClipper.Operand(Polygon), CreateOperandFromShapes(DifferenceShapes, origin)) : Polygon;
            if (Operand == OperandType.Union)
            {
                Polygon = clipper.Union(CreateOperandFromShapes(Shapes2, origin), new PolygonClipper.Operand(Polygon));
            }
            AddToCache(Polygon);
            return Polygon;
        }
    }

    private static PolygonClipper.Operand CreateOperandFromShape(Shape shape, WPos origin)
    {
        var operand = new PolygonClipper.Operand();
        operand.AddPolygon(shape.ToPolygon(origin));
        return operand;
    }

    private static PolygonClipper.Operand CreateOperandFromShapes(IReadOnlyList<Shape>? shapes, WPos origin)
    {
        var operand = new PolygonClipper.Operand();
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

    public override bool Check(WPos position, WPos origin, Angle rotation)
    {
        return (Polygon ?? GetCombinedPolygon(origin)).Contains(position - origin);
    }

    private static int CreateCacheKey(IReadOnlyList<Shape> shapes1, IReadOnlyList<Shape> shapes2, IReadOnlyList<Shape> differenceShapes, OperandType operand, WPos origin)
    {
        var hashCode = new HashCode();
        var count1 = shapes1.Count;
        var count2 = shapes2.Count;
        var count3 = differenceShapes.Count;
        for (var i = 0; i < count1; ++i)
        {
            hashCode.Add(shapes1[i].GetHashCode());
        }
        for (var i = 0; i < count2; ++i)
        {
            hashCode.Add(shapes2[i].GetHashCode());
        }
        for (var i = 0; i < count3; ++i)
        {
            hashCode.Add(differenceShapes[i].GetHashCode());
        }
        hashCode.Add(operand);
        hashCode.Add(origin);
        return hashCode.ToHashCode();
    }

    public override void Draw(MiniArena arena, WPos origin, Angle rotation, uint color = default)
    {
        arena.ZoneRelPoly(hashkey, Polygon ?? GetCombinedPolygon(origin), color);
    }

    public override void Outline(MiniArena arena, WPos origin, Angle rotation, uint color = default, float thickness = 1f)
    {
        var combinedPolygon = Polygon ?? GetCombinedPolygon(origin);
        var parts = combinedPolygon.Parts;
        var count = parts.Count;
        for (var i = 0; i < count; ++i)
        {
            var part = parts[i];
            var exteriorEdges = part.ExteriorEdges;
            var exteriorLen = exteriorEdges.Length;
            for (var j = 0; j < exteriorLen; ++j)
            {
                var (start, end) = exteriorEdges[j];
                arena.PathLineTo(origin + start);
                if (j != exteriorLen - 1)
                    arena.PathLineTo(origin + end);
            }
            MiniArena.PathStroke(true, color, thickness);
            var holes = part.Holes;
            var lenHoles = holes.Length;
            for (var k = 0; k < lenHoles; ++k)
            {
                var interiorEdges = part.InteriorEdges(holes[k]);
                var interiorLen = interiorEdges.Length;
                for (var j = 0; j < interiorLen; ++j)
                {
                    var (start, end) = interiorEdges[j];
                    arena.PathLineTo(origin + start);
                    if (j != interiorLen - 1)
                        arena.PathLineTo(origin + end);
                }
                MiniArena.PathStroke(true, color, thickness);
            }
        }
    }

    public override Func<WPos, float> Distance(WPos origin, Angle rotation)
    {
        if (!isShapeDistanceInitialized)
        {
            shapeDistance = new PolygonWithHolesDistanceFunction(origin, Polygon ?? GetCombinedPolygon(origin));
            isShapeDistanceInitialized = true;
        }
        ref readonly var distance = ref shapeDistance;
        return InvertForbiddenZone ? distance.InvertedDistance : distance.Distance;
    }

    public override Func<WPos, float> InvertedDistance(WPos origin, Angle rotation)
    {
        if (!isShapeDistanceInitialized)
        {
            shapeDistance = new PolygonWithHolesDistanceFunction(origin, Polygon ?? GetCombinedPolygon(origin));
            isShapeDistanceInitialized = true;
        }
        ref readonly var distance = ref shapeDistance;
        return distance.InvertedDistance;
    }
}
