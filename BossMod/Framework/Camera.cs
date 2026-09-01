using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace BossMod;

[SkipLocalsInit]
sealed class Camera
{
    public static Camera? Instance;

    public Matrix4x4 ViewProj;
    public Vector4 NearPlane;
    public float CameraAzimuth; // facing north = 0, facing west = pi/4, facing south = +-pi/2, facing east = -pi/4
    public float CameraAltitude; // facing horizontally = 0, facing down = pi/4, facing up = -pi/4

    private enum WorldPrimitiveRunKind : byte { Lines, Curves, ProjectedArrows, ProjectedShapes, BillboardText }

    private struct WorldPrimitiveRun
    {
        public WorldPrimitiveRunKind Kind;
        public int Start;
        public int Count;
        public int CurveLineCount;
    }

    private readonly struct WorldProjectedShapeBinding(
        RelSimplifiedComplexPolygon? shapeSdf, WPos shapeSdfOrigin,
        RelSimplifiedComplexPolygon? arenaSdf, WPos arenaSdfOrigin)
    {
        public readonly RelSimplifiedComplexPolygon? ShapeSdf = shapeSdf;
        public readonly WPos ShapeSdfOrigin = shapeSdfOrigin;
        public readonly RelSimplifiedComplexPolygon? ArenaSdf = arenaSdf;
        public readonly WPos ArenaSdfOrigin = arenaSdfOrigin;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(in WorldProjectedShapeBinding other)
            => ReferenceEquals(ShapeSdf, other.ShapeSdf) && ShapeSdfOrigin == other.ShapeSdfOrigin
                && ReferenceEquals(ArenaSdf, other.ArenaSdf) && ArenaSdfOrigin == other.ArenaSdfOrigin;
    }

    private readonly struct WorldBillboardTextCommand(Vector3 center, string text, float renderSize, uint color, uint outlineColor, float outlineWidth, bool iconFont)
    {
        public readonly Vector3 Center = center;
        public readonly string Text = text;
        public readonly float RenderSize = renderSize;
        public readonly uint Color = color;
        public readonly uint OutlineColor = outlineColor;
        public readonly float OutlineWidth = outlineWidth;
        public readonly bool IconFont = iconFont;
    }

    private readonly List<Dx11ArenaRenderer.WorldLineInstance> _worldDrawLines = [];
    private readonly List<Dx11ArenaRenderer.WorldCurveInstance> _worldDrawCurves = [];
    private readonly List<Dx11ArenaRenderer.WorldProjectedArrowInstance> _worldProjectedArrows = [];
    private readonly List<Dx11ArenaRenderer.WorldProjectedShapeInstance> _worldProjectedShapes = [];
    private readonly List<WorldProjectedShapeBinding> _worldProjectedShapeBindings = [];
    private readonly List<WorldBillboardTextCommand> _worldBillboardTexts = [];
    private readonly List<WorldPrimitiveRun> _worldPrimitiveRuns = [];
    // Slot 0 is permanently reserved for identity. Keeping that invariant across frames removes an EnsureIdentity call from every single world-line submission and avoids re-adding the
    // identity transform after each batch
    private readonly List<Dx11ArenaRenderer.WorldLineTransform> _worldTransforms = [Dx11ArenaRenderer.WorldLineTransform.Identity];

    public unsafe void Update()
    {
        var controlCamera = CameraManager.Instance()->GetActiveCamera();
        var renderCamera = controlCamera != null ? controlCamera->SceneCamera.RenderCamera : null;
        if (renderCamera == null)
        {
            return;
        }

        ref var view = ref renderCamera->ViewMatrix;
        view.M44 = 1f; // for whatever reason, game doesn't initialize it...
        ref var proj = ref renderCamera->ProjectionMatrix;
        ViewProj = view * proj;
        // Game uses reverse-z. Keep the explicit world-space near plane used by the GPU line shader.
        NearPlane = new(view.M13, view.M23, view.M33, view.M43 + renderCamera->NearPlane);

        CameraAzimuth = MathF.Atan2(view.M13, view.M33);
        CameraAltitude = MathF.Asin(view.M23);
    }

