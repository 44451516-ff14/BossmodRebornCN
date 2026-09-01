using System.Diagnostics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BossMod;

// radius is the largest horizontal/vertical dimension: radius for circle, max of width/height for rect
// note: this class to represent *relative* arena bounds (relative to arena center) - the reason being that in some cases effective center moves every frame, and bounds caches a lot (clip poly & base map for pathfinding)
// note: if arena bounds are changed, new instance is recreated; max approx error can change without recreating the instance

[SkipLocalsInit]
public abstract class ArenaBounds(float radius, float mapResolution, float scaleFactor = 1f, bool allowObstacleMap = false, bool allowDrawing3DArenaBounds = true)
{
    public const float DefaultWorldProjectionHeight = 2.5f;
    public readonly float Radius = radius;
    public readonly float InvRadius = 1f / radius;
    public readonly float MapResolution = mapResolution;
    public readonly float ScaleFactor = scaleFactor;
    public readonly bool AllowObstacleMap = allowObstacleMap;
    public readonly bool AllowDrawing3DArenaBounds = allowDrawing3DArenaBounds; // doesn't make sense for every arena, such as hunt marks

    // World-space reference plane for projected shapes. NaN uses the boss-height fallback
    public float Y = float.NaN;
    // Optional independent base plane for the 3D arena border. NaN inherits resolved Y (including its
    // boss-height fallback).
    public float BorderY = float.NaN;
    // Maximum vertical receiver band for projected shapes. Zero is valid and disables height
    // projection; NaN values fall back to DefaultWorldProjectionHeight.
    public float WorldProjectionHeight = DefaultWorldProjectionHeight;
    // World-space morphological radius (yalms) used to close small, fully surrounded receiver holes.
    // Zero disables closing; runtime use clamps finite values to [0, 2].
    // it is strongly advised to only use this as a last resort, since it is very expensive, especially when zoomed in
    // try using WorldProjectionHeight = 0, a small y offset and extra stencil shapes for big holes if needed instead
    public float WorldProjectionHoleFillRadius = 0f;

    // fields below are used for clipping & drawing borders
    public RelSimplifiedComplexPolygon Shape;

    public float ScreenHalfSize
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
            }
        }
    }

    public abstract void PathfindMap(Pathfinding.Map map, WPos center);
    public abstract bool Contains(in WDir offset);
    public abstract float IntersectRay(in WDir originOffset, in WDir dir);
    public abstract WDir ClampToBounds(in WDir offset);
}

[SkipLocalsInit]
public sealed class ArenaBoundsCircle : ArenaBounds
{
    public ArenaBoundsCircle(float Radius, float MapResolution = 0.5f, bool AllowObstacleMap = false, bool AllowDrawing3DArenaBounds = true) : base(Radius, MapResolution, allowObstacleMap: AllowObstacleMap, allowDrawing3DArenaBounds: AllowDrawing3DArenaBounds)
    {
        Shape = new(new Polygon(default, Radius, 128).Contour(default));
        Shape.InitPolygonIndex();
    }

    private Pathfinding.Map? _cachedMap;

