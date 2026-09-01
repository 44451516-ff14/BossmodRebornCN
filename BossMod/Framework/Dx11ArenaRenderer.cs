using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;

namespace BossMod;

[SkipLocalsInit]
public static unsafe partial class Dx11ArenaRenderer
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MeshVertex
    {
        // NDC position; custom mesh VS is intentionally just a passthrough
        public Vector2 Pos;
        public uint Col;

        // Per-triangle bit mask, repeated on all three vertices:
        // bit 0 = edge BC (opposite vertex A) is a visible polygon boundary
        // bit 1 = edge CA (opposite vertex B) is a visible polygon boundary
        // bit 2 = edge AB (opposite vertex C) is a visible polygon boundary
        // Zero disables mesh-edge AA entirely (used by arena mask/background meshes)
        public uint BoundaryMask;

    }

    // Compact payload for the specialized actor/marker triangle stroke. Its byte layout matches
    // AnalyticInstance's descriptor shape, but the pipeline owns a dedicated input-layout object created from the triangle-stroke VS signature
    [StructLayout(LayoutKind.Sequential)]
    private struct PrimitiveTriangleStrokeInstance
    {
        public Vector2 AL;
        public Vector2 AR;
        public Vector2 BL;
        public Vector2 BR;
        public Vector4 CLR; // xy = CL, zw = CR
        public uint Col;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StrokeInstance
    {
        // Compact joined-polyline segment. The VS reconstructs neighboring directions/miter joins
        // and expands one instance into the six raster vertices for the segment. Positions are NDC;
        // NdcToPx converts NDC deltas directly to framebuffer pixels
        public Vector2 PrevNdc;
        public Vector2 ANdc;
        public Vector2 BNdc;
        public Vector2 NextNdc;
        public Vector2 NdcToPx;
        public Vector2 WidthsPx; // x = colored half-width, y = shadow half-width (0 when disabled)
        public uint Col;
        public uint ShadowCol;
        public uint Flags; // bit 0 = start cap, bit 1 = end cap
    }

    private const float OneThird = 1f / 3f;
    public const int MaxWorldLineTransforms = 1024;
    // Shared indexed-quad buffer also contains sequential quads for procedural WorldCurve lines.
    // 65k generated lines per curve is far beyond practical tessellation while keeping the one-time
    // index buffer small (~1.5 MiB). Ordinary quad draws consume only the first six indices.
    private const int MaxIndexedWorldCurveLines = 65536;

    // Affine local-to-world transform for GPU-projected Camera/world overlays
    // Stored as a normal row-vector System.Numerics matrix so local * World matches
    // FFXIVClientStructs Matrix4x3.TransformCoordinate semantics
    [StructLayout(LayoutKind.Sequential)]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]

    public struct WorldLineTransform(in Matrix4x4 matrix) : IEquatable<WorldLineTransform>
    {
        public Matrix4x4 Matrix = matrix;

        public static WorldLineTransform Identity => new(Matrix4x4.Identity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(WorldLineTransform other) => Matrix.Equals(other.Matrix);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object? obj) => obj is WorldLineTransform other && Equals(other);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly int GetHashCode() => Matrix.GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly Vector3 TransformPoint(in Vector3 point) => Vector3.Transform(point, Matrix);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WorldLineTransform left, WorldLineTransform right) => left.Equals(right);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WorldLineTransform left, WorldLineTransform right) => !(left == right);
    }

    // Immutable local-space edge cached by high-volume world visualizers
    [StructLayout(LayoutKind.Sequential)]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly struct WorldLineLocalSegment(Vector3 from, Vector3 to)
    {
        public readonly Vector3 From = from;
        public readonly Vector3 To = to;
    }

    // Compact GPU line instance. Endpoints may be world-space (TransformIndex=0 / identity)
    // or collider-local with TransformIndex selecting the packet's local-to-world table
    [StructLayout(LayoutKind.Sequential)]
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly struct WorldLineInstance(Vector3 from, Vector3 to, uint color, float thickness = 1f, uint transformIndex = 0u)
    {
        public readonly Vector3 From = from;
        public readonly float Thickness = thickness;
        public readonly Vector3 To = to;
        public readonly uint Col = color;
        public readonly uint TransformIndex = transformIndex;
    }

    public enum WorldCurveKind : uint
    {
        Circle = 1,
        Sphere = 2,
        Cylinder = 3,
        ArcSector = 4
    }

    // Compact procedural world-curve instance. The GPU expands one instance into all requested
    // line segments, then feeds each generated endpoint pair through the same projection/near-plane and screen-space AA path as WorldLineInstance
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WorldCurveInstance
    {
        public readonly Vector3 Center;
        public readonly float Radius;
        // Generation parameters are precomputed once on the CPU so the procedural VS only needs one
        // sincos per generated line instead of independently evaluating both endpoints:
        // Circle/Sphere: x/y = sin/cos angular step, z = reciprocal segment count.
        // Cylinder: x = local half-height, y/z = sin/cos angular step, w = reciprocal segment count.
        // ArcSector: x = first arc angle, y = angular step, z/w = sin/cos angular step.
        public readonly Vector4 Params;
        public readonly uint Col;
        public readonly float Thickness;
        public readonly uint TransformIndex;
        // low 3 bits = WorldCurveKind, upper bits = base angular segment count
        public readonly uint Packed;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WorldCurveInstance(Vector3 center, float radius, Vector4 @params, uint color, float thickness, uint transformIndex, WorldCurveKind kind, int segments)
        {
            Center = center;
            Radius = radius;
            Params = @params;
            Col = color;
            Thickness = thickness;
            TransformIndex = transformIndex;
            Packed = (uint)segments * 8u + (uint)kind;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldCurveInstance Circle(Vector3 center, float radius, uint color, float thickness, int segments, uint transformIndex = 0u)
        {
            var invSegments = segments > 0 ? 1f / segments : 0f;
            var (sinStep, cosStep) = MathF.SinCos(2f * MathF.PI * invSegments);
            return new(center, radius, new Vector4(sinStep, cosStep, invSegments, 0f), color, thickness, transformIndex, WorldCurveKind.Circle, segments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldCurveInstance Sphere(Vector3 center, float radius, uint color, float thickness, int segments, uint transformIndex = 0u)
        {
            var invSegments = segments > 0 ? 1f / segments : 0f;
            var (sinStep, cosStep) = MathF.SinCos(2f * MathF.PI * invSegments);
            return new(center, radius, new Vector4(sinStep, cosStep, invSegments, 0f), color, thickness, transformIndex, WorldCurveKind.Sphere, segments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldCurveInstance Cylinder(Vector3 center, float radius, float halfHeight, uint color, float thickness, int segments, uint transformIndex = 0u)
        {
            var invSegments = segments > 0 ? 1f / segments : 0f;
            var (sinStep, cosStep) = MathF.SinCos(2f * MathF.PI * invSegments);
            return new(center, radius, new Vector4(halfHeight, sinStep, cosStep, invSegments), color, thickness, transformIndex, WorldCurveKind.Cylinder, segments);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WorldCurveInstance ArcSector(Vector3 center, float radius, Vector2 directionSinCos, Vector2 halfWidthSinCos, uint color, float thickness, int arcSegments, uint transformIndex = 0u)
        {
            // These direction vectors use (sin, cos). Recover the two angles once per curve, then
            // precompute the per-segment rotation so the VS can derive endpoint B from endpoint A.
            var centerAngle = MathF.Atan2(directionSinCos.X, directionSinCos.Y);
            var halfWidth = MathF.Atan2(halfWidthSinCos.X, halfWidthSinCos.Y);
            if (halfWidth < 0f)
            {
                halfWidth += 2f * MathF.PI;
            }
            halfWidth = Math.Min(halfWidth, MathF.PI);
            var step = arcSegments > 0 ? 2f * halfWidth / arcSegments : 0f;
            var (sinStep, cosStep) = MathF.SinCos(step);
            return new(center, radius, new Vector4(centerAngle - halfWidth, step, sinStep, cosStep), color, thickness, transformIndex, WorldCurveKind.ArcSector, arcSegments);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WorldProjectedArrowInstance(Vector3 origin, float length, Vector2 directionXZ, float shaftWidth, float headLength, float headWidth, float projectionHeight, uint color)
    {
        public readonly Vector3 Origin = origin;
        public readonly float Length = length;
        public readonly Vector2 DirectionXZ = directionXZ;
        public readonly Vector2 Widths = new(Math.Max(0f, shaftWidth), Math.Max(0f, headWidth)); // x shaft width, y head width
        public readonly Vector2 HeadProjection = new(Math.Clamp(headLength, 0f, Math.Max(0f, length)), Math.Max(0f, projectionHeight)); // x head length, y +/- projection height
        public readonly uint Col = color;
    }

    public enum WorldProjectedShapeKind : uint
    {
        CircleDonut = 0,
        Rect = 1,
        Cone = 2,
        Capsule = 3,
        ArcCapsule = 4,
        Cross = 5,
        Triangle = 6,
        Sdf = 7,
        Eye3D = 8,
    }

    // World-projected MiniArena primitive. Most kinds reconstruct the visible scene surface and
    // evaluate an X/Z SDF there; Eye3D instead ray-intersects a volumetric analytic lens. BoundsXZ
    // remains an absolute world-space conservative AABB for the shared screen-quad VS path.
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WorldProjectedShapeInstance
    {
        public readonly Vector3 Origin;
        public readonly float ProjectionHeight;
        public readonly Vector2 BoundsMinXZ;
        public readonly Vector2 BoundsMaxXZ;
        public readonly Vector2 DirectionXZ;
        public readonly Vector2 Aux;
        public readonly Vector4 Params0;
        public readonly Vector4 Params1;
        public readonly uint Col;
        public readonly uint Packed; // low 8 bits kind, bit 8 = outline-only, bit 9 = filled shape with outline, bit 10 = suppress zone wave, bit 11 = inverted gaze
        public readonly float OutlineWidth;
        public readonly uint OutlineCol;
        public readonly Vector2 WaveOriginXZ;

        private WorldProjectedShapeInstance(Vector3 origin, float projectionHeight, Vector2 boundsMinXZ, Vector2 boundsMaxXZ,
            Vector2 directionXZ, Vector2 aux, Vector4 params0, Vector4 params1, uint color, WorldProjectedShapeKind kind, float outlineWidth, uint outlineColor = 0, bool fillWithOutline = false, Vector2 waveOriginXZ = default, bool suppressZoneWave = false, bool invertedGaze = false)
        {
            Origin = origin;
            ProjectionHeight = Math.Max(0f, projectionHeight);
            var pad = new Vector2(Math.Max(0f, outlineWidth) * 0.5f + 0.02f);
            BoundsMinXZ = boundsMinXZ - pad;
            BoundsMaxXZ = boundsMaxXZ + pad;
            DirectionXZ = directionXZ;
            Aux = aux;
            Params0 = params0;
            Params1 = params1;
            Col = color;
            OutlineWidth = Math.Max(0f, outlineWidth);
            OutlineCol = outlineColor;
            WaveOriginXZ = waveOriginXZ;
            Packed = (uint)kind | (OutlineWidth > 0f ? (fillWithOutline ? 0x200u : 0x100u) : 0u) | (suppressZoneWave ? 0x400u : 0u) | (invertedGaze ? 0x800u : 0u);
        }

        public static WorldProjectedShapeInstance Circle(Vector3 center, float radius, uint color, float projectionHeight, float outlineWidth = 0f, float innerRadius = 0f)
        {
            radius = Math.Max(0f, radius);
            innerRadius = Math.Clamp(innerRadius, 0f, radius);
            var centerXZ = center.XZ();
            var r = new Vector2(radius);
            return new(center, projectionHeight, centerXZ - r, centerXZ + r, default, default,
                new Vector4(radius, innerRadius, 0f, 0f), default, color, WorldProjectedShapeKind.CircleDonut, outlineWidth, waveOriginXZ: centerXZ);
        }

        public static WorldProjectedShapeInstance Rect(Vector3 origin, Vector2 directionXZ, float lenFront, float lenBack, float halfWidth, uint color, float projectionHeight, float outlineWidth = 0f)
        {
            var halfLength = 0.5f * Math.Max(0f, lenFront + lenBack);
            var centerShift = 0.5f * (lenFront - lenBack);
            var originXZ = origin.XZ();
            var centerXZ = originXZ + directionXZ * centerShift;
            var center = new Vector3(centerXZ.X, origin.Y, centerXZ.Y);
            var perp = new Vector2(-directionXZ.Y, directionXZ.X);
            var extent = Vector2.Abs(directionXZ) * halfLength + Vector2.Abs(perp) * Math.Max(0f, halfWidth);
            return new(center, projectionHeight, centerXZ - extent, centerXZ + extent, directionXZ, default,
                new Vector4(halfLength, Math.Max(0f, halfWidth), 0f, 0f), default, color, WorldProjectedShapeKind.Rect, outlineWidth, waveOriginXZ: originXZ);
        }

        public static WorldProjectedShapeInstance Cone(Vector3 center, float innerRadius, float outerRadius, Vector2 directionXZ, float halfAngle, uint color, float projectionHeight, float outlineWidth = 0f)
        {
            outerRadius = Math.Max(0f, outerRadius);
            innerRadius = Math.Clamp(innerRadius, 0f, outerRadius);
            halfAngle = Math.Clamp(Math.Abs(halfAngle), 0f, MathF.PI);
            var centerXZ = center.XZ();
            var (sinHalfAngle, cosHalfAngle) = MathF.SinCos(halfAngle);
            var r = new Vector2(outerRadius);
            return new(center, projectionHeight, centerXZ - r, centerXZ + r, directionXZ, new Vector2(sinHalfAngle, cosHalfAngle),
                new Vector4(outerRadius, innerRadius, halfAngle, 0f), default, color, WorldProjectedShapeKind.Cone, outlineWidth, waveOriginXZ: centerXZ);
        }

        public static WorldProjectedShapeInstance Capsule(Vector3 start, Vector2 directionXZ, float radius, float length, uint color, float projectionHeight, float outlineWidth = 0f, bool suppressZoneWave = false)
        {
            radius = Math.Max(0f, radius);
            length = Math.Max(0f, length);
            var halfSegment = 0.5f * length;
            var startXZ = start.XZ();
            var centerXZ = startXZ + directionXZ * halfSegment;
            var center = new Vector3(centerXZ.X, start.Y, centerXZ.Y);
            var endA = centerXZ - directionXZ * halfSegment;
            var endB = centerXZ + directionXZ * halfSegment;
            var r = new Vector2(radius);
            return new(center, projectionHeight, Vector2.Min(endA, endB) - r, Vector2.Max(endA, endB) + r, directionXZ, default,
                new Vector4(halfSegment, radius, 0f, 0f), default, color, WorldProjectedShapeKind.Capsule, outlineWidth,
                waveOriginXZ: startXZ, suppressZoneWave: suppressZoneWave);
        }

        public static WorldProjectedShapeInstance ArcCapsule(Vector3 orbitCenter, Vector2 startDirectionXZ, float orbitRadius, float radius, float angularLength, uint color, float projectionHeight, float outlineWidth = 0f, bool suppressZoneWave = false)
        {
            orbitRadius = Math.Max(0f, orbitRadius);
            radius = Math.Max(0f, radius);
            var (sinSweep, cosSweep) = MathF.SinCos(angularLength);
            var startX = startDirectionXZ.X;
            var startY = startDirectionXZ.Y;
            var endDirectionXZ = new Vector2(startX * cosSweep + startY * sinSweep, startY * cosSweep - startX * sinSweep);
            var centerXZ = orbitCenter.XZ();
            var extent = new Vector2(orbitRadius + radius);
            return new(orbitCenter, projectionHeight, centerXZ - extent, centerXZ + extent, startDirectionXZ, endDirectionXZ,
                new Vector4(orbitRadius, radius, angularLength, 0f), default, color, WorldProjectedShapeKind.ArcCapsule, outlineWidth,
                waveOriginXZ: centerXZ + startDirectionXZ * orbitRadius, suppressZoneWave: suppressZoneWave);
        }

        // Camera-facing analytic 3D eye. The pixel shader interprets this as the intersection of two
        // equal ellipsoids (a biconvex lens), then adds a larger animated mist volume around it.
        // ProjectionHeight is deliberately the conservative world-space bound radius here rather than
        // a terrain receiver band; Eye3D takes a dedicated volumetric path in projected_shape_ps.
        public static WorldProjectedShapeInstance Eye3D(Vector3 center, float halfWidth, float halfHeight, float halfDepth, float mistRadius, uint color, uint borderColor, bool inverted = false)
        {
            halfWidth = Math.Max(0.05f, halfWidth);
            halfHeight = Math.Clamp(halfHeight, 0.05f, halfWidth);
            halfDepth = Math.Max(0.05f, halfDepth);
            mistRadius = Math.Max(0f, mistRadius);
            var centerXZ = center.XZ();
            var boundRadius = Math.Max(Math.Max(halfWidth + mistRadius, halfHeight + mistRadius), halfDepth + mistRadius);
            var e = new Vector2(boundRadius);
            return new(center, boundRadius, centerXZ - e, centerXZ + e, default, default,
                new Vector4(halfWidth, halfHeight, halfDepth, mistRadius), new Vector4(boundRadius, 0f, 0f, 0f),
                color, WorldProjectedShapeKind.Eye3D, 0f, borderColor, waveOriginXZ: centerXZ, invertedGaze: inverted);
        }

        public static WorldProjectedShapeInstance Cross(Vector3 center, Vector2 directionXZ, float range, float halfWidth, uint color, float projectionHeight, float outlineWidth = 0f)
        {
            range = Math.Max(0f, range);
            halfWidth = Math.Max(0f, halfWidth);
            var centerXZ = center.XZ();
            var e = new Vector2(Math.Max(range, halfWidth));
            return new(center, projectionHeight, centerXZ - e, centerXZ + e, directionXZ, default,
                new Vector4(range, halfWidth, 0f, 0f), default, color, WorldProjectedShapeKind.Cross, outlineWidth, waveOriginXZ: centerXZ);
        }

        public static WorldProjectedShapeInstance Triangle(Vector3 a, Vector3 b, Vector3 c, uint color, float projectionHeight, float outlineWidth = 0f)
        {
            var center = (a + b + c) * OneThird;
            var centerXZ = center.XZ();
            var aXZ = a.XZ();
            var bXZ = b.XZ();
            var cXZ = c.XZ();
            var aRel = aXZ - centerXZ;
            var bRel = bXZ - centerXZ;
            var cRel = cXZ - centerXZ;
            var min = Vector2.Min(aXZ, Vector2.Min(bXZ, cXZ));
            var max = Vector2.Max(aXZ, Vector2.Max(bXZ, cXZ));
            return new(center, projectionHeight, min, max, aRel, bRel, new Vector4(cRel.X, cRel.Y, 0f, 0f), default, color, WorldProjectedShapeKind.Triangle, outlineWidth, waveOriginXZ: aXZ);
        }

        // One projected triangle instance containing both fill and outline. This is primarily used by
        // actor markers so scene-depth reconstruction, character classification and arena clipping are evaluated once
        public static WorldProjectedShapeInstance TriangleFilledOutlined(Vector3 a, Vector3 b, Vector3 c, uint fillColor, uint outlineColor, float projectionHeight, float outlineWidth, float boundsProjectionHeight = 0f)
        {
            var center = (a + b + c) * OneThird;
            var centerXZ = center.XZ();
            var aXZ = a.XZ();
            var bXZ = b.XZ();
            var cXZ = c.XZ();
            var aRel = aXZ - centerXZ;
            var bRel = bXZ - centerXZ;
            var cRel = cXZ - centerXZ;
            var min = Vector2.Min(aXZ, Vector2.Min(bXZ, cXZ));
            var max = Vector2.Max(aXZ, Vector2.Max(bXZ, cXZ));
            var conservativeHeight = Math.Max(projectionHeight, boundsProjectionHeight);
            return new(center, projectionHeight, min, max, aRel, bRel,
                new Vector4(cRel.X, cRel.Y, 0f, 0f), new Vector4(conservativeHeight, 0f, 0f, 0f), fillColor, WorldProjectedShapeKind.Triangle, outlineWidth, outlineColor, fillWithOutline: true, suppressZoneWave: true);
        }

        public static WorldProjectedShapeInstance Sdf(Vector3 referenceOrigin, Vector2 boundsMinXZ, Vector2 boundsMaxXZ, uint color, float projectionHeight, float outlineWidth = 0f)
            => new(referenceOrigin, projectionHeight, boundsMinXZ, boundsMaxXZ, default, default, default, default, color, WorldProjectedShapeKind.Sdf, outlineWidth, waveOriginXZ: referenceOrigin.XZ());

        private WorldProjectedShapeInstance(in WorldProjectedShapeInstance source, Vector4 params1, uint packed)
        {
            Origin = source.Origin;
            ProjectionHeight = source.ProjectionHeight;
            BoundsMinXZ = source.BoundsMinXZ;
            BoundsMaxXZ = source.BoundsMaxXZ;
            DirectionXZ = source.DirectionXZ;
            Aux = source.Aux;
            Params0 = source.Params0;
            Params1 = params1;
            Col = source.Col;
            Packed = packed;
            OutlineWidth = source.OutlineWidth;
            OutlineCol = source.OutlineCol;
            WaveOriginXZ = source.WaveOriginXZ;
        }

        // Params1.y carries the optional world-space closing radius without growing the instance or input layout. ProjectionHeight == 0 selects the authored reference plane.
        public WorldProjectedShapeInstance WithHoleFillRadius(float holeFillRadius)
        {
            var params1 = Params1;
            params1.Y = Math.Clamp(holeFillRadius, 0f, 2f);
            return new(this, params1, Packed);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WorldProjectedSdfConstants
    {
        public Vector4 ShapeSdfMap;
        public Vector4 ArenaSdfMap;
        public Vector4 Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ZoneWaveConstants
    {
        public Vector4 Params; // x = elapsed seconds; remaining lanes reserved for future tuning
    }

    private struct WorldProjectedSdfBinding
    {
        public ID3D11ShaderResourceView* View;
        // xy = world-space padded-domain minimum, zw = reciprocal world-space span.
        public Vector4 Map;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WorldLineConstants
    {
        public Matrix4x4 ViewProj;
        public Matrix4x4 InvViewProj;
        public Vector4 NearPlane;
        public Vector4 Viewport; // x/y framebuffer dimensions, z logical->framebuffer pixel scale
        // xy maps framebuffer pixels to scene-depth texels; zw maps framebuffer pixels to NDC.
        // Supplying these once per packet removes uniform divisions from every world-space pixel/vertex.
        public Vector4 RasterScale;
        public Vector4 SceneDepthParams; // xy scene depth actual dimensions, z line-occlusion tolerance in world units, w = scene depth available
        public Vector4 SceneInfoParams; // x = character-classification texture available, y = near-black character threshold
    }

    // Prefix-compatible with D3D11_SHADER_RESOURCE_VIEW_DESC for a Texture2D view. TerraFX exposes
    // the native anonymous union through generated nested types; this compact descriptor keeps the
    // callsite independent of those generated union member names.
    [StructLayout(LayoutKind.Sequential)]
    private struct Texture2DShaderResourceViewDesc
    {
        public DXGI_FORMAT Format;
        public D3D_SRV_DIMENSION ViewDimension;
        public uint MostDetailedMip;
        public uint MipLevels;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TextInstance
    {
        // One glyph quad. RectNdc = (minX, minY, maxX, maxY), UvRect = (u0, v0, u1, v1).
        // The VS expands this to the same six triangle-list vertices as the other indexed-quad paths.
        public Vector4 RectNdc;
        public Vector4 UvRect;
        public uint Col;
        public uint OutlineCol;
        public float OutlineWidthPx;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WorldTextInstance
    {
        // Fixed-screen-size glyph quad anchored at a true world-space point. RectPx stores framebuffer-
        // pixel offsets from the projected anchor; the world-text VS applies those offsets in clip space
        // while preserving the anchor depth for scene-depth occlusion in the pixel shader.
        public Vector3 Center;
        public float Reserved;
        public Vector4 RectPx;
        public Vector4 UvRect;
        public uint Col;
        public uint OutlineCol;
        public float OutlineWidthPx;
    }

    // Immutable metrics loaded once from the embedded compiled MSDF metadata. Plane bounds are in em
    // units with a bottom-up Y axis; UVs are normalized to Direct3D's top-left texture convention
    private readonly struct ArenaFontGlyph(float advance, Vector4 planeBounds, Vector4 uvRect, bool hasQuad)
    {
        public readonly float Advance = advance;
        public readonly Vector4 PlaneBounds = planeBounds; // left, bottom, right, top in em
        public readonly Vector4 UvRect = uvRect;      // u0, v0, u1, v1, top-left texture origin
        public readonly bool HasQuad = hasQuad;
    }

    private readonly struct ArenaFontMetrics(float lineHeight, float ascender, float descender)
    {
        public readonly float LineHeight = lineHeight;
        public readonly float Ascender = ascender;
        public readonly float Descender = descender;
    }

    // Fixed-layout v2 metadata records used by arena_font_msdf.bin. These structs are deliberately
    // 4-byte packed and contain no references so the embedded byte payload can be viewed directly with
    // MemoryMarshal.Cast<byte, T>() instead of decoding individual fields with BinaryReader.
    private const uint ArenaFontBinaryMagic = 0x46534D42u; // bytes: B M S F
    private const uint ArenaFontBinaryVersion = 2u;
    private const uint ArenaFontBinaryAtlasTypeMsdf = 1u;
    private const uint ArenaFontVariantText = 1u;
    private const uint ArenaFontVariantIcons = 2u;
    private const uint ArenaFontGlyphHasQuad = 1u;
    private const int ArenaFontBinaryHeaderSize = 40;
    private const int ArenaFontBinaryVariantSize = 44;
    private const int ArenaFontBinaryGlyphSize = 44;
    private const int ArenaFontBinaryKerningSize = 12;

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct ArenaFontBinaryHeader
    {
        public uint Magic;
        public uint Version;
        public uint AtlasType;
        public uint YOrigin;
        public float DistanceRange;
        public float DistanceRangeMiddle;
        public float Size;
        public int Width;
        public int Height;
        public int VariantCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct ArenaFontBinaryVariant
    {
        public uint Kind;
        public float EmSize;
        public float LineHeight;
        public float Ascender;
        public float Descender;
        public float UnderlineY;
        public float UnderlineThickness;
        public int GlyphOffset;
        public int GlyphCount;
        public int KerningOffset;
        public int KerningCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct ArenaFontBinaryGlyph
    {
        public uint Unicode;
        public float Advance;
        public float PlaneLeft;
        public float PlaneBottom;
        public float PlaneRight;
        public float PlaneTop;
        public float U0;
        public float V0;
        public float U1;
        public float V1;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct ArenaFontBinaryKerning
    {
        public uint Unicode1;
        public uint Unicode2;
        public float Advance;
    }

    private sealed unsafe class ArenaSdfResource
    {
        public ID3D11ShaderResourceView* View;
        public float MinX;
        public float MinZ;
        public float SpanX;
        public float SpanZ;
        public float InvSpanX;
        public float InvSpanZ;
        public int Width;
        public int Height;
        public int ByteSize;
        public long LastUse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OutlineSdfConstants
    {
        // uv.x = dot(float3(SV_POSITION.xy, 1), UvRow0.xyz)
        // uv.y = dot(float3(SV_POSITION.xy, 1), UvRow1.xyz)
        // UvRow1.w converts arena-local SDF world units to framebuffer pixels.
        public Vector4 UvRow0;
        public Vector4 UvRow1;
        // xy converts distance outside the finite UV domain back to framebuffer pixels.
        // This is invariant for a binding and avoids two length/reciprocal pairs per SDF sample.
        public Vector4 OutsideScale;
        // xy/zw are the explicit SampleGrad x/y gradients, including the stable -0.5 LOD bias.
        public Vector4 MipGrad;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AnalyticInstance
    {
        // One generic analytic instance. The VS emits an axis-aligned bounding quad; the PS evaluates
        // the actual shape in screen-pixel space. Params.W is the shape kind (0 circle/donut, 1 rect, 2 cone, 3 capsule, 4 arc capsule, 5 cross, 6 eye/lens)
        public Vector2 CenterNdc;
        public Vector2 ExtentNdc;
        public Vector2 ExtentPx;
        public Vector2 DirectionScreen;
        public Vector4 Params;
        public uint Col;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OutlineInstance
    {
        public Vector2 CenterNdc;
        public Vector2 ExtentNdc;
        public Vector2 ExtentPx;
        public Vector2 DirectionScreen;
        public Vector4 Params;
        // Extra per-shape payload: cone = (sinHalfAngle, halfAngle), arc capsule = precomputed
        // end direction, triangle = (third vertex Y, 0). Other analytic outline kinds leave it zero.
        public Vector2 Extra;
        public Vector2 WidthsPx; // x = colored-line half-width, y = shadow half-width
        public uint Col;
        public uint ShadowCol;
    }

    private enum SegmentKind : byte
    {
        Mesh,
        PrimitiveMesh,
        PrimitiveTriangleStroke,
        Stroke,
        WorldLine,
        WorldCurve,
        WorldProjectedArrow,
        WorldProjectedShape,
        Analytic,
        ScreenAnalytic,
        AnalyticOutline,
        AnalyticOutlineUnclipped,
        CustomSdfFill,
        CustomSdfOutline,
        ArenaSdfOutline,
        AnalyticClipEdgeOverlay,
        CustomClipEdgeOverlay,
        Text,
        WorldText,
        Sprite
    }

    private struct DrawSegment
    {
        public SegmentKind Kind;
        public int Start;
        public int Count;
        // Index into BatchPacket.CustomSdfs for segments that sample a custom polygon SDF
        // Ordinary segments leave this at -1
        public int CustomSdfBinding;
        // Index into BatchPacket.Sprites for textured screen quads; ordinary segments leave this at -1.
        public int SpriteBinding;
    }

    private struct CustomSdfBinding
    {
        public ID3D11ShaderResourceView* View;
        public OutlineSdfConstants Constants;
    }

    private struct SpriteBinding
    {
        public ID3D11ShaderResourceView* View;
    }

    // Clipped outlines are drawn normally in submission order, then only the arena-generated
    // clipping edge is replayed once at EndArena. This lets that edge win over the arena border
    // without moving the geometry or promoting the whole outline above later arena content
    private struct DeferredOutlineOverlay
    {
        public OutlineInstance Instance;
        public SegmentKind Kind;
        public ID3D11ShaderResourceView* ArenaSdfView;
        public OutlineSdfConstants ArenaSdfConstants;
        public ID3D11ShaderResourceView* CustomSdfView;
        public OutlineSdfConstants CustomSdfConstants;
        public Vector2 ClipOffset;
        public Vector2 ClipScale;
        public int FramebufferWidth;
        public int FramebufferHeight;
    }

    private sealed class BatchPacket
    {
        public MeshVertex[]? MeshVertices;
        public int MeshVertexCount;
        public int ArenaSdfMaskOutlineStart = -1;
        public PrimitiveTriangleStrokeInstance[]? PrimitiveTriangleStrokes;
        public int PrimitiveTriangleStrokeCount;
        public StrokeInstance[]? StrokeInstances;
        public int StrokeInstanceCount;
        public WorldLineInstance[]? WorldLines;
        public int WorldLineCount;
        public WorldCurveInstance[]? WorldCurves;
        public int WorldCurveCount;
        public WorldProjectedArrowInstance[]? WorldProjectedArrows;
        public int WorldProjectedArrowCount;
        public WorldProjectedShapeInstance[]? WorldProjectedShapes;
        public int WorldProjectedShapeCount;
        public WorldProjectedSdfBinding[]? WorldProjectedSdfs;
        public int WorldProjectedSdfCount;
        public WorldLineTransform[]? WorldLineTransforms;
        public int WorldLineTransformCount;
        public WorldLineConstants WorldLineConstants;
        public ID3D11ShaderResourceView* SceneDepthView;
        public ID3D11ShaderResourceView* SceneInfoView;
        public AnalyticInstance[]? Analytics;
        public int AnalyticCount;
        public OutlineInstance[]? Outlines;
        public int OutlineCount;
        public TextInstance[]? TextInstances;
        public int TextInstanceCount;
        public WorldTextInstance[]? WorldTextInstances;
        public int WorldTextInstanceCount;
        public SpriteBinding[]? Sprites;
        public int SpriteCount;
        public ID3D11ShaderResourceView* ArenaSdfView;
        public OutlineSdfConstants OutlineSdfConstants;
        public CustomSdfBinding[]? CustomSdfs;
        public int CustomSdfCount;
        public bool NeedsStencil;
        public bool UsesZoneWave;
        // True when any submitted segment explicitly switches away from the inherited depth/stencil state.
        // Computed while building so the render callback does not rescan the ordered segment list.
        public bool ModifiesDepthState;
        public long StencilKey;
        public DrawSegment[]? Segments;
        public int SegmentCount;
        public Vector2 ClipOffset;
        public Vector2 ClipScale;
        public int FramebufferWidth;
        public int FramebufferHeight;
        // Relative byte offsets in the shared dynamic upload buffer are finalized when the packet is
        // assembled, keeping the render callback focused on mapping/copying/drawing.
        public int UploadBytes;
        public uint MeshOffsetBytes;
        public uint PrimitiveTriangleStrokeOffsetBytes;
        public uint StrokeOffsetBytes;
        public uint WorldLineOffsetBytes;
        public uint WorldCurveOffsetBytes;
        public uint WorldProjectedArrowOffsetBytes;
        public uint WorldProjectedShapeOffsetBytes;
        public uint AnalyticOffsetBytes;
        public uint OutlineOffsetBytes;
        public uint TextOffsetBytes;
        public uint WorldTextOffsetBytes;
        public int SubmitFrame;
        // Camera/world packets render into the texture presented by WorldOverlayNode instead of the
        // composited swapchain. A separate marker packet resolves that texture once per frame.
        public bool WorldOverlayTarget;
        public bool IsWorldOverlayPresent;
        // Deferred clipping-edge replay is correctness-sensitive: it must land after and obscure
        // the arena border. Give these packets a fresh dynamic-buffer backing allocation instead of sharing the same-frame NO_OVERWRITE ring with the border packet
        public bool IsDeferredOverlay;
    }

    private static readonly ImDrawCallback DrawCallback = RenderBatchCallback;
    // DX11 objects below are process/game-owned COM resources used from the deferred ImGui render callback.
    // Hot reload can call Shutdown while a callback from the previous UI frame is still executing, so
    // teardown must be mutually exclusive with callback execution. Lock ordering is always _rendererLock -> PendingPackets when both are needed.
    private static readonly Lock _rendererLock = new();
    private static volatile bool _shutDown = true;
    private static bool _isInitialized;
    private static readonly Dictionary<nint, BatchPacket> PendingPackets = [];
    private static readonly Stack<BatchPacket> BatchPacketPool = new(16);

    // Arena shapes are considered immutable: replacement creates a new polygon object, so reference identity is
    // both the cache key and the invalidation/version rule
    private static readonly Dictionary<RelSimplifiedComplexPolygon, ArenaSdfResource> ArenaSdfCache = [with(ReferenceEqualityComparer.Instance)];
    // Custom polygons are immutable by convention too; reference identity avoids structural hashing and
    // makes cache invalidation identical to arena-shape replacement. Adaptive resolutions make an entry
    // count alone a poor memory limit, so both caches use byte budgets with a generous entry-count guard
    private const long ArenaSdfCacheBudgetBytes = 8L * 1024 * 1024;
    private const long CustomSdfCacheBudgetBytes = 12L * 1024 * 1024;
    private const int MaxCachedArenaSdfs = 16;
    private const int MaxCachedCustomSdfs = 64;
    private const float TargetSdfPixelsPerTexel = 0.5f;
    private const int MinSdfLongResolution = 64;
    private const int MaxSdfResolution = 1024;
    private const int SdfResolutionAlignment = 16;
    private static readonly Dictionary<RelSimplifiedComplexPolygon, ArenaSdfResource> CustomSdfCache = [with(ReferenceEqualityComparer.Instance)];
    // Byte totals only change on create/upgrade/evict. Keep them incrementally instead of scanning
    // every cache entry on every SDF lookup in the steady-state render path
    private static long _arenaSdfCacheBytes;
    private static long _customSdfCacheBytes;
    private static long _arenaSdfUseCounter;
    private static long _nextPacketId;
    private static long _nextArenaStencilKey;
    private static long _renderedStencilKey;
    private static int _submitFrame = -1;

    private static ID3D11Device* _device;
    private static ID3D11DeviceContext* _context;

    private static ID3D11VertexShader* _meshVertexShader;
    private static ID3D11PixelShader* _meshPixelShader;
    private static ID3D11InputLayout* _meshInputLayout;

    // Compact instanced triangle-stroke pipeline for the hot actor marker outline path.
    private static ID3D11VertexShader* _primitiveTriangleStrokeVertexShader;
    private static ID3D11InputLayout* _primitiveTriangleStrokeInputLayout;

    private static ID3D11VertexShader* _strokeVertexShader;
    private static ID3D11PixelShader* _strokePixelShader;
    private static ID3D11InputLayout* _strokeInputLayout;

    private static ID3D11VertexShader* _worldLineVertexShader;
    private static ID3D11InputLayout* _worldLineInputLayout;
    private static ID3D11VertexShader* _worldCurveVertexShader;
    private static ID3D11InputLayout* _worldCurveInputLayout;
    private static ID3D11VertexShader* _worldProjectedArrowVertexShader;
    private static ID3D11PixelShader* _worldProjectedArrowPixelShader;
    private static ID3D11InputLayout* _worldProjectedArrowInputLayout;
    private static ID3D11VertexShader* _worldProjectedShapeVertexShader;
    private static ID3D11PixelShader* _worldProjectedShapePixelShader;
    private static ID3D11InputLayout* _worldProjectedShapeInputLayout;
    private static ID3D11VertexShader* _worldOverlayVertexShader;
    private static ID3D11PixelShader* _worldOverlayPixelShader;
    private static ID3D11Buffer* _worldProjectedSdfConstantBuffer;
    private static ID3D11Buffer* _worldLineConstantBuffer;
    private static ID3D11Buffer* _worldLineTransformBuffer;
    private static ID3D11ShaderResourceView* _sceneDepthView;
    private static nint _sceneDepthTextureIdentity;
    private static ID3D11ShaderResourceView* _sceneInfoView;
    private static nint _sceneInfoViewIdentity;
    private static ID3D11Texture2D* _worldOverlayBaseTexture;
    private static ID3D11RenderTargetView* _worldOverlayBaseRenderTarget;
    private static ID3D11ShaderResourceView* _worldOverlayBaseView;
    private static ID3D11Texture2D* _worldOverlayOutputTexture;
    private static ID3D11RenderTargetView* _worldOverlayOutputRenderTarget;
    private static ID3D11ShaderResourceView* _worldOverlayOutputView;
    private static WorldOverlayNode? _worldOverlayNode;
    private static int _worldOverlayWidth;
    private static int _worldOverlayHeight;
    private static int _worldOverlayBaseFrame = -1;
    private static bool _worldOverlayLastSucceeded;

    // Stable world-render settings. Scene depth hides line/curve pixels behind world geometry. Native
    // UI ordering is handled structurally by WorldOverlayNode's background layer, never inferred from
    // the already-composited backbuffer (whose alpha also contains world particle billboards).
    private const float WorldSceneOcclusionTolerance = 0.05f;
    private const float WorldSceneCharacterMaskThreshold = 0.05f;
    public static bool WorldSceneDepthAvailable => _sceneDepthView != null;
    public static bool WorldNativeUiLayerAvailable => _worldOverlayLastSucceeded && _worldOverlayNode != null;

    private static ID3D11VertexShader* _analyticVertexShader;
    private static ID3D11PixelShader* _analyticPixelShader;
    private static ID3D11InputLayout* _analyticInputLayout;

    private static ID3D11VertexShader* _textVertexShader;
    private static ID3D11PixelShader* _textPixelShader;
    private static ID3D11PixelShader* _spritePixelShader;
    private static ID3D11InputLayout* _textInputLayout;
    private static ID3D11VertexShader* _worldTextVertexShader;
    private static ID3D11PixelShader* _worldTextPixelShader;
    private static ID3D11InputLayout* _worldTextInputLayout;
    // Arena text is completely independent of ImGui's dynamic font atlas. The immutable MSDF
    // texture and metrics are embedded alongside the compiled shaders and owned by this renderer.
    private static ID3D11ShaderResourceView* _arenaFontAtlasView;
    private static Dictionary<uint, ArenaFontGlyph>? _arenaTextGlyphs;
    private static Dictionary<uint, ArenaFontGlyph>? _arenaIconGlyphs;
    private static Dictionary<ulong, float>? _arenaTextKerning;
    private static ArenaFontMetrics _arenaTextMetrics;
    private static ArenaFontMetrics _arenaIconMetrics;

    private static ID3D11VertexShader* _outlineShapeVertexShader;
    private static ID3D11PixelShader* _outlineShapePixelShader;
    private static ID3D11PixelShader* _outlineUnclippedPixelShader;
    private static ID3D11PixelShader* _customOutlinePixelShader;
    private static ID3D11PixelShader* _arenaSdfOutlinePixelShader;
    private static ID3D11PixelShader* _arenaSdfStencilPixelShader;
    private static ID3D11PixelShader* _customSdfFillPixelShader;
    private static ID3D11PixelShader* _outlineClipEdgePixelShader;
    private static ID3D11PixelShader* _customClipEdgePixelShader;
    private static ID3D11InputLayout* _outlineShapeInputLayout;

    private static ID3D11DepthStencilState* _stencilWriteState;
    private static ID3D11DepthStencilState* _stencilTestState;
    private static ID3D11DepthStencilState* _stencilDisabledState;
    private static ID3D11DepthStencilView* _stencilView;
    private static ID3D11SamplerState* _arenaSdfSampler;
    private static ID3D11Buffer* _outlineSdfConstantBuffer;
    private static ID3D11Buffer* _customSdfConstantBuffer;
    private static ID3D11Buffer* _zoneWaveConstantBuffer;
    private static readonly long ZoneWaveEpoch = Stopwatch.GetTimestamp();
    // Shared immutable 0,1,2 / 0,2,3 quad indices. Stroke/world-line/analytic/outline/text VS paths only need four unique corners
    private static ID3D11Buffer* _quadIndexBuffer;
    private static int _stencilWidth;
    private static int _stencilHeight;

    // All transient geometry/instance payloads share one dynamic D3D11 vertex buffer. Input layouts select the interpretation at bind time; packet-relative stream offsets are computed once during packet assembly and only rebased when uploaded
    private static ID3D11Buffer* _uploadVertexBuffer;
    private static int _uploadVertexCapacityBytes;
    private static int _uploadRenderFrame = -1;
    private static int _uploadCursorBytes;
    private static uint _uploadMeshOffsetBytes;
    private static uint _uploadPrimitiveTriangleStrokeOffsetBytes;
    private static uint _uploadStrokeOffsetBytes;
    private static uint _uploadWorldLineOffsetBytes;
    private static uint _uploadWorldCurveOffsetBytes;
    private static uint _uploadWorldProjectedArrowOffsetBytes;
    private static uint _uploadWorldProjectedShapeOffsetBytes;
    private static uint _uploadAnalyticOffsetBytes;
    private static uint _uploadOutlineOffsetBytes;
    private static uint _uploadTextOffsetBytes;
    private static uint _uploadWorldTextOffsetBytes;

    private static OutlineSdfConstants _lastOutlineSdfConstants;
    private static OutlineSdfConstants _lastCustomSdfConstants;
    private static WorldProjectedSdfConstants _lastWorldProjectedSdfConstants;
    private static WorldLineConstants _lastUploadedWorldLineConstants;
    private static bool _outlineSdfConstantsValid;
    private static bool _customSdfConstantsValid;
    private static bool _worldProjectedSdfConstantsValid;
    private static bool _uploadedWorldLineConstantsValid;
    private static int _zoneWaveUploadFrame = -1;

    // Current arena/run build state
    private static MeshVertex[]? _buildMeshVertices;
    private static int _buildMeshVertexCount;
    private static int _buildArenaSdfMaskOutlineStart = -1;
    private static PrimitiveTriangleStrokeInstance[]? _buildPrimitiveTriangleStrokes;
    private static int _buildPrimitiveTriangleStrokeCount;
    private static StrokeInstance[]? _buildStrokeInstances;
    private static int _buildStrokeInstanceCount;
    private static WorldLineInstance[]? _buildWorldLines;
    private static int _buildWorldLineCount;
    private static WorldCurveInstance[]? _buildWorldCurves;
    private static int _buildWorldCurveCount;
    private static WorldProjectedArrowInstance[]? _buildWorldProjectedArrows;
    private static int _buildWorldProjectedArrowCount;
    private static WorldProjectedShapeInstance[]? _buildWorldProjectedShapes;
    private static int _buildWorldProjectedShapeCount;
    private static WorldProjectedSdfBinding[]? _buildWorldProjectedSdfs;
    private static int _buildWorldProjectedSdfCount;
    private static WorldLineTransform[]? _buildWorldLineTransforms;
    private static int _buildWorldLineTransformCount;
    private static WorldLineConstants _buildWorldLineConstants;
    private static bool _buildWorldLineConstantsValid;
    private static Matrix4x4 _buildWorldLineViewProj;
    private static Matrix4x4 _buildWorldLineInvViewProj;
    private static Vector4 _buildWorldLineNearPlane;
    private static Vector2 _buildWorldSceneDepthSize;
    private static bool _buildWorldSceneDepthAvailable;
    private static bool _buildWorldSceneInfoAvailable;
    private static bool _buildWorldLineConfigured;
    private static AnalyticInstance[]? _buildAnalytics;
    private static int _buildAnalyticCount;
    private static OutlineInstance[]? _buildOutlines;
    private static int _buildOutlineCount;
    private static TextInstance[]? _buildTextInstances;
    private static int _buildTextInstanceCount;
    private static WorldTextInstance[]? _buildWorldTextInstances;
    private static int _buildWorldTextInstanceCount;
    private static SpriteBinding[]? _buildSprites;
    private static int _buildSpriteCount;
    private static bool _buildNeedsStencil;
    private static bool _buildUsesZoneWave;
    private static bool _buildModifiesDepthState;
    private static long _arenaStencilKey;
    private static bool _arenaStencilMaskQueued;
    private static DrawSegment[]? _buildSegments;
    private static int _buildSegmentCount;
    // ImDrawList-style path replacement. Points remain arena-local until PathStroke so arcs and lines share the same distance-based AA stroke path as AddLine/AddPolygon
    private static readonly List<WDir> BuildPath = [with(128)];
    private const float PathArcMaxErrorPx = 0.25f;
    private const int PathArcMaxSegments = 512;
    private static readonly List<DeferredOutlineOverlay> DeferredOutlineOverlays = [with(16)];
    private static bool _arenaActive;
    private static bool _arenaPrepared;
    private static ImDrawListPtr _buildDrawList;
    private static Vector2 _buildClipOffset;
    private static Vector2 _buildClipScale;
    private static Vector2 _buildViewportPos;
    private static Vector2 _buildViewportSize;
    // Standalone screen batches (Camera/world overlays) are attached to a background draw list and
    // therefore provide their viewport explicitly instead of inheriting the current ImGui window
    private static bool _buildViewportOverride;
    private static Vector2 _buildViewportOverridePos;
    private static Vector2 _buildViewportOverrideSize;
    // Precomputed transform invariants for the current active DX11 arena. These are initialized lazily with the viewport/framebuffer state
    private static Vector2 _buildNdcScale;
    private static Vector2 _buildNdcOffset;
    private static Vector2 _buildCenterNdc;
    private static Vector2 _buildExtentNdcScale;
    private static Vector2 _buildNdcToPx;
    private static Vector4 _buildLocalToNdc; // x-from-X, x-from-Z, y-from-X, y-from-Z
    private static float _buildPixelScale;
    private static float _buildWorldPixelScale;
    // Maximum framebuffer pixel scale for SDF resolution selection, cached once per arena
    private static float _buildSdfResolutionPixelScale;
    private static float _buildOutlineAaPadScreen;
    private static float _buildInvScreenScale;
    private static float _buildDirectionCos;
    private static float _buildDirectionSin;
    private static float _buildAbsScaledCos;
    private static float _buildAbsScaledSin;
    private static float _buildSdfWxPx;
    private static float _buildSdfWxPy;
    private static float _buildSdfWzPx;
    private static float _buildSdfWzPy;
    private static float _buildSdfWx0;
    private static float _buildSdfWz0;
    private static int _buildFramebufferWidth;
    private static int _buildFramebufferHeight;

    internal static void SetWorldOverlayNode(WorldOverlayNode? node)
    {
        lock (_rendererLock)
        {
            _worldOverlayNode = node;
            _worldOverlayLastSucceeded = node != null && _worldOverlayOutputView != null;
        }
    }

    // Arena world-local -> screen transform, copied from MiniArena.Begin().
    private static float _buildCenterX;
    private static float _buildCenterY;
    private static float _buildScaledCos;
    private static float _buildScaledSin;
    private static float _buildScreenScale;
    private static RelSimplifiedComplexPolygon? _arenaShape;
    private static ArenaSdfResource? _buildArenaSdf;
    private static OutlineSdfConstants _buildOutlineSdfConstants;
    private static CustomSdfBinding[]? _buildCustomSdfs;
    private static int _buildCustomSdfCount;

    // call once during plugin initialization with dalamud.UiBuilder.DeviceHandle
    public static void Initialize(nint deviceHandle)
    {
        // Dispose any previous renderer generation first. Shutdown serializes against an in-flight
        // RenderBatchCallback and leaves the gate closed until all new resources are ready.
        Shutdown();

        lock (_rendererLock)
        {
            _device = (ID3D11Device*)deviceHandle;
            _device->AddRef();

            ID3D11DeviceContext* context = null;
            _device->GetImmediateContext(&context);
            _context = context;

            if (_context != null && CreateShadersAndLayouts() && CreateStencilStates() && CreateArenaSdfPipelineResources() && CreateArenaFontResources())
            {
                // Publish the initialized generation only after every object required by callbacks exists.
                _shutDown = false;
                _isInitialized = true;
                return;
            }
        }

        // Initialization failed. Keep the gate closed and release whatever was created.
        Shutdown();
    }

    public static void Shutdown()
    {
        lock (_rendererLock)
        {
            // Close the gate before touching any COM object. A callback that has not started yet will
            // return without using renderer state; an already-running callback owns _rendererLock, so
            // we wait here until its draw/state-restore finally block has completed.
            _shutDown = true;
            _isInitialized = false;
            _arenaActive = false;
            BuildPath.Clear();
            ResetBuildRun(returnArrays: true);

            lock (PendingPackets)
            {
                foreach (var packet in PendingPackets.Values)
                {
                    ReturnPacketArrays(packet);
                }
                PendingPackets.Clear();
                _submitFrame = -1;
            }

            Release(ref _uploadVertexBuffer);
            Release(ref _outlineSdfConstantBuffer);
            Release(ref _customSdfConstantBuffer);
            Release(ref _zoneWaveConstantBuffer);
            Release(ref _quadIndexBuffer);
            Release(ref _arenaSdfSampler);
            Release(ref _stencilView);
            Release(ref _stencilWriteState);
            Release(ref _stencilTestState);
            Release(ref _stencilDisabledState);
            Release(ref _meshInputLayout);
            Release(ref _strokeInputLayout);
            Release(ref _worldLineInputLayout);
            Release(ref _worldCurveInputLayout);
            Release(ref _worldProjectedArrowInputLayout);
            Release(ref _worldProjectedShapeInputLayout);
            Release(ref _worldProjectedSdfConstantBuffer);
            Release(ref _worldLineConstantBuffer);
            Release(ref _worldLineTransformBuffer);
            Release(ref _sceneDepthView);
            _sceneDepthTextureIdentity = 0;
            Release(ref _sceneInfoView);
            _sceneInfoViewIdentity = 0;
            ReleaseWorldOverlayResources();
            Release(ref _analyticInputLayout);
            Release(ref _textInputLayout);
            Release(ref _worldTextInputLayout);
            Release(ref _arenaFontAtlasView);
            _arenaTextGlyphs = null;
            _arenaIconGlyphs = null;
            _arenaTextKerning = null;
            _arenaTextMetrics = default;
            _arenaIconMetrics = default;
            Release(ref _outlineShapeInputLayout);
            Release(ref _meshVertexShader);
            Release(ref _meshPixelShader);
            Release(ref _primitiveTriangleStrokeVertexShader);
            Release(ref _primitiveTriangleStrokeInputLayout);
            Release(ref _strokeVertexShader);
            Release(ref _strokePixelShader);
            Release(ref _worldLineVertexShader);
            Release(ref _worldCurveVertexShader);
            Release(ref _worldProjectedArrowVertexShader);
            Release(ref _worldProjectedArrowPixelShader);
            Release(ref _worldProjectedShapeVertexShader);
            Release(ref _worldProjectedShapePixelShader);
            Release(ref _worldOverlayVertexShader);
            Release(ref _worldOverlayPixelShader);
            Release(ref _analyticVertexShader);
            Release(ref _analyticPixelShader);
            Release(ref _textVertexShader);
            Release(ref _textPixelShader);
            Release(ref _spritePixelShader);
            Release(ref _worldTextVertexShader);
            Release(ref _worldTextPixelShader);
            Release(ref _outlineShapeVertexShader);
            Release(ref _outlineShapePixelShader);
            Release(ref _outlineUnclippedPixelShader);
            Release(ref _customOutlinePixelShader);
            Release(ref _arenaSdfOutlinePixelShader);
            Release(ref _arenaSdfStencilPixelShader);
            Release(ref _customSdfFillPixelShader);
            Release(ref _outlineClipEdgePixelShader);
            Release(ref _customClipEdgePixelShader);

            ReleaseDeferredOutlineOverlays();

            foreach (var resource in ArenaSdfCache.Values)
            {
                var view = resource.View;
                if (view != null)
                {
                    view->Release();
                }
            }
            ArenaSdfCache.Clear();
            foreach (var resource in CustomSdfCache.Values)
            {
                var view = resource.View;
                if (view != null)
                {
                    view->Release();
                }
            }
            CustomSdfCache.Clear();
            _arenaSdfCacheBytes = 0;
            _customSdfCacheBytes = 0;
            _arenaSdfUseCounter = 0;
            _arenaShape = null;
            _buildArenaSdf = null;
            _buildCustomSdfs = null;
            _buildCustomSdfCount = 0;

            if (_context != null)
            {
                _context->Release();
                _context = null;
            }

            if (_device != null)
            {
                _device->Release();
                _device = null;
            }

            _uploadVertexCapacityBytes = 0;
            _uploadRenderFrame = -1;
            _uploadCursorBytes = 0;
            _uploadMeshOffsetBytes = 0u;
            _uploadPrimitiveTriangleStrokeOffsetBytes = 0u;
            _uploadStrokeOffsetBytes = 0u;
            _uploadWorldLineOffsetBytes = 0u;
            _uploadWorldCurveOffsetBytes = 0u;
            _uploadWorldProjectedArrowOffsetBytes = 0u;
            _uploadWorldProjectedShapeOffsetBytes = 0u;
            _uploadAnalyticOffsetBytes = 0u;
            _uploadOutlineOffsetBytes = 0u;
            _uploadTextOffsetBytes = 0u;
            _outlineSdfConstantsValid = false;
            _customSdfConstantsValid = false;
            _worldProjectedSdfConstantsValid = false;
            _uploadedWorldLineConstantsValid = false;
            _lastWorldProjectedSdfConstants = default;
            _lastUploadedWorldLineConstants = default;
            _zoneWaveUploadFrame = -1;
            _stencilWidth = 0;
            _stencilHeight = 0;
            _arenaStencilKey = 0L;
            _arenaStencilMaskQueued = false;
            _renderedStencilKey = 0L;
        }
    }

    // Starts accumulation for one MiniArena. Arena background and clipping are SDF-driven. Arena shapes are considered immutable and keyed by object identity
    public static bool BeginArena(ImDrawListPtr drawList, RelSimplifiedComplexPolygon arenaShape, float centerX, float centerY, float scaledCos, float scaledSin, float screenScale)
    {
        if (!_isInitialized)
        {
            return false;
        }

        if (_arenaActive)
        {
            EndArena();
        }

        ReleaseDeferredOutlineOverlays();
        BuildPath.Clear();
        ResetBuildRun(returnArrays: true);

        _buildDrawList = drawList;
        _arenaActive = true;
        _arenaPrepared = false;
        _buildViewportOverride = false;
        _arenaShape = arenaShape;
        _buildArenaSdf = null;
        _buildOutlineSdfConstants = default;
        _buildCustomSdfs = null;
        _buildCustomSdfCount = 0;
        _buildWorldLineConfigured = false;
        _buildWorldLineConstantsValid = false;
        _buildWorldLineTransformCount = 0;
        _buildWorldLineInvViewProj = default;
        _buildWorldSceneDepthSize = default;
        _buildWorldSceneDepthAvailable = false;
        _buildWorldSceneInfoAvailable = false;
        _buildCenterX = centerX;
        _buildCenterY = centerY;
        _buildScaledCos = scaledCos;
        _buildScaledSin = scaledSin;
        _buildScreenScale = screenScale;
        // Renderer build state is single-threaded by design (the immediate D3D11/ImGui render thread)
        _arenaStencilKey = NextArenaStencilKey();
        _arenaStencilMaskQueued = false;

        // Viewport/framebuffer state is deliberately deferred until the first actual DX11 primitive.
        // Empty arenas therefore do not cross into ImGui viewport/IO queries or packet-frame cleanup.
        return true;
    }

    // Changes the clipping polygon for subsequently submitted arena primitives. The current ordered
    // run is flushed first, so each callback owns exactly one arena SDF/stencil generation. Switching
    // back to an earlier polygon still receives a fresh generation key because another mask may have
    // overwritten the private stencil target in between.
    public static bool SetArenaStencil(RelSimplifiedComplexPolygon arenaShape)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return false;
        }
        if (ReferenceEquals(_arenaShape, arenaShape))
        {
            return true;
        }

        if (!Flush())
        {
            return false;
        }

        _arenaShape = arenaShape;
        _buildArenaSdf = null;
        _buildOutlineSdfConstants = default;
        _arenaStencilKey = NextArenaStencilKey();
        _arenaStencilMaskQueued = false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long NextArenaStencilKey()
    {
        var key = ++_nextArenaStencilKey;
        if (key == 0L)
        {
            key = ++_nextArenaStencilKey;
        }
        return key;
    }

    // Ensures an arbitrary world-projection clip has a cached SDF while the radar's real screen scale
    // is available. This deliberately does not replace _arenaShape or _buildArenaSdf: the live 2D
    // stencil remains untouched while the later world batch reuses the immutable cached resource.
    public static bool PrepareArenaSdfForWorldProjection(RelSimplifiedComplexPolygon arenaShape)
    {
        if (!_isInitialized || !_arenaActive || arenaShape == null)
        {
            return false;
        }
        EnsureBuildRunStarted();
        return GetOrCreateArenaSdfResource(arenaShape) != null;
    }

    // Starts one standalone screen-space batch on an arbitrary ImGui draw list. This is used by
    // Camera/world overlays: ImGui only hosts the deferred callback; all line rasterization is DX11.
    // Coordinates submitted through AppendScreenLine are absolute logical screen coordinates.
    public static bool BeginScreenBatch(ImDrawListPtr drawList, Vector2 viewportPos, Vector2 viewportSize)
    {
        if (!_isInitialized)
        {
            return false;
        }

        if (_arenaActive)
        {
            EndArena();
        }

        ReleaseDeferredOutlineOverlays();
        BuildPath.Clear();
        ResetBuildRun(returnArrays: true);

        _buildDrawList = drawList;
        _arenaActive = true;
        _arenaPrepared = false;
        _buildViewportOverride = true;
        _buildViewportOverridePos = viewportPos;
        _buildViewportOverrideSize = new Vector2(Math.Max(1f, viewportSize.X), Math.Max(1f, viewportSize.Y));
        _arenaShape = null;
        _buildArenaSdf = null;
        _buildOutlineSdfConstants = default;
        _buildCustomSdfs = null;
        _buildCustomSdfCount = 0;
        _buildWorldLineConfigured = false;
        _buildWorldLineConstantsValid = false;
        _buildWorldLineTransformCount = 0;
        _buildWorldLineInvViewProj = default;
        _buildWorldSceneDepthSize = default;
        _buildWorldSceneDepthAvailable = false;
        _buildWorldSceneInfoAvailable = false;

        // Identity local transform. Standalone screen primitives bypass arena-local conversion, but
        // keeping these invariants valid makes shared packet/setup code safe and inexpensive.
        _buildCenterX = 0f;
        _buildCenterY = 0f;
        _buildScaledCos = 1f;
        _buildScaledSin = 0f;
        _buildScreenScale = 1f;
        _arenaStencilKey = 0L;
        _arenaStencilMaskQueued = false;
        return true;
    }

    // Starts a standalone Camera/world batch whose primitives are projected by the GPU
    public static bool BeginWorldBatch(ImDrawListPtr drawList, Vector2 viewportPos, Vector2 viewportSize, in Matrix4x4 viewProj, in Vector4 nearPlane, ReadOnlySpan<WorldLineTransform> transforms, bool needsProjectedReceiverMask)
    {
        if (!BeginScreenBatch(drawList, viewportPos, viewportSize))
        {
            return false;
        }

        var len = transforms.Length;
        if (len > MaxWorldLineTransforms)
        {
            EndScreenBatch();
            return false;
        }

        if (len != 0)
        {
            EnsureWorldLineTransformBuildCapacity(len);
            transforms.CopyTo(_buildWorldLineTransforms);
        }
        _buildWorldLineTransformCount = len;

        // Capture projection in the same UI-submission epoch in which the scene resources below are
        // selected. The deferred DX callback can execute after the game thread has advanced the camera;
        // reading it there mixes the next matrix with this packet's depth and briefly rejects patches
        // of otherwise valid floor geometry while the camera moves.
        if (!TryReadStableGameViewProjection(out var snapshotViewProj) || !TryBuildWorldProjectionSnapshot(snapshotViewProj, out var snapshotInvViewProj, out var snapshotNearPlane))
        {
            snapshotViewProj = viewProj;
            if (!TryBuildWorldProjectionSnapshot(snapshotViewProj, out snapshotInvViewProj, out snapshotNearPlane))
            {
                snapshotInvViewProj = Matrix4x4.Identity;
                snapshotNearPlane = nearPlane;
            }
        }
        _buildWorldLineViewProj = snapshotViewProj;
        _buildWorldLineInvViewProj = snapshotInvViewProj;
        _buildWorldLineNearPlane = snapshotNearPlane;
        _buildWorldSceneDepthAvailable = TryUpdateSceneDepthResource(out _buildWorldSceneDepthSize);
        _buildWorldSceneInfoAvailable = false;
        if (needsProjectedReceiverMask && _buildWorldSceneDepthAvailable && TryUpdateSceneInfoResource(out var sceneInfoSize))
        {
            // Both resources are produced by the same deferred scene pass and normally have identical
            // active dimensions. Refuse the mask if that invariant changes: sampling a mismatched
            // classification texel would cut unrelated floor pixels around character silhouettes.
            _buildWorldSceneInfoAvailable = Math.Abs(sceneInfoSize.X - _buildWorldSceneDepthSize.X) < 0.5f
                && Math.Abs(sceneInfoSize.Y - _buildWorldSceneDepthSize.Y) < 0.5f;
        }
        _buildWorldLineConfigured = true;
        return true;
    }

    // Appends compact local/world line instances without CPU clipping/projection
    public static void AppendWorldLines(ReadOnlySpan<WorldLineInstance> lines)
    {
        if (lines.IsEmpty || !_isInitialized || !_arenaActive || !_buildWorldLineConfigured || _buildWorldLineTransformCount == 0)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureWorldLineConstants();
        var len = lines.Length;
        EnsureWorldLineBuildCapacity(_buildWorldLineCount + len);
        var start = _buildWorldLineCount;
        lines.CopyTo(_buildWorldLines!.AsSpan(start, len));
        _buildWorldLineCount += len;
        AppendSegment(SegmentKind.WorldLine, start, len);
    }

    // Appends procedural line-based world curves. lineCountPerInstance is the exact number of
    // generated line segments for each instance in this contiguous run; runs with different counts
    // stay separate so one indexed instanced draw can expand every curve without CPU endpoint generation
    public static void AppendWorldCurves(ReadOnlySpan<WorldCurveInstance> curves, int lineCountPerInstance)
    {
        if (curves.IsEmpty || lineCountPerInstance <= 0 || lineCountPerInstance > MaxIndexedWorldCurveLines || !_isInitialized || !_arenaActive || !_buildWorldLineConfigured || _buildWorldLineTransformCount == 0)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureWorldLineConstants();
        var len = curves.Length;
        EnsureWorldCurveBuildCapacity(_buildWorldCurveCount + len);
        var start = _buildWorldCurveCount;
        curves.CopyTo(_buildWorldCurves!.AsSpan(start, len));
        _buildWorldCurveCount += len;
        // WorldCurve does not use a custom SDF binding; this existing integer payload stores the
        // per-instance generated line count and also prevents incompatible runs from coalescing
        AppendSegment(SegmentKind.WorldCurve, start, len, lineCountPerInstance);
    }

    // Projected arrows are X/Z shapes evaluated against reconstructed FFXIV scene depth. The sampled
    // scene surface supplies the final world height, so these do not use the line/curve occlusion test.
    public static void AppendWorldProjectedArrows(ReadOnlySpan<WorldProjectedArrowInstance> arrows)
    {
        if (arrows.IsEmpty || !_isInitialized || !_arenaActive || !_buildWorldLineConfigured)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureWorldLineConstants();
        var len = arrows.Length;
        EnsureWorldProjectedArrowBuildCapacity(_buildWorldProjectedArrowCount + len);
        var start = _buildWorldProjectedArrowCount;
        arrows.CopyTo(_buildWorldProjectedArrows!.AsSpan(start, len));
        _buildWorldProjectedArrowCount += len;
        AppendSegment(SegmentKind.WorldProjectedArrow, start, len);
    }

    // Appends one ordered run of world-projected MiniArena primitives sharing the same optional
    // custom-shape and arena SDF bindings. Camera groups only consecutive compatible shapes, so this
    // reduces binding resolution/copy overhead without changing draw order or visual semantics.
    public static void AppendWorldProjectedShapes(ReadOnlySpan<WorldProjectedShapeInstance> shapes,
        RelSimplifiedComplexPolygon? shapeSdf = null, WPos shapeSdfOrigin = default,
        RelSimplifiedComplexPolygon? arenaSdf = null, WPos arenaSdfOrigin = default)
    {
        if (shapes.IsEmpty || !_isInitialized || !_arenaActive || !_buildWorldLineConfigured)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureWorldLineConstants();

        var shapeBinding = shapeSdf != null ? GetOrAddWorldProjectedSdfBinding(shapeSdf, shapeSdfOrigin, arenaCache: false) : -1;

        // A raw SDF primitive cannot render without its custom-shape resource. This normally follows
        // directly from shapeSdf != null, but retain the fail-closed invariant for generic callers.
        var len = shapes.Length;
        if (shapeBinding < 0)
        {
            for (var i = 0; i < len; ++i)
            {
                if ((shapes[i].Packed & 0xFFu) == (uint)WorldProjectedShapeKind.Sdf)
                {
                    return;
                }
            }
        }

        var arenaBinding = arenaSdf != null ? GetOrAddWorldProjectedSdfBinding(arenaSdf, arenaSdfOrigin, arenaCache: true) : -1;
        if (arenaSdf != null && arenaBinding < 0)
        {
            return;
        }

        EnsureWorldProjectedShapeBuildCapacity(_buildWorldProjectedShapeCount + len);
        var start = _buildWorldProjectedShapeCount;
        shapes.CopyTo(_buildWorldProjectedShapes!.AsSpan(start, len));
        _buildWorldProjectedShapeCount += len;
        // Only bind/update the animation constant buffer when this projected run contains a filled
        // zone. Outline-only primitives and actor markers (bit 10) stay on the static path.
        for (var i = 0; i < len; ++i)
        {
            ref readonly var shape = ref shapes[i];
            if ((shape.Packed & 0x100u) == 0u && (shape.Packed & 0x400u) == 0u)
            {
                _buildUsesZoneWave = true;
                break;
            }
        }
        AppendSegment(SegmentKind.WorldProjectedShape, start, len, shapeBinding, arenaBinding);
    }

    public static void AppendArenaBackground(uint color)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();
        // Build the SDF stencil once, then render the visible background through the normal mesh pipeline as a fixed two-triangle rectangle covering the SDF bounds
        EnsureStencilMaskBuilt();

        var sdf = _buildArenaSdf!;
        var minX = sdf.MinX;
        var minZ = sdf.MinZ;
        var maxX = minX + sdf.SpanX;
        var maxZ = minZ + sdf.SpanZ;

        EnsureMeshBuildCapacity(_buildMeshVertexCount + 6);
        var start = _buildMeshVertexCount;
        var dst = start;
        var vertices = _buildMeshVertices!;
        var a = new WDir(minX, minZ);
        var b = new WDir(maxX, minZ);
        var c = new WDir(maxX, maxZ);
        var d = new WDir(minX, maxZ);
        WriteMeshVertex(a, color, 0u, vertices, ref dst);
        WriteMeshVertex(b, color, 0u, vertices, ref dst);
        WriteMeshVertex(c, color, 0u, vertices, ref dst);
        WriteMeshVertex(a, color, 0u, vertices, ref dst);
        WriteMeshVertex(c, color, 0u, vertices, ref dst);
        WriteMeshVertex(d, color, 0u, vertices, ref dst);
        _buildMeshVertexCount = dst;

        AppendSegment(SegmentKind.Mesh, start, 6);
    }

    // Appends a relative complex polygon through the cached custom-SDF mesh-quad path. Polygons use PolygonBoundaryIndex2D's exact SIMD bulk builder
    public static void AppendRelPoly(RelSimplifiedComplexPolygon polygon, uint color)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        AppendCustomSdfFill(polygon, color);
    }

    private static void AppendCustomSdfFill(RelSimplifiedComplexPolygon polygon, uint color)
    {
        EnsureBuildRunStarted();
        EnsureStencilMaskBuilt();

        var custom = GetOrCreateCustomSdf(polygon);
        if (custom == null)
        {
            return;
        }
        var binding = GetOrAddBuildCustomSdfBinding(custom);

        // Use the mesh vertex/color path for the visible quad. The custom SDF is only responsible for rejecting pixels outside the polygon
        EnsureMeshBuildCapacity(_buildMeshVertexCount + 6);
        var start = _buildMeshVertexCount;
        var dst = start;
        var vertices = _buildMeshVertices!;
        var minX = custom.MinX;
        var minZ = custom.MinZ;
        var spanX = custom.SpanX;
        var spanZ = custom.SpanZ;
        var a = new WDir(minX, minZ);
        var b = new WDir(minX + spanX, minZ);
        var c = new WDir(minX + spanX, minZ + spanZ);
        var d = new WDir(minX, minZ + spanZ);
        WriteMeshVertex(a, color, 0u, vertices, ref dst);
        WriteMeshVertex(b, color, 0u, vertices, ref dst);
        WriteMeshVertex(c, color, 0u, vertices, ref dst);
        WriteMeshVertex(a, color, 0u, vertices, ref dst);
        WriteMeshVertex(c, color, 0u, vertices, ref dst);
        WriteMeshVertex(d, color, 0u, vertices, ref dst);
        _buildMeshVertexCount = dst;
        AppendSegment(SegmentKind.CustomSdfFill, start, 6, binding);
    }

    // Appends one raw arena-local triangle, the private stencil mask clips it to ArenaBounds
    public static void AppendTriangle(in WDir a, in WDir b, in WDir c, uint color)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureStencilMaskBuilt();
        EnsureMeshBuildCapacity(_buildMeshVertexCount + 3);

        var start = _buildMeshVertexCount;
        var vertices = _buildMeshVertices!;
        var dst = start;
        const uint boundaryMask = 0b111;
        WriteMeshVertex(a, color, boundaryMask, vertices, ref dst);
        WriteMeshVertex(b, color, boundaryMask, vertices, ref dst);
        WriteMeshVertex(c, color, boundaryMask, vertices, ref dst);

        _buildMeshVertexCount = dst;
        _buildNeedsStencil = true;
        AppendSegment(SegmentKind.Mesh, start, 3);
    }

    // Appends one unclipped general-purpose triangle. Unlike ZoneTri this does not use the arena
    // stencil; it is intended for actor markers and other primitives that may legitimately extend
    // onto the arena margins. True triangle edges get the mesh shader's derivative AA.
    public static void AppendPrimitiveTriangle(in WDir a, in WDir b, in WDir c, uint color)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureMeshBuildCapacity(_buildMeshVertexCount + 3);

        var start = _buildMeshVertexCount;
        var vertices = _buildMeshVertices!;
        var dst = start;
        const uint boundaryMask = 0b111u;
        WriteMeshVertex(a, color, boundaryMask, vertices, ref dst);
        WriteMeshVertex(b, color, boundaryMask, vertices, ref dst);
        WriteMeshVertex(c, color, boundaryMask, vertices, ref dst);
        _buildMeshVertexCount = dst;

        AppendSegment(SegmentKind.PrimitiveMesh, start, 3);
    }

    // Specialized closed-triangle stroke for the very hot actor-shadow/triangle-outline path.
    // It transforms the three points once, normalizes each edge once, computes the three joins
    // once, then emits the same six AA mesh triangles as the generic closed polyline
    public static void AppendPrimitiveTriangleStroke(in WDir a, in WDir b, in WDir c, uint color, float thickness)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();

        var pa = LocalToScreen(a);
        var pb = LocalToScreen(b);
        var pc = LocalToScreen(c);

        var d0 = NormalizeEdge(pb - pa);
        var d1 = NormalizeEdge(pc - pb);
        var d2 = NormalizeEdge(pa - pc);

        var n0 = new Vector2(-d0.Y, d0.X);
        var n1 = new Vector2(-d1.Y, d1.X);
        var n2 = new Vector2(-d2.Y, d2.X);
        var halfWidth = 0.5f * thickness;

        var oa = JoinOffset(n2, n0, halfWidth);
        var ob = JoinOffset(n0, n1, halfWidth);
        var oc = JoinOffset(n1, n2, halfWidth);

        // Six unique stroked corner positions feed all 18 mesh-AA triangle vertices.
        var aL = ScreenToNdc(pa + oa);
        var aR = ScreenToNdc(pa - oa);
        var bL = ScreenToNdc(pb + ob);
        var bR = ScreenToNdc(pb - ob);
        var cL = ScreenToNdc(pc + oc);
        var cR = ScreenToNdc(pc - oc);

        EnsurePrimitiveTriangleStrokeBuildCapacity(_buildPrimitiveTriangleStrokeCount + 1);
        var start = _buildPrimitiveTriangleStrokeCount;
        _buildPrimitiveTriangleStrokes![_buildPrimitiveTriangleStrokeCount++] = new PrimitiveTriangleStrokeInstance
        {
            AL = aL,
            AR = aR,
            BL = bL,
            BR = bR,
            CLR = new Vector4(cL.X, cL.Y, cR.X, cR.Y),
            Col = color,
        };
        AppendSegment(SegmentKind.PrimitiveTriangleStroke, start, 1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector2 NormalizeEdge(Vector2 d)
        {
            var lenSq = d.LengthSquared();
            return lenSq > 1e-8f ? d * (1f / MathF.Sqrt(lenSq)) : default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector2 JoinOffset(Vector2 previousNormal, Vector2 nextNormal, float halfWidth)
        {
            var sum = previousNormal + nextNormal;
            var sumLenSq = sum.LengthSquared();
            if (sumLenSq <= 1e-8f)
            {
                return nextNormal * halfWidth;
            }

            var miter = sum * (1f / MathF.Sqrt(sumLenSq));
            var denom = Vector2.Dot(miter, nextNormal);
            if (MathF.Abs(denom) < 0.2f)
            {
                denom = MathF.CopySign(0.2f, denom == 0f ? 1f : denom);
            }

            var result = miter * (halfWidth / denom);
            var maxLength = halfWidth * 4f;
            var resultLenSq = result.LengthSquared();
            if (resultLenSq > maxLength * maxLength)
            {
                result *= maxLength / MathF.Sqrt(resultLenSq);
            }
            return result;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 LocalToScreen(in WDir p)
    {
        var pX = p.X;
        var pZ = p.Z;
        return new(_buildCenterX + pX * _buildScaledCos - pZ * _buildScaledSin, _buildCenterY + pZ * _buildScaledCos + pX * _buildScaledSin);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 LocalToNdc(in WDir p)
    {
        var pX = p.X;
        var pZ = p.Z;
        return new(_buildCenterNdc.X + pX * _buildLocalToNdc.X + pZ * _buildLocalToNdc.Y, _buildCenterNdc.Y + pX * _buildLocalToNdc.Z + pZ * _buildLocalToNdc.W);
    }

    // Adds one arena-local point to the current MiniArena path, merging exact/near duplicates
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PathLineTo(in WDir point)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        AppendPathPoint(point);
    }

    // Adds an arena-local circular arc to the current path. Angles use MiniArena world convention:
    // 0 points south (+Z), positive angles rotate counterclockwise toward +X. Tessellation is chosen
    // from screen-space sagitta error so zoom/arena scale changes do not make curves visibly polygonal.
    public static void PathArcTo(in WDir center, float radius, float minAngleRadians, float maxAngleRadians)
    {
        if (!_isInitialized || !_arenaActive || !(radius > 0f))
        {
            return;
        }

        var span = maxAngleRadians - minAngleRadians;
        var absSpan = Math.Abs(span);
        if (absSpan <= 1e-7f)
        {
            var (sin, cos) = MathF.SinCos(minAngleRadians);
            AppendPathPoint(new WDir(center.X + radius * sin, center.Z + radius * cos));
            return;
        }

        // For a circle, sagitta = r * (1 - cos(step/2)). Keep that below a quarter logical pixel.
        var radiusPx = Math.Abs(radius * _buildScreenScale);
        float maxStep;
        if (!(radiusPx > PathArcMaxErrorPx))
        {
            maxStep = absSpan;
        }
        else
        {
            var cosHalfStep = Math.Clamp(1f - PathArcMaxErrorPx / radiusPx, -1f, 1f);
            maxStep = 2f * MathF.Acos(cosHalfStep);
            if (maxStep <= 1e-5f)
            {
                maxStep = absSpan;
            }
        }

        var segmentCount = Math.Clamp((int)MathF.Ceiling(absSpan / maxStep), 1, PathArcMaxSegments);
        var step = span / segmentCount;
        for (var i = 0; i <= segmentCount; ++i)
        {
            var angle = i == segmentCount ? maxAngleRadians : minAngleRadians + step * i;
            var (sin, cos) = MathF.SinCos(angle);
            AppendPathPoint(new WDir(center.X + radius * sin, center.Z + radius * cos));
        }
    }

    // Strokes and clears the current path through the distance-based DX11 polyline pipeline. Keep the
    // existing void API intact; MiniArena uses the result-bearing sibling to mirror only paths that
    // the 2D renderer actually accepted.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PathStroke(bool closed, uint color, float thickness)
        => PathStrokeWithResult(closed, color, thickness);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PathStrokeWithResult(bool closed, uint color, float thickness)
    {
        if (!_isInitialized || !_arenaActive || BuildPath.Count < (closed ? 3 : 2))
        {
            BuildPath.Clear();
            return false;
        }

        // A very common AddLine-style is exactly two points. Keep it out of the generic polyline scratch/dedup/segment loop and write the single stroke instance directly
        if (!closed && BuildPath.Count == 2)
        {
            AppendArenaLineFast(BuildPath[0], BuildPath[1], color, thickness);
        }
        else
        {
            AppendPolyline(CollectionsMarshal.AsSpan(BuildPath), closed, color, thickness);
        }
        // Matches ImDrawList::PathStroke semantics: consuming a path always clears it
        BuildPath.Clear();
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendPathPoint(in WDir point)
    {
        if (BuildPath.Count != 0)
        {
            var previous = BuildPath[^1];
            var dx = previous.X - point.X;
            var dz = previous.Z - point.Z;
            if (dx * dx + dz * dz <= 1e-12f)
            {
                return;
            }
        }
        BuildPath.Add(point);
    }

    // Fast path for the common two-point arena line
    // Consecutive calls coalesce into one Stroke draw segment in AppendSegment
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendArenaLineFast(in WDir from, in WDir to, uint color, float thickness, uint shadowColor = 0u, float shadowThickness = 0f)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        // Preserve AppendPolyline's screen-space near-duplicate threshold, but do it before arena preparation so a degenerate line remains essentially free
        var dx = to.X - from.X;
        var dz = to.Z - from.Z;
        var screenDx = dx * _buildScaledCos - dz * _buildScaledSin;
        var screenDy = dz * _buildScaledCos + dx * _buildScaledSin;
        if (screenDx * screenDx + screenDy * screenDy <= 1e-8f)
        {
            return;
        }

        EnsureBuildRunStarted();
        AppendStrokeLineNdc(LocalToNdc(from), LocalToNdc(to), color, thickness, shadowColor, shadowThickness);
    }

    // Appends a screen-thickness polyline as compact segment instances. CPU work is limited to
    // local->screen/NDC conversion and adjacent-point deduplication; the stroke VS reconstructs
    // segment directions, miter joins, caps, and the six raster vertices. Shadow + foreground are
    // composed in one pixel-shader pass so a shadowed line uploads only one instance/segment
    public static void AppendPolyline(ReadOnlySpan<WDir> points, bool closed, uint color, float thickness, uint shadowColor = 0u, float shadowThickness = 0f)
    {
        var len = points.Length;
        if (!_isInitialized || !_arenaActive || len < 2)
        {
            return;
        }

        // Avoid the generic scratch/dedup/neighbor loop for AddLine-style submissions
        if (!closed && len == 2)
        {
            AppendArenaLineFast(points[0], points[1], color, thickness, shadowColor, shadowThickness);
            return;
        }
        if (closed && len == 4)
        {
            AppendQuadStroke(points[0], points[1], points[2], points[3], color, thickness, shadowColor, shadowThickness);
            return;
        }
        Vector2[]? rented = null;
        try
        {
            EnsureBuildRunStarted();

            var pointStorage = len <= 256 ? stackalloc Vector2[len] : (rented = ArrayPool<Vector2>.Shared.Rent(len)).AsSpan(0, len);

            // Once the point count is final, reuse the same scratch span for NDC coordinates so each surviving point is converted once
            var count = 0;
            for (var i = 0; i < len; ++i)
            {
                var p = points[i];
                var pX = p.X;
                var pZ = p.Z;
                var screen = new Vector2(
                    _buildCenterX + pX * _buildScaledCos - pZ * _buildScaledSin,
                    _buildCenterY + pZ * _buildScaledCos + pX * _buildScaledSin);
                if (count == 0 || Vector2.DistanceSquared(pointStorage[count - 1], screen) > 1e-8f)
                {
                    pointStorage[count++] = screen;
                }
            }

            if (closed && count > 1 && Vector2.DistanceSquared(pointStorage[0], pointStorage[count - 1]) <= 1e-8f)
            {
                --count;
            }

            if (count < 2 || closed && count < 3)
            {
                return;
            }

            for (var i = 0; i < count; ++i)
            {
                pointStorage[i] = ScreenToNdc(pointStorage[i]);
            }
            var ndcPoints = pointStorage[..count];
            var segmentCount = closed ? count : count - 1;
            EnsureStrokeInstanceBuildCapacity(_buildStrokeInstanceCount + segmentCount);

            var start = _buildStrokeInstanceCount;
            var instances = _buildStrokeInstances!;
            var pixelScale = Math.Max(_buildPixelScale, 1e-5f);
            var ndcToPx = _buildNdcToPx;
            var colorHalfWidthPx = 0.5f * thickness * pixelScale;
            var hasShadow = shadowColor != 0u && shadowThickness > 0f;
            var shadowHalfWidthPx = hasShadow ? 0.5f * shadowThickness * pixelScale : 0f;
            if (!hasShadow)
            {
                shadowColor = 0u;
            }

            const uint startCap = 0x1u;
            const uint endCap = 0x2u;

            for (var i = 0; i < segmentCount; ++i)
            {
                var j = i + 1;
                if (j == count)
                {
                    j = 0;
                }

                var a = ndcPoints[i];
                var b = ndcPoints[j];
                var prev = i != 0 ? ndcPoints[i - 1] : (closed ? ndcPoints[count - 1] : a);
                var nextIndex = j + 1;
                var next = nextIndex < count ? ndcPoints[nextIndex] : (closed ? ndcPoints[0] : b);

                var flags = 0u;
                if (!closed && i == 0)
                {
                    flags |= startCap;
                }
                if (!closed && i + 1 == segmentCount)
                {
                    flags |= endCap;
                }

                instances[_buildStrokeInstanceCount++] = new StrokeInstance
                {
                    PrevNdc = prev,
                    ANdc = a,
                    BNdc = b,
                    NextNdc = next,
                    NdcToPx = ndcToPx,
                    WidthsPx = new Vector2(colorHalfWidthPx, shadowHalfWidthPx),
                    Col = color,
                    ShadowCol = shadowColor,
                    Flags = flags,
                };
            }

            AppendSegment(SegmentKind.Stroke, start, segmentCount);
        }
        finally
        {
            if (rented != null)
            {
                ArrayPool<Vector2>.Shared.Return(rented);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendQuadStroke(in WDir p0, in WDir p1, in WDir p2, in WDir p3, uint color, float thickness, uint shadowColor = 0u, float shadowThickness = 0f)
    {
        EnsureBuildRunStarted();

        var a = LocalToNdc(p0);
        var b = LocalToNdc(p1);
        var c = LocalToNdc(p2);
        var d = LocalToNdc(p3);

        var pixelScale = Math.Max(_buildPixelScale, 1e-5f);

        var hasShadow = shadowColor != 0u && shadowThickness > 0f;
        if (!hasShadow)
        {
            shadowColor = 0u;
        }

        var widths = new Vector2(0.5f * thickness * pixelScale, hasShadow ? 0.5f * shadowThickness * pixelScale : 0f);

        EnsureStrokeInstanceBuildCapacity(_buildStrokeInstanceCount + 4);

        var start = _buildStrokeInstanceCount;
        var instances = _buildStrokeInstances!;

        instances[_buildStrokeInstanceCount++] = new StrokeInstance
        {
            PrevNdc = d,
            ANdc = a,
            BNdc = b,
            NextNdc = c,
            NdcToPx = _buildNdcToPx,
            WidthsPx = widths,
            Col = color,
            ShadowCol = shadowColor,
            Flags = 0u
        };

        instances[_buildStrokeInstanceCount++] = new StrokeInstance
        {
            PrevNdc = a,
            ANdc = b,
            BNdc = c,
            NextNdc = d,
            NdcToPx = _buildNdcToPx,
            WidthsPx = widths,
            Col = color,
            ShadowCol = shadowColor,
            Flags = 0u
        };

        instances[_buildStrokeInstanceCount++] = new StrokeInstance
        {
            PrevNdc = b,
            ANdc = c,
            BNdc = d,
            NextNdc = a,
            NdcToPx = _buildNdcToPx,
            WidthsPx = widths,
            Col = color,
            ShadowCol = shadowColor,
            Flags = 0u
        };

        instances[_buildStrokeInstanceCount++] = new StrokeInstance
        {
            PrevNdc = c,
            ANdc = d,
            BNdc = a,
            NextNdc = b,
            NdcToPx = _buildNdcToPx,
            WidthsPx = widths,
            Col = color,
            ShadowCol = shadowColor,
            Flags = 0u
        };

        AppendSegment(SegmentKind.Stroke, start, 4);
    }

    private static void AppendScreenLine(Vector2 from, Vector2 to, uint color, float thickness, uint shadowColor = 0u, float shadowThickness = 0f)
    {
        if (!_isInitialized || !_arenaActive || Vector2.DistanceSquared(from, to) <= 1e-12f)
        {
            return;
        }

        EnsureBuildRunStarted();
        AppendStrokeLineNdc(ScreenToNdc(from), ScreenToNdc(to), color, thickness, shadowColor, shadowThickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendStrokeLineNdc(Vector2 a, Vector2 b, uint color, float thickness, uint shadowColor, float shadowThickness)
    {
        var pixelScale = Math.Max(_buildPixelScale, 1e-5f);
        var hasShadow = shadowColor != 0u && shadowThickness > 0f;
        if (!hasShadow)
        {
            shadowColor = 0u;
        }

        EnsureStrokeInstanceBuildCapacity(_buildStrokeInstanceCount + 1);
        var start = _buildStrokeInstanceCount;
        _buildStrokeInstances![_buildStrokeInstanceCount++] = new StrokeInstance
        {
            PrevNdc = a,
            ANdc = a,
            BNdc = b,
            NextNdc = b,
            NdcToPx = _buildNdcToPx,
            WidthsPx = new Vector2(0.5f * thickness * pixelScale, hasShadow ? 0.5f * shadowThickness * pixelScale : 0f),
            Col = color,
            ShadowCol = shadowColor,
            Flags = 0x3u,
        };
        AppendSegment(SegmentKind.Stroke, start, 1);
    }

    // Adds one unclipped, fixed-size screen circle whose center is anchored in arena-local world space
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendArenaScreenCircle(in WDir centerOffset, float radius, uint color)
        => AppendArenaScreenCircle(centerOffset, default, radius, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendArenaScreenCircle(in WDir centerOffset, Vector2 screenOffset, float radius, uint color)
    {
        if (!_isInitialized || !_arenaActive || !(radius > 0f))
        {
            return;
        }

        EnsureBuildRunStarted();
        var pixelScale = Math.Max(_buildPixelScale, 1e-5f);
        var aaPadScreen = 1.5f / pixelScale;
        AppendArenaScreenAnalytic(centerOffset, screenOffset, new Vector2(radius + aaPadScreen), default, new Vector4(radius * pixelScale, 0f, 0f, 0f), color);
    }

    // Adds one unclipped, analytic almond/eye lens whose center is anchored in arena-local world space, the shape is the exact intersection of two equal circles
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendArenaScreenEye(in WDir centerOffset, float halfWidth, float halfHeight, uint color)
        => AppendArenaScreenEye(centerOffset, default, halfWidth, halfHeight, color);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendArenaScreenEye(in WDir centerOffset, Vector2 screenOffset, float halfWidth, float halfHeight, uint color)
    {
        if (!_isInitialized || !_arenaActive || !(halfWidth > 0f) || !(halfHeight > 0f))
        {
            return;
        }

        EnsureBuildRunStarted();
        var pixelScale = MathF.Max(_buildPixelScale, 1e-5f);
        var radius = (halfWidth * halfWidth + halfHeight * halfHeight) / (2f * halfHeight);
        var offset = radius - halfHeight;
        var aaPadScreen = 1.5f / pixelScale;
        AppendArenaScreenAnalytic(centerOffset, screenOffset, new Vector2(halfWidth + aaPadScreen, halfHeight + aaPadScreen), default, new Vector4(radius * pixelScale, offset * pixelScale, 0f, 6f), color);
    }

    private static void AppendArenaScreenAnalytic(in WDir centerOffset, Vector2 screenOffset, Vector2 extentScreen, Vector2 directionScreen, Vector4 parameters, uint color)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        // Keep ScreenAnalytic so indicators outside ArenaBounds are not stencil-clipped
        EnsureArenaPrepared();
        EnsureAnalyticBuildCapacity(_buildAnalyticCount + 1);

        var start = _buildAnalyticCount;
        _buildAnalytics![_buildAnalyticCount++] = new AnalyticInstance
        {
            CenterNdc = LocalToNdc(centerOffset) + screenOffset * _buildNdcScale,
            ExtentNdc = extentScreen * _buildExtentNdcScale,
            ExtentPx = extentScreen * _buildPixelScale,
            DirectionScreen = directionScreen,
            Params = parameters,
            Col = color,
        };

        AppendSegment(SegmentKind.ScreenAnalytic, start, 1);
    }

    // Adds one analytic circle
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendCircle(in WDir centerOffset, float radius, uint color) => AppendCircleDonut(centerOffset, 0f, radius, color, isDonut: false);

    // Adds one analytic donut
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendDonut(in WDir centerOffset, float innerRadius, float outerRadius, uint color) => AppendCircleDonut(centerOffset, innerRadius, outerRadius, color, isDonut: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendCircleDonut(in WDir centerOffset, float innerRadius, float outerRadius, uint color, bool isDonut)
    {
        if (!_isInitialized || !_arenaActive || outerRadius <= 0f || innerRadius >= outerRadius)
        {
            return;
        }

        innerRadius = Math.Max(0f, innerRadius);
        var outerScreen = outerRadius * _buildScreenScale;
        EnsureArenaPrepared();
        var outerPx = outerRadius * _buildWorldPixelScale;
        var innerPx = innerRadius * _buildWorldPixelScale;
        AppendAnalytic(centerOffset, new Vector2(outerScreen), default, new Vector4(outerPx, innerPx, 0f, 0f), color);
    }

    // Adds an analytic directional rectangle
    public static void AppendRect(in WDir originOffset, in WDir direction, float lenFront, float lenBack, float halfWidth, uint color)
    {
        if (!_isInitialized || !_arenaActive || halfWidth <= 0f || lenFront + lenBack <= 0f || !TryNormalizeScreenDirection(direction, out var dirScreen, out var dirWorldX, out var dirWorldZ))
        {
            return;
        }

        var halfLengthWorld = 0.5f * (lenFront + lenBack);
        var centerShiftWorld = 0.5f * (lenFront - lenBack);
        var centerOffset = new WDir(originOffset.X + dirWorldX * centerShiftWorld, originOffset.Z + dirWorldZ * centerShiftWorld);

        var halfLengthScreen = halfLengthWorld * _buildScreenScale;
        var halfWidthScreen = halfWidth * _buildScreenScale;
        var perp = new Vector2(-dirScreen.Y, dirScreen.X);
        var extentScreen = new Vector2(Math.Abs(dirScreen.X) * halfLengthScreen + Math.Abs(perp.X) * halfWidthScreen, Math.Abs(dirScreen.Y) * halfLengthScreen + Math.Abs(perp.Y) * halfWidthScreen);

        AppendAnalytic(centerOffset, extentScreen, dirScreen, new Vector4(halfLengthWorld * _buildWorldPixelScale, halfWidth * _buildWorldPixelScale, 0f, 1f), color);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 PadAnalyticFillExtent(Vector2 tightExtentScreen, Vector2 previousExtentScreen)
    {
        // Analytic fill SDFs have an AA fringe outside the geometric zero contour. Keep up to two
        // framebuffer pixels around a tightened bound, but never rasterize beyond the previous
        // conservative quad
        var padScreen = 2f / Math.Max(_buildPixelScale, 1e-5f);
        return new(Math.Min(previousExtentScreen.X, tightExtentScreen.X + padScreen), Math.Min(previousExtentScreen.Y, tightExtentScreen.Y + padScreen));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 CrossExtentScreen(Vector2 directionScreen, float rangeScreen, float halfWidthScreen)
    {
        var ax = Math.Abs(directionScreen.X);
        var ay = Math.Abs(directionScreen.Y);
        return new(Math.Max(ax * rangeScreen + ay * halfWidthScreen, ax * halfWidthScreen + ay * rangeScreen),
            Math.Max(ay * rangeScreen + ax * halfWidthScreen, ay * halfWidthScreen + ax * rangeScreen));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 ConeExtentScreen(Vector2 directionScreen, float outerRadiusScreen, float sinHalfAngle, float cosHalfAngle)
    {
        // The radial extrema are the two sector endpoints plus any axis direction contained by the
        // angular interval. Because AppendAnalytic uses a center-symmetric quad, test both signs of
        // each axis via abs(direction.component) >= cos(halfAngle).
        var sideA = new Vector2(
            directionScreen.X * cosHalfAngle + directionScreen.Y * sinHalfAngle,
            directionScreen.Y * cosHalfAngle - directionScreen.X * sinHalfAngle);
        var sideB = new Vector2(
            directionScreen.X * cosHalfAngle - directionScreen.Y * sinHalfAngle,
            directionScreen.Y * cosHalfAngle + directionScreen.X * sinHalfAngle);
        var maxAbsX = Math.Abs(directionScreen.X) >= cosHalfAngle
            ? 1f
            : Math.Max(Math.Abs(sideA.X), Math.Abs(sideB.X));
        var maxAbsY = Math.Abs(directionScreen.Y) >= cosHalfAngle
            ? 1f
            : Math.Max(Math.Abs(sideA.Y), Math.Abs(sideB.Y));
        return new Vector2(maxAbsX * outerRadiusScreen, maxAbsY * outerRadiusScreen);
    }

    private static Vector2 ArcCapsuleExtentScreen(Vector2 startDirectionScreen, Vector2 endDirectionScreen, float angularLengthRadians, float orbitRadiusScreen, float radiusScreen)
    {
        var maxAbsX = Math.Max(Math.Abs(startDirectionScreen.X), Math.Abs(endDirectionScreen.X));
        var maxAbsY = Math.Max(Math.Abs(startDirectionScreen.Y), Math.Abs(endDirectionScreen.Y));
        var absSweep = Math.Abs(angularLengthRadians);
        const float twoPi = 2f * MathF.PI;
        if (absSweep >= twoPi - 1e-5f)
        {
            maxAbsX = maxAbsY = 1f;
        }
        else
        {
            // Positive BossMod sweep rotates clockwise in screen coordinates. Convert the start
            // direction to the ordinary screen-space polar angle once, then test the four cardinal
            // extrema against the directed interval. The small epsilon keeps the bound conservative at cardinal/end-point coincidences
            var startAngle = MathF.Atan2(startDirectionScreen.Y, startDirectionScreen.X);
            if (ArcSweepContainsAngle(startAngle, angularLengthRadians, 0f) || ArcSweepContainsAngle(startAngle, angularLengthRadians, MathF.PI))
            {
                maxAbsX = 1f;
            }
            if (ArcSweepContainsAngle(startAngle, angularLengthRadians, 0.5f * MathF.PI) || ArcSweepContainsAngle(startAngle, angularLengthRadians, -0.5f * MathF.PI))
            {
                maxAbsY = 1f;
            }
        }

        // Radius expands the centerline AABB in both axes. Add a tiny logical-screen pad solely to
        // guard floating-point interval tests; this is far below the AA footprint
        const float conservativePad = 1e-4f;
        return new Vector2(maxAbsX * orbitRadiusScreen + radiusScreen + conservativePad, maxAbsY * orbitRadiusScreen + radiusScreen + conservativePad);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ArcSweepContainsAngle(float startAngle, float sweep, float candidateAngle)
    {
        const float twoPi = 2f * MathF.PI;
        var rel = sweep >= 0f ? startAngle - candidateAngle : candidateAngle - startAngle;
        rel %= twoPi;
        if (rel < 0f)
        {
            rel += twoPi;
        }
        return rel <= Math.Abs(sweep) + 1e-5f;
    }

    // Adds an analytic cross: the union of two centered orthogonal rectangles.
    // One instance is used so translucent colors are blended only once in the overlapping center.
    public static void AppendCross(in WDir centerOffset, in WDir direction, float range, float halfWidth, uint color)
    {
        if (!_isInitialized || !_arenaActive || range <= 0f || halfWidth <= 0f || !TryNormalizeScreenDirection(direction, out var dirScreen, out _, out _))
        {
            return;
        }

        var rangeScreen = range * _buildScreenScale;
        var halfWidthScreen = halfWidth * _buildScreenScale;

        var tightExtentScreen = CrossExtentScreen(dirScreen, rangeScreen, halfWidthScreen);
        var previousLocalExtent = Math.Max(rangeScreen, halfWidthScreen);
        var previousExtent = (Math.Abs(dirScreen.X) + Math.Abs(dirScreen.Y)) * previousLocalExtent;
        var extentScreen = PadAnalyticFillExtent(tightExtentScreen, new Vector2(previousExtent));

        AppendAnalytic(centerOffset, extentScreen, dirScreen, new Vector4(range * _buildWorldPixelScale, halfWidth * _buildWorldPixelScale, 0f, 5f), color);
    }

    // Adds an analytic annular sector/cone
    public static void AppendCone(in WDir centerOffset, float innerRadius, float outerRadius, in WDir direction, float halfAngleRadians, uint color)
    {
        if (!_isInitialized || !_arenaActive || outerRadius <= 0f || innerRadius >= outerRadius || halfAngleRadians <= 0f || !TryNormalizeScreenDirection(direction, out var dirScreen, out _, out _))
        {
            return;
        }

        innerRadius = Math.Max(0f, innerRadius);
        halfAngleRadians = Math.Min(MathF.PI, halfAngleRadians);
        var (sinHalfAngle, cosHalfAngle) = MathF.SinCos(halfAngleRadians);
        var outerScreen = outerRadius * _buildScreenScale;
        var outerPx = outerRadius * _buildWorldPixelScale;
        var innerPx = innerRadius * _buildWorldPixelScale;
        var tightExtentScreen = ConeExtentScreen(dirScreen, outerScreen, sinHalfAngle, cosHalfAngle);
        var extentScreen = PadAnalyticFillExtent(tightExtentScreen, new Vector2(outerScreen));

        AppendAnalytic(centerOffset, extentScreen, dirScreen, new Vector4(outerPx, innerPx, cosHalfAngle, 2f), color);
    }

    // Adds an analytic straight capsule around [start, start + direction * length]
    public static void AppendCapsule(in WDir startOffset, in WDir direction, float radius, float length, uint color)
    {
        if (!_isInitialized || !_arenaActive || radius <= 0f || length < 0f)
        {
            return;
        }
        if (!TryNormalizeScreenDirection(direction, out var dirScreen, out var dirWorldX, out var dirWorldZ))
        {
            // A zero-length capsule is just a circle
            AppendCircle(startOffset, radius, color);
            return;
        }

        var halfSegmentWorld = 0.5f * length;
        var centerOffset = new WDir(startOffset.X + dirWorldX * halfSegmentWorld, startOffset.Z + dirWorldZ * halfSegmentWorld);
        var halfSegmentScreen = halfSegmentWorld * _buildScreenScale;
        var radiusScreen = radius * _buildScreenScale;
        var extentScreen = new Vector2(Math.Abs(dirScreen.X) * halfSegmentScreen + radiusScreen, Math.Abs(dirScreen.Y) * halfSegmentScreen + radiusScreen);

        AppendAnalytic(centerOffset, extentScreen, dirScreen, new Vector4(halfSegmentWorld * _buildWorldPixelScale, radius * _buildWorldPixelScale, 0f, 3f), color);
    }

    // Adds an analytic curved capsule: the radius-neighborhood of the circular arc that starts at startOffset and rotates around orbitCenter by angularLengthRadians
    public static void AppendArcCapsule(in WDir startOffset, in WDir toOrbitCenter, float angularLengthRadians, float radius, uint color)
    {
        if (!_isInitialized || !_arenaActive || radius <= 0f)
        {
            return;
        }

        var orbitCenterX = toOrbitCenter.X;
        var orbitCenterZ = toOrbitCenter.Z;
        var orbitRadiusSq = orbitCenterX * orbitCenterX + orbitCenterZ * orbitCenterZ;
        if (!(orbitRadiusSq > 1e-12f) || Math.Abs(angularLengthRadians) < 1e-6f)
        {
            AppendCircle(startOffset, radius, color);
            return;
        }

        var orbitRadius = MathF.Sqrt(orbitRadiusSq);
        var invOrbitRadius = 1f / orbitRadius;
        var orbitCenterOffset = new WDir(startOffset.X + orbitCenterX, startOffset.Z + orbitCenterZ);

        // We already normalized the radial vector while computing orbitRadius, so do not feed it
        // through the generic direction normalizer (which would calculate another length/sqrt).
        EnsureArenaPrepared();
        var startDirectionScreen = UnitWorldDirectionToScreen(-orbitCenterX * invOrbitRadius, -orbitCenterZ * invOrbitRadius);

        var twoPi = 2f * MathF.PI;
        if (Math.Abs(angularLengthRadians) >= twoPi - 1e-5f)
        {
            // A full revolution is simply the radius-neighborhood of a circle. Reuse the donut shader;
            // when radius >= orbitRadius the inner radius naturally collapses to zero.
            var inner = Math.Max(0f, orbitRadius - radius);
            var outer = orbitRadius + radius;
            AppendDonut(orbitCenterOffset, inner, outer, color);
            return;
        }

        var orbitRadiusScreen = orbitRadius * _buildScreenScale;
        var radiusScreen = radius * _buildScreenScale;
        var (sinSweep, cosSweep) = MathF.SinCos(angularLengthRadians);
        var endDirectionScreen = new Vector2(
            startDirectionScreen.X * cosSweep + startDirectionScreen.Y * sinSweep,
            startDirectionScreen.Y * cosSweep - startDirectionScreen.X * sinSweep);
        var tightExtentScreen = ArcCapsuleExtentScreen(startDirectionScreen, endDirectionScreen, angularLengthRadians, orbitRadiusScreen, radiusScreen);
        var extentScreen = PadAnalyticFillExtent(tightExtentScreen, new Vector2(orbitRadiusScreen + radiusScreen));

        AppendAnalytic(orbitCenterOffset, extentScreen, startDirectionScreen, new Vector4(orbitRadius * _buildWorldPixelScale, radius * _buildWorldPixelScale, angularLengthRadians, 4f), color);
    }

    private static void AppendAnalytic(in WDir centerOffset, Vector2 extentScreen, Vector2 directionScreen, Vector4 parameters, uint color)
    {
        EnsureBuildRunStarted();
        EnsureStencilMaskBuilt();
        EnsureAnalyticBuildCapacity(_buildAnalyticCount + 1);

        var centerNdc = LocalToNdc(centerOffset);
        var extentNdc = extentScreen * _buildExtentNdcScale;
        var extentPx = extentScreen * _buildPixelScale;

        var start = _buildAnalyticCount;
        _buildAnalytics![_buildAnalyticCount++] = new AnalyticInstance
        {
            CenterNdc = centerNdc,
            ExtentNdc = extentNdc,
            ExtentPx = extentPx,
            DirectionScreen = directionScreen,
            Params = parameters,
            Col = color,
        };

        _buildNeedsStencil = true;
        AppendSegment(SegmentKind.Analytic, start, 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendCircleOutline(in WDir centerOffset, float radius, uint color, float lineThickness, uint shadowColor, float shadowThickness)
        => AppendCircleDonutOutline(centerOffset, 0f, radius, color, lineThickness, shadowColor, shadowThickness, clipToArena: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendCircleOutlineUnclipped(in WDir centerOffset, float radius, uint color, float lineThickness, uint shadowColor, float shadowThickness)
        => AppendCircleDonutOutline(centerOffset, 0f, radius, color, lineThickness, shadowColor, shadowThickness, clipToArena: false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendDonutOutline(in WDir centerOffset, float innerRadius, float outerRadius, uint color, float lineThickness, uint shadowColor, float shadowThickness)
        => AppendCircleDonutOutline(centerOffset, innerRadius, outerRadius, color, lineThickness, shadowColor, shadowThickness, clipToArena: true);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendCircleDonutOutline(in WDir centerOffset, float innerRadius, float outerRadius, uint color, float lineThickness, uint shadowColor, float shadowThickness, bool clipToArena)
    {
        if (!_isInitialized || !_arenaActive || outerRadius <= 0f || innerRadius >= outerRadius)
        {
            return;
        }

        innerRadius = Math.Max(0f, innerRadius);
        EnsureArenaPrepared();
        var outerScreen = outerRadius * _buildScreenScale;
        AppendAnalyticOutline(centerOffset, new Vector2(outerScreen), default, new Vector4(outerRadius * _buildWorldPixelScale, innerRadius * _buildWorldPixelScale, 0f, 0f),
            color, lineThickness, shadowColor, shadowThickness, clipToArena: clipToArena);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendRectOutline(in WDir originOffset, in WDir direction, float lenFront, float lenBack, float halfWidth, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive || halfWidth <= 0f || lenFront + lenBack <= 0f || !TryNormalizeScreenDirection(direction, out var dirScreen, out var dirWorldX, out var dirWorldZ))
        {
            return;
        }
        var halfLengthWorld = 0.5f * (lenFront + lenBack);
        var centerShiftWorld = 0.5f * (lenFront - lenBack);
        var centerOffset = new WDir(originOffset.X + dirWorldX * centerShiftWorld, originOffset.Z + dirWorldZ * centerShiftWorld);
        var halfLengthScreen = halfLengthWorld * _buildScreenScale;
        var halfWidthScreen = halfWidth * _buildScreenScale;
        var perp = new Vector2(-dirScreen.Y, dirScreen.X);
        var extentScreen = new Vector2(Math.Abs(dirScreen.X) * halfLengthScreen + Math.Abs(perp.X) * halfWidthScreen, Math.Abs(dirScreen.Y) * halfLengthScreen + Math.Abs(perp.Y) * halfWidthScreen);
        AppendAnalyticOutline(centerOffset, extentScreen, dirScreen, new Vector4(halfLengthWorld * _buildWorldPixelScale, halfWidth * _buildWorldPixelScale, 0f, 1f),
            color, lineThickness, shadowColor, shadowThickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendCrossOutline(in WDir centerOffset, in WDir direction, float range, float halfWidth, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive || range <= 0f || halfWidth <= 0f || !TryNormalizeScreenDirection(direction, out var dirScreen, out _, out _))
        {
            return;
        }

        var rangeScreen = range * _buildScreenScale;
        var halfWidthScreen = halfWidth * _buildScreenScale;
        var extentScreen = CrossExtentScreen(dirScreen, rangeScreen, halfWidthScreen);
        AppendAnalyticOutline(centerOffset, extentScreen, dirScreen, new Vector4(range * _buildWorldPixelScale, halfWidth * _buildWorldPixelScale, 0f, 5f),
            color, lineThickness, shadowColor, shadowThickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendConeOutline(in WDir centerOffset, float innerRadius, float outerRadius, in WDir direction, float halfAngleRadians, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive || outerRadius <= 0f || innerRadius >= outerRadius || halfAngleRadians <= 0f || !TryNormalizeScreenDirection(direction, out var dirScreen, out _, out _))
        {
            return;
        }

        innerRadius = Math.Max(0f, innerRadius);
        halfAngleRadians = Math.Min(MathF.PI, Math.Abs(halfAngleRadians));
        var (sinHalfAngle, cosHalfAngle) = MathF.SinCos(halfAngleRadians);
        var outerScreen = outerRadius * _buildScreenScale;
        var extentScreen = ConeExtentScreen(dirScreen, outerScreen, sinHalfAngle, cosHalfAngle);
        // Params.z/Extra carry precomputed angle terms so the outline PS does no uniform sin/cos.
        AppendAnalyticOutline(centerOffset, extentScreen, dirScreen, new Vector4(outerRadius * _buildWorldPixelScale, innerRadius * _buildWorldPixelScale, cosHalfAngle, 2f),
            color, lineThickness, shadowColor, shadowThickness, new Vector2(sinHalfAngle, halfAngleRadians));
    }

    public static void AppendCapsuleOutline(in WDir startOffset, in WDir direction, float radius, float length, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive || radius <= 0f || length < 0f)
        {
            return;
        }
        if (!TryNormalizeScreenDirection(direction, out var dirScreen, out var dirWorldX, out var dirWorldZ))
        {
            AppendCircleOutline(startOffset, radius, color, lineThickness, shadowColor, shadowThickness);
            return;
        }

        var halfSegmentWorld = 0.5f * length;
        var centerOffset = new WDir(startOffset.X + dirWorldX * halfSegmentWorld, startOffset.Z + dirWorldZ * halfSegmentWorld);
        var halfSegmentScreen = halfSegmentWorld * _buildScreenScale;
        var radiusScreen = radius * _buildScreenScale;
        var extentScreen = new Vector2(Math.Abs(dirScreen.X) * halfSegmentScreen + radiusScreen, Math.Abs(dirScreen.Y) * halfSegmentScreen + radiusScreen);
        AppendAnalyticOutline(centerOffset, extentScreen, dirScreen, new Vector4(halfSegmentWorld * _buildWorldPixelScale, radius * _buildWorldPixelScale, 0f, 3f),
            color, lineThickness, shadowColor, shadowThickness);
    }

    public static void AppendArcCapsuleOutline(in WDir startOffset, in WDir toOrbitCenter, float angularLengthRadians, float radius, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive || radius <= 0f)
        {
            return;
        }

        var orbitcenterX = toOrbitCenter.X;
        var orbitcenterZ = toOrbitCenter.Z;
        var orbitRadiusSq = orbitcenterX * orbitcenterX + orbitcenterZ * orbitcenterZ;
        if (!(orbitRadiusSq > 1e-12f) || MathF.Abs(angularLengthRadians) < 1e-6f)
        {
            AppendCircleOutline(startOffset, radius, color, lineThickness, shadowColor, shadowThickness);
            return;
        }

        var orbitRadius = MathF.Sqrt(orbitRadiusSq);
        var invOrbitRadius = 1f / orbitRadius;
        var orbitCenterOffset = new WDir(startOffset.X + orbitcenterX, startOffset.Z + orbitcenterZ);
        EnsureArenaPrepared();
        var startDirectionScreen = UnitWorldDirectionToScreen(-orbitcenterX * invOrbitRadius, -orbitcenterZ * invOrbitRadius);

        var twoPi = 2f * MathF.PI;
        if (Math.Abs(angularLengthRadians) >= twoPi - 1e-5f)
        {
            var inner = Math.Max(0f, orbitRadius - radius);
            var outer = orbitRadius + radius;
            AppendDonutOutline(orbitCenterOffset, inner, outer, color, lineThickness, shadowColor, shadowThickness);
            return;
        }

        var orbitRadiusScreen = orbitRadius * _buildScreenScale;
        var radiusScreen = radius * _buildScreenScale;
        var (sinSweep, cosSweep) = MathF.SinCos(angularLengthRadians);
        var startX = startDirectionScreen.X;
        var startY = startDirectionScreen.Y;
        var endDirectionScreen = new Vector2(startX * cosSweep + startY * sinSweep, startY * cosSweep - startX * sinSweep);
        var extentScreen = ArcCapsuleExtentScreen(startDirectionScreen, endDirectionScreen, angularLengthRadians, orbitRadiusScreen, radiusScreen);
        AppendAnalyticOutline(orbitCenterOffset, extentScreen, startDirectionScreen,
            new Vector4(orbitRadius * _buildWorldPixelScale, radius * _buildWorldPixelScale, angularLengthRadians, 4f),
            color, lineThickness, shadowColor, shadowThickness, endDirectionScreen);
    }

    // Appends an arbitrary triangle outline analytically. The triangle itself is represented by its
    // three screen-local vertices and intersected with the cached arena SDF in the pixel shader
    public static void AppendTriangleOutline(in WDir a, in WDir b, in WDir c, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        var abX = b.X - a.X;
        var abZ = b.Z - a.Z;
        var acX = c.X - a.X;
        var acZ = c.Z - a.Z;
        if (Math.Abs(abX * acZ - abZ * acX) < 1e-8f)
        {
            return;
        }

        var centerOffset = new WDir((a.X + b.X + c.X) * OneThird, (a.Z + b.Z + c.Z) * OneThird);

        var pixelScale = LocalPixelScale();
        var centerOffsetX = centerOffset.X;
        var centerOffsetZ = centerOffset.Z;
        var laLogical = LocalDeltaToScreen(new WDir(a.X - centerOffsetX, a.Z - centerOffsetZ));
        var lbLogical = LocalDeltaToScreen(new WDir(b.X - centerOffsetX, b.Z - centerOffsetZ));
        var lcLogical = LocalDeltaToScreen(new WDir(c.X - centerOffsetX, c.Z - centerOffsetZ));
        var extentScreen = new Vector2(
            Math.Max(Math.Abs(laLogical.X), Math.Max(Math.Abs(lbLogical.X), Math.Abs(lcLogical.X))),
            Math.Max(Math.Abs(laLogical.Y), Math.Max(Math.Abs(lbLogical.Y), Math.Abs(lcLogical.Y))));

        var la = laLogical * pixelScale;
        var lb = lbLogical * pixelScale;
        var lc = lcLogical * pixelScale;

        AppendAnalyticOutline(centerOffset, extentScreen, la, new Vector4(lb.X, lb.Y, lc.X, 6f), color, lineThickness, shadowColor, shadowThickness, new Vector2(lc.Y, 0f));
    }

    // Appends the visible arena border directly from the same cached arena SDF used by clipped
    // outlines. This keeps polygonal/approximated-circle borders geometrically identical to the
    // clipping boundary and avoids mitered-polyline joins protruding over clipped outlines.
    public static void AppendArenaOutline(uint color, float lineThickness, uint shadowColor = 0u, float shadowThickness = 0u)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();
        if (!EnsureArenaSdfForBuild())
        {
            return;
        }

        EnsureOutlineBuildCapacity(_buildOutlineCount + 1);

        var pixelScale = _buildPixelScale;
        lineThickness = Math.Max(0f, lineThickness);
        shadowThickness = Math.Max(lineThickness, shadowThickness);
        if ((shadowColor >> 24) == 0u)
        {
            shadowThickness = lineThickness;
        }

        var arena = _buildArenaSdf!;
        var spanX = arena.SpanX;
        var spanZ = arena.SpanZ;
        var centerOffset = new WDir(arena.MinX + 0.5f * spanX, arena.MinZ + 0.5f * spanZ);
        var halfX = 0.5f * spanX;
        var halfZ = 0.5f * spanZ;
        var extentScreen = new Vector2(
            _buildAbsScaledCos * halfX + _buildAbsScaledSin * halfZ,
            _buildAbsScaledSin * halfX + _buildAbsScaledCos * halfZ);

        var padScreen = 0.5f * shadowThickness + _buildOutlineAaPadScreen;
        var expandedExtentScreen = extentScreen + new Vector2(padScreen);
        var centerNdc = LocalToNdc(centerOffset);
        var extentNdc = expandedExtentScreen * _buildExtentNdcScale;

        var start = _buildOutlineCount;
        _buildOutlines![_buildOutlineCount++] = new OutlineInstance
        {
            CenterNdc = centerNdc,
            ExtentNdc = extentNdc,
            ExtentPx = expandedExtentScreen * pixelScale,
            DirectionScreen = default,
            Params = default,
            Extra = default,
            WidthsPx = new Vector2(0.5f * lineThickness * pixelScale, 0.5f * shadowThickness * pixelScale),
            Col = color,
            ShadowCol = shadowColor,
        };

        AppendSegment(SegmentKind.ArenaSdfOutline, start, 1);
    }

    // Appends a clipped custom polygon outline through a cached local-space SDF. Build packets retain
    // every distinct custom SDF they reference; the callback switches t2/b2 per segment as needed
    public static void AppendCustomOutline(RelSimplifiedComplexPolygon polygon, uint color, float lineThickness, uint shadowColor, float shadowThickness)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();
        if (!EnsureArenaSdfForBuild())
        {
            return;
        }

        var custom = GetOrCreateCustomSdf(polygon);
        if (custom == null)
        {
            return;
        }
        var binding = GetOrAddBuildCustomSdfBinding(custom);

        EnsureOutlineBuildCapacity(_buildOutlineCount + 1);

        var pixelScale = _buildPixelScale;
        lineThickness = Math.Max(0f, lineThickness);
        shadowThickness = Math.Max(lineThickness, shadowThickness);
        if ((shadowColor >> 24) == 0u)
        {
            shadowThickness = lineThickness;
        }

        // The final clipped outline is entirely contained by the arena SDF domain (plus stroke
        // width), so rasterizing one arena-sized quad is sufficient regardless of custom bounds.
        var arena = _buildArenaSdf!;
        var spanX = arena.SpanX;
        var spanZ = arena.SpanZ;
        var centerOffset = new WDir(arena.MinX + 0.5f * spanX, arena.MinZ + 0.5f * spanZ);
        var halfX = 0.5f * spanX;
        var halfZ = 0.5f * spanZ;
        var extentScreen = new Vector2(_buildAbsScaledCos * halfX + _buildAbsScaledSin * halfZ, _buildAbsScaledSin * halfX + _buildAbsScaledCos * halfZ);

        var padScreen = 0.5f * shadowThickness + _buildOutlineAaPadScreen;
        var expandedExtentScreen = extentScreen + new Vector2(padScreen);
        var centerNdc = LocalToNdc(centerOffset);
        var extentNdc = expandedExtentScreen * _buildExtentNdcScale;

        var start = _buildOutlineCount;
        var instance = new OutlineInstance
        {
            CenterNdc = centerNdc,
            ExtentNdc = extentNdc,
            ExtentPx = expandedExtentScreen * pixelScale,
            DirectionScreen = default,
            Params = default,
            Extra = default,
            WidthsPx = new Vector2(0.5f * lineThickness * pixelScale, 0.5f * shadowThickness * pixelScale),
            Col = color,
            ShadowCol = shadowColor,
        };
        _buildOutlines![_buildOutlineCount++] = instance;

        AppendSegment(SegmentKind.CustomSdfOutline, start, 1, binding);
        DeferClippedOutlineOverlay(ref instance, SegmentKind.CustomClipEdgeOverlay, _buildArenaSdf!, custom);
    }

    private static void AppendAnalyticOutline(in WDir centerOffset, Vector2 extentScreen, Vector2 directionScreen, Vector4 parameters, uint color, float lineThickness, uint shadowColor, float shadowThickness, Vector2 extra = default, bool clipToArena = true)
    {
        if (!_isInitialized || !_arenaActive)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureOutlineBuildCapacity(_buildOutlineCount + 1);
        if (clipToArena && !EnsureArenaSdfForBuild())
        {
            return;
        }

        var pixelScale = _buildPixelScale;
        lineThickness = Math.Max(0f, lineThickness);
        shadowThickness = Math.Max(lineThickness, shadowThickness);
        if ((shadowColor >> 24) == 0u)
        {
            shadowThickness = lineThickness;
        }

        var padScreen = 0.5f * shadowThickness + _buildOutlineAaPadScreen;
        var expandedExtentScreen = extentScreen + new Vector2(padScreen);
        var centerNdc = LocalToNdc(centerOffset);
        var extentNdc = expandedExtentScreen * _buildExtentNdcScale;

        var start = _buildOutlineCount;
        var instance = new OutlineInstance
        {
            CenterNdc = centerNdc,
            ExtentNdc = extentNdc,
            ExtentPx = expandedExtentScreen * pixelScale,
            DirectionScreen = directionScreen,
            Params = parameters,
            Extra = extra,
            WidthsPx = new Vector2(0.5f * lineThickness * pixelScale, 0.5f * shadowThickness * pixelScale),
            Col = color,
            ShadowCol = shadowColor,
        };
        _buildOutlines![_buildOutlineCount++] = instance;

        AppendSegment(clipToArena ? SegmentKind.AnalyticOutline : SegmentKind.AnalyticOutlineUnclipped, start, 1);
        if (clipToArena)
        {
            DeferClippedOutlineOverlay(ref instance, SegmentKind.AnalyticClipEdgeOverlay, _buildArenaSdf!, null);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 LocalDeltaToScreen(in WDir p)
    {
        var pX = p.X;
        var pZ = p.Z;
        return new(pX * _buildScaledCos - pZ * _buildScaledSin, pZ * _buildScaledCos + pX * _buildScaledSin);
    }

    private static int GetOrAddWorldProjectedSdfBinding(RelSimplifiedComplexPolygon polygon, WPos worldOrigin, bool arenaCache)
    {
        var resource = GetOrCreateWorldProjectedSdfResource(polygon, arenaCache);
        if (resource == null || resource.View == null)
        {
            return -1;
        }

        var map = new Vector4(worldOrigin.X + resource.MinX, worldOrigin.Z + resource.MinZ, resource.InvSpanX, resource.InvSpanZ);
        var bindings = _buildWorldProjectedSdfs;
        for (var i = 0; i < _buildWorldProjectedSdfCount; ++i)
        {
            if (bindings![i].View == resource.View && bindings[i].Map == map)
            {
                return i;
            }
        }

        EnsureWorldProjectedSdfBindingBuildCapacity(_buildWorldProjectedSdfCount + 1);
        resource.View->AddRef();
        var index = _buildWorldProjectedSdfCount++;
        _buildWorldProjectedSdfs![index] = new WorldProjectedSdfBinding { View = resource.View, Map = map };
        return index;
    }

    private static ArenaSdfResource? GetOrCreateWorldProjectedSdfResource(RelSimplifiedComplexPolygon polygon, bool arenaCache)
    {
        var cache = arenaCache ? ArenaSdfCache : CustomSdfCache;
        if (cache.TryGetValue(polygon, out var cached))
        {
            cached.LastUse = ++_arenaSdfUseCounter;
            return cached;
        }

        // The 2D radar normally creates this resource. Projection-only mode needs to create it itself on the first cache miss
        var savedScale = _buildSdfResolutionPixelScale;
        _buildSdfResolutionPixelScale = Math.Max(savedScale, 8f);

        ArenaSdfResource? resource;
        try
        {
            resource = BuildAdaptiveSdfResource(polygon);
        }
        finally
        {
            _buildSdfResolutionPixelScale = savedScale;
        }

        if (resource == null)
        {
            return null;
        }

        cache.Add(polygon, resource);
        resource.LastUse = ++_arenaSdfUseCounter;

        if (arenaCache)
        {
            _arenaSdfCacheBytes += resource.ByteSize;
            TrimSdfCache(ArenaSdfCache, resource, ref _arenaSdfCacheBytes, ArenaSdfCacheBudgetBytes, MaxCachedArenaSdfs);
        }
        else
        {
            _customSdfCacheBytes += resource.ByteSize;
            TrimSdfCache(CustomSdfCache, resource, ref _customSdfCacheBytes, CustomSdfCacheBudgetBytes, MaxCachedCustomSdfs);
        }

        return resource;
    }

    private static int GetOrAddBuildCustomSdfBinding(ArenaSdfResource resource)
    {
        var bindings = _buildCustomSdfs;
        for (var i = 0; i < _buildCustomSdfCount; ++i)
        {
            if (bindings![i].View == resource.View)
            {
                return i;
            }
        }

        EnsureCustomSdfBindingBuildCapacity(_buildCustomSdfCount + 1);
        // Retain immediately, not only at Flush(). Cache trimming may evict this resource while the
        // packet is still being assembled, and the deferred callback must remain independent of that
        resource.View->AddRef();
        var index = _buildCustomSdfCount++;
        _buildCustomSdfs![index] = new CustomSdfBinding
        {
            View = resource.View,
            Constants = BuildOutlineSdfConstants(resource),
        };
        return index;
    }

    private static ArenaSdfResource? GetOrCreateCustomSdf(RelSimplifiedComplexPolygon polygon)
    {
        if (!CustomSdfCache.TryGetValue(polygon, out var resource))
        {
            resource = BuildAdaptiveSdfResource(polygon);
            if (resource == null)
            {
                return null;
            }
            CustomSdfCache.Add(polygon, resource);
            _customSdfCacheBytes += resource.ByteSize;
            resource.LastUse = ++_arenaSdfUseCounter;
            TrimSdfCache(CustomSdfCache, resource, ref _customSdfCacheBytes, CustomSdfCacheBudgetBytes, MaxCachedCustomSdfs);
            return resource;
        }

        if (NeedsSdfResolutionUpgrade(resource))
        {
            // Cache entries only grow when the same immutable polygon is later viewed at a higher
            // framebuffer scale. Never downgrade on zoom-out; that avoids texture rebuild churn.
            var replacement = BuildAdaptiveSdfResource(polygon);
            if (replacement != null && Math.Max(replacement.Width, replacement.Height) > Math.Max(resource.Width, resource.Height))
            {
                CustomSdfCache[polygon] = replacement;
                _customSdfCacheBytes += replacement.ByteSize - resource.ByteSize;
                if (resource.View != null)
                {
                    resource.View->Release();
                }
                resource = replacement;
                resource.LastUse = ++_arenaSdfUseCounter;
                TrimSdfCache(CustomSdfCache, resource, ref _customSdfCacheBytes, CustomSdfCacheBudgetBytes, MaxCachedCustomSdfs);
                return resource;
            }
            if (replacement != null && replacement.View != null)
            {
                replacement.View->Release();
            }
        }

        resource.LastUse = ++_arenaSdfUseCounter;
        return resource;
    }

    private static bool EnsureArenaSdfForBuild()
    {
        if (_buildArenaSdf != null)
        {
            return true;
        }

        var polygon = _arenaShape;
        if (polygon == null)
        {
            return false;
        }

        var resource = GetOrCreateArenaSdfResource(polygon);
        if (resource == null)
        {
            return false;
        }

        _buildArenaSdf = resource;
        _buildOutlineSdfConstants = BuildOutlineSdfConstants(resource);
        return true;
    }

    private static ArenaSdfResource? GetOrCreateArenaSdfResource(RelSimplifiedComplexPolygon polygon)
    {
        if (!ArenaSdfCache.TryGetValue(polygon, out var resource))
        {
            resource = BuildAdaptiveSdfResource(polygon);
            if (resource == null)
            {
                return null;
            }
            ArenaSdfCache.Add(polygon, resource);
            _arenaSdfCacheBytes += resource.ByteSize;
            resource.LastUse = ++_arenaSdfUseCounter;
            TrimSdfCache(ArenaSdfCache, resource, ref _arenaSdfCacheBytes, ArenaSdfCacheBudgetBytes, MaxCachedArenaSdfs);
        }
        else if (NeedsSdfResolutionUpgrade(resource))
        {
            var replacement = BuildAdaptiveSdfResource(polygon);
            if (replacement != null && Math.Max(replacement.Width, replacement.Height) > Math.Max(resource.Width, resource.Height))
            {
                ArenaSdfCache[polygon] = replacement;
                _arenaSdfCacheBytes += replacement.ByteSize - resource.ByteSize;
                if (resource.View != null)
                {
                    resource.View->Release();
                }
                resource = replacement;
                resource.LastUse = ++_arenaSdfUseCounter;
                TrimSdfCache(ArenaSdfCache, resource, ref _arenaSdfCacheBytes, ArenaSdfCacheBudgetBytes, MaxCachedArenaSdfs);
            }
            else
            {
                if (replacement != null && replacement.View != null)
                {
                    replacement.View->Release();
                }
                resource.LastUse = ++_arenaSdfUseCounter;
            }
        }
        else
        {
            resource.LastUse = ++_arenaSdfUseCounter;
        }
        return resource;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool NeedsSdfResolutionUpgrade(ArenaSdfResource resource)
        => Math.Max(resource.Width, resource.Height) < ComputeSdfLongResolution(Math.Max(resource.SpanX, resource.SpanZ));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int ComputeSdfLongResolution(float maxSpan)
    {
        // The SDF stores world-space distance and is bilinearly sampled. Keep texels at roughly
        // half a framebuffer pixel or finer, while retaining a small floor for zoomed
        // out arenas and a hard cap for memory/build cost. The extra density matters most at
        // polygon corners, where bilinear interpolation otherwise bends the SDF zero contour.
        var screenPixels = maxSpan * Math.Max(_buildSdfResolutionPixelScale, 1e-5f);
        var requested = (int)MathF.Ceiling(screenPixels / TargetSdfPixelsPerTexel);
        requested = Math.Clamp(requested, MinSdfLongResolution, MaxSdfResolution);
        return Math.Min(MaxSdfResolution, AlignSdfResolution(requested));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AlignSdfResolution(int value) => (value + SdfResolutionAlignment - 1) & -SdfResolutionAlignment;

    private static void TrimSdfCache(Dictionary<RelSimplifiedComplexPolygon, ArenaSdfResource> cache, ArenaSdfResource keep,
        ref long cacheBytes, long byteBudget, int maxEntries)
    {
        // Called only after create/upgrade, when size or entry count can actually exceed a limit.
        // Eviction is rare, so a linear LRU scan here is cheaper than maintaining a second ordered structure.
        while ((cacheBytes > byteBudget || cache.Count > maxEntries) && cache.Count > 1)
        {
            RelSimplifiedComplexPolygon? oldestKey = null;
            ArenaSdfResource? oldest = null;
            foreach (var pair in cache)
            {
                if (ReferenceEquals(pair.Value, keep))
                {
                    continue;
                }
                if (oldest == null || pair.Value.LastUse < oldest.LastUse)
                {
                    oldestKey = pair.Key;
                    oldest = pair.Value;
                }
            }

            if (oldestKey == null || oldest == null)
            {
                break;
            }

            cache.Remove(oldestKey);
            cacheBytes -= oldest.ByteSize;
            if (oldest.View != null)
            {
                oldest.View->Release();
            }
        }
    }

    private static ArenaSdfResource? BuildAdaptiveSdfResource(RelSimplifiedComplexPolygon polygon)
    {
        // The exact boundary index is required below to build every SDF mip, and it already owns the
        // polygon's raw local AABB. Reusing it avoids a separate full contour scan and gives all SDF
        // and projected-quad paths one consistent bounds source
        var index = polygon.VerifyPolygonIndexExistance();
        index.GetBounds(out Vector2 boundsMin, out var boundsMax);

        var minX = boundsMin.X;
        var minZ = boundsMin.Y;
        var maxX = boundsMax.X;
        var maxZ = boundsMax.Y;

        var rawSpanX = Math.Max(maxX - minX, 1e-3f);
        var rawSpanZ = Math.Max(maxZ - minZ, 1e-3f);
        var maxRawSpan = Math.Max(rawSpanX, rawSpanZ);

        // Keep enough positive-distance space around the polygon for centered thick outlines. The SDF remains local-space and cached; camera rotation does not invalidate it.
        var padding = Math.Max(0.5f, maxRawSpan * 0.125f);
        minX -= padding;
        minZ -= padding;
        maxX += padding;
        maxZ += padding;

        var spanX = maxX - minX;
        var spanZ = maxZ - minZ;
        var maxSpan = Math.Max(spanX, spanZ);
        var longResolution = ComputeSdfLongResolution(maxSpan);
        // Padding guarantees even a degenerate/thin polygon has a useful short dimension
        var invMax = 1f / maxSpan;
        var width = Math.Clamp(AlignSdfResolution((int)MathF.Ceiling(longResolution * spanX * invMax)), SdfResolutionAlignment, MaxSdfResolution);
        var height = Math.Clamp(AlignSdfResolution((int)MathF.Ceiling(longResolution * spanZ * invMax)), SdfResolutionAlignment, MaxSdfResolution);
        // Build a full mip chain, but do NOT derive coarse levels by averaging the previous SDF.
        // Averaging scalar distances moves/rounds the zero contour at polygon corners. Instead every
        // mip independently samples the exact polygon distance field over the same world-space domain.
        var mipLevels = 1;
        var mipWidth = width;
        var mipHeight = height;
        var totalSampleCount = width * height;
        while (mipWidth > 1 || mipHeight > 1)
        {
            mipWidth = Math.Max(1, mipWidth >> 1);
            mipHeight = Math.Max(1, mipHeight >> 1);
            totalSampleCount += mipWidth * mipHeight;
            ++mipLevels;
        }

        var baseSampleCount = width * height;
        var samples = ArrayPool<float>.Shared.Rent(baseSampleCount);
        var packed = ArrayPool<ushort>.Shared.Rent(totalSampleCount);
        try
        {
            // PolygonBoundaryIndex2D evaluates the true unique-edge distance field at each mip's own texel centers
            var packedOffset = 0;
            mipWidth = width;
            mipHeight = height;
            for (var mip = 0; mip < mipLevels; ++mip)
            {
                var mipSampleCount = mipWidth * mipHeight;
                index.FillSignedDistanceGrid(samples, mipWidth, mipHeight, minX, minZ, spanX, spanZ);
                for (var i = 0; i < mipSampleCount; ++i)
                {
                    packed[packedOffset + i] = BitConverter.HalfToUInt16Bits((Half)samples[i]);
                }
                packedOffset += mipSampleCount;
                mipWidth = Math.Max(1, mipWidth >> 1);
                mipHeight = Math.Max(1, mipHeight >> 1);
            }

            D3D11_TEXTURE2D_DESC desc = default;
            desc.Width = (uint)width;
            desc.Height = (uint)height;
            desc.MipLevels = (uint)mipLevels;
            desc.ArraySize = 1u;
            desc.Format = (DXGI_FORMAT)54; // DXGI_FORMAT_R16_FLOAT
            desc.SampleDesc.Count = 1u;
            desc.Usage = 0; // DEFAULT
            desc.BindFlags = 0x8u; // SHADER_RESOURCE

            ID3D11Texture2D* texture = null;
            fixed (ushort* source = packed)
            {
                var init = stackalloc D3D11_SUBRESOURCE_DATA[mipLevels];
                var texelOffset = 0;
                mipWidth = width;
                mipHeight = height;
                for (var mip = 0; mip < mipLevels; ++mip)
                {
                    var mipSampleCount = mipWidth * mipHeight;
                    init[mip] = default;
                    init[mip].pSysMem = source + texelOffset;
                    init[mip].SysMemPitch = (uint)(mipWidth * sizeof(ushort));
                    init[mip].SysMemSlicePitch = (uint)(mipSampleCount * sizeof(ushort));
                    texelOffset += mipSampleCount;
                    mipWidth = Math.Max(1, mipWidth >> 1);
                    mipHeight = Math.Max(1, mipHeight >> 1);
                }
                _device->CreateTexture2D(&desc, init, &texture);
            }

            if (texture == null)
            {
                return null;
            }

            ID3D11ShaderResourceView* view = null;
            _device->CreateShaderResourceView((ID3D11Resource*)texture, null, &view);
            texture->Release();
            if (view == null)
            {
                return null;
            }

            return new ArenaSdfResource
            {
                View = view,
                MinX = minX,
                MinZ = minZ,
                SpanX = spanX,
                SpanZ = spanZ,
                InvSpanX = 1f / spanX,
                InvSpanZ = 1f / spanZ,
                Width = width,
                Height = height,
                ByteSize = totalSampleCount * sizeof(ushort),
                LastUse = 0L,
            };
        }
        finally
        {
            ArrayPool<ushort>.Shared.Return(packed, clearArray: false);
            ArrayPool<float>.Shared.Return(samples, clearArray: false);
        }
    }

    private static OutlineSdfConstants BuildOutlineSdfConstants(ArenaSdfResource sdf)
    {
        var invSpanX = sdf.InvSpanX;
        var invSpanZ = sdf.InvSpanZ;
        var uv0 = new Vector4(_buildSdfWxPx * invSpanX, _buildSdfWxPy * invSpanX, (_buildSdfWx0 - sdf.MinX) * invSpanX, 0f);
        var uv1 = new Vector4(_buildSdfWzPx * invSpanZ, _buildSdfWzPy * invSpanZ, (_buildSdfWz0 - sdf.MinZ) * invSpanZ, _buildWorldPixelScale);
        var uv0X = uv0.X;
        var uv1X = uv1.X;
        var uv0Y = uv0.Y;
        var uv1Y = uv1.Y;
        var gradU = MathF.Sqrt(uv0X * uv0X + uv0Y * uv0Y);
        var gradV = MathF.Sqrt(uv1X * uv1X + uv1Y * uv1Y);
        var outsideScale = new Vector4(1f / Math.Max(gradU, 1e-7f), 1f / Math.Max(gradV, 1e-7f), 0f, 0f);
        const float sdfMipGradScale = 0.70710678f;
        var mipGrad = new Vector4(uv0X * sdfMipGradScale, uv1X * sdfMipGradScale, uv0Y * sdfMipGradScale, uv1Y * sdfMipGradScale);
        return new OutlineSdfConstants { UvRow0 = uv0, UvRow1 = uv1, OutsideScale = outsideScale, MipGrad = mipGrad };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameSdfConstants(in OutlineSdfConstants a, in OutlineSdfConstants b)
        => a.UvRow0 == b.UvRow0 && a.UvRow1 == b.UvRow1 && a.OutsideScale == b.OutsideScale && a.MipGrad == b.MipGrad;

    private static void UploadOutlineSdfConstants(in OutlineSdfConstants constants)
    {
        if (_outlineSdfConstantsValid && SameSdfConstants(_lastOutlineSdfConstants, constants))
        {
            return;
        }

        D3D11_MAPPED_SUBRESOURCE mapped = default;
        var hr = _context->Map((ID3D11Resource*)_outlineSdfConstantBuffer, 0u, (D3D11_MAP)4, 0u, &mapped);

        try
        {
            *(OutlineSdfConstants*)mapped.pData = constants;
            _lastOutlineSdfConstants = constants;
            _outlineSdfConstantsValid = true;
        }
        finally
        {
            _context->Unmap((ID3D11Resource*)_outlineSdfConstantBuffer, 0u);
        }
    }

    private static void UploadCustomSdfConstants(in OutlineSdfConstants constants)
    {
        if (_customSdfConstantsValid && SameSdfConstants(_lastCustomSdfConstants, constants))
        {
            return;
        }

        D3D11_MAPPED_SUBRESOURCE mapped = default;
        var hr = _context->Map((ID3D11Resource*)_customSdfConstantBuffer, 0u, (D3D11_MAP)4, 0u, &mapped);
        try
        {
            *(OutlineSdfConstants*)mapped.pData = constants;
            _lastCustomSdfConstants = constants;
            _customSdfConstantsValid = true;
        }
        finally
        {
            _context->Unmap((ID3D11Resource*)_customSdfConstantBuffer, 0u);
        }
    }

    private static void UploadWorldProjectedSdfConstants(in WorldProjectedSdfConstants constants)
    {
        if (_worldProjectedSdfConstantBuffer == null || (_worldProjectedSdfConstantsValid && SameWorldProjectedSdfConstants(_lastWorldProjectedSdfConstants, constants)))
        {
            return;
        }
        D3D11_MAPPED_SUBRESOURCE mapped = default;
        _context->Map((ID3D11Resource*)_worldProjectedSdfConstantBuffer, 0u, (D3D11_MAP)4, 0u, &mapped);
        try
        {
            *(WorldProjectedSdfConstants*)mapped.pData = constants;
            _lastWorldProjectedSdfConstants = constants;
            _worldProjectedSdfConstantsValid = true;
        }
        finally
        {
            _context->Unmap((ID3D11Resource*)_worldProjectedSdfConstantBuffer, 0u);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameWorldProjectedSdfConstants(in WorldProjectedSdfConstants a, in WorldProjectedSdfConstants b)
        => a.ShapeSdfMap == b.ShapeSdfMap && a.ArenaSdfMap == b.ArenaSdfMap && a.Flags == b.Flags;

    private static void UploadZoneWaveConstants(int submitFrame)
    {
        // All packets submitted by one ImGui frame should share a phase. Besides avoiding tiny visual
        // seams across layer/stencil packets, this reduces a dynamic constant-buffer map to once/frame.
        if (_zoneWaveConstantBuffer == null || _zoneWaveUploadFrame == submitFrame)
        {
            return;
        }

        D3D11_MAPPED_SUBRESOURCE mapped = default;
        var hr = _context->Map((ID3D11Resource*)_zoneWaveConstantBuffer, 0u, (D3D11_MAP)4, 0u, &mapped);
        if (hr >= 0)
        {
            var elapsedSeconds = (float)((Stopwatch.GetTimestamp() - ZoneWaveEpoch) / (double)Stopwatch.Frequency);
            *(ZoneWaveConstants*)mapped.pData = new ZoneWaveConstants { Params = new Vector4(elapsedSeconds, 0f, 0f, 0f) };
            _context->Unmap((ID3D11Resource*)_zoneWaveConstantBuffer, 0u);
            _zoneWaveUploadFrame = submitFrame;
        }
    }

    private static bool CreateArenaSdfPipelineResources()
    {
        D3D11_SAMPLER_DESC samplerDesc = default;
        samplerDesc.Filter = (D3D11_FILTER)0x15; // MIN_MAG_MIP_LINEAR
        samplerDesc.AddressU = (D3D11_TEXTURE_ADDRESS_MODE)3; // CLAMP
        samplerDesc.AddressV = (D3D11_TEXTURE_ADDRESS_MODE)3;
        samplerDesc.AddressW = (D3D11_TEXTURE_ADDRESS_MODE)3;
        samplerDesc.MaxAnisotropy = 1u;
        samplerDesc.MipLODBias = 0f; // explicit -0.5 bias is applied to SampleGrad gradients in the SDF shaders
        samplerDesc.ComparisonFunc = (D3D11_COMPARISON_FUNC)1; // NEVER
        samplerDesc.MinLOD = 0f;
        samplerDesc.MaxLOD = float.MaxValue;

        ID3D11SamplerState* sampler = null;
        _device->CreateSamplerState(&samplerDesc, &sampler);

        _arenaSdfSampler = sampler;

        D3D11_BUFFER_DESC desc = default;
        desc.ByteWidth = (uint)sizeof(OutlineSdfConstants);
        desc.Usage = (D3D11_USAGE)2; // DYNAMIC
        desc.BindFlags = 0x4u; // CONSTANT_BUFFER
        desc.CPUAccessFlags = 0x10000u; // WRITE
        ID3D11Buffer* buffer = null;
        _device->CreateBuffer(&desc, null, &buffer);

        _outlineSdfConstantBuffer = buffer;

        ID3D11Buffer* customBuffer = null;
        _device->CreateBuffer(&desc, null, &customBuffer);

        _customSdfConstantBuffer = customBuffer;

        D3D11_BUFFER_DESC zoneWaveDesc = default;
        zoneWaveDesc.ByteWidth = (uint)sizeof(ZoneWaveConstants);
        zoneWaveDesc.Usage = (D3D11_USAGE)2;
        zoneWaveDesc.BindFlags = 0x4u;
        zoneWaveDesc.CPUAccessFlags = 0x10000u;
        ID3D11Buffer* zoneWaveBuffer = null;
        _device->CreateBuffer(&zoneWaveDesc, null, &zoneWaveBuffer);
        _zoneWaveConstantBuffer = zoneWaveBuffer;

        D3D11_BUFFER_DESC projectedSdfDesc = default;
        projectedSdfDesc.ByteWidth = (uint)sizeof(WorldProjectedSdfConstants);
        projectedSdfDesc.Usage = (D3D11_USAGE)2;
        projectedSdfDesc.BindFlags = 0x4u;
        projectedSdfDesc.CPUAccessFlags = 0x10000u;
        ID3D11Buffer* projectedSdfBuffer = null;
        _device->CreateBuffer(&projectedSdfDesc, null, &projectedSdfBuffer);
        _worldProjectedSdfConstantBuffer = projectedSdfBuffer;

        D3D11_BUFFER_DESC worldLineDesc = default;
        worldLineDesc.ByteWidth = (uint)sizeof(WorldLineConstants);
        worldLineDesc.Usage = (D3D11_USAGE)2;
        worldLineDesc.BindFlags = 0x4u;
        worldLineDesc.CPUAccessFlags = 0x10000u;
        ID3D11Buffer* worldLineBuffer = null;
        _device->CreateBuffer(&worldLineDesc, null, &worldLineBuffer);

        _worldLineConstantBuffer = worldLineBuffer;

        D3D11_BUFFER_DESC worldTransformDesc = default;
        worldTransformDesc.ByteWidth = (uint)(MaxWorldLineTransforms * sizeof(WorldLineTransform));
        worldTransformDesc.Usage = (D3D11_USAGE)2;
        worldTransformDesc.BindFlags = 0x4u;
        worldTransformDesc.CPUAccessFlags = 0x10000u;
        ID3D11Buffer* worldTransformBuffer = null;
        _device->CreateBuffer(&worldTransformDesc, null, &worldTransformBuffer);

        _worldLineTransformBuffer = worldTransformBuffer;

        // One shared index buffer serves both ordinary single quads and procedural WorldCurve runs.
        // Each generated curve line owns four unique VS vertex ids but six triangle-list indices:
        // 0,1,2,0,2,3; 4,5,6,4,6,7; ...
        // Existing quad draws use the first six indices unchanged. WorldCurve draws consume a longer
        // prefix, allowing the post-transform cache to reuse two vertices per generated line
        var quadIndexCount = MaxIndexedWorldCurveLines * 6;
        D3D11_BUFFER_DESC quadIndexDesc = default;
        quadIndexDesc.ByteWidth = (uint)(quadIndexCount * sizeof(uint));
        quadIndexDesc.Usage = 0; // D3D11_USAGE_DEFAULT
        quadIndexDesc.BindFlags = 0x2u; // D3D11_BIND_INDEX_BUFFER
        ID3D11Buffer* quadIndexBuffer = null;
        _device->CreateBuffer(&quadIndexDesc, null, &quadIndexBuffer);

        var quadIndices = ArrayPool<uint>.Shared.Rent(quadIndexCount);
        try
        {
            for (var line = 0; line < MaxIndexedWorldCurveLines; ++line)
            {
                var vertex = (uint)(line * 4);
                var index = line * 6;
                quadIndices[index] = vertex;
                quadIndices[index + 1] = vertex + 1u;
                quadIndices[index + 2] = vertex + 2u;
                quadIndices[index + 3] = vertex;
                quadIndices[index + 4] = vertex + 2u;
                quadIndices[index + 5] = vertex + 3u;
            }

            fixed (uint* indices = quadIndices)
                _context->UpdateSubresource((ID3D11Resource*)quadIndexBuffer, 0u, null, indices, 0u, 0u);
        }
        finally
        {
            ArrayPool<uint>.Shared.Return(quadIndices);
        }
        _quadIndexBuffer = quadIndexBuffer;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float LocalPixelScale()
    {
        EnsureArenaPrepared();
        return _buildPixelScale;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 UnitWorldDirectionToScreen(float worldX, float worldZ)
        => new(worldX * _buildDirectionCos - worldZ * _buildDirectionSin, worldZ * _buildDirectionCos + worldX * _buildDirectionSin);

    private static bool TryNormalizeScreenDirection(in WDir direction, out Vector2 screenDirection, out float worldX, out float worldZ)
    {
        var dirX = direction.X;
        var dirZ = direction.Z;
        var lenSq = dirX * dirX + dirZ * dirZ;
        if (!(lenSq > 1e-12f))
        {
            screenDirection = default;
            worldX = worldZ = 0f;
            return false;
        }

        EnsureArenaPrepared();

        // Most callers pass Angle.ToDirection() or an already-normalized direction. Avoid sqrt/div
        // entirely for that common case; fall back to exact normalization for arbitrary WDir callers
        if (Math.Abs(lenSq - 1f) <= 1e-6f)
        {
            worldX = dirX;
            worldZ = dirZ;
        }
        else
        {
            var invLen = 1f / MathF.Sqrt(lenSq);
            worldX = dirX * invLen;
            worldZ = dirZ * invLen;
        }

        screenDirection = UnitWorldDirectionToScreen(worldX, worldZ);
        return true;
    }

    // Appends a Dalamud-owned RGBA texture as a screen-space quad. Keep ImTextureID opaque at the
    // public call sites; unwrap it only here at the DX11 backend boundary. Dalamud's texture-wrap
    // implementation documents Handle as the low-level TerraFX IUnknown (DX11: SRV). AddRef immediately
    // so the deferred callback owns the resource independently of GetWrapOrEmpty's current-frame lifetime.
    public static void AppendSpriteScreen(Vector2 min, Vector2 max, IDalamudTextureWrap texture, uint color = 0xFFFFFFFFu)
    {
        if (!_arenaActive || !(max.X > min.X) || !(max.Y > min.Y) || (color & 0xFF000000u) == 0)
        {
            return;
        }

        var textureId = texture.Handle;
        var rawHandle = Unsafe.BitCast<ImTextureID, ulong>(textureId);
        if (rawHandle == 0)
        {
            return;
        }

        EnsureBuildRunStarted();

        var view = (ID3D11ShaderResourceView*)(nuint)rawHandle;
        EnsureSpriteBuildCapacity(_buildSpriteCount + 1);
        var binding = _buildSpriteCount++;
        view->AddRef();
        _buildSprites![binding] = new SpriteBinding { View = view };

        var start = _buildTextInstanceCount;
        EnsureTextInstanceBuildCapacity(start + 1);
        var p00 = ScreenToNdc(min);
        var p11 = ScreenToNdc(max);
        _buildTextInstances![start] = new TextInstance
        {
            RectNdc = new Vector4(p00.X, p00.Y, p11.X, p11.Y),
            UvRect = new Vector4(0f, 0f, 1f, 1f),
            Col = color,
            OutlineCol = 0u,
            OutlineWidthPx = 0f,
        };
        _buildTextInstanceCount = start + 1;
        AppendSegment(SegmentKind.Sprite, start, 1, spriteBinding: binding);
    }

    // Appends centered screen-space text from the renderer-owned immutable MSDF atlas. No ImFontPtr,
    // ImFontGlyph, ImTextureID, or dynamic ImGui atlas state survives into the deferred callback.
    public static void AppendTextScreen(Vector2 center, string text, float renderSize, uint color, uint outlineColor = 0u, float outlineWidthPx = 0f)
        => AppendMsdfTextScreen(center, text, renderSize, color, iconFont: false, outlineColor, outlineWidthPx);

    // Font Awesome 5 Free Solid lives in the same atlas as the text font but has its own metrics table.
    public static void AppendIconScreen(Vector2 center, string text, float renderSize, uint color)
        => AppendMsdfTextScreen(center, text, renderSize, color, iconFont: true, 0u, 0f);

    // Camera-facing, fixed-screen-size text/icon anchored at a true world-space point. Unlike
    // AppendTextScreen this path participates in world scene-depth occlusion; native UI ordering is
    // provided by the background overlay addon that presents the completed world texture.
    public static void AppendWorldTextBillboard(Vector3 center, string text, float renderSize, uint color, bool iconFont = false, uint outlineColor = 0u, float outlineWidthPx = 0f)
        => AppendMsdfTextWorldBillboard(center, text, renderSize, color, iconFont, outlineColor, outlineWidthPx);

    private static void AppendMsdfTextScreen(Vector2 center, string text, float renderSize, uint color, bool iconFont, uint outlineColor, float outlineWidthPx)
    {
        if (!_arenaActive || !(renderSize > 0f) || string.IsNullOrEmpty(text) || (color & 0xFF000000u) == 0)
        {
            return;
        }

        var glyphs = iconFont ? _arenaIconGlyphs : _arenaTextGlyphs;
        if (glyphs == null || glyphs.Count == 0)
        {
            return;
        }

        var metrics = iconFont ? _arenaIconMetrics : _arenaTextMetrics;
        var lineHeightEm = metrics.LineHeight > 0f ? metrics.LineHeight : 1f;
        var lineAdvance = lineHeightEm * renderSize;

        // First pass measures using the exact same advances/kerning as emission. Missing ordinary
        // text falls back to '?'; missing icon codepoints are skipped instead of drawing misleading glyphs.
        var lineWidth = 0f;
        var maxWidth = 0f;
        var lineCount = 1;
        var visibleGlyphs = 0;
        var previous = 0u;
        var hasPrevious = false;

        var len = text.Length;
        for (var i = 0; i < len;)
        {
            var codepoint = ReadCodepoint(text, ref i);
            if (codepoint == '\r')
            {
                continue;
            }
            if (codepoint == '\n')
            {
                maxWidth = Math.Max(maxWidth, lineWidth);
                lineWidth = 0f;
                ++lineCount;
                previous = 0u;
                hasPrevious = false;
                continue;
            }

            if (!TryGetArenaGlyph(glyphs, codepoint, iconFont, out var glyph, out var resolvedCodepoint))
            {
                previous = 0u;
                hasPrevious = false;
                continue;
            }

            if (!iconFont && hasPrevious)
            {
                lineWidth += GetArenaTextKerning(previous, resolvedCodepoint) * renderSize;
            }
            lineWidth += glyph.Advance * renderSize;
            if (glyph.HasQuad)
            {
                ++visibleGlyphs;
            }
            previous = resolvedCodepoint;
            hasPrevious = true;
        }
        maxWidth = Math.Max(maxWidth, lineWidth);

        if (visibleGlyphs == 0)
        {
            return;
        }

        EnsureBuildRunStarted();
        var start = _buildTextInstanceCount;
        EnsureTextInstanceBuildCapacity(start + visibleGlyphs);
        var instances = _buildTextInstances!;
        var dst = start;

        var x0 = MathF.Floor(center.X - 0.5f * maxWidth);
        // Font metrics use an em-space baseline with +Y upward. Center the complete line box and
        // convert to screen +Y-down coordinates when applying each glyph's plane bounds below.
        var baseline0 = center.Y - 0.5f * (lineCount - 1) * lineAdvance + 0.5f * (metrics.Ascender + metrics.Descender) * renderSize;
        var x = x0;
        var baseline = baseline0;
        previous = 0u;
        hasPrevious = false;

        for (var i = 0; i < text.Length;)
        {
            var codepoint = ReadCodepoint(text, ref i);
            if (codepoint == '\r')
            {
                continue;
            }
            if (codepoint == '\n')
            {
                x = x0;
                baseline += lineAdvance;
                previous = 0;
                hasPrevious = false;
                continue;
            }

            if (!TryGetArenaGlyph(glyphs, codepoint, iconFont, out var glyph, out var resolvedCodepoint))
            {
                previous = 0;
                hasPrevious = false;
                continue;
            }

            if (!iconFont && hasPrevious)
            {
                x += GetArenaTextKerning(previous, resolvedCodepoint) * renderSize;
            }

            if (glyph.HasQuad)
            {
                var pb = glyph.PlaneBounds;
                var x1 = x + pb.X * renderSize;
                var y1 = baseline - pb.W * renderSize; // top in +Y-up -> screen top
                var x2 = x + pb.Z * renderSize;
                var y2 = baseline - pb.Y * renderSize; // bottom in +Y-up -> screen bottom
                var p00 = ScreenToNdc(x1, y1);
                var p11 = ScreenToNdc(x2, y2);
                instances[dst++] = new TextInstance
                {
                    RectNdc = new Vector4(p00.X, p00.Y, p11.X, p11.Y),
                    UvRect = glyph.UvRect,
                    Col = color,
                    OutlineCol = outlineColor,
                    OutlineWidthPx = outlineWidthPx,
                };
            }

            x += glyph.Advance * renderSize;
            previous = resolvedCodepoint;
            hasPrevious = true;
        }

        var count = dst - start;
        _buildTextInstanceCount = dst;
        AppendSegment(SegmentKind.Text, start, count);
    }

    private static void AppendMsdfTextWorldBillboard(Vector3 center, string text, float renderSize, uint color, bool iconFont, uint outlineColor, float outlineWidthPx)
    {
        if (!_arenaActive || !_buildWorldLineConfigured || renderSize <= 0f || string.IsNullOrEmpty(text) || (color & 0xFF000000u) == 0)
        {
            return;
        }

        var glyphs = iconFont ? _arenaIconGlyphs : _arenaTextGlyphs;
        if (glyphs == null || glyphs.Count == 0)
        {
            return;
        }

        var metrics = iconFont ? _arenaIconMetrics : _arenaTextMetrics;
        var lineHeightEm = metrics.LineHeight > 0f ? metrics.LineHeight : 1f;
        var lineAdvance = lineHeightEm * renderSize;

        // Measure exactly as the screen-text path does so centering, fallback glyphs and kerning are
        // identical. The only difference is that emitted rectangles remain offsets around the world
        // anchor instead of being converted to absolute NDC on the CPU.
        var lineWidth = 0f;
        var maxWidth = 0f;
        var lineCount = 1;
        var visibleGlyphs = 0;
        var previous = 0u;
        var hasPrevious = false;

        var len = text.Length;
        for (var i = 0; i < len;)
        {
            var codepoint = ReadCodepoint(text, ref i);
            if (codepoint == '\r')
            {
                continue;
            }
            if (codepoint == '\n')
            {
                maxWidth = Math.Max(maxWidth, lineWidth);
                lineWidth = 0f;
                ++lineCount;
                previous = 0u;
                hasPrevious = false;
                continue;
            }

            if (!TryGetArenaGlyph(glyphs, codepoint, iconFont, out var glyph, out var resolvedCodepoint))
            {
                previous = 0u;
                hasPrevious = false;
                continue;
            }

            if (!iconFont && hasPrevious)
            {
                lineWidth += GetArenaTextKerning(previous, resolvedCodepoint) * renderSize;
            }
            lineWidth += glyph.Advance * renderSize;
            if (glyph.HasQuad)
            {
                ++visibleGlyphs;
            }
            previous = resolvedCodepoint;
            hasPrevious = true;
        }
        maxWidth = Math.Max(maxWidth, lineWidth);
        if (visibleGlyphs == 0)
        {
            return;
        }

        EnsureBuildRunStarted();
        EnsureWorldLineConstants();

        // RectPx is stored in physical framebuffer pixels because the world-text VS uses RasterScale
        // (2 / framebuffer dimension) directly. This keeps the public fontSize semantics in logical
        // ImGui pixels while remaining correct on high-DPI framebuffers
        var scaleX = Math.Max(_buildClipScale.X, 1e-5f);
        var scaleY = Math.Max(_buildClipScale.Y, 1e-5f);

        var start = _buildWorldTextInstanceCount;
        EnsureWorldTextInstanceBuildCapacity(start + visibleGlyphs);
        var instances = _buildWorldTextInstances!;
        var dst = start;

        var x0 = -0.5f * maxWidth;
        var baseline0 = -0.5f * (lineCount - 1) * lineAdvance + 0.5f * (metrics.Ascender + metrics.Descender) * renderSize;
        var x = x0;
        var baseline = baseline0;
        previous = 0u;
        hasPrevious = false;

        for (var i = 0; i < len;)
        {
            var codepoint = ReadCodepoint(text, ref i);
            if (codepoint == '\r')
            {
                continue;
            }
            if (codepoint == '\n')
            {
                x = x0;
                baseline += lineAdvance;
                previous = 0u;
                hasPrevious = false;
                continue;
            }

            if (!TryGetArenaGlyph(glyphs, codepoint, iconFont, out var glyph, out var resolvedCodepoint))
            {
                previous = 0u;
                hasPrevious = false;
                continue;
            }

            if (!iconFont && hasPrevious)
            {
                x += GetArenaTextKerning(previous, resolvedCodepoint) * renderSize;
            }

            if (glyph.HasQuad)
            {
                var pb = glyph.PlaneBounds;
                var x1 = (x + pb.X * renderSize) * scaleX;
                var y1 = (baseline - pb.W * renderSize) * scaleY;
                var x2 = (x + pb.Z * renderSize) * scaleX;
                var y2 = (baseline - pb.Y * renderSize) * scaleY;
                instances[dst++] = new WorldTextInstance
                {
                    Center = center,
                    RectPx = new Vector4(x1, y1, x2, y2),
                    UvRect = glyph.UvRect,
                    Col = color,
                    OutlineCol = outlineColor,
                    // Match AppendTextScreen semantics: glyph dimensions follow framebuffer scale,
                    // while the MSDF outline width is passed to the pixel shader unchanged
                    OutlineWidthPx = Math.Max(0f, outlineWidthPx)
                };
            }

            x += glyph.Advance * renderSize;
            previous = resolvedCodepoint;
            hasPrevious = true;
        }

        var count = dst - start;
        _buildWorldTextInstanceCount = dst;
        AppendSegment(SegmentKind.WorldText, start, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryGetArenaGlyph(Dictionary<uint, ArenaFontGlyph> glyphs, uint codepoint, bool iconFont, out ArenaFontGlyph glyph, out uint resolvedCodepoint)
    {
        if (glyphs.TryGetValue(codepoint, out glyph))
        {
            resolvedCodepoint = codepoint;
            return true;
        }
        if (!iconFont && codepoint != '?' && glyphs.TryGetValue('?', out glyph))
        {
            resolvedCodepoint = '?';
            return true;
        }
        resolvedCodepoint = 0;
        glyph = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetArenaTextKerning(uint left, uint right)
    {
        var kerning = _arenaTextKerning;
        if (kerning == null)
        {
            return 0f;
        }
        var key = ((ulong)left << 32) | right;
        return kerning.TryGetValue(key, out var amount) ? amount : 0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint ReadCodepoint(string text, ref int index)
    {
        var first = text[index++];
        if (char.IsHighSurrogate(first) && index < text.Length)
        {
            var second = text[index];
            if (char.IsLowSurrogate(second))
            {
                ++index;
                return (uint)char.ConvertToUtf32(first, second);
            }
        }
        return char.IsSurrogate(first) ? 0xFFFDu : first;
    }

    // Flushes the current ordered run into one ImGui callback. The callback may issue multiple GPU
    // draws if mesh and analytic shapes alternate, but state save/restore and callback overhead are paid only once for the run
    public static bool Flush()
    {
        if (!_arenaActive || _buildSegmentCount == 0)
        {
            return true;
        }

        var segmentCount = _buildSegmentCount;
        var segmentArray = _buildSegments!;

        var packet = RentBatchPacket();
        packet.MeshVertices = _buildMeshVertices;
        packet.MeshVertexCount = _buildMeshVertexCount;
        packet.ArenaSdfMaskOutlineStart = _buildArenaSdfMaskOutlineStart;
        packet.PrimitiveTriangleStrokes = _buildPrimitiveTriangleStrokes;
        packet.PrimitiveTriangleStrokeCount = _buildPrimitiveTriangleStrokeCount;
        packet.StrokeInstances = _buildStrokeInstances;
        packet.StrokeInstanceCount = _buildStrokeInstanceCount;
        packet.WorldLines = _buildWorldLines;
        packet.WorldLineCount = _buildWorldLineCount;
        packet.WorldCurves = _buildWorldCurves;
        packet.WorldCurveCount = _buildWorldCurveCount;
        packet.WorldProjectedArrows = _buildWorldProjectedArrows;
        packet.WorldProjectedArrowCount = _buildWorldProjectedArrowCount;
        packet.WorldProjectedShapes = _buildWorldProjectedShapes;
        packet.WorldProjectedShapeCount = _buildWorldProjectedShapeCount;
        packet.WorldProjectedSdfs = _buildWorldProjectedSdfs;
        packet.WorldProjectedSdfCount = _buildWorldProjectedSdfCount;
        packet.WorldLineTransforms = _buildWorldLineTransforms;
        packet.WorldLineTransformCount = _buildWorldLineTransformCount;
        packet.WorldLineConstants = _buildWorldLineConstants;
        packet.SceneDepthView = _buildWorldSceneDepthAvailable ? _sceneDepthView : null;
        packet.SceneInfoView = _buildWorldSceneInfoAvailable ? _sceneInfoView : null;
        packet.Analytics = _buildAnalytics;
        packet.AnalyticCount = _buildAnalyticCount;
        packet.Outlines = _buildOutlines;
        packet.OutlineCount = _buildOutlineCount;
        packet.TextInstances = _buildTextInstances;
        packet.TextInstanceCount = _buildTextInstanceCount;
        packet.WorldTextInstances = _buildWorldTextInstances;
        packet.WorldTextInstanceCount = _buildWorldTextInstanceCount;
        packet.Sprites = _buildSprites;
        packet.SpriteCount = _buildSpriteCount;
        packet.ArenaSdfView = _buildArenaSdf?.View;
        packet.OutlineSdfConstants = _buildOutlineSdfConstants;
        packet.CustomSdfs = _buildCustomSdfs;
        packet.CustomSdfCount = _buildCustomSdfCount;
        packet.NeedsStencil = _buildNeedsStencil;
        packet.UsesZoneWave = _buildUsesZoneWave;
        packet.ModifiesDepthState = _buildModifiesDepthState;
        packet.StencilKey = _buildNeedsStencil ? _arenaStencilKey : 0;
        packet.Segments = segmentArray;
        packet.SegmentCount = segmentCount;
        packet.ClipOffset = _buildClipOffset;
        packet.ClipScale = _buildClipScale;
        packet.FramebufferWidth = _buildFramebufferWidth;
        packet.FramebufferHeight = _buildFramebufferHeight;
        packet.WorldOverlayTarget = _buildWorldLineConfigured;
        PreparePacketUploadLayout(packet);

        if (packet.ArenaSdfView != null)
        {
            packet.ArenaSdfView->AddRef();
        }
        if (packet.SceneDepthView != null)
        {
            packet.SceneDepthView->AddRef();
        }
        if (packet.SceneInfoView != null)
        {
            packet.SceneInfoView->AddRef();
        }
        // Custom SDF bindings were AddRef'd when they entered the build packet, so cache eviction is safe before Flush(). Ownership of those retained references transfers to BatchPacket
        // Packet owns these arrays after this point
        _buildMeshVertices = null;
        _buildPrimitiveTriangleStrokes = null;
        _buildStrokeInstances = null;
        _buildWorldLines = null;
        _buildWorldCurves = null;
        _buildWorldProjectedArrows = null;
        _buildWorldProjectedShapes = null;
        _buildWorldProjectedSdfs = null;
        _buildWorldLineTransforms = null;
        _buildAnalytics = null;
        _buildOutlines = null;
        _buildTextInstances = null;
        _buildWorldTextInstances = null;
        _buildSprites = null;
        _buildCustomSdfs = null;
        _buildSegments = null;
        _buildMeshVertexCount = 0;
        _buildArenaSdfMaskOutlineStart = -1;
        _buildPrimitiveTriangleStrokeCount = 0;
        _buildStrokeInstanceCount = 0;
        _buildWorldLineCount = 0;
        _buildWorldCurveCount = 0;
        _buildWorldProjectedArrowCount = 0;
        _buildWorldProjectedShapeCount = 0;
        _buildWorldProjectedSdfCount = 0;
        _buildWorldLineTransformCount = 0;
        _buildWorldLineConstantsValid = false;
        _buildAnalyticCount = 0;
        _buildOutlineCount = 0;
        _buildTextInstanceCount = 0;
        _buildWorldTextInstanceCount = 0;
        _buildSpriteCount = 0;
        _buildSegmentCount = 0;
        _buildNeedsStencil = false;
        _buildUsesZoneWave = false;
        _buildModifiesDepthState = false;
        _buildArenaSdf = null;
        _buildOutlineSdfConstants = default;
        _buildCustomSdfCount = 0;
        var packetId = RegisterPacket(packet);
        try
        {
            _buildDrawList.AddCallback(DrawCallback, (void*)packetId);
            if (packet.ArenaSdfMaskOutlineStart >= 0)
            {
                _arenaStencilMaskQueued = true;
            }
            return true;
        }
        catch (Exception)
        {
            if (RemovePacket(packetId) is BatchPacket abandoned)
            {
                ReturnPacketArrays(abandoned);
            }
            return false;
        }
    }

    public static void EndArena()
    {
        if (!_arenaActive)
        {
            return;
        }

        Flush();
        BuildPath.Clear();
        QueueDeferredOutlineOverlays();
        FinishArena();
    }

    // Finishes a standalone screen-space batch without arena-border replay work
    public static void EndScreenBatch()
    {
        if (!_arenaActive)
        {
            return;
        }

        Flush();
        BuildPath.Clear();
        FinishArena();
    }

    // Queued after all Camera/world callbacks, including on frames with no world primitives. The
    // marker clears stale content and resolves the accumulated premultiplied render target into the
    // straight-alpha texture consumed by FFXIV's native background overlay node.
    public static void QueueWorldOverlayPresent(ImDrawListPtr drawList, Vector2 viewportSize)
    {
        if (!_isInitialized)
        {
            return;
        }

        PrepareFrame();
        var scale = ImGui.GetIO().DisplayFramebufferScale;
        var scaleX = scale.X > 0f ? scale.X : 1f;
        var scaleY = scale.Y > 0f ? scale.Y : 1f;
        var packet = RentBatchPacket();
        packet.IsWorldOverlayPresent = true;
        packet.FramebufferWidth = Math.Max(1, (int)MathF.Round(viewportSize.X * scaleX));
        packet.FramebufferHeight = Math.Max(1, (int)MathF.Round(viewportSize.Y * scaleY));

        var packetId = RegisterPacket(packet);
        try
        {
            drawList.AddCallback(DrawCallback, (void*)packetId);
        }
        catch
        {
            if (RemovePacket(packetId) is BatchPacket abandoned)
                ReturnPacketArrays(abandoned);
        }
    }

    private static void DeferClippedOutlineOverlay(ref OutlineInstance instance, SegmentKind kind, ArenaSdfResource arenaSdf, ArenaSdfResource? customSdf)
    {
        if (arenaSdf.View == null)
        {
            return;
        }

        // Layer-aware arenas can use several clipping SDFs in one ordered draw stream. Retain the
        // arena binding per overlay; QueueDeferredOutlineOverlays coalesces consecutive equal bindings
        // into packets after the visible arena border has been submitted
        arenaSdf.View->AddRef();

        if (customSdf != null && customSdf.View != null)
        {
            customSdf.View->AddRef();
        }

        DeferredOutlineOverlays.Add(new DeferredOutlineOverlay
        {
            Instance = instance,
            Kind = kind,
            ArenaSdfView = arenaSdf.View,
            ArenaSdfConstants = BuildOutlineSdfConstants(arenaSdf),
            CustomSdfView = customSdf != null ? customSdf.View : null,
            CustomSdfConstants = customSdf != null ? BuildOutlineSdfConstants(customSdf) : default,
            ClipOffset = _buildClipOffset,
            ClipScale = _buildClipScale,
            FramebufferWidth = _buildFramebufferWidth,
            FramebufferHeight = _buildFramebufferHeight,
        });
    }

    private static bool QueueDeferredOutlineOverlays()
    {
        var count = DeferredOutlineOverlays.Count;
        if (count == 0)
        {
            return true;
        }

        try
        {
            var overlays = CollectionsMarshal.AsSpan(DeferredOutlineOverlays);
            var start = 0;
            while (start < count)
            {
                var end = start + 1;
                ref readonly var first = ref overlays[start];
                while (end < count)
                {
                    ref readonly var next = ref overlays[end];
                    if (next.ArenaSdfView != first.ArenaSdfView || next.ClipOffset != first.ClipOffset || next.ClipScale != first.ClipScale
                        || next.FramebufferWidth != first.FramebufferWidth || next.FramebufferHeight != first.FramebufferHeight)
                    {
                        break;
                    }
                    ++end;
                }

                QueueDeferredOutlineOverlayRange(overlays, start, end);
                start = end;
            }

            DeferredOutlineOverlays.Clear();
            return true;
        }
        catch (Exception)
        {
            ReleaseDeferredOutlineOverlays();
            return false;
        }
    }

    private static void QueueDeferredOutlineOverlayRange(Span<DeferredOutlineOverlay> overlays, int start, int end)
    {
        var count = end - start;
        var hasCustomSdf = false;
        for (var i = start; i < end; ++i)
        {
            if (overlays[i].CustomSdfView != null)
            {
                hasCustomSdf = true;
                break;
            }
        }

        BatchPacket? packet = null;
        try
        {
            ref var first = ref overlays[start];
            packet = RentBatchPacket();
            packet.Outlines = ArrayPool<OutlineInstance>.Shared.Rent(count);
            packet.Segments = ArrayPool<DrawSegment>.Shared.Rent(count); // worst case: every overlay changes kind/SDF
            packet.CustomSdfs = hasCustomSdf ? ArrayPool<CustomSdfBinding>.Shared.Rent(count) : null; // worst case: every overlay uses a distinct custom SDF
            packet.ArenaSdfView = first.ArenaSdfView;
            packet.OutlineSdfConstants = first.ArenaSdfConstants;
            packet.NeedsStencil = false;
            packet.ModifiesDepthState = true;
            packet.StencilKey = 0;
            packet.ClipOffset = first.ClipOffset;
            packet.ClipScale = first.ClipScale;
            packet.FramebufferWidth = first.FramebufferWidth;
            packet.FramebufferHeight = first.FramebufferHeight;
            packet.IsDeferredOverlay = true;
            first.ArenaSdfView = null; // packet owns this retained reference

            var customCount = 0;
            var segmentCount = 0;
            var customBindings = packet.CustomSdfs;
            for (var i = start; i < end; ++i)
            {
                ref var overlay = ref overlays[i];
                var localIndex = i - start;
                packet.Outlines[localIndex] = overlay.Instance;

                if (i != start && overlay.ArenaSdfView != null)
                {
                    // The packet already retained this run's arena SRV through its first overlay.
                    overlay.ArenaSdfView->Release();
                    overlay.ArenaSdfView = null;
                }

                var customBinding = -1;
                if (overlay.CustomSdfView != null)
                {
                    var view = overlay.CustomSdfView;
                    for (var b = 0; b < customCount; ++b)
                    {
                        if (customBindings![b].View == view)
                        {
                            customBinding = b;
                            break;
                        }
                    }

                    if (customBinding < 0)
                    {
                        customBinding = customCount++;
                        customBindings![customBinding] = new CustomSdfBinding
                        {
                            View = view,
                            Constants = overlay.CustomSdfConstants,
                        };
                        packet.CustomSdfCount = customCount;
                        overlay.CustomSdfView = null;
                    }
                    else
                    {
                        view->Release();
                        overlay.CustomSdfView = null;
                    }
                }

                if (segmentCount != 0)
                {
                    ref var previous = ref packet.Segments[segmentCount - 1];
                    if (previous.Kind == overlay.Kind && previous.CustomSdfBinding == customBinding && previous.Start + previous.Count == localIndex)
                    {
                        ++previous.Count;
                        continue;
                    }
                }

                packet.Segments[segmentCount++] = new DrawSegment
                {
                    Kind = overlay.Kind,
                    Start = localIndex,
                    Count = 1,
                    CustomSdfBinding = customBinding,
                };
            }

            packet.OutlineCount = count;
            packet.SegmentCount = segmentCount;
            packet.CustomSdfCount = customCount;
            PreparePacketUploadLayout(packet);

            var packetId = RegisterPacket(packet);
            packet = null; // PendingPackets owns it now.
            try
            {
                _buildDrawList.AddCallback(DrawCallback, (void*)packetId);
            }
            catch
            {
                if (RemovePacket(packetId) is BatchPacket abandoned)
                {
                    ReturnPacketArrays(abandoned);
                }
                throw;
            }
        }
        catch
        {
            if (packet != null)
            {
                ReturnPacketArrays(packet);
            }
            throw;
        }
    }

    private static void ReleaseDeferredOutlineOverlays()
    {
        var overlays = CollectionsMarshal.AsSpan(DeferredOutlineOverlays);
        var len = overlays.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var overlay = ref overlays[i];
            if (overlay.ArenaSdfView != null)
            {
                overlay.ArenaSdfView->Release();
                overlay.ArenaSdfView = null;
            }
            if (overlay.CustomSdfView != null)
            {
                overlay.CustomSdfView->Release();
                overlay.CustomSdfView = null;
            }
        }
        DeferredOutlineOverlays.Clear();
    }

    private static void FinishArena()
    {
        _arenaActive = false;
        _arenaPrepared = false;
        _buildViewportOverride = false;
        _buildWorldLineConfigured = false;
        _buildWorldLineConstantsValid = false;
        _buildWorldLineTransformCount = 0;
        _arenaShape = null;
        ReleaseDeferredOutlineOverlays();
        ResetBuildRun(returnArrays: true);
    }

    private static void EnsureArenaPrepared()
    {
        if (_arenaPrepared)
        {
            return;
        }

        PrepareFrame();

        if (_buildViewportOverride)
        {
            _buildViewportPos = _buildViewportOverridePos;
            _buildViewportSize = _buildViewportOverrideSize;
        }
        else
        {
            var viewport = ImGui.GetWindowViewport();
            _buildViewportPos = viewport.Pos;
            _buildViewportSize = viewport.Size;
        }
        var viewportSizeX = _buildViewportSize.X;
        var viewportSizeY = _buildViewportSize.Y;
        var viewportWidth = Math.Max(1f, viewportSizeX);
        var viewportHeight = Math.Max(1f, viewportSizeY);
        _buildNdcScale = new(2f / viewportWidth, -2f / viewportHeight);

        var viewportPosX = _buildViewportPos.X;
        var viewportPosY = _buildViewportPos.Y;
        _buildNdcOffset = new(-1f - viewportPosX * _buildNdcScale.X, 1f - viewportPosY * _buildNdcScale.Y);
        _buildCenterNdc = new(_buildCenterX * _buildNdcScale.X + _buildNdcOffset.X, _buildCenterY * _buildNdcScale.Y + _buildNdcOffset.Y);
        _buildExtentNdcScale = new(MathF.Abs(_buildNdcScale.X), MathF.Abs(_buildNdcScale.Y));
        _buildLocalToNdc = new(_buildScaledCos * _buildNdcScale.X, -_buildScaledSin * _buildNdcScale.X, _buildScaledSin * _buildNdcScale.Y, _buildScaledCos * _buildNdcScale.Y);

        _buildClipOffset = _buildViewportPos;
        _buildClipScale = ImGui.GetIO().DisplayFramebufferScale;

        if (_buildClipScale.X <= 0f)
        {
            _buildClipScale.X = 1f;
        }
        if (_buildClipScale.Y <= 0f)
        {
            _buildClipScale.Y = 1f;
        }

        var clipScaleX = _buildClipScale.X;
        var clipScaleY = _buildClipScale.Y;
        _buildFramebufferWidth = Math.Max(1, (int)MathF.Round(viewportSizeX * clipScaleX));
        _buildFramebufferHeight = Math.Max(1, (int)MathF.Round(viewportSizeY * clipScaleY));
        _buildNdcToPx = new(0.5f * _buildFramebufferWidth, -0.5f * _buildFramebufferHeight);

        _buildPixelScale = 0.5f * (clipScaleX + clipScaleY);
        var safePixelScale = Math.Max(_buildPixelScale, 1e-5f);
        _buildWorldPixelScale = _buildScreenScale * _buildPixelScale;
        _buildSdfResolutionPixelScale = _buildScreenScale * Math.Max(clipScaleX, clipScaleY);
        _buildOutlineAaPadScreen = 2f / safePixelScale;

        var safeScreenScale = Math.Max(_buildScreenScale, 1e-5f);
        _buildInvScreenScale = 1f / safeScreenScale;
        _buildDirectionCos = _buildScaledCos * _buildInvScreenScale;
        _buildDirectionSin = _buildScaledSin * _buildInvScreenScale;
        _buildAbsScaledCos = Math.Abs(_buildScaledCos);
        _buildAbsScaledSin = Math.Abs(_buildScaledSin);

        // Precompute framebuffer-pixel -> arena-local affine coefficients used by every SDF binding.
        // BuildOutlineSdfConstants then only has to apply the resource-specific min/span transform.
        var invClipX = 1f / clipScaleX;
        var invClipY = 1f / clipScaleY;
        var invClipXScale = invClipX * _buildInvScreenScale;
        var invCliYScale = invClipY * _buildInvScreenScale;
        _buildSdfWxPx = _buildDirectionCos * invClipXScale;
        _buildSdfWxPy = _buildDirectionSin * invCliYScale;
        _buildSdfWzPx = -_buildDirectionSin * invClipXScale;
        _buildSdfWzPy = _buildDirectionCos * invCliYScale;

        var logicalDx0 = viewportPosX - _buildCenterX;
        var logicalDy0 = viewportPosY - _buildCenterY;
        _buildSdfWx0 = (logicalDx0 * _buildDirectionCos + logicalDy0 * _buildDirectionSin) * _buildInvScreenScale;
        _buildSdfWz0 = (-logicalDx0 * _buildDirectionSin + logicalDy0 * _buildDirectionCos) * _buildInvScreenScale;

        _arenaPrepared = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureBuildRunStarted() => EnsureArenaPrepared();

    private static OutlineInstance BuildSdfFillInstance(ArenaSdfResource sdf, uint color)
    {
        var spanX = sdf.SpanX;
        var spanZ = sdf.SpanZ;
        var centerOffset = new WDir(sdf.MinX + 0.5f * spanX, sdf.MinZ + 0.5f * spanZ);
        var halfX = 0.5f * spanX;
        var halfZ = 0.5f * spanZ;
        var extentScreen = new Vector2(_buildAbsScaledCos * halfX + _buildAbsScaledSin * halfZ, _buildAbsScaledSin * halfX + _buildAbsScaledCos * halfZ);

        return new OutlineInstance
        {
            CenterNdc = LocalToNdc(centerOffset),
            ExtentNdc = extentScreen * _buildExtentNdcScale,
            ExtentPx = extentScreen * _buildPixelScale,
            DirectionScreen = default,
            Params = default,
            Extra = default,
            WidthsPx = default,
            Col = color,
            ShadowCol = 0u,
        };
    }

    private static void EnsureStencilMaskBuilt()
    {
        if (_buildArenaSdfMaskOutlineStart >= 0)
        {
            _buildNeedsStencil = true;
            return;
        }

        // The private stencil target survives between our callbacks and ImGui never binds it. Once
        // one packet for this arena has queued an SDF mask instance, subsequent packets only need the same generation key
        if (_arenaStencilMaskQueued)
        {
            _buildNeedsStencil = true;
            return;
        }

        if (!EnsureArenaSdfForBuild())
        {
            return;
        }

        EnsureOutlineBuildCapacity(_buildOutlineCount + 1);
        _buildArenaSdfMaskOutlineStart = _buildOutlineCount;
        _buildOutlines![_buildOutlineCount++] = BuildSdfFillInstance(_buildArenaSdf!, 0xFFFFFFFF);
        _buildNeedsStencil = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteNdcMeshVertex(Vector2 p, uint color, uint boundaryMask, MeshVertex[] vertices, ref int dst)
    {
        vertices[dst++] = new MeshVertex { Pos = p, Col = color, BoundaryMask = boundaryMask };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteMeshVertex(in WDir p, uint color, uint boundaryMask, MeshVertex[] vertices, ref int dst)
    {
        vertices[dst++] = new MeshVertex { Pos = LocalToNdc(p), Col = color, BoundaryMask = boundaryMask };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 ScreenToNdc(float x, float y) => new(x * _buildNdcScale.X + _buildNdcOffset.X, y * _buildNdcScale.Y + _buildNdcOffset.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 ScreenToNdc(Vector2 p) => new(p.X * _buildNdcScale.X + _buildNdcOffset.X, p.Y * _buildNdcScale.Y + _buildNdcOffset.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendSegment(SegmentKind kind, int start, int count, int customSdfBinding = -1, int spriteBinding = -1)
    {
        if (count == 0)
        {
            return;
        }

        // Mesh and arena-clipped analytic fills inherit the packet's current stencil mode. Every
        // other segment kind explicitly switches depth/stencil state in RenderBatchCallback.
        if (kind is not SegmentKind.Mesh and not SegmentKind.Analytic)
        {
            _buildModifiesDepthState = true;
        }

        var n = _buildSegmentCount;
        if (n != 0)
        {
            ref var previous = ref _buildSegments![n - 1];
            if (previous.Kind == kind && previous.CustomSdfBinding == customSdfBinding && previous.SpriteBinding == spriteBinding && previous.Start + previous.Count == start)
            {
                previous.Count += count;
                return;
            }
        }

        EnsureSegmentBuildCapacity(n + 1);
        _buildSegments![n] = new DrawSegment { Kind = kind, Start = start, Count = count, CustomSdfBinding = customSdfBinding, SpriteBinding = spriteBinding };
        _buildSegmentCount = n + 1;
    }

    private static void EnsureSegmentBuildCapacity(int required)
    {
        if (_buildSegments != null && required <= _buildSegments.Length)
        {
            return;
        }

        var capacity = _buildSegments?.Length ?? 32;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<DrawSegment>.Shared.Rent(capacity);
        if (_buildSegments != null)
        {
            if (_buildSegmentCount != 0)
            {
                Array.Copy(_buildSegments, replacement, _buildSegmentCount);
            }
            ArrayPool<DrawSegment>.Shared.Return(_buildSegments);
        }
        _buildSegments = replacement;
    }

    private static void EnsureMeshBuildCapacity(int required)
    {
        if (_buildMeshVertices != null && required <= _buildMeshVertices.Length)
        {
            return;
        }

        var capacity = _buildMeshVertices?.Length ?? 4096;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<MeshVertex>.Shared.Rent(capacity);
        if (_buildMeshVertices != null)
        {
            if (_buildMeshVertexCount != 0)
            {
                Array.Copy(_buildMeshVertices, replacement, _buildMeshVertexCount);
            }
            ArrayPool<MeshVertex>.Shared.Return(_buildMeshVertices);
        }
        _buildMeshVertices = replacement;
    }

    private static void EnsureWorldLineBuildCapacity(int required)
    {
        if (_buildWorldLines != null && required <= _buildWorldLines.Length)
        {
            return;
        }

        var capacity = _buildWorldLines?.Length ?? 256;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<WorldLineInstance>.Shared.Rent(capacity);
        if (_buildWorldLines != null)
        {
            if (_buildWorldLineCount != 0)
            {
                Array.Copy(_buildWorldLines, replacement, _buildWorldLineCount);
            }
            ArrayPool<WorldLineInstance>.Shared.Return(_buildWorldLines);
        }
        _buildWorldLines = replacement;
    }

    private static void EnsureWorldCurveBuildCapacity(int required)
    {
        if (_buildWorldCurves != null && required <= _buildWorldCurves.Length)
        {
            return;
        }

        var capacity = _buildWorldCurves?.Length ?? 32;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<WorldCurveInstance>.Shared.Rent(capacity);
        if (_buildWorldCurves != null)
        {
            if (_buildWorldCurveCount != 0)
            {
                Array.Copy(_buildWorldCurves, replacement, _buildWorldCurveCount);
            }
            ArrayPool<WorldCurveInstance>.Shared.Return(_buildWorldCurves);
        }
        _buildWorldCurves = replacement;
    }

    private static void EnsureWorldProjectedArrowBuildCapacity(int required)
    {
        if (_buildWorldProjectedArrows != null && required <= _buildWorldProjectedArrows.Length)
        {
            return;
        }

        var capacity = _buildWorldProjectedArrows?.Length ?? 16;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<WorldProjectedArrowInstance>.Shared.Rent(capacity);
        if (_buildWorldProjectedArrows != null)
        {
            if (_buildWorldProjectedArrowCount != 0)
            {
                Array.Copy(_buildWorldProjectedArrows, replacement, _buildWorldProjectedArrowCount);
            }
            ArrayPool<WorldProjectedArrowInstance>.Shared.Return(_buildWorldProjectedArrows);
        }
        _buildWorldProjectedArrows = replacement;
    }

    private static void EnsureWorldProjectedShapeBuildCapacity(int required)
    {
        if (_buildWorldProjectedShapes != null && required <= _buildWorldProjectedShapes.Length)
        {
            return;
        }

        var capacity = _buildWorldProjectedShapes?.Length ?? 64;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<WorldProjectedShapeInstance>.Shared.Rent(capacity);
        if (_buildWorldProjectedShapes != null)
        {
            if (_buildWorldProjectedShapeCount != 0)
            {
                Array.Copy(_buildWorldProjectedShapes, replacement, _buildWorldProjectedShapeCount);
            }
            ArrayPool<WorldProjectedShapeInstance>.Shared.Return(_buildWorldProjectedShapes);
        }
        _buildWorldProjectedShapes = replacement;
    }

    private static void EnsureWorldProjectedSdfBindingBuildCapacity(int required)
    {
        if (_buildWorldProjectedSdfs != null && required <= _buildWorldProjectedSdfs.Length)
        {
            return;
        }

        var capacity = _buildWorldProjectedSdfs?.Length ?? 8;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<WorldProjectedSdfBinding>.Shared.Rent(capacity);
        if (_buildWorldProjectedSdfs != null)
        {
            if (_buildWorldProjectedSdfCount != 0)
            {
                Array.Copy(_buildWorldProjectedSdfs, replacement, _buildWorldProjectedSdfCount);
            }
            ArrayPool<WorldProjectedSdfBinding>.Shared.Return(_buildWorldProjectedSdfs);
        }
        _buildWorldProjectedSdfs = replacement;
    }

    private static void EnsureWorldLineTransformBuildCapacity(int required)
    {
        if (_buildWorldLineTransforms != null && required <= _buildWorldLineTransforms.Length)
        {
            return;
        }

        var capacity = _buildWorldLineTransforms?.Length ?? 16;
        while (capacity < required)
        {
            capacity *= 2;
        }
        capacity = Math.Min(capacity, MaxWorldLineTransforms);

        var replacement = ArrayPool<WorldLineTransform>.Shared.Rent(capacity);
        if (_buildWorldLineTransforms != null)
        {
            if (_buildWorldLineTransformCount != 0)
            {
                Array.Copy(_buildWorldLineTransforms, replacement, _buildWorldLineTransformCount);
            }
            ArrayPool<WorldLineTransform>.Shared.Return(_buildWorldLineTransforms);
        }
        _buildWorldLineTransforms = replacement;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFinite(in Matrix4x4 matrix)
        => float.IsFinite(matrix.M11) && float.IsFinite(matrix.M12) && float.IsFinite(matrix.M13) && float.IsFinite(matrix.M14)
        && float.IsFinite(matrix.M21) && float.IsFinite(matrix.M22) && float.IsFinite(matrix.M23) && float.IsFinite(matrix.M24)
        && float.IsFinite(matrix.M31) && float.IsFinite(matrix.M32) && float.IsFinite(matrix.M33) && float.IsFinite(matrix.M34)
        && float.IsFinite(matrix.M41) && float.IsFinite(matrix.M42) && float.IsFinite(matrix.M43) && float.IsFinite(matrix.M44);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFinite(in Vector4 vector)
        => float.IsFinite(vector.X) && float.IsFinite(vector.Y) && float.IsFinite(vector.Z) && float.IsFinite(vector.W);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector4 ExtractReverseZNearPlane(in Matrix4x4 viewProj)
    {
        // HLSL uses row-vector world * ViewProj. With D3D reverse-Z, the near clipping inequality is
        // clip.z - clip.w < 0, so extracting column Z minus column W keeps clipping tied to exactly
        // the same matrix as projection. Reading RenderCamera.ViewMatrix separately can mix epochs.
        return new(
            viewProj.M13 - viewProj.M14,
            viewProj.M23 - viewProj.M24,
            viewProj.M33 - viewProj.M34,
            viewProj.M43 - viewProj.M44);
    }

    private static bool TryBuildWorldProjectionSnapshot(in Matrix4x4 viewProj, out Matrix4x4 invViewProj, out Vector4 nearPlane)
    {
        invViewProj = default;
        nearPlane = default;
        if (!IsFinite(viewProj) || !Matrix4x4.Invert(viewProj, out invViewProj) || !IsFinite(invViewProj))
            return false;

        nearPlane = ExtractReverseZNearPlane(viewProj);
        return IsFinite(nearPlane) && nearPlane.X * nearPlane.X + nearPlane.Y * nearPlane.Y + nearPlane.Z * nearPlane.Z > 1e-12f;
    }

    private static bool TryReadStableGameViewProjection(out Matrix4x4 viewProj)
    {
        viewProj = default;
        var control = Control.Instance();
        if (control == null)
            return false;

        // Control's 64-byte matrix has no sequence counter. Require three identical copies so UI
        // submission never records a torn but accidentally invertible game-thread update.
        Matrix4x4 first = control->ViewProjectionMatrix;
        Thread.MemoryBarrier();
        Matrix4x4 second = control->ViewProjectionMatrix;
        Thread.MemoryBarrier();
        Matrix4x4 third = control->ViewProjectionMatrix;
        if (!first.Equals(second) || !second.Equals(third))
            return false;

        viewProj = third;
        return true;
    }

    private static void EnsureWorldLineConstants()
    {
        if (_buildWorldLineConstantsValid || !_buildWorldLineConfigured)
        {
            return;
        }

        var framebufferWidth = Math.Max(_buildFramebufferWidth, 1);
        var framebufferHeight = Math.Max(_buildFramebufferHeight, 1);
        var depthWidth = Math.Max(_buildWorldSceneDepthSize.X, 1f);
        var depthHeight = Math.Max(_buildWorldSceneDepthSize.Y, 1f);
        _buildWorldLineConstants = new WorldLineConstants
        {
            ViewProj = _buildWorldLineViewProj,
            InvViewProj = _buildWorldLineInvViewProj,
            NearPlane = _buildWorldLineNearPlane,
            Viewport = new Vector4(framebufferWidth, framebufferHeight, Math.Max(_buildPixelScale, 1e-5f), 0f),
            RasterScale = new Vector4(depthWidth / framebufferWidth, depthHeight / framebufferHeight, 2f / framebufferWidth, 2f / framebufferHeight),
            SceneDepthParams = new Vector4(_buildWorldSceneDepthSize, WorldSceneOcclusionTolerance, _buildWorldSceneDepthAvailable ? 1f : 0f),
            SceneInfoParams = new Vector4(_buildWorldSceneInfoAvailable ? 1f : 0f, WorldSceneCharacterMaskThreshold, 0f, 0f),
        };
        _buildWorldLineConstantsValid = true;
    }

    private static void EnsurePrimitiveTriangleStrokeBuildCapacity(int required)
    {
        if (_buildPrimitiveTriangleStrokes != null && required <= _buildPrimitiveTriangleStrokes.Length)
        {
            return;
        }

        var capacity = _buildPrimitiveTriangleStrokes?.Length ?? 128;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<PrimitiveTriangleStrokeInstance>.Shared.Rent(capacity);
        if (_buildPrimitiveTriangleStrokes != null)
        {
            if (_buildPrimitiveTriangleStrokeCount != 0)
            {
                Array.Copy(_buildPrimitiveTriangleStrokes, replacement, _buildPrimitiveTriangleStrokeCount);
            }
            ArrayPool<PrimitiveTriangleStrokeInstance>.Shared.Return(_buildPrimitiveTriangleStrokes);
        }
        _buildPrimitiveTriangleStrokes = replacement;
    }

    private static void EnsureStrokeInstanceBuildCapacity(int required)
    {
        if (_buildStrokeInstances != null && required <= _buildStrokeInstances.Length)
        {
            return;
        }

        var capacity = _buildStrokeInstances?.Length ?? 256;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<StrokeInstance>.Shared.Rent(capacity);
        if (_buildStrokeInstances != null)
        {
            if (_buildStrokeInstanceCount != 0)
            {
                Array.Copy(_buildStrokeInstances, replacement, _buildStrokeInstanceCount);
            }
            ArrayPool<StrokeInstance>.Shared.Return(_buildStrokeInstances);
        }
        _buildStrokeInstances = replacement;
    }

    private static void EnsureAnalyticBuildCapacity(int required)
    {
        if (_buildAnalytics != null && required <= _buildAnalytics.Length)
        {
            return;
        }

        var capacity = _buildAnalytics?.Length ?? 128;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<AnalyticInstance>.Shared.Rent(capacity);
        if (_buildAnalytics != null)
        {
            if (_buildAnalyticCount != 0)
            {
                Array.Copy(_buildAnalytics, replacement, _buildAnalyticCount);
            }
            ArrayPool<AnalyticInstance>.Shared.Return(_buildAnalytics);
        }
        _buildAnalytics = replacement;
    }

    private static void EnsureOutlineBuildCapacity(int required)
    {
        if (_buildOutlines != null && required <= _buildOutlines.Length)
        {
            return;
        }

        var capacity = _buildOutlines?.Length ?? 64;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<OutlineInstance>.Shared.Rent(capacity);
        if (_buildOutlines != null)
        {
            if (_buildOutlineCount != 0)
            {
                Array.Copy(_buildOutlines, replacement, _buildOutlineCount);
            }
            ArrayPool<OutlineInstance>.Shared.Return(_buildOutlines);
        }
        _buildOutlines = replacement;
    }

    private static void EnsureWorldTextInstanceBuildCapacity(int required)
    {
        if (_buildWorldTextInstances != null && required <= _buildWorldTextInstances.Length)
            return;

        var capacity = _buildWorldTextInstances?.Length ?? 128;
        while (capacity < required)
            capacity *= 2;

        var replacement = ArrayPool<WorldTextInstance>.Shared.Rent(capacity);
        if (_buildWorldTextInstances != null)
        {
            if (_buildWorldTextInstanceCount != 0)
                Array.Copy(_buildWorldTextInstances, replacement, _buildWorldTextInstanceCount);
            ArrayPool<WorldTextInstance>.Shared.Return(_buildWorldTextInstances);
        }
        _buildWorldTextInstances = replacement;
    }

    private static void EnsureSpriteBuildCapacity(int required)
    {
        if (_buildSprites != null && required <= _buildSprites.Length)
        {
            return;
        }

        var capacity = _buildSprites?.Length ?? 16;
        while (capacity < required)
        {
            capacity *= 2;
        }
        var replacement = ArrayPool<SpriteBinding>.Shared.Rent(capacity);
        if (_buildSprites != null)
        {
            if (_buildSpriteCount != 0)
            {
                Array.Copy(_buildSprites, replacement, _buildSpriteCount);
            }
            ArrayPool<SpriteBinding>.Shared.Return(_buildSprites, clearArray: false);
        }
        _buildSprites = replacement;
    }

    private static void EnsureTextInstanceBuildCapacity(int required)
    {
        if (_buildTextInstances != null && required <= _buildTextInstances.Length)
        {
            return;
        }

        var capacity = _buildTextInstances?.Length ?? 256;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<TextInstance>.Shared.Rent(capacity);
        if (_buildTextInstances != null)
        {
            if (_buildTextInstanceCount != 0)
            {
                Array.Copy(_buildTextInstances, replacement, _buildTextInstanceCount);
            }
            ArrayPool<TextInstance>.Shared.Return(_buildTextInstances);
        }
        _buildTextInstances = replacement;
    }

    private static void EnsureCustomSdfBindingBuildCapacity(int required)
    {
        if (_buildCustomSdfs != null && required <= _buildCustomSdfs.Length)
        {
            return;
        }

        var capacity = _buildCustomSdfs?.Length ?? 8;
        while (capacity < required)
        {
            capacity *= 2;
        }

        var replacement = ArrayPool<CustomSdfBinding>.Shared.Rent(capacity);
        if (_buildCustomSdfs != null)
        {
            if (_buildCustomSdfCount != 0)
            {
                Array.Copy(_buildCustomSdfs, replacement, _buildCustomSdfCount);
            }
            ArrayPool<CustomSdfBinding>.Shared.Return(_buildCustomSdfs, clearArray: false);
        }
        _buildCustomSdfs = replacement;
    }

    private static void ResetBuildRun(bool returnArrays)
    {
        if (returnArrays)
        {
            if (_buildMeshVertices != null)
            {
                ArrayPool<MeshVertex>.Shared.Return(_buildMeshVertices);
            }
            if (_buildPrimitiveTriangleStrokes != null)
            {
                ArrayPool<PrimitiveTriangleStrokeInstance>.Shared.Return(_buildPrimitiveTriangleStrokes);
            }
            if (_buildStrokeInstances != null)
            {
                ArrayPool<StrokeInstance>.Shared.Return(_buildStrokeInstances);
            }
            if (_buildWorldLines != null)
            {
                ArrayPool<WorldLineInstance>.Shared.Return(_buildWorldLines);
            }
            if (_buildWorldCurves != null)
            {
                ArrayPool<WorldCurveInstance>.Shared.Return(_buildWorldCurves);
            }
            if (_buildWorldProjectedArrows != null)
            {
                ArrayPool<WorldProjectedArrowInstance>.Shared.Return(_buildWorldProjectedArrows);
            }
            if (_buildWorldProjectedShapes != null)
            {
                ArrayPool<WorldProjectedShapeInstance>.Shared.Return(_buildWorldProjectedShapes);
            }
            if (_buildWorldProjectedSdfs != null)
            {
                for (var i = 0; i < _buildWorldProjectedSdfCount; ++i)
                {
                    if (_buildWorldProjectedSdfs[i].View != null)
                    {
                        _buildWorldProjectedSdfs[i].View->Release();
                    }
                }
                ArrayPool<WorldProjectedSdfBinding>.Shared.Return(_buildWorldProjectedSdfs, clearArray: false);
            }
            if (_buildWorldLineTransforms != null)
            {
                ArrayPool<WorldLineTransform>.Shared.Return(_buildWorldLineTransforms);
            }
            if (_buildAnalytics != null)
            {
                ArrayPool<AnalyticInstance>.Shared.Return(_buildAnalytics);
            }
            if (_buildOutlines != null)
            {
                ArrayPool<OutlineInstance>.Shared.Return(_buildOutlines);
            }
            if (_buildTextInstances != null)
            {
                ArrayPool<TextInstance>.Shared.Return(_buildTextInstances);
            }
            if (_buildWorldTextInstances != null)
            {
                ArrayPool<WorldTextInstance>.Shared.Return(_buildWorldTextInstances);
            }
            if (_buildSprites != null)
            {
                for (var i = 0; i < _buildSpriteCount; ++i)
                {
                    var view = _buildSprites[i].View;
                    if (view != null)
                    {
                        view->Release();
                    }
                }
                ArrayPool<SpriteBinding>.Shared.Return(_buildSprites, clearArray: false);
            }
            if (_buildCustomSdfs != null)
            {
                for (var i = 0; i < _buildCustomSdfCount; ++i)
                {
                    if (_buildCustomSdfs[i].View != null)
                    {
                        _buildCustomSdfs[i].View->Release();
                    }
                }
                ArrayPool<CustomSdfBinding>.Shared.Return(_buildCustomSdfs, clearArray: false);
            }
            if (_buildSegments != null)
            {
                ArrayPool<DrawSegment>.Shared.Return(_buildSegments);
            }
        }

        _buildMeshVertices = null;
        _buildPrimitiveTriangleStrokes = null;
        _buildStrokeInstances = null;
        _buildWorldLines = null;
        _buildWorldCurves = null;
        _buildWorldProjectedArrows = null;
        _buildWorldProjectedShapes = null;
        _buildWorldProjectedSdfs = null;
        _buildWorldLineTransforms = null;
        _buildAnalytics = null;
        _buildOutlines = null;
        _buildTextInstances = null;
        _buildWorldTextInstances = null;
        _buildSprites = null;
        _buildCustomSdfs = null;
        _buildSegments = null;
        _buildMeshVertexCount = 0;
        _buildArenaSdfMaskOutlineStart = -1;
        _buildPrimitiveTriangleStrokeCount = 0;
        _buildStrokeInstanceCount = 0;
        _buildWorldLineCount = 0;
        _buildWorldCurveCount = 0;
        _buildWorldProjectedArrowCount = 0;
        _buildWorldProjectedShapeCount = 0;
        _buildWorldProjectedSdfCount = 0;
        _buildWorldLineTransformCount = 0;
        _buildWorldLineConstantsValid = false;
        _buildAnalyticCount = 0;
        _buildOutlineCount = 0;
        _buildTextInstanceCount = 0;
        _buildWorldTextInstanceCount = 0;
        _buildSpriteCount = 0;
        _buildSegmentCount = 0;
        _buildNeedsStencil = false;
        _buildUsesZoneWave = false;
        _buildModifiesDepthState = false;
        _buildArenaSdf = null;
        _buildOutlineSdfConstants = default;
        _buildCustomSdfCount = 0;
    }

    private static void RenderBatchCallback(ImDrawList* parentList, ImDrawCmd* cmd)
    {
        // The ImGui callback may run concurrently with plugin hot-reload teardown. Serialize the
        // complete callback, including its finally/state restoration, against Shutdown so none of
        // the global D3D11 objects can be released while native driver calls are in flight.
        lock (_rendererLock)
        {
            if (_shutDown)
            {
                return;
            }

            RenderBatchCallbackImpl(parentList, cmd);
        }
    }

    private static void RenderBatchCallbackImpl(ImDrawList* parentList, ImDrawCmd* cmd)
    {
        BatchPacket? packet = null;

        ID3D11Buffer* oldVertexBuffer = null;
        ID3D11Buffer* oldIndexBuffer = null;
        ID3D11InputLayout* oldInputLayout = null;
        ID3D11VertexShader* oldVertexShader = null;
        ID3D11PixelShader* oldPixelShader = null;
        // The arena renderer owns PS resource slots t1-t7. Text uses t3, world-space scene
        // occlusion uses FFXIV scene depth at t4, the per-pixel character classification uses t5,
        // and terrain-projected polygon/arena SDFs use t6/t7. Native UI ordering is structural.
        var oldPsSrvs = stackalloc ID3D11ShaderResourceView*[8];
        var oldAuxSamplers = stackalloc ID3D11SamplerState*[3];
        var oldAuxConstantBuffers = stackalloc ID3D11Buffer*[2];
        // Scratch storage must be allocated outside try/finally: Reuse these two slots for world VS capture, bind and restore
        var worldLineBufferScratch = stackalloc ID3D11Buffer*[2];
        for (var i = 0; i < 8; ++i)
        {
            oldPsSrvs[i] = null;
        }
        for (var i = 0; i < 3; ++i)
        {
            oldAuxSamplers[i] = null;
        }
        oldAuxConstantBuffers[0] = null;
        oldAuxConstantBuffers[1] = null;
        ID3D11ShaderResourceView* fontAtlasSrv = null;
        var capturedSrvFirstSlot = 0u;
        var capturedSrvCount = 0u;
        var capturedSamplerFirstSlot = 0u;
        var capturedSamplerCount = 0u;
        var capturedConstantBufferFirstSlot = 0u;
        var capturedConstantBufferCount = 0u;
        ID3D11Buffer* oldWorldLineVsConstantBuffers0 = null;
        ID3D11Buffer* oldWorldLineVsConstantBuffers1 = null;
        ID3D11Buffer* oldWorldLinePsConstantBuffer = null;
        ID3D11Buffer* oldWorldProjectedSdfPsConstantBuffer = null;
        ID3D11Buffer* oldZoneWavePsConstantBuffer = null;
        var worldLineVsConstantBuffersCaptured = false;
        var worldLineVsConstantBufferCount = 0u;
        var worldLinePsConstantBufferCaptured = false;
        var worldProjectedSdfPsConstantBufferCaptured = false;
        var zoneWavePsConstantBufferCaptured = false;
        ID3D11DepthStencilState* oldDepthStencilState = null;
        ID3D11RenderTargetView* oldRenderTarget = null;
        ID3D11DepthStencilView* oldDepthStencilView = null;
        var oldStride = 0u;
        var oldOffset = 0u;
        DXGI_FORMAT oldIndexFormat = default;
        var oldIndexOffset = 0u;
        var oldStencilRef = 0u;
        var coreStateCaptured = false;
        var indexStateCaptured = false;
        var depthStateCaptured = false;
        var auxSrvStateCaptured = false;
        var auxSamplerStateCaptured = false;
        var auxConstantBufferStateCaptured = false;
        var renderTargetCaptured = false;

        try
        {
            if (cmd == null || cmd->UserCallbackData == null || !_isInitialized)
            {
                return;
            }

            packet = RemovePacket((nint)cmd->UserCallbackData);
            if (packet == null)
            {
                return;
            }
            if (packet.IsWorldOverlayPresent)
            {
                PresentWorldOverlay(packet.FramebufferWidth, packet.FramebufferHeight, packet.SubmitFrame);
                return;
            }
            if (packet.Segments == null || packet.SegmentCount == 0)
            {
                return;
            }

            // Reject fully clipped callbacks before touching dynamic buffers or D3D state. This matters for collapsed/partially hidden arena windows where ImGui still carries the callback
            var clip = cmd->ClipRect;
            var coX = packet.ClipOffset.X;
            var coY = packet.ClipOffset.Y;
            var csX = packet.ClipScale.X;
            var csY = packet.ClipScale.Y;
            var clipMinX = (clip.X - coX) * csX;
            var clipMinY = (clip.Y - coY) * csY;
            var clipMaxX = (clip.Z - coX) * csX;
            var clipMaxY = (clip.W - coY) * csY;
            if (clipMaxX <= clipMinX || clipMaxY <= clipMinY)
            {
                return;
            }

            // If the native layer cannot allocate during startup/device pressure, keep world
            // primitives visible as an ordinary background draw for this frame. They remain free of
            // the particle-corrupted alpha mask even in this degraded path.
            if (packet.WorldOverlayTarget && !EnsureWorldOverlayResources(packet.FramebufferWidth, packet.FramebufferHeight))
                packet.WorldOverlayTarget = false;

            var uploadBytes = packet.UploadBytes;
            if (uploadBytes != 0 && !EnsureUploadVertexBuffer(uploadBytes))
            {
                return;
            }
            if (packet.NeedsStencil && !EnsureStencilTarget(packet.FramebufferWidth, packet.FramebufferHeight))
            {
                return;
            }
            var populateStencil = packet.NeedsStencil && _renderedStencilKey != packet.StencilKey;

            if (uploadBytes != 0)
            {
                UploadPacket(packet, uploadBytes);
            }

            // Capture only state this callback actually needs to restore. ImGui already uses TRIANGLELIST, and normal ImGui draw commands program their own scissor rectangles
            _context->IAGetVertexBuffers(0u, 1u, &oldVertexBuffer, &oldStride, &oldOffset);
            _context->IAGetInputLayout(&oldInputLayout);
            _context->VSGetShader(&oldVertexShader, null, null);
            _context->PSGetShader(&oldPixelShader, null, null);
            coreStateCaptured = true;

            if (packet.UsesZoneWave && _zoneWaveConstantBuffer != null)
            {
                _context->PSGetConstantBuffers(7u, 1u, &oldZoneWavePsConstantBuffer);
                zoneWavePsConstantBufferCaptured = true;
                UploadZoneWaveConstants(packet.SubmitFrame);
                var zoneWaveConstants = _zoneWaveConstantBuffer;
                _context->PSSetConstantBuffers(7u, 1u, &zoneWaveConstants);
            }

            var usesIndexedQuad = packet.AnalyticCount != 0 || packet.OutlineCount != 0 || packet.TextInstanceCount != 0 || packet.WorldTextInstanceCount != 0 ||
                packet.StrokeInstanceCount != 0 || packet.WorldLineCount != 0 || packet.WorldCurveCount != 0 || packet.WorldProjectedArrowCount != 0 || packet.WorldProjectedShapeCount != 0 || packet.NeedsStencil;
            if (usesIndexedQuad)
            {
                _context->IAGetIndexBuffer(&oldIndexBuffer, &oldIndexFormat, &oldIndexOffset);
                _context->IASetIndexBuffer(_quadIndexBuffer, (DXGI_FORMAT)42 /* R32_UINT */, 0u);
                indexStateCaptured = true;
            }

            var usesArenaSdf = packet.ArenaSdfView != null && (packet.OutlineCount != 0 || packet.NeedsStencil);
            var usesCustomSdf = packet.CustomSdfCount != 0;
            var usesWorldProjectedSdf = packet.WorldProjectedSdfCount != 0;
            var usesSdf = usesArenaSdf || usesCustomSdf || usesWorldProjectedSdf;
            var usesText = packet.TextInstanceCount != 0 || packet.WorldTextInstanceCount != 0;
            var usesWorldProjection = packet.WorldLineCount != 0 || packet.WorldCurveCount != 0 || packet.WorldProjectedArrowCount != 0 || packet.WorldProjectedShapeCount != 0 || packet.WorldTextInstanceCount != 0;
            var usesWorldTransforms = packet.WorldLineCount != 0 || packet.WorldCurveCount != 0;
            if (usesWorldProjection)
            {
                if (usesWorldTransforms && packet.WorldLineTransforms == null)
                {
                    return;
                }

                worldLineVsConstantBufferCount = usesWorldTransforms ? 2u : 1u;
                _context->VSGetConstantBuffers(1u, worldLineVsConstantBufferCount, worldLineBufferScratch);
                oldWorldLineVsConstantBuffers0 = worldLineBufferScratch[0];
                oldWorldLineVsConstantBuffers1 = usesWorldTransforms ? worldLineBufferScratch[1] : null;
                worldLineVsConstantBuffersCaptured = true;

                UploadWorldLineConstants(ref packet.WorldLineConstants);
                worldLineBufferScratch[0] = _worldLineConstantBuffer;
                if (usesWorldTransforms)
                {
                    UploadWorldLineTransforms(packet.WorldLineTransforms!, packet.WorldLineTransformCount);
                    worldLineBufferScratch[1] = _worldLineTransformBuffer;
                }
                _context->VSSetConstantBuffers(1u, worldLineVsConstantBufferCount, worldLineBufferScratch);

                // stroke_ps is shared by screen and world strokes. World VS paths mark their pixels
                // with flag bit 2; projected receivers additionally use scene classification at t5.
                _context->PSGetConstantBuffers(4u, 1u, &oldWorldLinePsConstantBuffer);
                worldLinePsConstantBufferCaptured = true;
                var worldConstants = _worldLineConstantBuffer;
                _context->PSSetConstantBuffers(4u, 1u, &worldConstants);

                if (packet.WorldProjectedShapeCount != 0)
                {
                    _context->PSGetConstantBuffers(5u, 1u, &oldWorldProjectedSdfPsConstantBuffer);
                    worldProjectedSdfPsConstantBufferCaptured = true;
                    var projectedSdfConstants = _worldProjectedSdfConstantBuffer;
                    _context->PSSetConstantBuffers(5u, 1u, &projectedSdfConstants);

                }
            }

            // The build path records whether any segment explicitly switches depth/stencil mode,
            // avoiding an ordered-segment rescan in this latency-sensitive callback
            if (packet.NeedsStencil || packet.ModifiesDepthState)
            {
                _context->OMGetDepthStencilState(&oldDepthStencilState, &oldStencilRef);
                depthStateCaptured = true;
            }

            if (usesText)
            {
                // Text samples the renderer-owned immutable MSDF atlas. It is created once during
                // Initialize and released only while holding _rendererLock in Shutdown, so glyph UVs
                // and the texture identity are a permanent matched pair for this renderer generation.
                fontAtlasSrv = _arenaFontAtlasView;
            }

            var usesWorldSceneDepth = usesWorldProjection && packet.SceneDepthView != null;
            var usesProjectedReceivers = packet.WorldProjectedArrowCount != 0 || packet.WorldProjectedShapeCount != 0;
            var usesWorldSceneInfo = usesProjectedReceivers && packet.SceneInfoView != null;
            var highestAuxSrvSlot = usesWorldProjectedSdf ? 7u : usesWorldSceneInfo ? 5u : usesWorldSceneDepth ? 4u : usesText ? 3u : usesSdf ? 2u : 0u;
            if (highestAuxSrvSlot != 0u)
            {
                // Capture one contiguous t1..tN range. This preserves any backend-owned resources in
                // the gaps while allowing world-space strokes to temporarily occupy t4.
                _context->PSGetShaderResources(1u, highestAuxSrvSlot, oldPsSrvs + 1);
                capturedSrvFirstSlot = 1u;
                capturedSrvCount = highestAuxSrvSlot;
                auxSrvStateCaptured = true;
            }

            if (usesWorldSceneDepth)
            {
                var sceneDepth = packet.SceneDepthView;
                _context->PSSetShaderResources(4u, 1u, &sceneDepth);
            }
            if (usesWorldSceneInfo)
            {
                var sceneInfo = packet.SceneInfoView;
                _context->PSSetShaderResources(5u, 1u, &sceneInfo);
            }
            if (usesSdf && usesText)
            {
                _context->PSGetSamplers(1u, 3u, oldAuxSamplers);
                capturedSamplerFirstSlot = 1u;
                capturedSamplerCount = 3u;
                auxSamplerStateCaptured = true;
            }
            else if (usesSdf)
            {
                _context->PSGetSamplers(1u, 1u, oldAuxSamplers);
                capturedSamplerFirstSlot = 1u;
                capturedSamplerCount = 1u;
                auxSamplerStateCaptured = true;
            }
            else if (usesText)
            {
                _context->PSGetSamplers(3u, 1u, oldAuxSamplers + 2);
                capturedSamplerFirstSlot = 3u;
                capturedSamplerCount = 1u;
                auxSamplerStateCaptured = true;
            }

            if (usesArenaSdf && usesCustomSdf)
            {
                _context->PSGetConstantBuffers(1u, 2u, oldAuxConstantBuffers);
                capturedConstantBufferFirstSlot = 1u;
                capturedConstantBufferCount = 2u;
                auxConstantBufferStateCaptured = true;
            }
            else if (usesArenaSdf)
            {
                _context->PSGetConstantBuffers(1u, 1u, oldAuxConstantBuffers);
                capturedConstantBufferFirstSlot = 1u;
                capturedConstantBufferCount = 1u;
                auxConstantBufferStateCaptured = true;
            }
            else if (usesCustomSdf)
            {
                _context->PSGetConstantBuffers(2u, 1u, oldAuxConstantBuffers + 1);
                capturedConstantBufferFirstSlot = 2u;
                capturedConstantBufferCount = 1u;
                auxConstantBufferStateCaptured = true;
            }

            // The MSDF atlas is renderer-owned; t1-t3 above are captured only so this callback can
            // restore the backend's state afterward.
            if (packet.NeedsStencil || packet.WorldOverlayTarget)
            {
                _context->OMGetRenderTargets(1u, &oldRenderTarget, &oldDepthStencilView);
                renderTargetCaptured = true;
            }

            if (packet.WorldOverlayTarget)
            {
                if (_worldOverlayBaseFrame != packet.SubmitFrame)
                {
                    float* transparent = stackalloc float[4];
                    transparent[0] = transparent[1] = transparent[2] = transparent[3] = 0f;
                    _context->ClearRenderTargetView(_worldOverlayBaseRenderTarget, transparent);
                    _worldOverlayBaseFrame = packet.SubmitFrame;
                }
                var overlayTarget = _worldOverlayBaseRenderTarget;
                _context->OMSetRenderTargets(1u, &overlayTarget, null);
            }

            var scissor = new RECT
            {
                left = Math.Max(0, (int)clipMinX),
                top = Math.Max(0, (int)clipMinY),
                right = Math.Min(packet.FramebufferWidth, (int)clipMaxX),
                bottom = Math.Min(packet.FramebufferHeight, (int)clipMaxY),
            };
            if (scissor.right <= scissor.left || scissor.bottom <= scissor.top)
            {
                return;
            }

            _context->RSSetScissorRects(1u, &scissor);

            // Track actual IA/VB/VS family separately from the pixel shader. All outline variants
            // share one vertex path; CustomSdfFill shares the mesh vertex path.
            const byte vertexNone = 0, vertexMesh = 1, vertexStroke = 2, vertexAnalytic = 3, vertexOutline = 4, vertexText = 5, vertexWorldLine = 6, vertexWorldCurve = 7, vertexPrimitiveTriangleStroke = 8, vertexWorldProjectedArrow = 9, vertexWorldProjectedShape = 10, vertexWorldText = 11;
            var boundVertexFamily = vertexNone;
            ID3D11PixelShader* boundPixelShader = null;
            var boundCustomSdfBinding = -1;
            var boundWorldShapeSdfBinding = int.MinValue;
            var boundWorldArenaSdfBinding = int.MinValue;
            var boundSpriteBinding = -1; // -1 means renderer font atlas is currently bound at t3

            if (usesArenaSdf)
            {
                UploadOutlineSdfConstants(packet.OutlineSdfConstants);
            }

            // Constant-buffer objects are packet-invariant. Bind b1/b2 once up front; custom-SDF
            // binding changes only need to update b2 contents and t2 afterwards.
            if (usesArenaSdf && usesCustomSdf)
            {
                var sdfConstants = stackalloc ID3D11Buffer*[2];
                sdfConstants[0] = _outlineSdfConstantBuffer;
                sdfConstants[1] = _customSdfConstantBuffer;
                _context->PSSetConstantBuffers(1u, 2u, sdfConstants);
            }
            else if (usesArenaSdf)
            {
                var sdfConstants = _outlineSdfConstantBuffer;
                _context->PSSetConstantBuffers(1u, 1u, &sdfConstants);
            }
            else if (usesCustomSdf)
            {
                var sdfConstants = _customSdfConstantBuffer;
                _context->PSSetConstantBuffers(2u, 1u, &sdfConstants);
            }

            // Arena SDF (t1) and text atlas (t3) can be installed together when both are used.
            // Preserve the captured t2 value until/if the first custom-SDF segment replaces it.
            if (usesArenaSdf && usesText && fontAtlasSrv != null)
            {
                var views = stackalloc ID3D11ShaderResourceView*[3];
                views[0] = packet.ArenaSdfView;
                views[1] = oldPsSrvs[2];
                views[2] = fontAtlasSrv;
                _context->PSSetShaderResources(1u, 3u, views);
            }
            else
            {
                if (usesArenaSdf)
                {
                    var sdfView = packet.ArenaSdfView;
                    _context->PSSetShaderResources(1u, 1u, &sdfView);
                }
                if (usesText && fontAtlasSrv != null)
                {
                    var textView = fontAtlasSrv;
                    _context->PSSetShaderResources(3u, 1u, &textView);
                }
            }

            // s1 and s3 use the same sampler. When both are needed, update the contiguous s1-s3
            // range in one driver call while preserving the captured s2 binding.
            if (usesSdf && usesText)
            {
                var samplers = stackalloc ID3D11SamplerState*[3];
                samplers[0] = _arenaSdfSampler;
                samplers[1] = oldAuxSamplers[1];
                samplers[2] = _arenaSdfSampler;
                _context->PSSetSamplers(1u, 3u, samplers);
            }
            else if (usesSdf)
            {
                var sdfSampler = _arenaSdfSampler;
                _context->PSSetSamplers(1u, 1u, &sdfSampler);
            }
            else if (usesText)
            {
                var textSampler = _arenaSdfSampler;
                _context->PSSetSamplers(3u, 1u, &textSampler);
            }

            if (packet.NeedsStencil)
            {
                if (populateStencil)
                {
                    // Populate our private stencil buffer only once for this arena. The DSV is private
                    // to Dx11ArenaRenderer, so its contents remain valid across intervening ImGui draw
                    // commands and later callbacks until another arena generation replaces it.
                    _context->ClearDepthStencilView(_stencilView, 0x2 /* D3D11_CLEAR_STENCIL */, 1f, 0);
                    // Stencil generation is intentionally colorless. The visible arena background
                    // is drawn immediately afterward as a solid bounds quad through this exact mask.
                    _context->OMSetRenderTargets(0u, null, _stencilView);
                    _context->OMSetDepthStencilState(_stencilWriteState, 1u);
                    BindPipeline(vertexOutline, _arenaSdfStencilPixelShader, ref boundVertexFamily, ref boundPixelShader);
                    _context->DrawIndexedInstanced(6u, 1u, 0u, 0, (uint)packet.ArenaSdfMaskOutlineStart);
                    _renderedStencilKey = packet.StencilKey;
                }

                // Pair the active ImGui RTV with our private stencil for all arena-clipped draws
                var rtv = packet.WorldOverlayTarget ? _worldOverlayBaseRenderTarget : oldRenderTarget;
                _context->OMSetRenderTargets(1, &rtv, _stencilView);
                _context->OMSetDepthStencilState(_stencilTestState, 1u);
            }

            // 0 = inherited/unknown, 1 = arena stencil test, 2 = stencil disabled.
            var boundDepthMode = packet.NeedsStencil ? 1 : 0;
            var segments = packet.Segments!;
            var count = packet.SegmentCount;
            for (var segmentIndex = 0; segmentIndex < count; ++segmentIndex)
            {
                var segment = segments[segmentIndex];
                switch (segment.Kind)
                {
                    case SegmentKind.CustomSdfFill:
                        if (!packet.NeedsStencil || !BindCustomSdf(packet, segment.CustomSdfBinding, ref boundCustomSdfBinding))
                        {
                            break;
                        }

                        // Arena clipping comes from the already-populated stencil, just like the ZoneRelPoly path
                        // The pixel shader evaluates the custom SDF and applies a centered one-pixel coverage ramp at its boundary
                        if (boundDepthMode != 1)
                        {
                            _context->OMSetDepthStencilState(_stencilTestState, 1u);
                            boundDepthMode = 1;
                        }
                        BindPipeline(vertexMesh, _customSdfFillPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->Draw((uint)segment.Count, (uint)segment.Start);
                        break;

                    case SegmentKind.AnalyticOutline:
                        if (packet.ArenaSdfView == null)
                        {
                            break;
                        }

                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexOutline, _outlineShapePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.AnalyticOutlineUnclipped:
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexOutline, _outlineUnclippedPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.ArenaSdfOutline:
                        if (packet.ArenaSdfView == null)
                        {
                            break;
                        }

                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexOutline, _arenaSdfOutlinePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.CustomSdfOutline:
                        if (packet.ArenaSdfView == null || !BindCustomSdf(packet, segment.CustomSdfBinding, ref boundCustomSdfBinding))
                        {
                            break;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexOutline, _customOutlinePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.AnalyticClipEdgeOverlay:
                        if (packet.ArenaSdfView == null)
                        {
                            break;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexOutline, _outlineClipEdgePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.CustomClipEdgeOverlay:
                        if (packet.ArenaSdfView == null || !BindCustomSdf(packet, segment.CustomSdfBinding, ref boundCustomSdfBinding))
                        {
                            break;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexOutline, _customClipEdgePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.Text:
                        if (fontAtlasSrv == null)
                        {
                            break;
                        }
                        if (boundSpriteBinding != -1)
                        {
                            var textView = fontAtlasSrv;
                            _context->PSSetShaderResources(3u, 1u, &textView);
                            boundSpriteBinding = -1;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexText, _textPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.WorldText:
                        if (fontAtlasSrv == null)
                        {
                            break;
                        }
                        if (boundSpriteBinding != -1)
                        {
                            var textView = fontAtlasSrv;
                            _context->PSSetShaderResources(3u, 1u, &textView);
                            boundSpriteBinding = -1;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexWorldText, _worldTextPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;

                    case SegmentKind.Sprite:
                        var binding = segment.SpriteBinding;
                        if ((uint)binding >= (uint)packet.SpriteCount || packet.Sprites == null)
                        {
                            break;
                        }
                        if (boundSpriteBinding != binding)
                        {
                            var spriteView = packet.Sprites[binding].View;
                            if (spriteView == null)
                                break;
                            _context->PSSetShaderResources(3u, 1u, &spriteView);
                            boundSpriteBinding = binding;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexText, _spritePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.ScreenAnalytic:
                        // Fixed-size screen decorations are intentionally not clipped by ArenaBounds
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexAnalytic, _analyticPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.WorldLine:
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexWorldLine, _strokePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.WorldCurve:
                        if (segment.CustomSdfBinding <= 0)
                        {
                            break;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexWorldCurve, _strokePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        var indicesPerCurve = (uint)segment.CustomSdfBinding * 6u;
                        _context->DrawIndexedInstanced(indicesPerCurve, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.WorldProjectedArrow:
                        // The arrow pixel shader samples scene depth to reconstruct the visible world surface;
                        // it is projected onto that surface rather than depth-occluded like line geometry
                        if (packet.SceneDepthView == null)
                        {
                            break;
                        }
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexWorldProjectedArrow, _worldProjectedArrowPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.WorldProjectedShape:
                        if (packet.SceneDepthView == null)
                        {
                            break;
                        }
                        BindWorldProjectedSdfs(packet, segment.CustomSdfBinding, segment.SpriteBinding, ref boundWorldShapeSdfBinding, ref boundWorldArenaSdfBinding);
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexWorldProjectedShape, _worldProjectedShapePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.Stroke:
                        // Polylines are intentionally unclipped, matching the existing primitive path
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexStroke, _strokePixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                    case SegmentKind.PrimitiveTriangleStroke:
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexPrimitiveTriangleStroke, _meshPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawInstanced(18u, (uint)segment.Count, 0u, (uint)segment.Start);
                        break;
                    case SegmentKind.PrimitiveMesh:
                        // General-purpose primitives are intentionally not clipped to ArenaBounds
                        // Keep the ImGui window scissor, but bypass our private arena stencil
                        if (boundDepthMode != 2)
                        {
                            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);
                            boundDepthMode = 2;
                        }
                        BindPipeline(vertexMesh, _meshPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->Draw((uint)segment.Count, (uint)segment.Start);
                        break;
                    case SegmentKind.Mesh:
                        if (packet.NeedsStencil && boundDepthMode != 1)
                        {
                            _context->OMSetDepthStencilState(_stencilTestState, 1u);
                            boundDepthMode = 1;
                        }
                        BindPipeline(vertexMesh, _meshPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->Draw((uint)segment.Count, (uint)segment.Start);
                        break;
                    case SegmentKind.Analytic:
                        if (packet.NeedsStencil && boundDepthMode != 1)
                        {
                            _context->OMSetDepthStencilState(_stencilTestState, 1u);
                            boundDepthMode = 1;
                        }
                        BindPipeline(vertexAnalytic, _analyticPixelShader, ref boundVertexFamily, ref boundPixelShader);
                        _context->DrawIndexedInstanced(6u, (uint)segment.Count, 0u, 0, (uint)segment.Start);
                        break;
                }
            }
        }
        finally
        {
            if (renderTargetCaptured)
            {
                if (oldRenderTarget != null)
                {
                    var restoreRtv = oldRenderTarget;
                    _context->OMSetRenderTargets(1u, &restoreRtv, oldDepthStencilView);
                }
                else
                {
                    _context->OMSetRenderTargets(0u, null, oldDepthStencilView);
                }
            }

            if (depthStateCaptured)
            {
                _context->OMSetDepthStencilState(oldDepthStencilState, oldStencilRef);
            }

            if (worldLineVsConstantBuffersCaptured)
            {
                worldLineBufferScratch[0] = oldWorldLineVsConstantBuffers0;
                worldLineBufferScratch[1] = oldWorldLineVsConstantBuffers1;
                _context->VSSetConstantBuffers(1u, worldLineVsConstantBufferCount, worldLineBufferScratch);
            }
            if (worldLinePsConstantBufferCaptured)
            {
                _context->PSSetConstantBuffers(4u, 1u, &oldWorldLinePsConstantBuffer);
            }
            if (worldProjectedSdfPsConstantBufferCaptured)
            {
                _context->PSSetConstantBuffers(5u, 1u, &oldWorldProjectedSdfPsConstantBuffer);
            }
            if (zoneWavePsConstantBufferCaptured)
            {
                _context->PSSetConstantBuffers(7u, 1u, &oldZoneWavePsConstantBuffer);
            }

            if (indexStateCaptured)
            {
                _context->IASetIndexBuffer(oldIndexBuffer, oldIndexFormat, oldIndexOffset);
            }

            if (coreStateCaptured)
            {
                _context->IASetVertexBuffers(0u, 1u, &oldVertexBuffer, &oldStride, &oldOffset);
                _context->IASetInputLayout(oldInputLayout);
                _context->VSSetShader(oldVertexShader, null, 0u);
                _context->PSSetShader(oldPixelShader, null, 0u);
            }

            if (auxSrvStateCaptured)
            {
                _context->PSSetShaderResources(capturedSrvFirstSlot, capturedSrvCount, oldPsSrvs + (int)capturedSrvFirstSlot);
            }
            if (auxSamplerStateCaptured)
            {
                _context->PSSetSamplers(capturedSamplerFirstSlot, capturedSamplerCount, oldAuxSamplers + (int)capturedSamplerFirstSlot - 1);
            }
            if (auxConstantBufferStateCaptured)
            {
                _context->PSSetConstantBuffers(capturedConstantBufferFirstSlot, capturedConstantBufferCount, oldAuxConstantBuffers + (int)capturedConstantBufferFirstSlot - 1);
            }

            // No scissor restore is needed: the DX11 ImGui backend applies a fresh scissor to every
            // following normal draw command, while custom callbacks are responsible for their own

            if (oldIndexBuffer != null)
            {
                oldIndexBuffer->Release();
            }
            if (oldVertexBuffer != null)
            {
                oldVertexBuffer->Release();
            }
            if (oldInputLayout != null)
            {
                oldInputLayout->Release();
            }
            if (oldVertexShader != null)
            {
                oldVertexShader->Release();
            }
            if (oldPixelShader != null)
            {
                oldPixelShader->Release();
            }
            if (auxSrvStateCaptured)
            {
                var first = (int)capturedSrvFirstSlot;
                var end = first + (int)capturedSrvCount;
                for (var i = first; i < end; ++i)
                {
                    if (oldPsSrvs[i] != null)
                    {
                        oldPsSrvs[i]->Release();
                    }
                }
            }
            if (auxSamplerStateCaptured)
            {
                var first = (int)capturedSamplerFirstSlot - 1;
                var end = first + (int)capturedSamplerCount;
                for (var i = first; i < end; ++i)
                {
                    if (oldAuxSamplers[i] != null)
                    {
                        oldAuxSamplers[i]->Release();
                    }
                }
            }
            if (auxConstantBufferStateCaptured)
            {
                var first = (int)capturedConstantBufferFirstSlot - 1;
                var end = first + (int)capturedConstantBufferCount;
                for (var i = first; i < end; ++i)
                {
                    if (oldAuxConstantBuffers[i] != null)
                    {
                        oldAuxConstantBuffers[i]->Release();
                    }
                }
            }
            if (oldWorldLineVsConstantBuffers0 != null)
            {
                oldWorldLineVsConstantBuffers0->Release();
            }
            if (oldWorldLineVsConstantBuffers1 != null)
            {
                oldWorldLineVsConstantBuffers1->Release();
            }
            if (oldWorldLinePsConstantBuffer != null)
            {
                oldWorldLinePsConstantBuffer->Release();
            }
            if (oldWorldProjectedSdfPsConstantBuffer != null)
            {
                oldWorldProjectedSdfPsConstantBuffer->Release();
            }
            if (oldZoneWavePsConstantBuffer != null)
            {
                oldZoneWavePsConstantBuffer->Release();
            }
            if (oldDepthStencilState != null)
            {
                oldDepthStencilState->Release();
            }
            if (oldRenderTarget != null)
            {
                oldRenderTarget->Release();
            }
            if (oldDepthStencilView != null)
            {
                oldDepthStencilView->Release();
            }

            if (packet != null)
            {
                ReturnPacketArrays(packet);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SameWorldLineConstants(ref WorldLineConstants a, ref WorldLineConstants b)
        => a.ViewProj.Equals(b.ViewProj) && a.InvViewProj.Equals(b.InvViewProj) && a.NearPlane == b.NearPlane && a.Viewport == b.Viewport && a.RasterScale == b.RasterScale
            && a.SceneDepthParams == b.SceneDepthParams && a.SceneInfoParams == b.SceneInfoParams;

    private static void UploadWorldLineConstants(ref WorldLineConstants constants)
    {
        if (_uploadedWorldLineConstantsValid && SameWorldLineConstants(ref _lastUploadedWorldLineConstants, ref constants))
        {
            return;
        }

        D3D11_MAPPED_SUBRESOURCE mapped = default;
        _context->Map((ID3D11Resource*)_worldLineConstantBuffer, 0u, (D3D11_MAP)4, 0u, &mapped);
        try
        {
            *(WorldLineConstants*)mapped.pData = constants;
            _lastUploadedWorldLineConstants = constants;
            _uploadedWorldLineConstantsValid = true;
        }
        finally
        {
            _context->Unmap((ID3D11Resource*)_worldLineConstantBuffer, 0u);
        }
    }

    private static void UploadWorldLineTransforms(WorldLineTransform[] transforms, int count)
    {
        D3D11_MAPPED_SUBRESOURCE mapped = default;
        _context->Map((ID3D11Resource*)_worldLineTransformBuffer, 0u, (D3D11_MAP)4, 0u, &mapped);
        try
        {
            fixed (WorldLineTransform* source = transforms)
            {
                var bytes = (nuint)(count * sizeof(WorldLineTransform));
                Buffer.MemoryCopy(source, mapped.pData, (nuint)(MaxWorldLineTransforms * sizeof(WorldLineTransform)), bytes);
            }
        }
        finally
        {
            _context->Unmap((ID3D11Resource*)_worldLineTransformBuffer, 0u);
        }
    }

    private static void BindVertexFamily(byte family)
    {
        var buffer = _uploadVertexBuffer;
        switch (family)
        {
            case 1: // mesh
                {
                    var stride = (uint)sizeof(MeshVertex);
                    var offset = _uploadMeshOffsetBytes;
                    _context->IASetInputLayout(_meshInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_meshVertexShader, null, 0u);
                    break;
                }
            case 8: // compact primitive triangle stroke; layout is intentionally analytic-compatible
                {
                    var stride = (uint)sizeof(PrimitiveTriangleStrokeInstance);
                    var offset = _uploadPrimitiveTriangleStrokeOffsetBytes;
                    _context->IASetInputLayout(_primitiveTriangleStrokeInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_primitiveTriangleStrokeVertexShader, null, 0u);
                    break;
                }
            case 2: // stroke
                {
                    var stride = (uint)sizeof(StrokeInstance);
                    var offset = _uploadStrokeOffsetBytes;
                    _context->IASetInputLayout(_strokeInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_strokeVertexShader, null, 0u);
                    break;
                }
            case 6: // world line instance
                {
                    var stride = (uint)sizeof(WorldLineInstance);
                    var offset = _uploadWorldLineOffsetBytes;
                    _context->IASetInputLayout(_worldLineInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_worldLineVertexShader, null, 0u);
                    break;
                }
            case 7: // procedural world curve instance
                {
                    var stride = (uint)sizeof(WorldCurveInstance);
                    var offset = _uploadWorldCurveOffsetBytes;
                    _context->IASetInputLayout(_worldCurveInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_worldCurveVertexShader, null, 0u);
                    break;
                }
            case 9: // terrain-projected world arrow
                {
                    var stride = (uint)sizeof(WorldProjectedArrowInstance);
                    var offset = _uploadWorldProjectedArrowOffsetBytes;
                    _context->IASetInputLayout(_worldProjectedArrowInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_worldProjectedArrowVertexShader, null, 0u);
                    break;
                }
            case 10: // terrain-projected MiniArena shape
                {
                    var stride = (uint)sizeof(WorldProjectedShapeInstance);
                    var offset = _uploadWorldProjectedShapeOffsetBytes;
                    _context->IASetInputLayout(_worldProjectedShapeInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_worldProjectedShapeVertexShader, null, 0u);
                    break;
                }
            case 11: // camera-facing world text/icon billboard
                {
                    var stride = (uint)sizeof(WorldTextInstance);
                    var offset = _uploadWorldTextOffsetBytes;
                    _context->IASetInputLayout(_worldTextInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_worldTextVertexShader, null, 0u);
                    break;
                }
            case 3: // analytic
                {
                    var stride = (uint)sizeof(AnalyticInstance);
                    var offset = _uploadAnalyticOffsetBytes;
                    _context->IASetInputLayout(_analyticInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_analyticVertexShader, null, 0u);
                    break;
                }
            case 4: // outline
                {
                    var stride = (uint)sizeof(OutlineInstance);
                    var offset = _uploadOutlineOffsetBytes;
                    _context->IASetInputLayout(_outlineShapeInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_outlineShapeVertexShader, null, 0u);
                    break;
                }
            case 5: // text
                {
                    var stride = (uint)sizeof(TextInstance);
                    var offset = _uploadTextOffsetBytes;
                    _context->IASetInputLayout(_textInputLayout);
                    _context->IASetVertexBuffers(0u, 1u, &buffer, &stride, &offset);
                    _context->VSSetShader(_textVertexShader, null, 0u);
                    break;
                }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool BindCustomSdf(BatchPacket packet, int bindingIndex, ref int boundBindingIndex)
    {
        if ((uint)bindingIndex >= (uint)packet.CustomSdfCount || packet.CustomSdfs == null)
        {
            return false;
        }
        if (boundBindingIndex == bindingIndex)
        {
            return true;
        }

        ref var binding = ref packet.CustomSdfs[bindingIndex];
        if (binding.View == null)
        {
            return false;
        }

        UploadCustomSdfConstants(binding.Constants);
        var view = binding.View;
        _context->PSSetShaderResources(2u, 1u, &view);
        boundBindingIndex = bindingIndex;
        return true;
    }

    private static void BindWorldProjectedSdfs(BatchPacket packet, int shapeBindingIndex, int arenaBindingIndex,
        ref int boundShapeBindingIndex, ref int boundArenaBindingIndex)
    {
        if (shapeBindingIndex == boundShapeBindingIndex && arenaBindingIndex == boundArenaBindingIndex)
        {
            return;
        }

        var constants = default(WorldProjectedSdfConstants);
        var views = stackalloc ID3D11ShaderResourceView*[2];
        views[0] = null;
        views[1] = null;
        if (packet.WorldProjectedSdfs != null)
        {
            if ((uint)shapeBindingIndex < (uint)packet.WorldProjectedSdfCount)
            {
                ref var binding = ref packet.WorldProjectedSdfs[shapeBindingIndex];
                views[0] = binding.View;
                constants.ShapeSdfMap = binding.Map;
                constants.Flags.X = binding.View != null ? 1f : 0f;
            }
            if ((uint)arenaBindingIndex < (uint)packet.WorldProjectedSdfCount)
            {
                ref var binding = ref packet.WorldProjectedSdfs[arenaBindingIndex];
                views[1] = binding.View;
                constants.ArenaSdfMap = binding.Map;
                constants.Flags.Y = binding.View != null ? 1f : 0f;
            }
        }

        UploadWorldProjectedSdfConstants(constants);
        // A packet with no projected SDF bindings must not touch t6/t7: those slots were not captured,
        // and the zero flags already guarantee the shader never samples them. This also removes one
        // driver call from the common analytic projected-shape path.
        if (packet.WorldProjectedSdfCount != 0)
        {
            _context->PSSetShaderResources(6u, 2u, views);
        }
        boundShapeBindingIndex = shapeBindingIndex;
        boundArenaBindingIndex = arenaBindingIndex;
    }

    private static void BindPipeline(byte vertexFamily, ID3D11PixelShader* pixelShader, ref byte boundVertexFamily, ref ID3D11PixelShader* boundPixelShader)
    {
        if (boundVertexFamily != vertexFamily)
        {
            BindVertexFamily(vertexFamily);
            boundVertexFamily = vertexFamily;
        }
        if (boundPixelShader != pixelShader)
        {
            _context->PSSetShader(pixelShader, null, 0u);
            boundPixelShader = pixelShader;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int AlignUploadOffset(int value) => (value + 15) & ~15;

    private static void PreparePacketUploadLayout(BatchPacket packet)
    {
        var offset = 0;

        packet.MeshOffsetBytes = (uint)offset;
        offset += packet.MeshVertexCount * sizeof(MeshVertex);

        offset = AlignUploadOffset(offset);
        packet.PrimitiveTriangleStrokeOffsetBytes = (uint)offset;
        offset += packet.PrimitiveTriangleStrokeCount * sizeof(PrimitiveTriangleStrokeInstance);

        offset = AlignUploadOffset(offset);
        packet.StrokeOffsetBytes = (uint)offset;
        offset += packet.StrokeInstanceCount * sizeof(StrokeInstance);

        offset = AlignUploadOffset(offset);
        packet.WorldLineOffsetBytes = (uint)offset;
        offset += packet.WorldLineCount * sizeof(WorldLineInstance);

        offset = AlignUploadOffset(offset);
        packet.WorldCurveOffsetBytes = (uint)offset;
        offset += packet.WorldCurveCount * sizeof(WorldCurveInstance);

        offset = AlignUploadOffset(offset);
        packet.WorldProjectedArrowOffsetBytes = (uint)offset;
        offset += packet.WorldProjectedArrowCount * sizeof(WorldProjectedArrowInstance);

        offset = AlignUploadOffset(offset);
        packet.WorldProjectedShapeOffsetBytes = (uint)offset;
        offset += packet.WorldProjectedShapeCount * sizeof(WorldProjectedShapeInstance);

        offset = AlignUploadOffset(offset);
        packet.AnalyticOffsetBytes = (uint)offset;
        offset += packet.AnalyticCount * sizeof(AnalyticInstance);

        offset = AlignUploadOffset(offset);
        packet.OutlineOffsetBytes = (uint)offset;
        offset += packet.OutlineCount * sizeof(OutlineInstance);

        offset = AlignUploadOffset(offset);
        packet.TextOffsetBytes = (uint)offset;
        offset += packet.TextInstanceCount * sizeof(TextInstance);

        offset = AlignUploadOffset(offset);
        packet.WorldTextOffsetBytes = (uint)offset;
        offset += packet.WorldTextInstanceCount * sizeof(WorldTextInstance);

        packet.UploadBytes = offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ApplyPacketUploadOffsets(BatchPacket packet, int baseOffset)
    {
        var add = (uint)baseOffset;
        _uploadMeshOffsetBytes = packet.MeshOffsetBytes + add;
        _uploadPrimitiveTriangleStrokeOffsetBytes = packet.PrimitiveTriangleStrokeOffsetBytes + add;
        _uploadStrokeOffsetBytes = packet.StrokeOffsetBytes + add;
        _uploadWorldLineOffsetBytes = packet.WorldLineOffsetBytes + add;
        _uploadWorldCurveOffsetBytes = packet.WorldCurveOffsetBytes + add;
        _uploadWorldProjectedArrowOffsetBytes = packet.WorldProjectedArrowOffsetBytes + add;
        _uploadWorldProjectedShapeOffsetBytes = packet.WorldProjectedShapeOffsetBytes + add;
        _uploadAnalyticOffsetBytes = packet.AnalyticOffsetBytes + add;
        _uploadOutlineOffsetBytes = packet.OutlineOffsetBytes + add;
        _uploadTextOffsetBytes = packet.TextOffsetBytes + add;
        _uploadWorldTextOffsetBytes = packet.WorldTextOffsetBytes + add;
    }

    private static void UploadPacket(BatchPacket packet, int totalBytes)
    {
        // Start each render frame with DISCARD, then append later packets with NO_OVERWRITE while
        // capacity permits. This avoids forcing a resource rename/discard for every custom-SDF packet.
        var baseOffset = 0;
        var mapType = (D3D11_MAP)4; // WRITE_DISCARD
        if (!packet.IsDeferredOverlay && _uploadRenderFrame == packet.SubmitFrame)
        {
            var candidate = AlignUploadOffset(_uploadCursorBytes);
            if (candidate <= _uploadVertexCapacityBytes - totalBytes)
            {
                baseOffset = candidate;
                mapType = (D3D11_MAP)5; // WRITE_NO_OVERWRITE
            }
        }
        else
        {
            // A deferred clipping-edge overlay deliberately DISCARDs even within the same frame.
            // D3D11 resource renaming keeps preceding border draws on the old backing allocation,
            // while this final replay gets isolated storage and therefore cannot alias their data.
            _uploadRenderFrame = packet.SubmitFrame;
        }

        ApplyPacketUploadOffsets(packet, baseOffset);

        D3D11_MAPPED_SUBRESOURCE mapped = default;
        _context->Map((ID3D11Resource*)_uploadVertexBuffer, 0, mapType, 0, &mapped);
        try
        {
            var basePtr = (byte*)mapped.pData;

            if (packet.MeshVertexCount != 0)
            {
                var sourceArray = packet.MeshVertices;
                var bytes = (nuint)(packet.MeshVertexCount * sizeof(MeshVertex));
                fixed (MeshVertex* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadMeshOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadMeshOffsetBytes), bytes);
            }

            if (packet.PrimitiveTriangleStrokeCount != 0)
            {
                var sourceArray = packet.PrimitiveTriangleStrokes;
                var bytes = (nuint)(packet.PrimitiveTriangleStrokeCount * sizeof(PrimitiveTriangleStrokeInstance));
                fixed (PrimitiveTriangleStrokeInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadPrimitiveTriangleStrokeOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadPrimitiveTriangleStrokeOffsetBytes), bytes);
            }

            if (packet.StrokeInstanceCount != 0)
            {
                var sourceArray = packet.StrokeInstances;
                var bytes = (nuint)(packet.StrokeInstanceCount * sizeof(StrokeInstance));
                fixed (StrokeInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadStrokeOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadStrokeOffsetBytes), bytes);
            }

            if (packet.WorldLineCount != 0)
            {
                var sourceArray = packet.WorldLines;
                var bytes = (nuint)(packet.WorldLineCount * sizeof(WorldLineInstance));
                fixed (WorldLineInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadWorldLineOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadWorldLineOffsetBytes), bytes);
            }

            if (packet.WorldCurveCount != 0)
            {
                var sourceArray = packet.WorldCurves;
                var bytes = (nuint)(packet.WorldCurveCount * sizeof(WorldCurveInstance));
                fixed (WorldCurveInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadWorldCurveOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadWorldCurveOffsetBytes), bytes);
            }

            if (packet.WorldProjectedArrowCount != 0)
            {
                var sourceArray = packet.WorldProjectedArrows;
                var bytes = (nuint)(packet.WorldProjectedArrowCount * sizeof(WorldProjectedArrowInstance));
                fixed (WorldProjectedArrowInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadWorldProjectedArrowOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadWorldProjectedArrowOffsetBytes), bytes);
            }

            if (packet.WorldProjectedShapeCount != 0)
            {
                var sourceArray = packet.WorldProjectedShapes;
                var bytes = (nuint)(packet.WorldProjectedShapeCount * sizeof(WorldProjectedShapeInstance));
                fixed (WorldProjectedShapeInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadWorldProjectedShapeOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadWorldProjectedShapeOffsetBytes), bytes);
            }

            if (packet.AnalyticCount != 0)
            {
                var sourceArray = packet.Analytics;
                var bytes = (nuint)(packet.AnalyticCount * sizeof(AnalyticInstance));
                fixed (AnalyticInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadAnalyticOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadAnalyticOffsetBytes), bytes);
            }

            if (packet.OutlineCount != 0)
            {
                var sourceArray = packet.Outlines;
                var bytes = (nuint)(packet.OutlineCount * sizeof(OutlineInstance));
                fixed (OutlineInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadOutlineOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadOutlineOffsetBytes), bytes);
            }

            if (packet.TextInstanceCount != 0)
            {
                var sourceArray = packet.TextInstances;
                var bytes = (nuint)(packet.TextInstanceCount * sizeof(TextInstance));
                fixed (TextInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadTextOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadTextOffsetBytes), bytes);
            }

            if (packet.WorldTextInstanceCount != 0)
            {
                var sourceArray = packet.WorldTextInstances;
                var bytes = (nuint)(packet.WorldTextInstanceCount * sizeof(WorldTextInstance));
                fixed (WorldTextInstance* source = sourceArray)
                    Buffer.MemoryCopy(source, basePtr + (int)_uploadWorldTextOffsetBytes, (nuint)(_uploadVertexCapacityBytes - (int)_uploadWorldTextOffsetBytes), bytes);
            }
        }
        finally
        {
            _context->Unmap((ID3D11Resource*)_uploadVertexBuffer, 0u);
            _uploadCursorBytes = AlignUploadOffset(baseOffset + totalBytes);
        }
    }

    private static bool EnsureUploadVertexBuffer(int requiredBytes)
    {
        if (_uploadVertexBuffer != null && requiredBytes <= _uploadVertexCapacityBytes)
        {
            return true;
        }

        Release(ref _uploadVertexBuffer);
        _uploadRenderFrame = -1;
        _uploadCursorBytes = 0;
        var capacity = Math.Max(128 * 1024, _uploadVertexCapacityBytes);
        while (capacity < requiredBytes)
        {
            capacity *= 2;
        }

        if (!CreateDynamicVertexBuffer(capacity, out _uploadVertexBuffer))
        {
            _uploadVertexCapacityBytes = 0;
            return false;
        }

        _uploadVertexCapacityBytes = capacity;
        return true;
    }

    private static bool CreateDynamicVertexBuffer(int byteWidth, out ID3D11Buffer* buffer)
    {
        buffer = null;
        D3D11_BUFFER_DESC desc = default;
        desc.ByteWidth = (uint)byteWidth;
        desc.Usage = (D3D11_USAGE)2; // D3D11_USAGE_DYNAMIC
        desc.BindFlags = 0x1u; // D3D11_BIND_VERTEX_BUFFER
        desc.CPUAccessFlags = 0x10000u; // D3D11_CPU_ACCESS_WRITE

        ID3D11Buffer* created = null;
        var hr = _device->CreateBuffer(&desc, null, &created);
        buffer = created;
        return hr >= 0 && created != null;
    }

    private static bool EnsureStencilTarget(int width, int height)
    {
        if (_stencilView != null && _stencilWidth == width && _stencilHeight == height)
        {
            return true;
        }

        Release(ref _stencilView);
        _stencilWidth = 0;
        _stencilHeight = 0;
        _renderedStencilKey = 0L;

        D3D11_TEXTURE2D_DESC desc = default;
        desc.Width = (uint)width;
        desc.Height = (uint)height;
        desc.MipLevels = 1u;
        desc.ArraySize = 1u;
        desc.Format = (DXGI_FORMAT)45; // DXGI_FORMAT_D24_UNORM_S8_UINT
        desc.SampleDesc.Count = 1u;
        desc.SampleDesc.Quality = 0u;
        desc.Usage = 0; // D3D11_USAGE_DEFAULT
        desc.BindFlags = 0x40u; // D3D11_BIND_DEPTH_STENCIL

        ID3D11Texture2D* texture = null;
        _device->CreateTexture2D(&desc, null, &texture);

        ID3D11DepthStencilView* view = null;
        _device->CreateDepthStencilView((ID3D11Resource*)texture, null, &view);
        texture->Release(); // view owns its own resource reference

        _stencilView = view;
        _stencilWidth = width;
        _stencilHeight = height;
        return true;
    }

    private static byte[]? LoadFontResourceBytes(string fileName)
    {
        var assembly = typeof(Dx11ArenaRenderer).Assembly;
        var resourceName = $"BossMod.Fonts.Compiled.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        var len = stream?.Length ?? 0;
        if (stream == null || len <= 0 || len > int.MaxValue)
        {
            return null;
        }
        var bytes = GC.AllocateUninitializedArray<byte>((int)len);
        stream.ReadExactly(bytes);
        return bytes;
    }

    private static bool CreateArenaFontResources()
    {
        var rgbBottomUp = LoadFontResourceBytes("arena_font_msdf.rgb");
        var assembly = typeof(Dx11ArenaRenderer).Assembly;
        using var metadataStream = assembly.GetManifestResourceStream("BossMod.Fonts.Compiled.arena_font_msdf.bin");
        if (rgbBottomUp == null || metadataStream == null || metadataStream.Length <= 0 || metadataStream.Length > int.MaxValue)
        {
            return false;
        }

        try
        {
            if (!BitConverter.IsLittleEndian)
            {
                return false;
            }

            // Assembly manifest resources are normally exposed as UnmanagedMemoryStream. In that case
            // point a ReadOnlySpan directly at the embedded bytes, avoiding even the ~80 KiB metadata copy.
            // Fall back to one byte[] copy on runtimes that do not expose a pointer-backed resource stream.
            if (metadataStream is System.IO.UnmanagedMemoryStream unmanaged)
            {
                unmanaged.Position = 0;
                byte* resourcePointer = null;
                resourcePointer = unmanaged.PositionPointer;
                if (resourcePointer != null)
                {
                    var metadata = new ReadOnlySpan<byte>(resourcePointer, (int)unmanaged.Length);
                    return CreateArenaFontResourcesFromBinary(rgbBottomUp, metadata);
                }
                metadataStream.Position = 0;
            }

            var metadataBytes = GC.AllocateUninitializedArray<byte>((int)metadataStream.Length);
            metadataStream.ReadExactly(metadataBytes);
            return CreateArenaFontResourcesFromBinary(rgbBottomUp, metadataBytes);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool CreateArenaFontResourcesFromBinary(byte[] rgbBottomUp, ReadOnlySpan<byte> metadata)
    {
        var lenM = metadata.Length;
        if (sizeof(ArenaFontBinaryHeader) != ArenaFontBinaryHeaderSize || sizeof(ArenaFontBinaryVariant) != ArenaFontBinaryVariantSize ||
            sizeof(ArenaFontBinaryGlyph) != ArenaFontBinaryGlyphSize || sizeof(ArenaFontBinaryKerning) != ArenaFontBinaryKerningSize ||
            lenM < ArenaFontBinaryHeaderSize)
        {
            return false;
        }

        var headerRecords = MemoryMarshal.Cast<byte, ArenaFontBinaryHeader>(metadata[..ArenaFontBinaryHeaderSize]);
        if (headerRecords.Length != 1)
        {
            return false;
        }

        ref readonly var header = ref headerRecords[0];
        if (header.Magic != ArenaFontBinaryMagic || header.Version != ArenaFontBinaryVersion || header.AtlasType != ArenaFontBinaryAtlasTypeMsdf ||
            header.YOrigin != 0u || header.Width <= 0 || header.Height <= 0 || header.VariantCount is < 0 or > 64)
        {
            return false;
        }

        var variantBytes = header.VariantCount * ArenaFontBinaryVariantSize;
        var dataStart = ArenaFontBinaryHeaderSize + variantBytes;
        if (dataStart > lenM)
        {
            return false;
        }

        var variants = MemoryMarshal.Cast<byte, ArenaFontBinaryVariant>(metadata.Slice(ArenaFontBinaryHeaderSize, variantBytes));
        if (variants.Length != header.VariantCount)
        {
            return false;
        }

        // Validate the converter's tightly packed section layout before taking any further casts
        var expectedOffset = dataStart;
        var lenV = variants.Length;
        for (var i = 0; i < lenV; ++i)
        {
            ref readonly var variant = ref variants[i];
            var countG = variant.GlyphCount;
            var countK = variant.KerningCount;
            if (variant.Kind > ArenaFontVariantIcons || countG is < 0 or > 1_000_000 || countK is < 0 or > 10_000_000 || variant.GlyphOffset != expectedOffset)
            {
                return false;
            }

            expectedOffset += countG * ArenaFontBinaryGlyphSize;
            if (variant.KerningOffset != expectedOffset)
            {
                return false;
            }
            expectedOffset += countK * ArenaFontBinaryKerningSize;
            if (expectedOffset > lenM)
            {
                return false;
            }
        }
        if (expectedOffset != lenM)
        {
            return false;
        }

        var width = header.Width;
        var height = header.Height;

        // -format bin emits exactly the three MSDF channels as raw bytes. msdfgen's atlas bitmap
        // is bottom-up, while D3D11 texture memory/UV convention is top-down. The v2 metadata converter
        // has already normalized UVs; the texture bytes still need the same one-time row flip/alpha expansion.
        var rgbByteCount = width * height * 3;
        var rgbaByteCount = width * height * 4;
        if (rgbBottomUp.Length != rgbByteCount)
        {
            return false;
        }
        var rgbaTopDown = GC.AllocateUninitializedArray<byte>(rgbaByteCount);
        for (var dstY = 0; dstY < height; ++dstY)
        {
            var srcY = height - 1 - dstY;
            var src = srcY * width * 3;
            var dst = dstY * width * 4;
            for (var x = 0; x < width; ++x)
            {
                rgbaTopDown[dst++] = rgbBottomUp[src++];
                rgbaTopDown[dst++] = rgbBottomUp[src++];
                rgbaTopDown[dst++] = rgbBottomUp[src++];
                rgbaTopDown[dst++] = 0xFF;
            }
        }

        Dictionary<uint, ArenaFontGlyph>? textGlyphs = null;
        Dictionary<uint, ArenaFontGlyph>? iconGlyphs = null;
        Dictionary<ulong, float>? textKerning = null;
        ArenaFontMetrics textMetrics = default;
        ArenaFontMetrics iconMetrics = default;

        for (var variantIndex = 0; variantIndex < lenV; ++variantIndex)
        {
            ref readonly var variant = ref variants[variantIndex];
            var isText = variant.Kind == ArenaFontVariantText;
            var isIcons = variant.Kind == ArenaFontVariantIcons;
            if (!isText && !isIcons)
            {
                continue;
            }

            var countG = variant.GlyphCount;
            var metrics = new ArenaFontMetrics(variant.LineHeight, variant.Ascender, variant.Descender);
            var glyphBytes = countG * ArenaFontBinaryGlyphSize;
            var glyphRecords = MemoryMarshal.Cast<byte, ArenaFontBinaryGlyph>(metadata.Slice(variant.GlyphOffset, glyphBytes));
            var lenG = glyphRecords.Length;
            if (glyphRecords.Length != countG)
            {
                return false;
            }

            var glyphTable = new Dictionary<uint, ArenaFontGlyph>(countG);
            for (var i = 0; i < lenG; ++i)
            {
                ref readonly var glyph = ref glyphRecords[i];
                if ((glyph.Flags & ~ArenaFontGlyphHasQuad) != 0u)
                {
                    return false;
                }

                var hasQuad = (glyph.Flags & ArenaFontGlyphHasQuad) != 0u;
                glyphTable[glyph.Unicode] = hasQuad ? new ArenaFontGlyph(glyph.Advance, new Vector4(glyph.PlaneLeft, glyph.PlaneBottom, glyph.PlaneRight, glyph.PlaneTop),
                        new Vector4(glyph.U0, glyph.V0, glyph.U1, glyph.V1), hasQuad: true) : new ArenaFontGlyph(glyph.Advance, default, default, hasQuad: false);
            }

            var countK = variant.KerningCount;
            var variantKerning = isText ? new Dictionary<ulong, float>(countK) : null;
            if (countK != 0)
            {
                var kerningBytes = countK * ArenaFontBinaryKerningSize;
                var kerningRecords = MemoryMarshal.Cast<byte, ArenaFontBinaryKerning>(metadata.Slice(variant.KerningOffset, kerningBytes));
                var lenK = kerningRecords.Length;
                if (lenK != countK)
                {
                    return false;
                }

                if (variantKerning != null)
                {
                    for (var i = 0; i < lenK; ++i)
                    {
                        ref readonly var kerning = ref kerningRecords[i];
                        variantKerning[((ulong)kerning.Unicode1 << 32) | kerning.Unicode2] = kerning.Advance;
                    }
                }
            }

            if (isText)
            {
                textGlyphs = glyphTable;
                textMetrics = metrics;
                textKerning = variantKerning;
            }
            else
            {
                iconGlyphs = glyphTable;
                iconMetrics = metrics;
            }
        }

        if (textGlyphs == null || iconGlyphs == null || !textGlyphs.ContainsKey('?'))
        {
            return false;
        }

        D3D11_TEXTURE2D_DESC textureDesc = default;
        textureDesc.Width = (uint)width;
        textureDesc.Height = (uint)height;
        textureDesc.MipLevels = 1u;
        textureDesc.ArraySize = 1u;
        textureDesc.Format = (DXGI_FORMAT)28; // R8G8B8A8_UNORM -- MSDF values are linear, never sRGB
        textureDesc.SampleDesc.Count = 1u;
        textureDesc.Usage = 0; // D3D11_USAGE_DEFAULT
        textureDesc.BindFlags = 0x8u; // D3D11_BIND_SHADER_RESOURCE

        ID3D11Texture2D* texture = null;
        fixed (byte* pixels = rgbaTopDown)
        {
            D3D11_SUBRESOURCE_DATA initialData = default;
            initialData.pSysMem = pixels;
            initialData.SysMemPitch = (uint)(width * 4);
            _device->CreateTexture2D(&textureDesc, &initialData, &texture);
        }

        ID3D11ShaderResourceView* view = null;
        _device->CreateShaderResourceView((ID3D11Resource*)texture, null, &view);
        texture->Release();

        _arenaFontAtlasView = view;
        _arenaTextGlyphs = textGlyphs;
        _arenaIconGlyphs = iconGlyphs;
        _arenaTextKerning = textKerning;
        _arenaTextMetrics = textMetrics;
        _arenaIconMetrics = iconMetrics;
        return true;
    }

    private static bool CreateShadersAndLayouts()
    {
        if (!CreateVertexShaderResource("world_overlay_vs.cso", out _worldOverlayVertexShader, out _) ||
            !CreatePixelShaderResource("world_overlay_ps.cso", out _worldOverlayPixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("mesh_vs.cso", out _meshVertexShader, out var meshVsBytecode))
        {
            return false;
        }
        CreateMeshInputLayout(meshVsBytecode, out _meshInputLayout);
        if (_meshInputLayout == null)
        {
            return false;
        }
        if (!CreateVertexShaderResource("primitive_triangle_stroke_vs.cso", out _primitiveTriangleStrokeVertexShader, out var primitiveTriangleStrokeVsBytecode))
        {
            return false;
        }
        CreateAnalyticInputLayout(primitiveTriangleStrokeVsBytecode, out _primitiveTriangleStrokeInputLayout);
        if (_primitiveTriangleStrokeInputLayout == null)
        {
            return false;
        }
        if (!CreatePixelShaderResource("mesh_ps.cso", out _meshPixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("stroke_vs.cso", out _strokeVertexShader, out var strokeVsBytecode))
        {
            return false;
        }
        CreateStrokeInputLayout(strokeVsBytecode, out _strokeInputLayout);
        if (_strokeInputLayout == null)
        {
            return false;
        }
        if (!CreatePixelShaderResource("stroke_ps.cso", out _strokePixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("world_line_vs.cso", out _worldLineVertexShader, out var worldLineVsBytecode))
        {
            return false;
        }
        CreateWorldLineInputLayout(worldLineVsBytecode, out _worldLineInputLayout);
        if (_worldLineInputLayout == null)
        {
            return false;
        }
        if (!CreateVertexShaderResource("world_curve_vs.cso", out _worldCurveVertexShader, out var worldCurveVsBytecode))
        {
            return false;
        }
        CreateWorldCurveInputLayout(worldCurveVsBytecode, out _worldCurveInputLayout);
        if (_worldCurveInputLayout == null)
        {
            return false;
        }
        if (!CreateVertexShaderResource("projected_arrow_vs.cso", out _worldProjectedArrowVertexShader, out var projectedArrowVsBytecode))
        {
            return false;
        }
        CreateWorldProjectedArrowInputLayout(projectedArrowVsBytecode, out _worldProjectedArrowInputLayout);
        if (_worldProjectedArrowInputLayout == null || !CreatePixelShaderResource("projected_arrow_ps.cso", out _worldProjectedArrowPixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("projected_shape_vs.cso", out _worldProjectedShapeVertexShader, out var projectedShapeVsBytecode))
        {
            return false;
        }
        CreateWorldProjectedShapeInputLayout(projectedShapeVsBytecode, out _worldProjectedShapeInputLayout);
        if (_worldProjectedShapeInputLayout == null || !CreatePixelShaderResource("projected_shape_ps.cso", out _worldProjectedShapePixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("text_vs.cso", out _textVertexShader, out var textVsBytecode))
        {
            return false;
        }
        CreateTextInputLayout(textVsBytecode, out _textInputLayout);
        if (_textInputLayout == null)
        {
            return false;
        }
        if (!CreatePixelShaderResource("text_ps.cso", out _textPixelShader))
        {
            return false;
        }
        if (!CreatePixelShaderResource("sprite_ps.cso", out _spritePixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("world_text_vs.cso", out _worldTextVertexShader, out var worldTextVsBytecode))
        {
            return false;
        }
        CreateWorldTextInputLayout(worldTextVsBytecode, out _worldTextInputLayout);
        if (_worldTextInputLayout == null || !CreatePixelShaderResource("world_text_ps.cso", out _worldTextPixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("analytic_vs.cso", out _analyticVertexShader, out var analyticVsBytecode))
        {
            return false;
        }
        CreateAnalyticInputLayout(analyticVsBytecode, out _analyticInputLayout);
        if (_analyticInputLayout == null)
        {
            return false;
        }
        if (!CreatePixelShaderResource("analytic_ps.cso", out _analyticPixelShader))
        {
            return false;
        }
        if (!CreateVertexShaderResource("outline_shape_vs.cso", out _outlineShapeVertexShader, out var outlineShapeVsBytecode))
        {
            return false;
        }
        CreateOutlineShapeInputLayout(outlineShapeVsBytecode, out _outlineShapeInputLayout);
        if (_outlineShapeInputLayout == null)
        {
            return false;
        }

        return
            CreatePixelShaderResource("outline_shape_ps.cso", out _outlineShapePixelShader) &&
            CreatePixelShaderResource("outline_unclipped_ps.cso", out _outlineUnclippedPixelShader) &&
            CreatePixelShaderResource("outline_clip_edge_ps.cso", out _outlineClipEdgePixelShader) &&
            CreatePixelShaderResource("arena_sdf_outline_ps.cso", out _arenaSdfOutlinePixelShader) &&
            CreatePixelShaderResource("arena_sdf_stencil_ps.cso", out _arenaSdfStencilPixelShader) &&
            CreatePixelShaderResource("custom_sdf_fill_ps.cso", out _customSdfFillPixelShader) &&
            CreatePixelShaderResource("custom_outline_ps.cso", out _customOutlinePixelShader) &&
            CreatePixelShaderResource("custom_clip_edge_ps.cso", out _customClipEdgePixelShader);
    }

    private static byte[]? LoadShaderBytecode(string fileName)
    {
        var assembly = typeof(Dx11ArenaRenderer).Assembly;
        var resourceName = $"BossMod.Shaders.Compiled.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        var len = stream?.Length ?? 0;
        if (stream == null || len <= 0 || len > int.MaxValue)
        {
            return null;
        }
        var bytes = GC.AllocateUninitializedArray<byte>((int)len);
        stream.ReadExactly(bytes);
        return bytes;
    }

    private static bool CreateVertexShaderResource(string fileName, out ID3D11VertexShader* shader, out byte[] bytecode)
    {
        shader = null;
        bytecode = LoadShaderBytecode(fileName) ?? [];
        var len = bytecode.Length;
        if (len == 0)
        {
            return false;
        }

        fixed (byte* pBytecode = bytecode)
        {
            ID3D11VertexShader* created = null;
            _device->CreateVertexShader(pBytecode, (nuint)len, null, &created);
            shader = created;
            return true;
        }
    }

    private static bool CreatePixelShaderResource(string fileName, out ID3D11PixelShader* shader)
    {
        shader = null;
        var bytecode = LoadShaderBytecode(fileName);
        var len = bytecode?.Length ?? 0;
        if (bytecode != null && len == 0)
        {
            return false;
        }

        fixed (byte* pBytecode = bytecode)
        {
            ID3D11PixelShader* created = null;
            var hr = _device->CreatePixelShader(pBytecode, (nuint)len, null, &created);
            shader = created;
            return true;
        }
    }

    private static void CreateMeshInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");

        fixed (byte* pPosition = position)
        fixed (byte* pColor = color)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[3];
            elements[0] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pPosition,
                SemanticIndex = 0u,
                Format = (DXGI_FORMAT)16, // R32G32_FLOAT
                InputSlot = 0u,
                AlignedByteOffset = 0u,
                InputSlotClass = 0, // PER_VERTEX
                InstanceDataStepRate = 0u,
            };
            elements[1] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pColor,
                SemanticIndex = 0u,
                Format = (DXGI_FORMAT)28, // R8G8B8A8_UNORM
                InputSlot = 0u,
                AlignedByteOffset = 8u,
                InputSlotClass = 0,
                InstanceDataStepRate = 0u,
            };
            elements[2] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pTexcoord,
                SemanticIndex = 0u,
                Format = (DXGI_FORMAT)42, // R32_UINT
                InputSlot = 0u,
                AlignedByteOffset = 12u,
                InputSlotClass = 0,
                InstanceDataStepRate = 0u,
            };

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 3u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }
    }

    private static void CreateStrokeInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");

        fixed (byte* pPosition = position)
        fixed (byte* pColor = color)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[9];
            elements[0] = InstanceElement((sbyte*)pPosition, 0u, (DXGI_FORMAT)16, 0u);   // float2 prevNdc
            elements[1] = InstanceElement((sbyte*)pPosition, 1u, (DXGI_FORMAT)16, 8u);   // float2 aNdc
            elements[2] = InstanceElement((sbyte*)pPosition, 2u, (DXGI_FORMAT)16, 16u);  // float2 bNdc
            elements[3] = InstanceElement((sbyte*)pPosition, 3u, (DXGI_FORMAT)16, 24u);  // float2 nextNdc
            elements[4] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)16, 32u);  // float2 ndcToPx
            elements[5] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)16, 40u);  // float2 widthsPx
            elements[6] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 48u);     // RGBA8 foreground
            elements[7] = InstanceElement((sbyte*)pColor, 1u, (DXGI_FORMAT)28, 52u);     // RGBA8 shadow
            elements[8] = InstanceElement((sbyte*)pTexcoord, 3u, (DXGI_FORMAT)42, 56u);  // R32_UINT flags

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 9u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1, // PER_INSTANCE_DATA
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateWorldLineInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pPosition = position)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[5];
            elements[0] = InstanceElement((sbyte*)pPosition, 0u, (DXGI_FORMAT)6, 0u);
            elements[1] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)41, 12u);
            elements[2] = InstanceElement((sbyte*)pPosition, 1u, (DXGI_FORMAT)6, 16u);
            elements[3] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 28u);
            elements[4] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)42, 32u); // R32_UINT transform index

            ID3D11InputLayout* created = null;
            var hr = _device->CreateInputLayout(elements, 5u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateWorldCurveInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pPosition = position)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[7];
            elements[0] = InstanceElement((sbyte*)pPosition, 0u, (DXGI_FORMAT)6, 0u);   // float3 center
            elements[1] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)41, 12u); // float radius
            elements[2] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)2, 16u);  // float4 params
            elements[3] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 32u);    // RGBA8
            elements[4] = InstanceElement((sbyte*)pTexcoord, 2u, (DXGI_FORMAT)41, 36u); // float thickness
            elements[5] = InstanceElement((sbyte*)pTexcoord, 3u, (DXGI_FORMAT)42, 40u); // uint transform index
            elements[6] = InstanceElement((sbyte*)pTexcoord, 4u, (DXGI_FORMAT)42, 44u); // uint kind/segments

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 7u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateWorldProjectedArrowInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pPosition = position)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[6];
            elements[0] = InstanceElement((sbyte*)pPosition, 0u, (DXGI_FORMAT)6, 0u);   // float3 origin
            elements[1] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)41, 12u); // float length
            elements[2] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)16, 16u); // float2 directionXZ
            elements[3] = InstanceElement((sbyte*)pTexcoord, 2u, (DXGI_FORMAT)16, 24u); // float2 widths
            elements[4] = InstanceElement((sbyte*)pTexcoord, 3u, (DXGI_FORMAT)16, 32u); // float2 head/projection
            elements[5] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 40u);    // RGBA8

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 6u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateWorldProjectedShapeInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pPosition = position)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[11];
            elements[0] = InstanceElement((sbyte*)pPosition, 0u, (DXGI_FORMAT)6, 0u);    // float3 origin
            elements[1] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)41, 12u); // float projection height
            elements[2] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)2, 16u);  // float4 bounds XZ
            elements[3] = InstanceElement((sbyte*)pTexcoord, 2u, (DXGI_FORMAT)2, 32u);  // float4 direction/aux
            elements[4] = InstanceElement((sbyte*)pTexcoord, 3u, (DXGI_FORMAT)2, 48u);  // float4 params0
            elements[5] = InstanceElement((sbyte*)pTexcoord, 4u, (DXGI_FORMAT)2, 64u);  // float4 params1
            elements[6] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 80u);    // fill/primary RGBA8
            elements[7] = InstanceElement((sbyte*)pTexcoord, 5u, (DXGI_FORMAT)42, 84u); // uint packed kind/flags
            elements[8] = InstanceElement((sbyte*)pTexcoord, 6u, (DXGI_FORMAT)41, 88u); // float outline width
            elements[9] = InstanceElement((sbyte*)pColor, 1u, (DXGI_FORMAT)28, 92u);    // optional outline RGBA8
            elements[10] = InstanceElement((sbyte*)pTexcoord, 7u, (DXGI_FORMAT)16, 96u); // float2 wave origin XZ

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 11u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateTextInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[5];
            elements[0] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pTexcoord,
                SemanticIndex = 0u,
                Format = (DXGI_FORMAT)2, // R32G32B32A32_FLOAT
                InputSlot = 0u,
                AlignedByteOffset = 0u,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
            elements[1] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pTexcoord,
                SemanticIndex = 1u,
                Format = (DXGI_FORMAT)2, // R32G32B32A32_FLOAT
                InputSlot = 0u,
                AlignedByteOffset = 16u,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
            elements[2] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pColor,
                SemanticIndex = 0u,
                Format = (DXGI_FORMAT)28, // R8G8B8A8_UNORM
                InputSlot = 0u,
                AlignedByteOffset = 32u,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
            elements[3] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pColor,
                SemanticIndex = 1u,
                Format = (DXGI_FORMAT)28, // R8G8B8A8_UNORM
                InputSlot = 0u,
                AlignedByteOffset = 36u,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
            elements[4] = new D3D11_INPUT_ELEMENT_DESC
            {
                SemanticName = (sbyte*)pTexcoord,
                SemanticIndex = 2u,
                Format = (DXGI_FORMAT)41, // R32_FLOAT
                InputSlot = 0u,
                AlignedByteOffset = 40u,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 5u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }
    }

    private static void CreateWorldTextInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var position = Encoding.ASCII.GetBytes("POSITION\0");
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pPosition = position)
        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[6];
            elements[0] = InstanceElement((sbyte*)pPosition, 0u, (DXGI_FORMAT)6, 0u);   // float3 center
            elements[1] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)2, 16u); // float4 rect framebuffer px
            elements[2] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)2, 32u); // float4 uv rect
            elements[3] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 48u);   // RGBA8
            elements[4] = InstanceElement((sbyte*)pColor, 1u, (DXGI_FORMAT)28, 52u);   // RGBA8
            elements[5] = InstanceElement((sbyte*)pTexcoord, 2u, (DXGI_FORMAT)41, 56u); // outline px

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 6u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1,
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateAnalyticInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var texcoord = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var color = Encoding.ASCII.GetBytes("COLOR\0");

        fixed (byte* pTexcoord = texcoord)
        fixed (byte* pColor = color)
        fixed (byte* pBytecode = vsBytecode)
        {
            var elements = stackalloc D3D11_INPUT_ELEMENT_DESC[6];
            elements[0] = InstanceElement((sbyte*)pTexcoord, 0u, (DXGI_FORMAT)16, 0u);   // centerNdc float2
            elements[1] = InstanceElement((sbyte*)pTexcoord, 1u, (DXGI_FORMAT)16, 8u);   // extentNdc float2
            elements[2] = InstanceElement((sbyte*)pTexcoord, 2u, (DXGI_FORMAT)16, 16u);  // extentPx float2
            elements[3] = InstanceElement((sbyte*)pTexcoord, 3u, (DXGI_FORMAT)16, 24u);  // direction float2
            elements[4] = InstanceElement((sbyte*)pTexcoord, 4u, (DXGI_FORMAT)2, 32u);   // params float4
            elements[5] = InstanceElement((sbyte*)pColor, 0u, (DXGI_FORMAT)28, 48u);     // color RGBA8

            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(elements, 6u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }

        static D3D11_INPUT_ELEMENT_DESC InstanceElement(sbyte* semantic, uint semanticIndex, DXGI_FORMAT format, uint offset)
            => new()
            {
                SemanticName = semantic,
                SemanticIndex = semanticIndex,
                Format = format,
                InputSlot = 0u,
                AlignedByteOffset = offset,
                InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1, // PER_INSTANCE_DATA
                InstanceDataStepRate = 1u,
            };
    }

    private static void CreateOutlineShapeInputLayout(ReadOnlySpan<byte> vsBytecode, out ID3D11InputLayout* layout)
    {
        layout = null;
        var tex = Encoding.ASCII.GetBytes("TEXCOORD\0");
        var col = Encoding.ASCII.GetBytes("COLOR\0");
        fixed (byte* pTex = tex)
        fixed (byte* pCol = col)
        fixed (byte* pBytecode = vsBytecode)
        {
            var e = stackalloc D3D11_INPUT_ELEMENT_DESC[9];
            e[0] = Inst((sbyte*)pTex, 0u, (DXGI_FORMAT)16, 0u, 0u);
            e[1] = Inst((sbyte*)pTex, 1u, (DXGI_FORMAT)16, 8u, 0u);
            e[2] = Inst((sbyte*)pTex, 2u, (DXGI_FORMAT)16, 16u, 0u);
            e[3] = Inst((sbyte*)pTex, 3u, (DXGI_FORMAT)16, 24u, 0u);
            e[4] = Inst((sbyte*)pTex, 4u, (DXGI_FORMAT)2, 32u, 0u);
            e[5] = Inst((sbyte*)pTex, 7u, (DXGI_FORMAT)16, 48u, 0u); // extra
            e[6] = Inst((sbyte*)pTex, 6u, (DXGI_FORMAT)16, 56u, 0u); // widths
            e[7] = Inst((sbyte*)pCol, 0u, (DXGI_FORMAT)28, 64u, 0u);
            e[8] = Inst((sbyte*)pCol, 1u, (DXGI_FORMAT)28, 68u, 0u);
            ID3D11InputLayout* created = null;
            _device->CreateInputLayout(e, 9u, pBytecode, (nuint)vsBytecode.Length, &created);
            layout = created;
        }
    }

    private static D3D11_INPUT_ELEMENT_DESC Inst(sbyte* semantic, uint index, DXGI_FORMAT format, uint offset, uint slot)
        => new() { SemanticName = semantic, SemanticIndex = index, Format = format, InputSlot = slot, AlignedByteOffset = offset, InputSlotClass = (D3D11_INPUT_CLASSIFICATION)1, InstanceDataStepRate = 1 };

    private static bool EnsureWorldOverlayResources(int width, int height)
    {
        var node = _worldOverlayNode;
        if (node == null || !node.IsAttached || width <= 0 || height <= 0)
        {
            return false;
        }
        if (_worldOverlayBaseTexture != null && _worldOverlayBaseRenderTarget != null && _worldOverlayBaseView != null &&
            _worldOverlayOutputTexture != null && _worldOverlayOutputRenderTarget != null && _worldOverlayOutputView != null &&
            _worldOverlayWidth == width && _worldOverlayHeight == height)
        {
            return true;
        }

        if (!CreateWorldOverlayTexture(width, height, out var baseTexture, out var baseRenderTarget, out var baseView))
        {
            node.ReleaseTexture();
            ReleaseWorldOverlayResources();
            return false;
        }

        var outputWrapper = node.CreateTexture(width, height);
        var outputTexture = outputWrapper != null ? (ID3D11Texture2D*)outputWrapper->D3D11Texture2D : null;
        var outputView = outputWrapper != null ? (ID3D11ShaderResourceView*)outputWrapper->D3D11ShaderResourceView : null;
        if (outputWrapper == null || outputTexture == null || outputView == null)
        {
            Release(ref baseView);
            Release(ref baseRenderTarget);
            Release(ref baseTexture);
            if (outputWrapper != null)
            {
                outputWrapper->DecRef();
            }
            node.ReleaseTexture();
            ReleaseWorldOverlayResources();
            return false;
        }

        // Hold renderer-side references independently from the native Texture wrapper owned by the node
        // This makes resize and plugin teardown safe regardless of which owner releases first.
        outputTexture->AddRef();
        outputView->AddRef();
        ID3D11RenderTargetView* outputRenderTarget = null;
        if (_device->CreateRenderTargetView((ID3D11Resource*)outputTexture, null, &outputRenderTarget) < 0 || outputRenderTarget == null)
        {
            outputView->Release();
            outputTexture->Release();
            outputWrapper->DecRef();
            Release(ref baseView);
            Release(ref baseRenderTarget);
            Release(ref baseTexture);
            node.ReleaseTexture();
            ReleaseWorldOverlayResources();
            return false;
        }

        node.SetTexture(outputWrapper, width, height);
        ReleaseWorldOverlayResources();
        _worldOverlayBaseTexture = baseTexture;
        _worldOverlayBaseRenderTarget = baseRenderTarget;
        _worldOverlayBaseView = baseView;
        _worldOverlayOutputTexture = outputTexture;
        _worldOverlayOutputRenderTarget = outputRenderTarget;
        _worldOverlayOutputView = outputView;
        _worldOverlayWidth = width;
        _worldOverlayHeight = height;
        _worldOverlayBaseFrame = -1;
        return true;
    }

    private static bool CreateWorldOverlayTexture(int width, int height, out ID3D11Texture2D* texture, out ID3D11RenderTargetView* renderTarget, out ID3D11ShaderResourceView* view)
    {
        texture = null;
        renderTarget = null;
        view = null;

        D3D11_TEXTURE2D_DESC desc = default;
        desc.Width = (uint)width;
        desc.Height = (uint)height;
        desc.MipLevels = 1u;
        desc.ArraySize = 1u;
        desc.Format = (DXGI_FORMAT)87; // DXGI_FORMAT_B8G8R8A8_UNORM
        desc.SampleDesc.Count = 1u;
        desc.Usage = 0; // D3D11_USAGE_DEFAULT
        desc.BindFlags = 0x28u; // D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET

        ID3D11Texture2D* createdTexture = null;
        if (_device->CreateTexture2D(&desc, null, &createdTexture) < 0 || createdTexture == null)
        {
            return false;
        }

        ID3D11RenderTargetView* createdRenderTarget = null;
        if (_device->CreateRenderTargetView((ID3D11Resource*)createdTexture, null, &createdRenderTarget) < 0 || createdRenderTarget == null)
        {
            createdTexture->Release();
            return false;
        }

        ID3D11ShaderResourceView* createdView = null;
        if (_device->CreateShaderResourceView((ID3D11Resource*)createdTexture, null, &createdView) < 0 || createdView == null)
        {
            createdRenderTarget->Release();
            createdTexture->Release();
            return false;
        }

        texture = createdTexture;
        renderTarget = createdRenderTarget;
        view = createdView;
        return true;
    }

    private static void PresentWorldOverlay(int width, int height, int submitFrame)
    {
        _worldOverlayLastSucceeded = false;
        if (!EnsureWorldOverlayResources(width, height))
        {
            return;
        }

        if (_worldOverlayBaseFrame != submitFrame)
        {
            float* transparent = stackalloc float[4];
            transparent[0] = transparent[1] = transparent[2] = transparent[3] = 0f;
            _context->ClearRenderTargetView(_worldOverlayBaseRenderTarget, transparent);
            _worldOverlayBaseFrame = submitFrame;
        }

        ID3D11RenderTargetView* oldRenderTarget = null;
        ID3D11DepthStencilView* oldDepthStencilView = null;
        ID3D11BlendState* oldBlendState = null;
        ID3D11DepthStencilState* oldDepthStencilState = null;
        ID3D11VertexShader* oldVertexShader = null;
        ID3D11PixelShader* oldPixelShader = null;
        ID3D11ShaderResourceView* oldInputView = null;
        float* oldBlendFactor = stackalloc float[4];
        uint oldSampleMask = 0;
        uint oldStencilRef = 0;

        _context->OMGetRenderTargets(1u, &oldRenderTarget, &oldDepthStencilView);
        _context->OMGetBlendState(&oldBlendState, oldBlendFactor, &oldSampleMask);
        _context->OMGetDepthStencilState(&oldDepthStencilState, &oldStencilRef);
        _context->VSGetShader(&oldVertexShader, null, null);
        _context->PSGetShader(&oldPixelShader, null, null);
        _context->PSGetShaderResources(0u, 1u, &oldInputView);

        try
        {
            var outputTarget = _worldOverlayOutputRenderTarget;
            _context->OMSetRenderTargets(1u, &outputTarget, null);
            _context->OMSetBlendState(null, null, uint.MaxValue);
            _context->OMSetDepthStencilState(_stencilDisabledState, 0u);

            var scissor = new RECT { left = 0, top = 0, right = width, bottom = height };
            _context->RSSetScissorRects(1u, &scissor);
            _context->VSSetShader(_worldOverlayVertexShader, null, 0u);
            _context->PSSetShader(_worldOverlayPixelShader, null, 0u);
            var baseView = _worldOverlayBaseView;
            _context->PSSetShaderResources(0u, 1u, &baseView);
            _context->Draw(3u, 0u);
            _worldOverlayLastSucceeded = true;
        }
        finally
        {
            if (oldRenderTarget != null)
            {
                var restoreTarget = oldRenderTarget;
                _context->OMSetRenderTargets(1u, &restoreTarget, oldDepthStencilView);
            }
            else
            {
                _context->OMSetRenderTargets(0u, null, oldDepthStencilView);
            }
            _context->OMSetBlendState(oldBlendState, oldBlendFactor, oldSampleMask);
            _context->OMSetDepthStencilState(oldDepthStencilState, oldStencilRef);
            _context->VSSetShader(oldVertexShader, null, 0u);
            _context->PSSetShader(oldPixelShader, null, 0u);
            _context->PSSetShaderResources(0u, 1u, &oldInputView);

            if (oldInputView != null)
            {
                oldInputView->Release();
            }
            if (oldPixelShader != null)
            {
                oldPixelShader->Release();
            }
            if (oldVertexShader != null)
            {
                oldVertexShader->Release();
            }
            if (oldDepthStencilState != null)
            {
                oldDepthStencilState->Release();
            }
            if (oldBlendState != null)
            {
                oldBlendState->Release();
            }
            if (oldDepthStencilView != null)
            {
                oldDepthStencilView->Release();
            }
            if (oldRenderTarget != null)
            {
                oldRenderTarget->Release();
            }
        }
    }

    private static void ReleaseWorldOverlayResources()
    {
        Release(ref _worldOverlayOutputView);
        Release(ref _worldOverlayOutputRenderTarget);
        Release(ref _worldOverlayOutputTexture);
        Release(ref _worldOverlayBaseView);
        Release(ref _worldOverlayBaseRenderTarget);
        Release(ref _worldOverlayBaseTexture);
        _worldOverlayWidth = 0;
        _worldOverlayHeight = 0;
        _worldOverlayBaseFrame = -1;
        _worldOverlayLastSucceeded = false;
    }

    private static bool TryUpdateSceneDepthResource(out Vector2 actualSize)
    {
        actualSize = default;
        if (_device == null)
        {
            return false;
        }

        var manager = RenderTargetManager.Instance();
        var depth = manager != null ? manager->DepthStencil : null;
        if (depth == null || depth->D3D11Texture2D == null)
        {
            ReleaseSceneDepthResource();
            return false;
        }

        var texture = (ID3D11Texture2D*)depth->D3D11Texture2D;
        D3D11_TEXTURE2D_DESC textureDesc = default;
        texture->GetDesc(&textureDesc);
        if (textureDesc.SampleDesc.Count != 1u)
        {
            // The current shader path uses Texture2D.Load. FFXIV's normal scene depth is single-sample;
            // fail open (ordinary overlay) if that ever changes rather than binding the wrong SRV type.
            ReleaseSceneDepthResource();
            return false;
        }

        var srvFormat = SceneDepthSrvFormat(textureDesc.Format);
        if (srvFormat == 0)
        {
            ReleaseSceneDepthResource();
            return false;
        }

        var identity = (nint)texture;
        if (_sceneDepthView == null || _sceneDepthTextureIdentity != identity)
        {
            ID3D11ShaderResourceView* newView = null;
            D3D11_SHADER_RESOURCE_VIEW_DESC nativeDesc = default;
            var srvDesc = (Texture2DShaderResourceViewDesc*)&nativeDesc;
            *srvDesc = new Texture2DShaderResourceViewDesc
            {
                Format = srvFormat,
                ViewDimension = (D3D_SRV_DIMENSION)4, // D3D_SRV_DIMENSION_TEXTURE2D
                MostDetailedMip = 0u,
                MipLevels = 1u,
            };
            var hr = _device->CreateShaderResourceView((ID3D11Resource*)texture, &nativeDesc, &newView);
            if (hr < 0 || newView == null)
            {
                ReleaseSceneDepthResource();
                return false;
            }

            Release(ref _sceneDepthView);
            _sceneDepthView = newView;
            _sceneDepthTextureIdentity = identity;
        }

        var width = depth->ActualWidth > 0 ? (float)depth->ActualWidth : textureDesc.Width;
        var height = depth->ActualHeight > 0 ? (float)depth->ActualHeight : textureDesc.Height;
        if (width <= 0f || height <= 0f)
        {
            return false;
        }
        actualSize = new Vector2(width, height);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DXGI_FORMAT SceneDepthSrvFormat(DXGI_FORMAT source)
        => (int)source switch
        {
            19 or 20 or 21 => (DXGI_FORMAT)21, // R32G8X24_TYPELESS / D32_FLOAT_S8X24_UINT -> R32_FLOAT_X8X24_TYPELESS
            39 or 40 or 41 => (DXGI_FORMAT)41, // R32_TYPELESS / D32_FLOAT -> R32_FLOAT
            44 or 45 or 46 => (DXGI_FORMAT)46, // R24G8_TYPELESS / D24_UNORM_S8_UINT -> R24_UNORM_X8_TYPELESS
            _ => 0,
        };

    private static void ReleaseSceneDepthResource()
    {
        Release(ref _sceneDepthView);
        _sceneDepthTextureIdentity = 0;
    }

    private static bool TryUpdateSceneInfoResource(out Vector2 actualSize)
    {
        actualSize = default;
        if (_device == null)
        {
            return false;
        }

        var manager = RenderTargetManager.Instance();
        var sceneInfo = manager != null ? manager->GBuffers[3].Value : null;
        if (sceneInfo == null || sceneInfo->D3D11Texture2D == null || sceneInfo->D3D11ShaderResourceView == null)
        {
            ReleaseSceneInfoResource();
            return false;
        }

        var texture = (ID3D11Texture2D*)sceneInfo->D3D11Texture2D;
        D3D11_TEXTURE2D_DESC textureDesc = default;
        texture->GetDesc(&textureDesc);
        if (textureDesc.SampleDesc.Count != 1u)
        {
            // Projected receiver shaders use Texture2D.Load. Refuse an unexpected multisampled target
            // rather than binding it through the wrong shader resource type.
            ReleaseSceneInfoResource();
            return false;
        }

        var sourceView = (ID3D11ShaderResourceView*)sceneInfo->D3D11ShaderResourceView;
        var identity = (nint)sourceView;
        if (_sceneInfoView == null || _sceneInfoViewIdentity != identity)
        {
            // Unlike scene depth, this texture already has a compatible SRV owned by the game. Hold
            // our own reference because ImGui can execute the queued packet after the build call ends.
            sourceView->AddRef();
            Release(ref _sceneInfoView);
            _sceneInfoView = sourceView;
            _sceneInfoViewIdentity = identity;
        }

        var width = sceneInfo->ActualWidth > 0 ? (float)sceneInfo->ActualWidth : textureDesc.Width;
        var height = sceneInfo->ActualHeight > 0 ? (float)sceneInfo->ActualHeight : textureDesc.Height;
        if (width <= 0f || height <= 0f)
        {
            return false;
        }

        actualSize = new Vector2(width, height);
        return true;
    }

    private static void ReleaseSceneInfoResource()
    {
        Release(ref _sceneInfoView);
        _sceneInfoViewIdentity = 0;
    }

    private static bool CreateStencilStates()
    {
        if (!CreateStencilState(write: true, out _stencilWriteState) || !CreateStencilState(write: false, out _stencilTestState) || !CreateStencilDisabledState(out _stencilDisabledState))
        {
            return false;
        }
        return true;
    }

    private static bool CreateStencilState(bool write, out ID3D11DepthStencilState* state)
    {
        state = null;
        D3D11_DEPTH_STENCIL_DESC desc = default;
        desc.DepthEnable = BOOL.FALSE;
        desc.DepthWriteMask = 0; // ZERO
        desc.DepthFunc = (D3D11_COMPARISON_FUNC)8;      // ALWAYS
        desc.StencilEnable = BOOL.TRUE;
        desc.StencilReadMask = 0xFF;
        desc.StencilWriteMask = write ? (byte)0xFF : (byte)0;

        D3D11_DEPTH_STENCILOP_DESC face = default;
        face.StencilFailOp = (D3D11_STENCIL_OP)1;       // KEEP
        face.StencilDepthFailOp = (D3D11_STENCIL_OP)1;  // KEEP
        face.StencilPassOp = (D3D11_STENCIL_OP)(write ? 3 : 1); // REPLACE : KEEP
        face.StencilFunc = (D3D11_COMPARISON_FUNC)(write ? 8 : 3); // ALWAYS : EQUAL
        desc.FrontFace = face;
        desc.BackFace = face;

        ID3D11DepthStencilState* created = null;
        _device->CreateDepthStencilState(&desc, &created);
        state = created;
        return true;
    }

    private static bool CreateStencilDisabledState(out ID3D11DepthStencilState* state)
    {
        state = null;
        D3D11_DEPTH_STENCIL_DESC desc = default;
        desc.DepthEnable = BOOL.FALSE;
        desc.DepthWriteMask = 0;
        desc.DepthFunc = (D3D11_COMPARISON_FUNC)8; // ALWAYS
        desc.StencilEnable = BOOL.FALSE;

        ID3D11DepthStencilState* created = null;
        _device->CreateDepthStencilState(&desc, &created);
        state = created;
        return true;
    }

    private static void PrepareFrame()
    {
        var frame = ImGui.GetFrameCount();
        // MiniArena submission and the renderer callback can overlap across the ImGui/render
        // boundary. Keep frame cleanup synchronized with callback packet removal; otherwise a new
        // frame can reclaim a packet before its queued draw callback consumes it
        if (Volatile.Read(ref _submitFrame) == frame)
        {
            return;
        }

        // Packet lifetime is owned by the queued ImGui callback, not by the ImGui frame number.
        // UI submission can advance to frame N+1 while the render thread is still consuming frame N.
        // Reclaiming PendingPackets here races RenderBatchCallback: the callback can subsequently
        // receive a valid packet id that has already been removed/repooled and silently draw nothing.
        // Successfully queued packets are removed and returned by RenderBatchCallback's finally block.
        // Failed AddCallback registrations clean themselves up at the call site, and Shutdown() remains
        // the final owner for callbacks that are genuinely abandoned during teardown
        Volatile.Write(ref _submitFrame, frame);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BatchPacket RentBatchPacket()
        => BatchPacketPool.TryPop(out var packet) ? packet : new BatchPacket();

    private static nint RegisterPacket(BatchPacket packet)
    {
        packet.SubmitFrame = Volatile.Read(ref _submitFrame);
        // Every packet originates from a prepared arena/run, so frame cleanup already happened in
        // EnsureArenaPrepared. Synchronize ownership with RenderBatchCallback/PrepareFrame
        lock (PendingPackets)
        {
            nint id;
            do
            {
                id = (nint)(++_nextPacketId);
            } while (id == 0 || PendingPackets.ContainsKey(id));

            PendingPackets.Add(id, packet);
            return id;
        }
    }

    private static BatchPacket? RemovePacket(nint id)
    {
        lock (PendingPackets)
        {
            return !PendingPackets.Remove(id, out var packet) ? null : packet;
        }
    }

    private static void ReturnPacketArrays(BatchPacket packet)
    {
        if (packet.ArenaSdfView != null)
        {
            packet.ArenaSdfView->Release();
        }
        if (packet.SceneDepthView != null)
        {
            packet.SceneDepthView->Release();
        }
        if (packet.SceneInfoView != null)
        {
            packet.SceneInfoView->Release();
        }
        if (packet.CustomSdfs != null)
        {
            var count = packet.CustomSdfCount;
            for (var i = 0; i < count; ++i)
            {
                var view = packet.CustomSdfs[i].View;
                if (view != null)
                {
                    view->Release();
                }
            }
            ArrayPool<CustomSdfBinding>.Shared.Return(packet.CustomSdfs, clearArray: false);
        }
        if (packet.WorldProjectedSdfs != null)
        {
            var count = packet.WorldProjectedSdfCount;
            for (var i = 0; i < count; ++i)
            {
                var view = packet.WorldProjectedSdfs[i].View;
                if (view != null)
                {
                    view->Release();
                }
            }
            ArrayPool<WorldProjectedSdfBinding>.Shared.Return(packet.WorldProjectedSdfs, clearArray: false);
        }
        if (packet.Sprites != null)
        {
            var count = packet.SpriteCount;
            for (var i = 0; i < count; ++i)
            {
                var view = packet.Sprites[i].View;
                if (view != null)
                {
                    view->Release();
                }
            }
            ArrayPool<SpriteBinding>.Shared.Return(packet.Sprites, clearArray: false);
        }
        if (packet.MeshVertices is MeshVertex[] mv)
        {
            ArrayPool<MeshVertex>.Shared.Return(mv);
        }
        if (packet.PrimitiveTriangleStrokes is PrimitiveTriangleStrokeInstance[] pts)
        {
            ArrayPool<PrimitiveTriangleStrokeInstance>.Shared.Return(pts);
        }
        if (packet.StrokeInstances is StrokeInstance[] sv)
        {
            ArrayPool<StrokeInstance>.Shared.Return(sv);
        }
        if (packet.WorldLines is WorldLineInstance[] wli)
        {
            ArrayPool<WorldLineInstance>.Shared.Return(wli);
        }
        if (packet.WorldCurves is WorldCurveInstance[] wc)
        {
            ArrayPool<WorldCurveInstance>.Shared.Return(wc);
        }
        if (packet.WorldProjectedArrows is WorldProjectedArrowInstance[] wpa)
        {
            ArrayPool<WorldProjectedArrowInstance>.Shared.Return(wpa);
        }
        if (packet.WorldProjectedShapes is WorldProjectedShapeInstance[] wps)
        {
            ArrayPool<WorldProjectedShapeInstance>.Shared.Return(wps);
        }
        if (packet.WorldLineTransforms is WorldLineTransform[] wlt)
        {
            ArrayPool<WorldLineTransform>.Shared.Return(wlt);
        }
        if (packet.Analytics is AnalyticInstance[] a)
        {
            ArrayPool<AnalyticInstance>.Shared.Return(a);
        }
        if (packet.Outlines is OutlineInstance[] o)
        {
            ArrayPool<OutlineInstance>.Shared.Return(o);
        }
        if (packet.TextInstances is TextInstance[] t)
        {
            ArrayPool<TextInstance>.Shared.Return(t);
        }
        if (packet.WorldTextInstances is WorldTextInstance[] wt)
        {
            ArrayPool<WorldTextInstance>.Shared.Return(wt);
        }
        if (packet.Segments is DrawSegment[] s)
        {
            ArrayPool<DrawSegment>.Shared.Return(s);
        }

        packet.MeshVertices = null;
        packet.MeshVertexCount = 0;
        packet.ArenaSdfMaskOutlineStart = -1;
        packet.PrimitiveTriangleStrokes = null;
        packet.PrimitiveTriangleStrokeCount = 0;
        packet.StrokeInstances = null;
        packet.StrokeInstanceCount = 0;
        packet.WorldLines = null;
        packet.WorldLineCount = 0;
        packet.WorldCurves = null;
        packet.WorldCurveCount = 0;
        packet.WorldProjectedArrows = null;
        packet.WorldProjectedArrowCount = 0;
        packet.WorldProjectedShapes = null;
        packet.WorldProjectedShapeCount = 0;
        packet.WorldProjectedSdfs = null;
        packet.WorldProjectedSdfCount = 0;
        packet.WorldLineTransforms = null;
        packet.WorldLineTransformCount = 0;
        packet.WorldLineConstants = default;
        packet.SceneDepthView = null;
        packet.SceneInfoView = null;
        packet.Analytics = null;
        packet.AnalyticCount = 0;
        packet.Outlines = null;
        packet.OutlineCount = 0;
        packet.TextInstances = null;
        packet.TextInstanceCount = 0;
        packet.WorldTextInstances = null;
        packet.WorldTextInstanceCount = 0;
        packet.Sprites = null;
        packet.SpriteCount = 0;
        packet.ArenaSdfView = null;
        packet.OutlineSdfConstants = default;
        packet.CustomSdfs = null;
        packet.CustomSdfCount = 0;
        packet.NeedsStencil = false;
        packet.UsesZoneWave = false;
        packet.ModifiesDepthState = false;
        packet.StencilKey = 0;
        packet.Segments = null;
        packet.SegmentCount = 0;
        packet.ClipOffset = default;
        packet.ClipScale = default;
        packet.FramebufferWidth = 0;
        packet.FramebufferHeight = 0;
        packet.UploadBytes = 0;
        packet.MeshOffsetBytes = 0u;
        packet.PrimitiveTriangleStrokeOffsetBytes = 0u;
        packet.StrokeOffsetBytes = 0u;
        packet.WorldLineOffsetBytes = 0u;
        packet.WorldCurveOffsetBytes = 0u;
        packet.WorldProjectedArrowOffsetBytes = 0u;
        packet.WorldProjectedShapeOffsetBytes = 0u;
        packet.AnalyticOffsetBytes = 0u;
        packet.OutlineOffsetBytes = 0u;
        packet.TextOffsetBytes = 0u;
        packet.WorldTextOffsetBytes = 0u;
        packet.SubmitFrame = -1;
        packet.WorldOverlayTarget = false;
        packet.IsWorldOverlayPresent = false;
        packet.IsDeferredOverlay = false;
        BatchPacketPool.Push(packet);
    }

    private static void Release<T>(ref T* value) where T : unmanaged
    {
        if (value == null)
        {
            return;
        }

        // Every D3D11 interface begins with IUnknown; Release is slot-compatible. Cast through
        // IUnknown so this helper works for all TerraFX COM interface pointer types.
        ((IUnknown*)value)->Release();
        value = null;
    }
}