    public void DrawWorldPrimitives()
    {
        var viewport = ImGuiHelpers.MainViewport;
        if (_worldPrimitiveRuns.Count == 0)
        {
            Dx11ArenaRenderer.QueueWorldOverlayPresent(ImGui.GetBackgroundDrawList(), viewport.Size);
            return;
        }

        var batchStarted = false;
        try
        {
            var rendererTransforms = _worldDrawLines.Count != 0 || _worldDrawCurves.Count != 0
                ? CollectionsMarshal.AsSpan(_worldTransforms) : [];
            var needsProjectedReceiverMask = _worldProjectedShapes.Count != 0 || _worldProjectedArrows.Count != 0;

            batchStarted = Dx11ArenaRenderer.BeginWorldBatch(ImGui.GetBackgroundDrawList(), viewport.Pos, viewport.Size, ViewProj, NearPlane,
                rendererTransforms, needsProjectedReceiverMask);
            if (!batchStarted)
            {
                return;
            }

            var lines = CollectionsMarshal.AsSpan(_worldDrawLines);
            var curves = CollectionsMarshal.AsSpan(_worldDrawCurves);
            var arrows = CollectionsMarshal.AsSpan(_worldProjectedArrows);
            var projectedShapes = CollectionsMarshal.AsSpan(_worldProjectedShapes);
            var projectedShapeBindings = CollectionsMarshal.AsSpan(_worldProjectedShapeBindings);
            var billboardTexts = CollectionsMarshal.AsSpan(_worldBillboardTexts);
            var runs = CollectionsMarshal.AsSpan(_worldPrimitiveRuns);
            var len = runs.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var run = ref runs[i];
                switch (run.Kind)
                {
                    case WorldPrimitiveRunKind.Lines:
                        Dx11ArenaRenderer.AppendWorldLines(lines.Slice(run.Start, run.Count));
                        break;
                    case WorldPrimitiveRunKind.Curves:
                        Dx11ArenaRenderer.AppendWorldCurves(curves.Slice(run.Start, run.Count), run.CurveLineCount);
                        break;
                    case WorldPrimitiveRunKind.ProjectedArrows:
                        Dx11ArenaRenderer.AppendWorldProjectedArrows(arrows.Slice(run.Start, run.Count));
                        break;
                    case WorldPrimitiveRunKind.ProjectedShapes:
                        var end = run.Start + run.Count;

                        // Keep draw order identical while bulk-submitting consecutive shapes that use the
                        // same custom/arena SDF resources. Standard AOEs normally share one arena binding
                        var groupStart = run.Start;
                        while (groupStart < end)
                        {
                            ref readonly var binding = ref projectedShapeBindings[groupStart];
                            var groupEnd = groupStart + 1;
                            while (groupEnd < end && binding.Matches(projectedShapeBindings[groupEnd]))
                            {
                                ++groupEnd;
                            }

                            Dx11ArenaRenderer.AppendWorldProjectedShapes(projectedShapes[groupStart..groupEnd],
                                binding.ShapeSdf, binding.ShapeSdfOrigin, binding.ArenaSdf, binding.ArenaSdfOrigin);
                            groupStart = groupEnd;
                        }
                        break;
                    case WorldPrimitiveRunKind.BillboardText:
                        var billboardEnd = run.Start + run.Count;
                        for (var textIndex = run.Start; textIndex < billboardEnd; ++textIndex)
                        {
                            ref readonly var command = ref billboardTexts[textIndex];
                            Dx11ArenaRenderer.AppendWorldTextBillboard(command.Center, command.Text, command.RenderSize, command.Color,
                                command.IconFont, command.OutlineColor, command.OutlineWidth);
                        }
                        break;
                }
            }
        }
        finally
        {
            if (batchStarted)
            {
                Dx11ArenaRenderer.EndScreenBatch();
            }
            _worldDrawLines.Clear();
            _worldDrawCurves.Clear();
            _worldProjectedArrows.Clear();
            _worldProjectedShapes.Clear();
            _worldProjectedShapeBindings.Clear();
            _worldBillboardTexts.Clear();
            _worldPrimitiveRuns.Clear();
            if (_worldTransforms.Count > 1)
            {
                CollectionsMarshal.SetCount(_worldTransforms, 1);
            }
            Dx11ArenaRenderer.QueueWorldOverlayPresent(ImGui.GetBackgroundDrawList(), viewport.Size);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldLine(Vector3 start, Vector3 end, uint color, float thickness = 1f)
    {
        AppendWorldLineUnchecked(start, end, color, thickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendWorldLineUnchecked(Vector3 start, Vector3 end, uint color, float thickness)
    {
        var index = _worldDrawLines.Count;
        _worldDrawLines.Add(new(start, end, color, thickness, 0u));
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.Lines, index, 1, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendWorldCurveUnchecked(in Dx11ArenaRenderer.WorldCurveInstance curve, int lineCount)
    {
        var index = _worldDrawCurves.Count;
        _worldDrawCurves.Add(curve);
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.Curves, index, 1, lineCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendWorldProjectedArrowUnchecked(in Dx11ArenaRenderer.WorldProjectedArrowInstance arrow)
    {
        var index = _worldProjectedArrows.Count;
        _worldProjectedArrows.Add(arrow);
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.ProjectedArrows, index, 1, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendWorldProjectedShapeUnchecked(in Dx11ArenaRenderer.WorldProjectedShapeInstance shape,
        RelSimplifiedComplexPolygon? shapeSdf = null, WPos shapeSdfOrigin = default,
        RelSimplifiedComplexPolygon? arenaSdf = null, WPos arenaSdfOrigin = default, float holeFillRadius = 0f)
    {
        var index = _worldProjectedShapes.Count;
        _worldProjectedShapes.Add(shape.WithHoleFillRadius(holeFillRadius));
        _worldProjectedShapeBindings.Add(new(shapeSdf, shapeSdfOrigin, arenaSdf, arenaSdfOrigin));
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.ProjectedShapes, index, 1, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AppendWorldBillboardTextUnchecked(in WorldBillboardTextCommand command)
    {
        var index = _worldBillboardTexts.Count;
        _worldBillboardTexts.Add(command);
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.BillboardText, index, 1, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RecordWorldPrimitiveRun(WorldPrimitiveRunKind kind, int start, int count, int curveLineCount)
    {
        var runCount = _worldPrimitiveRuns.Count;
        if (runCount != 0)
        {
            var runs = CollectionsMarshal.AsSpan(_worldPrimitiveRuns);
            ref var last = ref runs[runCount - 1];
            if (last.Kind == kind && last.CurveLineCount == curveLineCount && last.Start + last.Count == start)
            {
                last.Count += count;
                return;
            }
        }

        _worldPrimitiveRuns.Add(new WorldPrimitiveRun { Kind = kind, Start = start, Count = count, CurveLineCount = curveLineCount });
    }

    // Bulk indexed world-space submission
    // Edge keys encode the first vertex in the high 32 bits and the second in the low 32 bits
    public void DrawWorldIndexedLines(ReadOnlySpan<Vector3> vertices, ReadOnlySpan<ulong> edges, uint color, float thickness = 1f)
    {
        var oldCount = _worldDrawLines.Count;
        var lenE = edges.Length;
        var reservedCount = oldCount + lenE;
        _worldDrawLines.EnsureCapacity(reservedCount);
        CollectionsMarshal.SetCount(_worldDrawLines, reservedCount);
        var destination = CollectionsMarshal.AsSpan(_worldDrawLines);
        var dst = oldCount;

        var lenV = vertices.Length;
        for (var i = 0; i < lenE; ++i)
        {
            var key = edges[i];
            var ia = (int)(key >> 32);
            var ib = (int)(uint)key;
            if ((uint)ia >= (uint)lenV || (uint)ib >= (uint)lenV || ia == ib)
            {
                continue;
            }
            destination[dst++] = new(vertices[ia], vertices[ib], color, thickness, 0u);
        }

        if (dst != reservedCount)
        {
            CollectionsMarshal.SetCount(_worldDrawLines, dst);
        }
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.Lines, oldCount, dst - oldCount, 0);
    }

    // High-throughput local-space submission. Cached PCB edges remain immutable in local space;
    // one transform-table index is attached to every edge and the GPU performs local->world->clip.
    // If the transform table is ever exhausted, this falls back to CPU-transforming only that call
    // so correctness is preserved without forcing a batch split.
    public void DrawLocalLines(ReadOnlySpan<Dx11ArenaRenderer.WorldLineLocalSegment> lines, ref Dx11ArenaRenderer.WorldLineTransform transform, uint color, float thickness = 1f)
    {
        var transformIndex = GetOrAddWorldTransform(ref transform);

        var oldCount = _worldDrawLines.Count;
        var len = lines.Length;
        var reservedCount = oldCount + len;
        _worldDrawLines.EnsureCapacity(reservedCount);
        CollectionsMarshal.SetCount(_worldDrawLines, reservedCount);
        var destination = CollectionsMarshal.AsSpan(_worldDrawLines);

        if (transformIndex != uint.MaxValue)
        {
            for (var i = 0; i < len; ++i)
            {
                ref readonly var line = ref lines[i];
                destination[oldCount + i] = new(line.From, line.To, color, thickness, transformIndex);
            }
        }
        else
        {
            // Extremely unlikely (>1023 distinct transforms in one overlay batch). Keep the frame correct instead of flushing/reordering the background draw list
            for (var i = 0; i < len; ++i)
            {
                ref readonly var line = ref lines[i];
                destination[oldCount + i] = new(transform.TransformPoint(line.From), transform.TransformPoint(line.To), color, thickness, 0u);
            }
        }
        RecordWorldPrimitiveRun(WorldPrimitiveRunKind.Lines, oldCount, len, 0);
    }

    private uint GetOrAddWorldTransform(ref Dx11ArenaRenderer.WorldLineTransform transform)
    {
        var last = _worldTransforms.Count - 1;
        if (_worldTransforms[last].Equals(transform))
        {
            return (uint)last;
        }

        if (_worldTransforms.Count >= Dx11ArenaRenderer.MaxWorldLineTransforms)
        {
            return uint.MaxValue;
        }

        _worldTransforms.Add(transform);
        return (uint)(_worldTransforms.Count - 1);
    }

    // Filled guidance arrow projected onto the visible world
    // X/Z defines the arrow footprint; origin.Y is the reference height used by the projection band.
    // Encounter-aware floor selection can later refine that policy without changing the arrow geometry.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldArrow(Vector3 origin, Angle direction, float length, uint color, float shaftWidth = 0.6f, float headLength = 1.5f, float headWidth = 1.6f, float projectionHeight = 2.5f)
    {
        if (length <= 1e-4f)
        {
            return;
        }
        var dir = direction.ToDirection();
        var dirXZ = dir.ToVec2();
        AppendWorldProjectedArrowUnchecked(new(origin, length, dirXZ, shaftWidth, headLength, headWidth, projectionHeight, color));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldArrow(Vector3 start, Vector3 end, uint color, float shaftWidth = 0.6f, float headLength = 1.5f, float headWidth = 1.6f, float projectionHeight = 2.5f)
    {
        var delta = new Vector2(end.X - start.X, end.Z - start.Z);
        var length = delta.Length();
        if (length <= 1e-4f)
        {
            return;
        }
        AppendWorldProjectedArrowUnchecked(new(start, length, delta / length, shaftWidth, headLength, headWidth, projectionHeight, color));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedCircle(Vector3 center, float radius, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float innerRadius = 0f, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Circle(center, radius, color, projectionHeight, outlineWidth, innerRadius),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedRect(Vector3 origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Rect(origin, new Vector2(direction.X, direction.Z), lenFront, lenBack, halfWidth, color, projectionHeight, outlineWidth),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedCone(Vector3 center, float innerRadius, float outerRadius, WDir direction, float halfAngle, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Cone(center, innerRadius, outerRadius, new Vector2(direction.X, direction.Z), halfAngle, color, projectionHeight, outlineWidth),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedCapsule(Vector3 start, WDir direction, float radius, float length, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, bool suppressZoneWave = false, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Capsule(start, new Vector2(direction.X, direction.Z), radius, length, color, projectionHeight, outlineWidth, suppressZoneWave),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedArcCapsule(Vector3 start, Vector3 orbitCenter, float angularLength, float radius, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, bool suppressZoneWave = false, float holeFillRadius = 0f)
    {
        var delta = new Vector2(start.X - orbitCenter.X, start.Z - orbitCenter.Z);
        var orbitRadius = delta.Length();
        if (orbitRadius <= 1e-5f)
        {
            return;
        }
        AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.ArcCapsule(orbitCenter, delta / orbitRadius, orbitRadius, radius, angularLength, color, projectionHeight, outlineWidth, suppressZoneWave),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);
    }

    // True analytic 3D eye volume. This shares the projected-shape batch so it inherits scene-depth
    // occlusion and the native background-overlay ordering, but its PS path ray-intersects a
    // camera-facing biconvex lens instead of projecting a 2D footprint onto terrain.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldEye(Vector3 center, float halfWidth, float halfHeight, float halfDepth, float mistRadius, uint color, uint borderColor, bool inverted = false)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Eye3D(center, halfWidth, halfHeight, halfDepth, mistRadius, color, borderColor, inverted));

    // Fixed-screen-size MSDF billboard anchored at a true world-space point. The GPU projects the
    // anchor at render time, keeps every glyph camera-facing, and applies scene-depth occlusion.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldTextBillboard(Vector3 center, string text, uint color, float fontSize = 17f, uint outlineColor = 0u, float outlineWidth = 0f)
    {
        if (string.IsNullOrEmpty(text) || (color & 0xFF000000u) == 0u)
        {
            return;
        }
        AppendWorldBillboardTextUnchecked(new(center, text, fontSize, color, outlineColor, Math.Max(0f, outlineWidth), false));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldIconBillboard(Vector3 center, string iconText, uint color, float fontSize = 17f)
    {
        if (string.IsNullOrEmpty(iconText) || (color & 0xFF000000u) == 0u)
        {
            return;
        }
        AppendWorldBillboardTextUnchecked(new(center, iconText, fontSize, color, 0u, 0f, true));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedCross(Vector3 center, WDir direction, float range, float halfWidth, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Cross(center, new Vector2(direction.X, direction.Z), range, halfWidth, color, projectionHeight, outlineWidth),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedTriangle(Vector3 a, Vector3 b, Vector3 c, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.Triangle(a, b, c, color, projectionHeight, outlineWidth),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    // Filled projected triangle with an optional outline in one GPU instance. Actor markers use this
    // to avoid reconstructing scene depth and applying actor/UI masks twice for the same footprint.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedTriangleFilledOutlined(Vector3 a, Vector3 b, Vector3 c, uint fillColor, uint outlineColor, float projectionHeight = 2.5f, float outlineWidth = 0f, float boundsProjectionHeight = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float holeFillRadius = 0f)
        => AppendWorldProjectedShapeUnchecked(Dx11ArenaRenderer.WorldProjectedShapeInstance.TriangleFilledOutlined(a, b, c, fillColor, outlineColor, projectionHeight, outlineWidth, boundsProjectionHeight),
            arenaSdf: arenaClip, arenaSdfOrigin: arenaOrigin, holeFillRadius: holeFillRadius);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawProjectedPolygon(Vector3 referenceOrigin, RelSimplifiedComplexPolygon polygon, WPos polygonWorldOrigin, uint color, float projectionHeight = 2.5f, float outlineWidth = 0f,
        RelSimplifiedComplexPolygon? arenaClip = null, WPos arenaOrigin = default, float holeFillRadius = 0f)
    {
        var index = polygon.VerifyPolygonIndexExistance();
        index.GetBounds(out Vector2 localMin, out var localMax);
        var origin = polygonWorldOrigin.ToVec2();
        var min = localMin + origin;
        var max = localMax + origin;
        var shape = Dx11ArenaRenderer.WorldProjectedShapeInstance.Sdf(referenceOrigin, min, max, color, projectionHeight, outlineWidth);
        AppendWorldProjectedShapeUnchecked(shape, polygon, polygonWorldOrigin, arenaClip, arenaOrigin, holeFillRadius);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldCone(Vector3 center, float radius, Angle direction, Angle halfWidth, uint color, float thickness = 1f)
    {
        const int segments = 256;
        const int segmentsP2 = segments + 2;

        var dir = direction.ToDirection();
        var half = halfWidth.ToDirection();
        var curve = Dx11ArenaRenderer.WorldCurveInstance.ArcSector(center, radius, dir.ToVec2(), half.ToVec2(), color, thickness, segments);
        AppendWorldCurveUnchecked(curve, segmentsP2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldCircle(Vector3 center, float radius, uint color, float thickness = 1f)
    {
        const int segments = 256;
        var curve = Dx11ArenaRenderer.WorldCurveInstance.Circle(center, radius, color, thickness, segments);
        AppendWorldCurveUnchecked(curve, segments);
    }

    // Pass in both Vec 3 center and Shape rectangle so we have the Y coordinate as well.
    public void DrawWorldRectangle(Vector3 center, Rectangle rectangle, uint color, float thickness = 1)
    {
        var pos = rectangle.Center;
        // pull out the 4 vertices directly from Shape Rectangle.
        var dirs = rectangle.Contour(pos);
        const int numSides = 4; // rectangles have 4 sides

        _worldDrawLines.EnsureCapacity(_worldDrawLines.Count + numSides);
        // align each line with vec3 center so that it is drawn at the correct Y value.
        var prev = center + dirs[3].ToVec3();
        // If the list of dirs has values we start at dirs[0] for first line and loop through the
        // array until we end up back at dirs[0]
        for (var i = 0; i < numSides; ++i)
        {
            var curr = center + dirs[i].ToVec3();
            AppendWorldLineUnchecked(curr, prev, color, thickness);
            prev = curr;
        }
    }

    // Shape agnostic drawing logic. Every Shape in Shapes.cs has a contour that is a list of WDir. Just pull that information
    // and peg it to the vec3 center for arena height. Camera doesn't need to redo the logic, just use what Shapes delivers.
    public void DrawWorldShape(Vector3 center, Shape shape, uint color, float thickness = 1)
    {
        var pos = new WPos(center.X, center.Z);
        var dirs = shape.Contour(pos);
        var dirsCount = dirs.Count;

        _worldDrawLines.EnsureCapacity(_worldDrawLines.Count + dirsCount);
        // align each line with vec3 center so that it is drawn at the correct Y value.
        var prev = center + dirs[dirsCount - 1].ToVec3(); // Start from the last point so we can complete the outline of the shape
        // If the list of dirs has values we start at dirs[0] for first line and loop through the
        // array until we end up back at dirs[0]
        for (var i = 0; i < dirsCount; ++i)
        {
            var curr = center + dirs[i].ToVec3();
            AppendWorldLineUnchecked(curr, prev, color, thickness);
            prev = curr;
        }
    }

    // Draw a shape from a polygon such as customArenaBounds : adapted from MiniArena.cs
    // Curves and circles will not look clean because these guts were designed for tiny radar shapes in mind.
    // Import to radar and then determine if it looks right instead of being bothered by jaggy curves for now.
    public void DrawWorldPoly(Vector3 center, RelSimplifiedComplexPolygon poly, uint color, float thickness = 1)
    {
        var parts = CollectionsMarshal.AsSpan(poly.Parts);
        var len = parts.Length;

        for (var i = 0; i < len; ++i)
        {
            var part = parts[i];

            DrawContour(part.Exterior);
            var countH = part.HoleStarts.Count;
            for (var h = 0; h < countH; ++h)
            {
                DrawContour(part.Interior(h));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DrawContour(ReadOnlySpan<WDir> contour)
        {
            var len = contour.Length;
            if (len == 0)
            {
                return;
            }

            _worldDrawLines.EnsureCapacity(_worldDrawLines.Count + len);
            var prev = center + contour[len - 1].ToVec3();
            for (var i = 0; i < len; ++i)
            {
                var curr = center + contour[i].ToVec3();
                AppendWorldLineUnchecked(curr, prev, color, thickness);
                prev = curr;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawWorldSphere(Vector3 center, float radius, uint color, float thickness = 1)
    {
        const int segments = 256;
        const int tripleSegments = 3 * segments;
        var curve = Dx11ArenaRenderer.WorldCurveInstance.Sphere(center, radius, color, thickness, segments);
        AppendWorldCurveUnchecked(curve, tripleSegments);
    }

    // Procedural local-space cylinder. Returns false only when the shared transform table is full,
    // allowing callers with cached explicit edges to preserve correctness as a very rare fallback
    public bool DrawLocalCylinder(ref Dx11ArenaRenderer.WorldLineTransform transform, uint color, float thickness = 1f, float radius = 1f, float halfHeight = 1f)
    {
        var transformIndex = GetOrAddWorldTransform(ref transform);
        if (transformIndex == uint.MaxValue)
        {
            return false;
        }
        const int segments = 256;
        const int tripleSegments = 3 * segments;

        var curve = Dx11ArenaRenderer.WorldCurveInstance.Cylinder(new Vector3(0f, halfHeight, 0f), radius, halfHeight, color, thickness, segments, transformIndex);
        AppendWorldCurveUnchecked(curve, tripleSegments);
        return true;
    }
}