    public override void PathfindMap(Pathfinding.Map map, WPos center) => map.Init(_cachedMap ??= BuildMap(), center);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WDir offset)
    {
        var radius = Radius;
        return offset.LengthSq() <= radius * radius;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float IntersectRay(in WDir originOffset, in WDir dir) => Intersect.RayCircle(originOffset, dir, Radius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override WDir ClampToBounds(in WDir offset)
    {
        var radius = Radius;
        return offset.LengthSq() > radius * radius ? offset * radius / offset.Length() : offset;
    }

    private Pathfinding.Map BuildMap()
    {
        var radius = Radius;
        var resolution = MapResolution;
        var threshold = radius * radius / (resolution * resolution); // square of bounds radius, in grid coordinates

        // For this even grid the nearest cell's farthest corner is at (1, 1) in grid coordinates.
        // A column with farthest-corner X coordinate cx can only contain a passable cell if cx^2 + 1 <= R^2,
        // so size the map to the largest column that can possibly survive the conservative full-cell test.
        var radiusCells = radius / resolution;
        var halfCells = (int)MathF.Floor(radiusCells);
        while (halfCells > 1 && (float)halfCells * halfCells + 1f > threshold)
        {
            --halfCells;
        }

        var width = 2 * halfCells;
        var map = new Pathfinding.Map();
        map.InitGrid(resolution, default, width, width);
        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;

        // Rasterize analytically. For increasing cy the largest admissible cx only moves inward, so across the
        // entire half-map this inner loop decrements at most halfCells times; no per-cell tests or square roots.
        var maxCX = halfCells;
        for (var cy = 1; cy <= halfCells; ++cy)
        {
            var cySq = (float)cy * cy;
            while (maxCX > 0 && (float)maxCX * maxCX + cySq > threshold)
            {
                --maxCX;
            }

            var blockedPerSide = halfCells - maxCX;
            if (blockedPerSide == 0)
            {
                continue;
            }

            var topRow = halfCells - cy;
            var bottomRow = halfCells + cy - 1;
            BlockRow(topRow, blockedPerSide);
            BlockRow(bottomRow, blockedPerSide);
        }

        return map;

        void BlockRow(int y, int blockedPerSide)
        {
            var row = y * width;
            if (blockedPerSide >= halfCells)
            {
                new Span<float>(pixelMaxG, row, width).Fill(-1000f);
                new Span<float>(pixelPriority, row, width).Fill(float.MinValue);
                return;
            }

            new Span<float>(pixelMaxG, row, blockedPerSide).Fill(-1000f);
            new Span<float>(pixelPriority, row, blockedPerSide).Fill(float.MinValue);

            var right = row + width - blockedPerSide;
            new Span<float>(pixelMaxG, right, blockedPerSide).Fill(-1000f);
            new Span<float>(pixelPriority, right, blockedPerSide).Fill(float.MinValue);
        }
    }

    public override string ToString() => $"{nameof(ArenaBoundsCircle)}, Radius {Radius}, MapResolution: {MapResolution}";
}

// if rotation is 0, half-width is along X and half-height is along Z
[SkipLocalsInit]
public abstract class ABRect : ArenaBounds
{
    public ABRect(float halfWidth, float halfHeight, Angle rotation = default, float MapResolution = 0.5f, bool AllowObstacleMap = false, bool AllowDrawing3DArenaBounds = true) : base(Math.Max(halfWidth, halfHeight), MapResolution, rotation != default ? CalculateScaleFactor(rotation) : 1f, AllowObstacleMap, AllowDrawing3DArenaBounds)
    {
        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        Rotation = rotation;
        Orientation = Rotation.ToDirection();

        var dx = Orientation.OrthoL() * HalfWidth;
        var dz = Orientation * HalfHeight;
        Shape = new([dx - dz, -dx - dz, -dx + dz, dx + dz]);
        Shape.InitPolygonIndex();
    }

    public readonly float HalfWidth;
    public readonly float HalfHeight;
    public readonly Angle Rotation;
    private Pathfinding.Map? _cachedMap;
    public readonly WDir Orientation;

    private static float CalculateScaleFactor(Angle Rotation)
    {
        var (sin, cos) = MathF.SinCos(Rotation.Rad);
        return Math.Abs(cos) + Math.Abs(sin);
    }

    public override void PathfindMap(Pathfinding.Map map, WPos center)
    {
        var source = _cachedMap ??= BuildMap();
        map.Init(source, center + source.Center.ToWDir());
    }

    private Pathfinding.Map BuildMap()
    {
        var resolution = MapResolution;
        var width = GridExtent(HalfWidth, resolution);
        var height = GridExtent(HalfHeight, resolution);

        // Existing map coordinates use floor(size/2) as the grid origin. For an odd dimension the geometric
        // midpoint is therefore +0.5 cell from Map.Center, so bias the cached center by -0.5 cell to keep the
        // grid itself centered on the arena rectangle.
        var dir = Orientation;
        var center = default(WPos);
        if ((width & 1) != 0)
        {
            center -= 0.5f * resolution * dir.OrthoL();
        }
        if ((height & 1) != 0)
        {
            center -= 0.5f * resolution * dir;
        }

        var map = new Pathfinding.Map();
        map.InitGrid(resolution, center, width, height, Rotation);

        return map;

        static int GridExtent(float halfExtent, float resolution)
        {
            var cells = 2f * halfExtent / resolution;
            var nearest = MathF.Round(cells);
            if (MathF.Abs(cells - nearest) <= 0.001f)
            {
                cells = nearest;
            }
            return (int)MathF.Floor(cells);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WDir offset) => offset.InRect(Orientation, HalfHeight, HalfHeight, HalfWidth);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float IntersectRay(in WDir originOffset, in WDir dir) => Intersect.RayRect(originOffset, dir, Orientation, HalfWidth, HalfHeight);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override WDir ClampToBounds(in WDir offset)
    {
        var orientation = Orientation;
        var halfWidth = HalfWidth;
        var halfHeight = HalfHeight;
        var offsetX = offset.Dot(orientation.OrthoL());
        var offsetY = offset.Dot(orientation);
        if (Math.Abs(offsetX) > halfWidth)
        {
            offsetX = Math.Sign(offsetX) * halfWidth;
        }
        if (Math.Abs(offsetY) > halfHeight)
        {
            offsetY = Math.Sign(offsetY) * halfHeight;
        }
        return orientation.OrthoL() * offsetX + orientation * offsetY;
    }
}

[SkipLocalsInit]
public sealed class ArenaBoundsRect(float halfWidth, float halfHeight, Angle rotation = default, float mapResolution = 0.5f, bool allowObstacleMap = false, bool allowDrawing3DArenaBounds = true) : ABRect(halfWidth, halfHeight, rotation, mapResolution, allowObstacleMap, allowDrawing3DArenaBounds)
{
    public override string ToString() => $"{nameof(ArenaBoundsRect)}, Radius {Radius}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}, MapResolution: {MapResolution}, ScaleFactor: {ScaleFactor}";
}
[SkipLocalsInit]
public sealed class ArenaBoundsSquare(float halfWidth, Angle rotation = default, float mapResolution = 0.5f, bool allowObstacleMap = false, bool allowDrawing3DArenaBounds = true) : ABRect(halfWidth, halfWidth, rotation, mapResolution, allowObstacleMap, allowDrawing3DArenaBounds)
{
    public override string ToString() => $"{nameof(ArenaBoundsSquare)}, Radius {Radius}, HalfWidth: {HalfWidth}, MapResolution: {MapResolution}, ScaleFactor: {ScaleFactor}";
}

// Optional 3D-world projection layer for vertically complex custom arenas. Shape uses the same
// arena-local X/Z coordinate system as ArenaBoundsCustom.Polygon; Y is the world-space floor
// reference used by terrain projection. NaN ProjectionHeight inherits its ArenaBounds value.
// BorderY optionally places the 3D border on a different plane and otherwise inherits Y.
// PathfindOffset is applied only to this layer's cached pathfinding source; NaN inherits the custom
// arena's global Offset value. Layers with the same non-null Shared2DGroup share one combined 2D
// stencil/border, restriction domain and pathfinding map while retaining independent physical Y.
// NaN WorldProjectionHoleFillRadius inherits ArenaBounds.WorldProjectionHoleFillRadius. Null
// ArenaStencilExclusions inherits ArenaBoundsCustom.ArenaStencilExclusions; an empty array
// explicitly disables the inherited exclusions for this layer.
public readonly struct ArenaProjectionLayer(RelSimplifiedComplexPolygon shape, float y, float projectionHeight = float.NaN, float pathfindOffset = float.NaN, int? shared2DGroup = null, float worldProjectionHoleFillRadius = float.NaN, float borderY = float.NaN, Shape[]? arenaStencilExclusions = null)
{
    public readonly RelSimplifiedComplexPolygon Shape = shape;
    public readonly float Y = y;
    public readonly float ProjectionHeight = projectionHeight;
    public readonly float PathfindOffset = pathfindOffset;
    public readonly int? Shared2DGroup = shared2DGroup;
    public readonly float WorldProjectionHoleFillRadius = worldProjectionHoleFillRadius;
    public readonly float BorderY = borderY;
    public readonly Shape[]? ArenaStencilExclusions = arenaStencilExclusions;
}

// custom complex polygon bounds
// for creating complex bounds by using arrays of shapes
// first array contains platforms that will be united, second optional array contains shapes that will be subtracted
// for convenience third array will optionally perform additional unions at the end
// ArenaStencilExclusions use the same X/Z coordinate system as the constructor's Shape arrays, but
// are subtracted only from the immutable 3D-world projection clip.
// offset shrinks the pathfinding map only, for example if the edges of the arena are deadly and floating point errors cause the AI to fall of the map or problems like that
// AdjustForHitbox adjusts both the visible map and the pathfinding map (ignores additional unions)
[SkipLocalsInit]
public sealed class ArenaBoundsCustom : ArenaBounds
{
    private Pathfinding.Map? _cachedMap;
    private Pathfinding.Map?[]? _cachedLayerMaps;
    private readonly RelSimplifiedComplexPolygon[]? _projectionLayer2DShapes;
    private readonly RelSimplifiedComplexPolygon _worldProjectionClip;
    private readonly RelSimplifiedComplexPolygon[]? _worldProjectionLayerClips;
    // Null keeps the single-floor path. Polygon remains the global logical boundary used by
    // Contains/ClampToBounds/IntersectRay; authored layers opt the 2D renderer, world projection and
    // AI pathfinding source into layer-aware behavior without changing those geometry calls.
    public readonly ArenaProjectionLayer[]? WorldProjectionLayers;
    // Render-only world-projection holes. These never participate in the 2D MiniArena or logical/
    // pathfinding geometry. Individual projection layers can inherit or override this array.
    public readonly Shape[] ArenaStencilExclusions;
    public readonly float HalfWidth, HalfHeight;
    private readonly float offset;
    public readonly WPos Center;

    public ArenaBoundsCustom(Shape[] UnionShapes, Shape[]? DifferenceShapes = null, Shape[]? AdditionalShapes = null, float MapResolution = 0.5f, float ScaleFactor = 1f, bool AllowObstacleMap = false, float Offset = default, bool AdjustForHitboxInwards = false, bool AdjustForHitboxOutwards = false, ArenaProjectionLayer[]? WorldProjectionLayers = null, Shape[]? ArenaStencilExclusions = null, bool AllowDrawing3DArenaBounds = true)
    : base(BuildBounds(UnionShapes, DifferenceShapes ?? [], AdditionalShapes ?? [], ScaleFactor, AdjustForHitboxInwards, AdjustForHitboxOutwards, out var poly, out var center, out var halfWidth, out var halfHeight), MapResolution, ScaleFactor, AllowObstacleMap, AllowDrawing3DArenaBounds)
    {
        Center = center;
        HalfWidth = halfWidth + Offset;
        HalfHeight = halfHeight + Offset;
        Shape = poly;
        this.WorldProjectionLayers = WorldProjectionLayers;
        this.ArenaStencilExclusions = ArenaStencilExclusions is { Length: > 0 } exclusions ? [.. exclusions] : [];
        _projectionLayer2DShapes = BuildProjectionLayer2DShapes(WorldProjectionLayers);
        _worldProjectionClip = BuildWorldProjectionClip(poly, this.ArenaStencilExclusions, Center);
        _worldProjectionLayerClips = BuildWorldProjectionLayerClips(WorldProjectionLayers, this.ArenaStencilExclusions, Center);
        offset = Offset;
    }

    private static float BuildBounds(Shape[] unionShapes, Shape[]? differenceShapes, Shape[]? additionalShapes, float scalefactor, bool adjustForHitboxInwards, bool adjustForHitboxOutwards, out RelSimplifiedComplexPolygon poly, out WPos center, out float halfWidth, out float halfHeight)
    {
        var properties = CalculatePolygonProperties(unionShapes, differenceShapes ?? [], additionalShapes ?? [], adjustForHitboxInwards, adjustForHitboxOutwards);
        center = properties.Center;
        halfWidth = properties.HalfWidth;
        halfHeight = properties.HalfHeight;
        poly = properties.Poly;
        return scalefactor == 1f ? properties.Radius : properties.Radius / scalefactor;
    }

    private static (WPos Center, float HalfWidth, float HalfHeight, float Radius, RelSimplifiedComplexPolygon Poly) CalculatePolygonProperties(Shape[] unionShapes, Shape[] differenceShapes, Shape[] additionalShapes, bool adjustForHitboxInwards, bool adjustForHitboxOutwards)
    {
        var unionPolygons = ParseShapes(unionShapes);
        var differencePolygons = ParseShapes(differenceShapes);
        var additionalPolygons = ParseShapes(additionalShapes);
        var combinedPoly = CombinePolygons(unionPolygons, differencePolygons, additionalPolygons, adjustForHitboxInwards ? -0.5f : adjustForHitboxOutwards ? 0.5f : default);

        var props = CalculateCenterAndRecenter(combinedPoly);
        var center = props.Center;
        var maxX = props.maxX;
        var minX = props.minX;
        var maxZ = props.maxZ;
        var minZ = props.minZ;
        var centerX = center.X;
        var centerZ = center.Z;
        var maxDistX = Math.Max(Math.Abs(maxX - centerX), Math.Abs(minX - centerX));
        var maxDistZ = Math.Max(Math.Abs(maxZ - centerZ), Math.Abs(minZ - centerZ));
        var halfWidth = (maxX - minX) * 0.5f;
        var halfHeight = (maxZ - minZ) * 0.5f;

        return (center, halfWidth, halfHeight, Math.Max(maxDistX, maxDistZ), combinedPoly);

        static RelSimplifiedComplexPolygon[] ParseShapes(Shape[] shapes)
        {
            var lenght = shapes.Length;
            var polygons = new RelSimplifiedComplexPolygon[lenght];
            for (var i = 0; i < lenght; ++i)
            {
                polygons[i] = shapes[i].ToPolygon(default);
            }
            return polygons;
        }
    }

    public override void PathfindMap(Pathfinding.Map map, WPos center)
    {
        var source = _cachedMap ??= BuildMap(Shape, offset);
        map.Init(source, center + source.Center.ToWDir());
    }

    // Initializes from one authored layer when the ID is valid, otherwise preserves the legacy
    // global-polygon behavior. Layers in one Shared2DGroup use the same lazily built source map.
    public void PathfindMap(Pathfinding.Map map, WPos center, int? layerID)
    {
        if (layerID is not int index || WorldProjectionLayers is not { Length: > 0 } layers || (uint)index >= (uint)layers.Length)
        {
            PathfindMap(map, center);
            return;
        }

        var cached = _cachedLayerMaps ??= new Pathfinding.Map?[layers.Length];
        var source = cached[index];
        ref readonly var layer = ref layers[index];
        if (source == null)
        {
            if (layer.Shared2DGroup is int sharedGroup)
            {
                source = BuildProjectionLayerGroupMap(layers, sharedGroup);
                for (var i = 0; i < layers.Length; ++i)
                {
                    if (layers[i].Shared2DGroup == sharedGroup)
                    {
                        cached[i] = source;
                    }
                }
            }
            else
            {
                var pathfindOffset = !float.IsNaN(layer.PathfindOffset) ? layer.PathfindOffset : offset;
                source = cached[index] = BuildMap(layer.Shape, pathfindOffset);
            }
        }
        map.Init(source, center + source.Center.ToWDir());
    }

    public bool IsValidProjectionLayer(int? layerID)
        => layerID is int index && WorldProjectionLayers is { Length: > 0 } layers && (uint)index < (uint)layers.Length;

    // Returns the combined presentation polygon for a layer. Invalid IDs intentionally fall back to
    // the global logical polygon
    public RelSimplifiedComplexPolygon ProjectionLayer2DShape(int? layerID)
        => layerID is int index && _projectionLayer2DShapes != null && (uint)index < (uint)_projectionLayer2DShapes.Length
            ? _projectionLayer2DShapes[index]
            : Shape;

    // Returns the immutable world-only clip for one physical projection layer. Invalid/null IDs use
    // the global custom-arena polygon and exclusions
    public RelSimplifiedComplexPolygon WorldProjectionClip(int? layerID = null)
        => layerID is int index && _worldProjectionLayerClips != null && (uint)index < (uint)_worldProjectionLayerClips.Length
            ? _worldProjectionLayerClips[index]
            : _worldProjectionClip;

    // Ungrouped layers remain isolated. A shared non-null group turns several physical floors into
    // one 2D visibility/restriction domain without changing their world-projection Y or clip shape.
    public bool ProjectionLayersShare2DGroup(int? firstLayerID, int? secondLayerID)
    {
        if (firstLayerID is not int first || secondLayerID is not int second || WorldProjectionLayers is not { Length: > 0 } layers
            || (uint)first >= (uint)layers.Length || (uint)second >= (uint)layers.Length)
        {
            return false;
        }
        if (first == second)
        {
            return true;
        }
        return layers[first].Shared2DGroup is int group && layers[second].Shared2DGroup == group;
    }

    // Finds the nearest authored floor. Supplying a valid current index applies hysteresis, which
    // keeps ordinary jumps around a floor midpoint from rapidly switching the active layer.
    public int ResolveProjectionLayer(float y, int currentLayer = -1, float switchHysteresis = 0.75f)
    {
        if (WorldProjectionLayers is not { Length: > 0 } layers)
        {
            return -1;
        }

        var nearest = 0;
        var nearestDelta = Math.Abs(y - layers[0].Y);
        for (var i = 1; i < layers.Length; ++i)
        {
            var delta = Math.Abs(y - layers[i].Y);
            if (delta < nearestDelta)
            {
                nearest = i;
                nearestDelta = delta;
            }
        }

        if ((uint)currentLayer >= (uint)layers.Length || currentLayer == nearest)
        {
            return nearest;
        }

        var currentDelta = Math.Abs(y - layers[currentLayer].Y);
        return nearestDelta + Math.Max(0f, switchHysteresis) < currentDelta ? nearest : currentLayer;
    }

    // Disjoint floors can share the same elevation (for example, islands joined by teleporters), so
    // prefer polygons containing the actor's X/Z position. If several polygons contain it, they are
    // vertically overlapping floors and the normal Y/hysteresis rule selects between that subset.
    // Outside every authored polygon (during a teleport/fall), fall back to the Y-only rule.
    public int ResolveProjectionLayer(in WDir positionOffset, float y, int currentLayer = -1, float switchHysteresis = 0.75f)
    {
        if (WorldProjectionLayers is not { Length: > 0 } layers)
        {
            return -1;
        }

        var nearest = -1;
        var nearestDelta = float.MaxValue;
        var currentContains = false;
        for (var i = 0; i < layers.Length; ++i)
        {
            var shape = layers[i].Shape;
            shape.VerifyPolygonIndexExistance();
            if (!shape.Contains(positionOffset))
            {
                continue;
            }

            currentContains |= i == currentLayer;
            var delta = Math.Abs(y - layers[i].Y);
            if (delta < nearestDelta)
            {
                nearest = i;
                nearestDelta = delta;
            }
        }

        if (nearest < 0)
        {
            return ResolveProjectionLayer(y, currentLayer, switchHysteresis);
        }
        if (!currentContains || currentLayer == nearest)
        {
            return nearest;
        }

        var currentDelta = Math.Abs(y - layers[currentLayer].Y);
        return nearestDelta + Math.Max(0f, switchHysteresis) < currentDelta ? nearest : currentLayer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Contains(in WDir offset) => Shape.Contains(offset);

    // useful to get forbidden directions if the player is origin of a self knockback
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddForbiddenDirections(in WDir centerOffset, Angle offset, AIHints hints, DateTime activation, float forbiddenDist, float safetyMargin = 1f) => Shape.AddForbiddenDirections(centerOffset, offset, hints, activation, forbiddenDist, safetyMargin);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override float IntersectRay(in WDir originOffset, in WDir dir) => Intersect.RayPolygon(originOffset, dir, Shape);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override WDir ClampToBounds(in WDir offset)
    {
        if (offset.AlmostEqual(default, 1f) || Math.Abs(offset.X) < 0.1f) // if actor is almost in the center of the arena, do nothing (eg donut arena or wall boss)
        {
            return offset;
        }
        return Shape.ClosestPointOnBoundary(offset);
    }

    private Pathfinding.Map BuildMap(RelSimplifiedComplexPolygon sourcePolygon, float pathfindOffset)
    {
        var polygon = pathfindOffset != default ? sourcePolygon.Offset(pathfindOffset) : sourcePolygon;
        var resolution = MapResolution;

        var bounds = CalculateOptimalGridBounds(polygon, resolution);

        // The axis-aligned candidate is evaluated first and wins all equal-cell-count ties. If it wins, keep the
        // polygon in its existing coordinate frame so offset == 0 can reuse Polygon's already-built boundary index.
        // Only a rotation that strictly reduces the pathfinding cell count pays for a transform.
        if (bounds.RequiresTransform)
        {
            polygon = TransformToGrid(polygon, bounds);
        }

        var map = new Pathfinding.Map();
        var height = bounds.Height;
        var width = bounds.Width;
        map.InitGrid(resolution, bounds.Center, width, height, bounds.Rotation);

        var pixelMaxG = map.PixelMaxG;
        var pixelPriority = map.PixelPriority;
        // var startTime = Stopwatch.GetTimestamp();
        // for (var i = 0; i < 10000; ++i)
        // {
        var halfCell = resolution * 0.49999f;
        var dx = new WDir(resolution, default);
        var dy = new WDir(default, resolution);

        // Transformed polygons are in map-local coordinates, so their grid is centered around local zero. Unrotated
        // polygons remain in arena-relative coordinates and can reuse their existing index; include Map.Center in the
        // raster origin so odd-grid parity shifts (and offset-polygon bbox shifts) need no polygon transformation

        var rasterCenter = bounds.RequiresTransform ? default : map.Center.ToWDir();
        var startPos = rasterCenter - ((width >> 1) - 0.5f) * dx - ((height >> 1) - 0.5f) * dy;

        // Reuse a full index when one already exists (the global custom-arena polygon normally has one).
        // Offset, transformed and layer-only polygons otherwise get a temporary six-field index containing
        // exactly what ClassifyAABBRect needs
        var existingRasterIndex = polygon.ExistingPolygonIndex;
        using var lightweightRasterIndex = existingRasterIndex == null ? PolygonBoundaryIndex2D.BuildForAABBRectClassification(polygon) : null;
        var rasterIndex = existingRasterIndex ?? lightweightRasterIndex!;

        Parallel.ForEach(Partitioner.Create(0, height), range =>
        {
            var r1 = range.Item1;
            var r2 = range.Item2;

            for (var y = r1; y < r2; ++y)
            {
                var rowOffset = y * width;
                var posY = startPos + y * dy;

                for (var x = 0; x < width; ++x)
                {
                    var cellCenter = posY + x * dx;
                    var relation = rasterIndex.ClassifyAABBRect(cellCenter, halfCell, halfCell);
                    if (relation == PolygonShapeRelation.Inside)
                    {
                        continue;
                    }

                    pixelMaxG[rowOffset + x] = -1000f;
                    pixelPriority[rowOffset + x] = float.MinValue;
                }
            }
        });
        // }
        // var rasterFinish = Stopwatch.GetTimestamp();
        // Service.Log($"raster time: {(rasterFinish - startTime) * 1000d / Stopwatch.Frequency}ms");
        CropRasterizedMap(map);
        return map;
    }

    // Offsets are applied per physical floor before unioning, allowing grouped islands to keep
    // independent edge cushions while still producing one source grid for teleporter pathfinding.
    private Pathfinding.Map BuildProjectionLayerGroupMap(ArenaProjectionLayer[] layers, int sharedGroup)
    {
        var operand = new PolygonClipper.Operand();
        var len = layers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var layer = ref layers[i];
            if (layer.Shared2DGroup != sharedGroup)
            {
                continue;
            }
            var pathfindOffset = !float.IsNaN(layer.PathfindOffset) ? layer.PathfindOffset : offset;
            operand.AddPolygon(pathfindOffset != default ? layer.Shape.Offset(pathfindOffset) : layer.Shape);
        }
        var combined = new PolygonClipper().Simplify(operand);
        return BuildMap(combined, default);
    }

    private static RelSimplifiedComplexPolygon BuildWorldProjectionClip(RelSimplifiedComplexPolygon baseShape, Shape[] exclusions, WPos center)
    {
        var len = exclusions.Length;
        if (len == 0)
        {
            return baseShape;
        }

        var remove = new PolygonClipper.Operand();

        for (var i = 0; i < len; ++i)
        {
            remove.AddPolygon(exclusions[i].ToPolygon(center));
        }
        return new PolygonClipper().Difference(new PolygonClipper.Operand(baseShape), remove);
    }

    private static RelSimplifiedComplexPolygon[]? BuildWorldProjectionLayerClips(ArenaProjectionLayer[]? layers, Shape[] globalExclusions, WPos center)
    {
        if (layers is not { Length: > 0 })
        {
            return null;
        }
        var len = layers.Length;
        var result = new RelSimplifiedComplexPolygon[len];
        for (var i = 0; i < len; ++i)
        {
            ref readonly var layer = ref layers[i];
            result[i] = BuildWorldProjectionClip(layer.Shape, layer.ArenaStencilExclusions ?? globalExclusions, center);
        }
        return result;
    }

    private static RelSimplifiedComplexPolygon[]? BuildProjectionLayer2DShapes(ArenaProjectionLayer[]? layers)
    {
        if (layers is not { Length: > 0 })
        {
            return null;
        }
        var len = layers.Length;
        var result = new RelSimplifiedComplexPolygon[len];
        for (var i = 0; i < len; ++i)
        {
            if (result[i] != null)
            {
                continue;
            }

            if (layers[i].Shared2DGroup is not int sharedGroup)
            {
                result[i] = layers[i].Shape;
                continue;
            }

            var operand = new PolygonClipper.Operand();
            var members = 0;
            for (var j = i; j < len; ++j)
            {
                if (layers[j].Shared2DGroup == sharedGroup)
                {
                    operand.AddPolygon(layers[j].Shape);
                    ++members;
                }
            }
            var combined = members == 1 ? layers[i].Shape : new PolygonClipper().Simplify(operand);
            for (var j = i; j < len; ++j)
            {
                if (layers[j].Shared2DGroup == sharedGroup)
                {
                    result[j] = combined;
                }
            }
        }
        return result;
    }

    private static int GridExtent(float extent, float resolution)
    {
        var cells = extent / resolution;
        var nearest = MathF.Round(cells);
        if (MathF.Abs(cells - nearest) <= 0.001f)
        {
            cells = nearest;
        }

        // A neighboring cell beyond this centered interval necessarily crosses the oriented bounding box, so it
        // can never survive the conservative full-cell-inside-polygon test
        return (int)MathF.Floor(cells);
    }

    private static RelSimplifiedComplexPolygon CombinePolygons(RelSimplifiedComplexPolygon[] unionPolygons, RelSimplifiedComplexPolygon[] differencePolygons, RelSimplifiedComplexPolygon[] secondUnionPolygons, float offset)
    {
        var clipper = new PolygonClipper();
        var operandUnion = new PolygonClipper.Operand();
        var operandDifference = new PolygonClipper.Operand();
        var operandSecondUnion = new PolygonClipper.Operand();

        var unionLen = unionPolygons.Length;
        for (var i = 0; i < unionLen; ++i)
        {
            operandUnion.AddPolygon(unionPolygons[i]);
        }
        var differenceLen = differencePolygons.Length;
        for (var i = 0; i < differenceLen; ++i)
        {
            operandDifference.AddPolygon(differencePolygons[i]);
        }
        var secUnionLen = secondUnionPolygons.Length;
        for (var i = 0; i < secUnionLen; ++i)
        {
            operandSecondUnion.AddPolygon(secondUnionPolygons[i]);
        }

        var combinedShape = clipper.Difference(operandUnion, operandDifference);
        var polyAdjust = offset != default ? combinedShape.Offset(offset, Clipper2Lib.JoinType.Round) : combinedShape;
        if (secUnionLen != 0)
        {
            polyAdjust = clipper.Union(new PolygonClipper.Operand(polyAdjust), operandSecondUnion);
        }
        return polyAdjust;
    }

    public override string ToString()
    {
        var parts = Shape.Parts;
        var count = parts.Count;
        var vertsCount = 0;
        for (var i = 0; i < count; ++i)
        {
            vertsCount += parts[i].Vertices.Count;
        }
        return $"{nameof(ArenaBoundsCustom)}, Radius {Radius}, HalfWidth: {HalfWidth}, HalfHeight: {HalfHeight}, MapResolution: {MapResolution}, Pathfinding offset: {offset}, Vertices: {vertsCount}, ScaleFactor: {ScaleFactor}";
    }

    private readonly struct OrientedGridBounds(WPos center, Angle rotation, int width, int height, bool requiresTransform)
    {
        public readonly WPos Center = center;
        public readonly Angle Rotation = rotation;
        public readonly int Width = width;
        public readonly int Height = height;
        public readonly bool RequiresTransform = requiresTransform;
    }

    // Find a compact oriented bounding rectangle. The exact minimum-area rectangle has an axis parallel to a
    // convex-hull edge. The axis-aligned candidate is evaluated first and is retained for every equal-cell-count tie,
    // allowing BuildMap to reuse the existing polygon/index. A rotated candidate is selected only when it strictly
    // reduces the pathfinding cell count; geometric area is only a tie-breaker between already-rotated candidates.
    //
    // Candidate extents use rotating calipers: after one initialization scan, the four support vertices (min/max on
    // each local axis) move monotonically around the convex hull as its edge direction rotates.
    // Grid dimensions are exact integers and may be odd. Shape-specific dead outer layers are removed after raster.
    private OrientedGridBounds CalculateOptimalGridBounds(RelSimplifiedComplexPolygon poly, float resolution)
    {
        var totalVertices = 0;
        var maxExteriorVertices = 0;
        var parts = poly.Parts;
        var partCount = parts.Count;

        for (var i = 0; i < partCount; ++i)
        {
            var count = parts[i].Exterior.Length;
            totalVertices += count;
            if (count > maxExteriorVertices)
            {
                maxExteriorVertices = count;
            }
        }

        // Each exterior is already a simple polygon contour in boundary order. Build its convex hull directly in
        // linear time with Melkman's deque algorithm before doing any global sort. Highly concave arena contours
        // commonly collapse from hundreds of vertices to only a few dozen hull vertices, so for multiple parts the
        // global monotone-chain sort sees only the union of those small per-part hulls. The common single-part case
        // skips sorting entirely.
        Span<WDir> hullStorage = stackalloc WDir[totalVertices * 2];
        Span<WDir> contourWorkspace = stackalloc WDir[maxExteriorVertices * 2 + 1];
        int hullCount;

        if (partCount == 1)
        {
            hullCount = BuildSimplePolygonHull(parts[0].Exterior, hullStorage, contourWorkspace);
        }
        else
        {
            Span<WDir> points = stackalloc WDir[totalVertices];
            var pointIndex = 0;
            for (var i = 0; i < partCount; ++i)
            {
                var exterior = parts[i].Exterior;
                pointIndex += BuildSimplePolygonHull(exterior, points[pointIndex..], contourWorkspace);
            }

            var reducedPoints = points[..pointIndex];
            reducedPoints.Sort(static (a, b) =>
            {
                var cmp = a.X.CompareTo(b.X);
                return cmp != 0 ? cmp : a.Z.CompareTo(b.Z);
            });
            hullCount = BuildConvexHull(reducedPoints, hullStorage);
        }

        var hull = hullStorage[..hullCount];

        var bestCellCount = long.MaxValue;
        var bestArea = float.MaxValue;
        OrientedGridBounds best = default;

        void EvaluateBounds(float minX, float maxX, float minZ, float maxZ, WDir xAxis, WDir zAxis, bool requiresTransform)
        {
            var extentX = maxX - minX;
            var extentZ = maxZ - minZ;
            var gridWidth = GridExtent(extentX, resolution);
            var gridHeight = GridExtent(extentZ, resolution);
            var cellCount = (long)gridWidth * gridHeight;
            var area = extentX * extentZ;

            // Never replace the unrotated baseline for an equal cell count: the transformed polygon would have the
            // same per-frame pathfinding footprint while BuildMap would pay an allocation, transform and full index
            // rebuild. Once a rotated candidate has strictly beaten the baseline, retain geometric area as a
            // deterministic tie-breaker between other rotated candidates with that same smaller cell count.
            if (cellCount > bestCellCount || cellCount == bestCellCount && (!best.RequiresTransform || area >= bestArea))
            {
                return;
            }

            var geometricCenter = xAxis * ((minX + maxX) * 0.5f) + zAxis * ((minZ + maxZ) * 0.5f);
            var rotation = zAxis.ToAngle();

            // Only transformed candidates may be transposed to make the larger dimension rows. Keeping the baseline
            // exactly axis-aligned is what permits reuse of the original polygon/index when rotation saves no cells.
            if (requiresTransform && gridWidth > gridHeight)
            {
                (gridWidth, gridHeight) = (gridHeight, gridWidth);
                rotation += 90f.Degrees();
            }

            var dir = rotation.ToDirection();
            var mapCenter = geometricCenter;
            if ((gridWidth & 1) != 0)
            {
                mapCenter -= 0.5f * resolution * dir.OrthoL();
            }
            if ((gridHeight & 1) != 0)
            {
                mapCenter -= 0.5f * resolution * dir;
            }

            bestCellCount = cellCount;
            bestArea = area;
            best = new(mapCenter.ToWPos(), rotation, gridWidth, gridHeight, requiresTransform);
        }

        // Establish the exact unrotated baseline first. Equal-cell-count rotated candidates can never displace it.
        if (hullCount != 0)
        {
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;
            for (var i = 0; i < hullCount; ++i)
            {
                var p = hull[i];
                if (p.X < minX)
                {
                    minX = p.X;
                }
                if (p.X > maxX)
                {
                    maxX = p.X;
                }
                if (p.Z < minZ)
                {
                    minZ = p.Z;
                }
                if (p.Z > maxZ)
                {
                    maxZ = p.Z;
                }
            }
            EvaluateBounds(minX, maxX, minZ, maxZ, new WDir(1f, 0f), new WDir(0f, 1f), false);
        }

        // Initialize the four support points for the first non-degenerate hull edge, then advance each one only
        // forward as subsequent edge axes rotate around the CCW hull.
        var firstEdge = -1;
        WDir firstXAxis = default;
        WDir firstZAxis = default;
        for (var i = 0; i < hullCount; ++i)
        {
            var edge = hull[(i + 1) % hullCount] - hull[i];
            var lenSq = edge.LengthSq();
            if (lenSq > 1e-12f)
            {
                firstEdge = i;
                firstXAxis = edge / MathF.Sqrt(lenSq);
                firstZAxis = firstXAxis.OrthoR();
                break;
            }
        }

        if (firstEdge >= 0)
        {
            FindSupportIndices(hull, firstXAxis, firstZAxis, out var minXIndex, out var maxXIndex, out var minZIndex, out var maxZIndex);

            for (var step = 0; step < hullCount; ++step)
            {
                var i = (firstEdge + step) % hullCount;
                var edge = hull[(i + 1) % hullCount] - hull[i];
                var lenSq = edge.LengthSq();
                if (lenSq <= 1e-5f)
                {
                    continue;
                }

                var xAxis = edge / MathF.Sqrt(lenSq);
                var zAxis = xAxis.OrthoR();

                if (step != 0)
                {
                    minXIndex = AdvanceSupport(hull, minXIndex, xAxis, false);
                    maxXIndex = AdvanceSupport(hull, maxXIndex, xAxis, true);
                    minZIndex = AdvanceSupport(hull, minZIndex, zAxis, false);
                    maxZIndex = AdvanceSupport(hull, maxZIndex, zAxis, true);
                }

                EvaluateBounds(hull[minXIndex].Dot(xAxis), hull[maxXIndex].Dot(xAxis),
                    hull[minZIndex].Dot(zAxis), hull[maxZIndex].Dot(zAxis),
                    xAxis, zAxis, true);
            }
        }

        return best;

        static void FindSupportIndices(ReadOnlySpan<WDir> hull, WDir xAxis, WDir zAxis, out int minXIndex, out int maxXIndex, out int minZIndex, out int maxZIndex)
        {
            minXIndex = maxXIndex = minZIndex = maxZIndex = 0;
            var hull0 = hull[0];
            var minX = hull0.Dot(xAxis);
            var maxX = minX;
            var minZ = hull0.Dot(zAxis);
            var maxZ = minZ;

            var len = hull.Length;
            for (var i = 1; i < len; ++i)
            {
                var p = hull[i];
                var px = p.Dot(xAxis);
                var pz = p.Dot(zAxis);
                if (px < minX)
                {
                    minX = px;
                    minXIndex = i;
                }
                if (px > maxX)
                {
                    maxX = px;
                    maxXIndex = i;
                }
                if (pz < minZ)
                {
                    minZ = pz;
                    minZIndex = i;
                }
                if (pz > maxZ)
                {
                    maxZ = pz;
                    maxZIndex = i;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int AdvanceSupport(ReadOnlySpan<WDir> hull, int index, WDir axis, bool findMax)
        {
            var count = hull.Length;
            while (true)
            {
                var next = index + 1;
                if (next == count)
                {
                    next = 0;
                }

                var currentProjection = hull[index].Dot(axis);
                var nextProjection = hull[next].Dot(axis);
                if (findMax ? nextProjection <= currentProjection : nextProjection >= currentProjection)
                {
                    return index;
                }
                index = next;
            }
        }

        // Linear convex hull for a simple polygon/polyline whose vertices are already in boundary order.
        // The deque stores the first hull vertex twice (at both ends) while processing; output is compacted without
        // the duplicate closing vertex and normalized to CCW order because the rotating-calipers pass relies on it.
        static int BuildSimplePolygonHull(ReadOnlySpan<WDir> polygon, Span<WDir> output, Span<WDir> deque)
        {
            var count = polygon.Length;
            // Start at any non-collinear cyclic triple. This avoids special handling for contours that begin with a
            // run of collinear edges while keeping the remaining input a simple boundary-ordered polyline.
            var start = -1;
            for (var i = 0; i < count; ++i)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % count];
                var c = polygon[(i + 2) % count];
                if ((b - a).Cross(c - b) != 0f)
                {
                    start = i;
                    break;
                }
            }

            // Degenerate all-collinear contour: the convex hull is just its two lexicographic extremes.
            if (start < 0)
            {
                var min = polygon[0];
                var max = min;
                var minX = min.X;
                var maxX = max.X;
                var minZ = min.Z;
                var maxZ = max.Z;

                for (var i = 1; i < count; ++i)
                {
                    var p = polygon[i];
                    var pX = p.X;
                    var pZ = p.Z;
                    if (pX < minX || pX == minX && pZ < minZ)
                    {
                        min = p;
                    }
                    if (pX > maxX || pX == maxX && pZ > maxZ)
                    {
                        max = p;
                    }
                }
                output[0] = min;
                if (max == min)
                {
                    return 1;
                }
                output[1] = max;
                return 2;
            }

            var p0 = polygon[start];
            var p1 = polygon[(start + 1) % count];
            var p2 = polygon[(start + 2) % count];
            var bottom = count - 2;
            var top = count + 1;

            if ((p1 - p0).Cross(p2 - p1) > 0f)
            {
                deque[bottom] = p2;
                deque[bottom + 1] = p0;
                deque[bottom + 2] = p1;
                deque[top] = p2;
            }
            else
            {
                deque[bottom] = p2;
                deque[bottom + 1] = p1;
                deque[bottom + 2] = p0;
                deque[top] = p2;
            }

            for (var step = 3; step < count; ++step)
            {
                var p = polygon[(start + step) % count];

                // Strict left turns match BuildConvexHull below: collinear boundary points are discarded.
                if (IsLeft(deque[bottom], deque[bottom + 1], p) && IsLeft(deque[top - 1], deque[top], p))
                {
                    continue;
                }

                while (!IsLeft(deque[bottom], deque[bottom + 1], p))
                {
                    ++bottom;
                }
                deque[--bottom] = p;

                while (!IsLeft(deque[top - 1], deque[top], p))
                {
                    --top;
                }
                deque[++top] = p;
            }

            // deque[bottom] == deque[top]; omit the duplicate closing vertex
            var hullCount = top - bottom;
            var hull = deque.Slice(bottom, hullCount);
            hull.CopyTo(output);

            // Melkman can emit either winding depending on the source contour. Normalize to CCW for calipers
            var signedArea2 = 0f;
            for (var i = 0; i < hullCount; ++i)
            {
                signedArea2 += output[i].Cross(output[(i + 1) % hullCount]);
            }
            if (signedArea2 < 0f)
            {
                for (int i = 0, j = hullCount - 1; i < j; ++i, --j)
                {
                    (output[i], output[j]) = (output[j], output[i]);
                }
            }

            return hullCount;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static bool IsLeft(WDir a, WDir b, WDir p) => (b - a).Cross(p - b) > 0f;
        }

        static int BuildConvexHull(ReadOnlySpan<WDir> points, Span<WDir> hull)
        {
            var countP = points.Length;
            var hullCount = 0;

            // Lower hull
            for (var i = 0; i < countP; ++i)
            {
                var p = points[i];

                while (hullCount >= 2)
                {
                    var a = hull[hullCount - 2];
                    var b = hull[hullCount - 1];

                    if ((b - a).Cross(p - b) > 0f)
                    {
                        break;
                    }

                    --hullCount;
                }

                hull[hullCount++] = p;
            }

            // Upper hull
            var lowerCount = hullCount;

            for (var i = countP - 2; i >= 0; --i)
            {
                var p = points[i];

                while (hullCount > lowerCount)
                {
                    var a = hull[hullCount - 2];
                    var b = hull[hullCount - 1];

                    if ((b - a).Cross(p - b) > 0f)
                    {
                        break;
                    }

                    --hullCount;
                }

                hull[hullCount++] = p;
            }

            // Final point duplicates the first
            --hullCount;
            return hullCount;
        }
    }

    private static void CropRasterizedMap(Pathfinding.Map map)
    {
        var width = map.Width;
        var height = map.Height;
        if (width <= 1 && height <= 1)
        {
            return;
        }

        var pixelMaxG = map.PixelMaxG;

        bool RowHasPassableCell(int y, int x1 = 0, int x2 = -1)
        {
            if (x2 < 0)
            {
                x2 = width - 1;
            }
            var index = y * width + x1;
            for (var x = x1; x <= x2; ++x, ++index)
            {
                if (pixelMaxG[index] >= 0f)
                {
                    return true;
                }
            }
            return false;
        }

        bool ColumnHasPassableCell(int x, int y1, int y2)
        {
            var index = y1 * width + x;
            for (var y = y1; y <= y2; ++y, index += width)
            {
                if (pixelMaxG[index] >= 0f)
                {
                    return true;
                }
            }
            return false;
        }

        var minY = 0;
        while (minY < height && !RowHasPassableCell(minY))
        {
            ++minY;
        }

        var maxY = height - 1;
        while (maxY > minY && !RowHasPassableCell(maxY))
        {
            --maxY;
        }

        var minX = 0;
        while (minX < width && !ColumnHasPassableCell(minX, minY, maxY))
        {
            ++minX;
        }

        var maxX = width - 1;
        while (maxX > minX && !ColumnHasPassableCell(maxX, minY, maxY))
        {
            --maxX;
        }

        var newWidth = maxX - minX + 1;
        var newHeight = maxY - minY + 1;
        if (newWidth == width && newHeight == height)
        {
            return;
        }

        var pixelPriority = map.PixelPriority;
        for (var y = 0; y < newHeight; ++y)
        {
            var oldRow = (minY + y) * width + minX;
            var newRow = y * newWidth;
            Array.Copy(pixelMaxG, oldRow, pixelMaxG, newRow, newWidth);
            Array.Copy(pixelPriority, oldRow, pixelPriority, newRow, newWidth);
        }

        // Preserve the exact old cell lattice for arbitrary odd/even crops. Map.Center is the grid vertex at
        // floor(size/2), so shift by the difference between the retained old and new logical origins
        var shiftXCells = minX + (newWidth >> 1) - (width >> 1);
        var shiftYCells = minY + (newHeight >> 1) - (height >> 1);

        var dir = map.Rotation.ToDirection();
        var dx = dir.OrthoL() * map.Resolution;
        var dy = dir * map.Resolution;
        map.Center += shiftXCells * dx + shiftYCells * dy;

        map.Width = newWidth;
        map.Height = newHeight;
        map.MinX = map.MinY = 0;
        map.MaxX = newWidth - 1;
        map.MaxY = newHeight - 1;
    }

    private RelSimplifiedComplexPolygon TransformToGrid(RelSimplifiedComplexPolygon poly, in OrientedGridBounds bounds)
    {
        var inverseRotation = (-bounds.Rotation).ToDirection();
        var offset = -bounds.Center.ToWDir().Rotate(inverseRotation);
        return poly.Transform(offset, inverseRotation);
    }

    private static (float minX, float maxX, float minZ, float maxZ, WPos Center) CalculateCenterAndRecenter(RelSimplifiedComplexPolygon poly)
    {
        var minX = float.MaxValue;
        var maxX = float.MinValue;
        var minZ = float.MaxValue;
        var maxZ = float.MinValue;
        var parts = poly.Parts;
        var count = parts.Count;

        if (Avx.IsSupported)
        {
            var vectorMin = Vector256.Create(float.PositiveInfinity);
            var vectorMax = Vector256.Create(float.NegativeInfinity);
            var countV = Vector256<float>.Count;
            for (var i = 0; i < count; ++i)
            {
                var values = MemoryMarshal.Cast<WDir, float>(parts[i].Exterior);
                var len = values.Length;
                var j = 0;
                if (len >= countV)
                {
                    ref var source = ref MemoryMarshal.GetReference(values);
                    var vectorEnd = len - countV;
                    for (; j <= vectorEnd; j += countV)
                    {
                        var v = Vector256.LoadUnsafe(ref source, (nuint)j);
                        vectorMin = Avx.Min(vectorMin, v);
                        vectorMax = Avx.Max(vectorMax, v);
                    }
                }

                // Every WDir contributes two floats, so a vector-sized prefix also leaves an even-sized tail
                for (; j < len; j += 2)
                {
                    var x = values[j];
                    var z = values[j + 1];
                    if (x < minX)
                    {
                        minX = x;
                    }
                    if (x > maxX)
                    {
                        maxX = x;
                    }
                    if (z < minZ)
                    {
                        minZ = z;
                    }
                    if (z > maxZ)
                    {
                        maxZ = z;
                    }
                }
            }

            // X occupies even lanes and Z odd lanes in the interleaved WDir layout
            for (var lane = 0; lane < countV; lane += 2)
            {
                var xMin = vectorMin.GetElement(lane);
                var xMax = vectorMax.GetElement(lane);
                var zMin = vectorMin.GetElement(lane + 1);
                var zMax = vectorMax.GetElement(lane + 1);
                if (xMin < minX)
                {
                    minX = xMin;
                }
                if (xMax > maxX)
                {
                    maxX = xMax;
                }
                if (zMin < minZ)
                {
                    minZ = zMin;
                }
                if (zMax > maxZ)
                {
                    maxZ = zMax;
                }
            }
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                var ext = parts[i].Exterior;
                var len = ext.Length;
                for (var j = 0; j < len; ++j)
                {
                    var vertex = ext[j];
                    var vX = vertex.X;
                    var vZ = vertex.Z;
                    if (vX < minX)
                    {
                        minX = vX;
                    }
                    if (vX > maxX)
                    {
                        maxX = vX;
                    }
                    if (vZ < minZ)
                    {
                        minZ = vZ;
                    }
                    if (vZ > maxZ)
                    {
                        maxZ = vZ;
                    }
                }
            }
        }

        var center = new WPos((minX + maxX) * 0.5f, (minZ + maxZ) * 0.5f);
        var dir = center.ToWDir();

        if (Avx.IsSupported)
        {
            var countV = Vector256<float>.Count;
            var centerX = center.X;
            var centerZ = center.Z;

            var centerVector = Vector256.Create(centerX, centerZ, centerX, centerZ, centerX, centerZ, centerX, centerZ);
            for (var i = 0; i < count; ++i)
            {
                var values = MemoryMarshal.Cast<WDir, float>(CollectionsMarshal.AsSpan(parts[i].Vertices));
                var len = values.Length;
                var j = 0;
                if (len >= countV)
                {
                    ref var destination = ref MemoryMarshal.GetReference(values);
                    var vectorEnd = len - countV;
                    for (; j <= vectorEnd; j += countV)
                    {
                        var v = Vector256.LoadUnsafe(ref destination, (nuint)j);
                        Avx.Subtract(v, centerVector).StoreUnsafe(ref destination, (nuint)j);
                    }
                }
                for (; j < len; j += 2)
                {
                    values[j] -= centerX;
                    values[j + 1] -= centerZ;
                }
            }
        }
        else
        {
            for (var i = 0; i < count; ++i)
            {
                var verts = CollectionsMarshal.AsSpan(parts[i].Vertices);
                var len = verts.Length;
                for (var j = 0; j < len; ++j)
                {
                    verts[j] -= dir;
                }
            }
        }

        poly.InitPolygonIndex();
        return (minX, maxX, minZ, maxZ, center);
    }
}
