/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Date      :  5 March 2025                                                    *
* Website   :  https://www.angusj.com                                          *
* Copyright :  Angus Johnson 2010-2025                                         *
* Purpose   :  This module contains simple functions that will likely cover    *
*              most polygon boolean and offsetting needs, while also avoiding  *
*              the inherent complexities of the other modules.                 *
* Thanks    :  Special thanks to Thong Nguyen, Guus Kuiper, Phil Stopford,     *
*           :  and Daniel Gosnell for their invaluable assistance with C#.     *
* License   :  https://www.boost.org/LICENSE_1_0.txt                           *
*******************************************************************************/

using System.Buffers;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Clipper2Lib;
// PRE-COMPILER CONDITIONAL ...
// USINGZ: For user defined Z-coordinates. See Clipper.SetZ
[SkipLocalsInit]
public static class Clipper
{
    private const double DoublePI = Math.Tau;
    private const double HalfPI = 0.5d * Math.PI;
    private const int ArrayPoolThreshold = 128;
    // One-entry thread-local caches eliminate setup churn in convenience APIs. Rent removes
    // the entry first, so callbacks and other reentrant calls still receive independent state.
    [ThreadStatic] private static Clipper64? _cachedClipper64;
    [ThreadStatic] private static ClipperD? _cachedClipperD;
    [ThreadStatic] private static int _cachedClipperDPrecision;
    [ThreadStatic] private static ClipperOffset? _cachedClipperOffset;
    [ThreadStatic] private static RectClip64? _cachedRectClip;
    [ThreadStatic] private static RectClipLines64? _cachedRectClipLines;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Path64 AllocatePath64(int count)
    {
        var result = new Path64(count);
        CollectionsMarshal.SetCount(result, count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PathD AllocatePathD(int count)
    {
        var result = new PathD(count);
        CollectionsMarshal.SetCount(result, count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Paths64 AllocatePaths64(int count)
    {
        var result = new Paths64(count);
        CollectionsMarshal.SetCount(result, count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PathsD AllocatePathsD(int count)
    {
        var result = new PathsD(count);
        CollectionsMarshal.SetCount(result, count);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Clipper64 RentClipper64()
    {
        var result = _cachedClipper64;
        _cachedClipper64 = null;
        return result ?? new Clipper64();
    }

    internal static void ReturnClipper64(Clipper64 clipper)
    {
        clipper.Clear();
        clipper.PreserveCollinear = true;
        clipper.ReverseSolution = false;
#if USINGZ
		clipper.ZCallback = null;
#endif
        _cachedClipper64 ??= clipper;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ClipperD RentClipperD(int precision)
    {
        var result = _cachedClipperD;
        _cachedClipperD = null;
        if (result != null && _cachedClipperDPrecision == precision)
        {
            return result;
        }
        return new ClipperD(precision);
    }

    private static void ReturnClipperD(ClipperD clipper, int precision)
    {
        clipper.Clear();
        clipper.PreserveCollinear = true;
        clipper.ReverseSolution = false;
#if USINGZ
		clipper.ZCallback = null;
#endif
        if (_cachedClipperD == null)
        {
            _cachedClipperD = clipper;
            _cachedClipperDPrecision = precision;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ClipperOffset RentClipperOffset(double miterLimit, double arcTolerance)
    {
        var result = _cachedClipperOffset;
        _cachedClipperOffset = null;
        result ??= new ClipperOffset();
        result.Clear();
        result.MiterLimit = miterLimit;
        result.ArcTolerance = arcTolerance;
        result.MergeGroups = true;
        result.PreserveCollinear = false;
        result.ReverseSolution = false;
        result.DeltaCallback = null;
#if USINGZ
		result.ZCallback = null;
#endif
        return result;
    }

    private static void ReturnClipperOffset(ClipperOffset offset)
    {
        offset.Clear();
        offset.DeltaCallback = null;
#if USINGZ
		offset.ZCallback = null;
#endif
        _cachedClipperOffset ??= offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RectClip64 RentRectClip(Rect64 rect)
    {
        var result = _cachedRectClip;
        _cachedRectClip = null;
        return result != null && result.HasBounds(rect) ? result : new RectClip64(rect);
    }

    private static void ReturnRectClip(RectClip64 clipper)
    {
        clipper.Reset();
        _cachedRectClip ??= clipper;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static RectClipLines64 RentRectClipLines(Rect64 rect)
    {
        var result = _cachedRectClipLines;
        _cachedRectClipLines = null;
        return result != null && result.HasBounds(rect) ? result : new RectClipLines64(rect);
    }

    private static void ReturnRectClipLines(RectClipLines64 clipper)
    {
        clipper.Reset();
        _cachedRectClipLines ??= clipper;
    }

    private static Rect64 invalidRect64 = new(false);
    public static Rect64 InvalidRect64 => invalidRect64;

    private static RectD invalidRectD = new(false);
    public static RectD InvalidRectD => invalidRectD;

    public static Paths64 Intersect(Paths64 subject, Paths64 clip, FillRule fillRule)
    {
        return BooleanOp(ClipType.Intersection, subject, clip, fillRule);
    }

    public static PathsD Intersect(PathsD subject, PathsD clip, FillRule fillRule, int precision = 2)
    {
        return BooleanOp(ClipType.Intersection, subject, clip, fillRule, precision);
    }

    public static Paths64 Union(Paths64 subject, FillRule fillRule)
    {
        return BooleanOp(ClipType.Union, subject, null, fillRule);
    }

    public static Paths64 Union(Paths64 subject, Paths64 clip, FillRule fillRule)
    {
        return BooleanOp(ClipType.Union, subject, clip, fillRule);
    }

    public static PathsD Union(PathsD subject, FillRule fillRule)
    {
        return BooleanOp(ClipType.Union, subject, null, fillRule);
    }

    public static PathsD Union(PathsD subject, PathsD clip, FillRule fillRule, int precision = 2)
    {
        return BooleanOp(ClipType.Union, subject, clip, fillRule, precision);
    }

    public static Paths64 Difference(Paths64 subject, Paths64 clip, FillRule fillRule)
    {
        return BooleanOp(ClipType.Difference, subject, clip, fillRule);
    }

    public static PathsD Difference(PathsD subject, PathsD clip, FillRule fillRule, int precision = 2)
    {
        return BooleanOp(ClipType.Difference, subject, clip, fillRule, precision);
    }

    public static Paths64 Xor(Paths64 subject, Paths64 clip, FillRule fillRule)
    {
        return BooleanOp(ClipType.Xor, subject, clip, fillRule);
    }

    public static PathsD Xor(PathsD subject, PathsD clip,
      FillRule fillRule, int precision = 2)
    {
        return BooleanOp(ClipType.Xor,
          subject, clip, fillRule, precision);
    }

    public static Paths64 BooleanOp(ClipType clipType,
      Paths64? subject, Paths64? clip, FillRule fillRule)
    {
        Paths64 solution = [];
        if (subject == null)
        {
            return solution;
        }
        var c = RentClipper64();
        try
        {
            c.AddPaths(subject, PathType.Subject);
            if (clip != null)
            {
                c.AddPaths(clip, PathType.Clip);
            }
            c.Execute(clipType, fillRule, solution);
        }
        finally
        {
            ReturnClipper64(c);
        }
        return solution;
    }

    public static void BooleanOp(ClipType clipType, Paths64? subject, Paths64? clip, PolyTree64 polytree, FillRule fillRule)
    {
        if (subject == null)
        {
            return;
        }
        var c = RentClipper64();
        try
        {
            c.AddPaths(subject, PathType.Subject);
            if (clip != null)
            {
                c.AddPaths(clip, PathType.Clip);
            }
            c.Execute(clipType, fillRule, polytree);
        }
        finally
        {
            ReturnClipper64(c);
        }
    }

    public static PathsD BooleanOp(ClipType clipType, PathsD subject, PathsD? clip, FillRule fillRule, int precision = 2)
    {
        PathsD solution = [];
        var c = RentClipperD(precision);
        try
        {
            c.AddSubject(subject);
            if (clip != null)
            {
                c.AddClip(clip);
            }
            c.Execute(clipType, fillRule, solution);
        }
        finally
        {
            ReturnClipperD(c, precision);
        }
        return solution;
    }

    public static void BooleanOp(ClipType clipType, PathsD? subject, PathsD? clip, PolyTreeD polytree, FillRule fillRule, int precision = 2)
    {
        if (subject == null)
        {
            return;
        }
        var c = RentClipperD(precision);
        try
        {
            c.AddPaths(subject, PathType.Subject);
            if (clip != null)
            {
                c.AddPaths(clip, PathType.Clip);
            }
            c.Execute(clipType, fillRule, polytree);
        }
        finally
        {
            ReturnClipperD(c, precision);
        }
    }

    public static Paths64 InflatePaths(Paths64 paths, double delta, JoinType joinType, EndType endType, double miterLimit = 2.0, double arcTolerance = 0.0)
    {
        Paths64 solution = [];
        var co = RentClipperOffset(miterLimit, arcTolerance);
        try
        {
            co.AddPaths(paths, joinType, endType);
            co.Execute(delta, solution);
        }
        finally
        {
            ReturnClipperOffset(co);
        }
        return solution;
    }

    public static PathsD InflatePaths(PathsD paths, double delta, JoinType joinType, EndType endType, double miterLimit = 2.0, int precision = 2, double arcTolerance = 0.0)
    {
        InternalClipper.CheckPrecision(precision);
        var scale = Math.Pow(10, precision);
        var tmp = ScalePaths64(paths, scale);
        var co = RentClipperOffset(miterLimit, scale * arcTolerance);
        try
        {
            co.AddPaths(tmp, joinType, endType);
            co.Execute(delta * scale, tmp); // reuse 'tmp' to receive (scaled) solution
        }
        finally
        {
            ReturnClipperOffset(co);
        }
        return ScalePathsD(tmp, 1d / scale);
    }

    public static Paths64 RectClip(Rect64 rect, Paths64 paths)
    {
        if (rect.IsEmpty() || paths.Count == 0)
        {
            return [];
        }
        var rc = RentRectClip(rect);
        try
        {
            return rc.Execute(paths);
        }
        finally
        {
            ReturnRectClip(rc);
        }
    }

    public static Paths64 RectClip(Rect64 rect, Path64 path)
    {
        if (rect.IsEmpty() || path.Count == 0)
        {
            return [];
        }
        var rc = RentRectClip(rect);
        try
        {
            return rc.Execute(path);
        }
        finally
        {
            ReturnRectClip(rc);
        }
    }

    public static PathsD RectClip(RectD rect, PathsD paths, int precision = 2)
    {
        InternalClipper.CheckPrecision(precision);
        if (rect.IsEmpty() || paths.Count == 0)
        {
            return [];
        }
        var scale = Math.Pow(10d, precision);
        var r = ScaleRect(rect, scale);
        var tmpPath = ScalePaths64(paths, scale);
        var rc = RentRectClip(r);
        try
        {
            tmpPath = rc.Execute(tmpPath);
        }
        finally
        {
            ReturnRectClip(rc);
        }
        return ScalePathsD(tmpPath, 1d / scale);
    }

    public static PathsD RectClip(RectD rect, PathD path, int precision = 2)
    {
        if (rect.IsEmpty() || path.Count == 0)
        {
            return [];
        }
        InternalClipper.CheckPrecision(precision);
        var scale = Math.Pow(10d, precision);
        var r = ScaleRect(rect, scale);
        var rc = RentRectClip(r);
        Paths64 result;
        try
        {
            result = rc.Execute(ScalePath64(path, scale));
        }
        finally
        {
            ReturnRectClip(rc);
        }
        return ScalePathsD(result, 1d / scale);
    }

    public static Paths64 RectClipLines(Rect64 rect, Paths64 paths)
    {
        if (rect.IsEmpty() || paths.Count == 0)
        {
            return [];
        }
        var rc = RentRectClipLines(rect);
        try
        {
            return rc.Execute(paths);
        }
        finally
        {
            ReturnRectClipLines(rc);
        }
    }

    public static Paths64 RectClipLines(Rect64 rect, Path64 path)
    {
        if (rect.IsEmpty() || path.Count == 0)
        {
            return [];
        }
        var rc = RentRectClipLines(rect);
        try
        {
            return rc.Execute(path);
        }
        finally
        {
            ReturnRectClipLines(rc);
        }
    }

    public static PathsD RectClipLines(RectD rect,
      PathsD paths, int precision = 2)
    {
        InternalClipper.CheckPrecision(precision);
        if (rect.IsEmpty() || paths.Count == 0)
        {
            return [];
        }
        var scale = Math.Pow(10d, precision);
        var r = ScaleRect(rect, scale);
        var tmpPath = ScalePaths64(paths, scale);
        var rc = RentRectClipLines(r);
        try
        {
            tmpPath = rc.Execute(tmpPath);
        }
        finally
        {
            ReturnRectClipLines(rc);
        }
        return ScalePathsD(tmpPath, 1d / scale);
    }

    public static PathsD RectClipLines(RectD rect, PathD path, int precision = 2)
    {
        if (rect.IsEmpty() || path.Count == 0)
        {
            return [];
        }
        InternalClipper.CheckPrecision(precision);
        var scale = Math.Pow(10d, precision);
        var r = ScaleRect(rect, scale);
        var rc = RentRectClipLines(r);
        Paths64 result;
        try
        {
            result = rc.Execute(ScalePath64(path, scale));
        }
        finally
        {
            ReturnRectClipLines(rc);
        }
        return ScalePathsD(result, 1d / scale);
    }

    public static Paths64 MinkowskiSum(Path64 pattern, Path64 path, bool isClosed)
    {
        return Minkowski.Sum(pattern, path, isClosed);
    }

    public static PathsD MinkowskiSum(PathD pattern, PathD path, bool isClosed)
    {
        return Minkowski.Sum(pattern, path, isClosed);
    }

    public static Paths64 MinkowskiDiff(Path64 pattern, Path64 path, bool isClosed)
    {
        return Minkowski.Diff(pattern, path, isClosed);
    }

    public static PathsD MinkowskiDiff(PathD pattern, PathD path, bool isClosed)
    {
        return Minkowski.Diff(pattern, path, isClosed);
    }

    public static double Area(Path64 path)
    {
        // https://en.wikipedia.org/wiki/Shoelace_formula
        var points = CollectionsMarshal.AsSpan(path);
        var cnt = points.Length;
        if (cnt < 3)
        {
            return 0.0;
        }
        var a = 0.0;
        var prevPt = points[cnt - 1];
        for (var i = 0; i < cnt; ++i)
        {
            ref var pt = ref points[i];
            a += (double)(prevPt.Y + pt.Y) * (prevPt.X - pt.X);
            prevPt = pt;
        }
        return a * 0.5;
    }

    public static double Area(Paths64 paths)
    {
        var a = 0.0;
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            a += Area(pathSpan[i]);
        }
        return a;
    }

    public static double Area(PathD path)
    {
        var points = CollectionsMarshal.AsSpan(path);
        var cnt = points.Length;
        if (cnt < 3)
        {
            return 0.0;
        }
        var a = 0.0;
        var prevPt = points[cnt - 1];
        for (var i = 0; i < cnt; ++i)
        {
            ref var pt = ref points[i];
            a += (prevPt.y + pt.y) * (prevPt.x - pt.x);
            prevPt = pt;
        }
        return a * 0.5;
    }

    public static double Area(PathsD paths)
    {
        var a = 0.0;
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            a += Area(pathSpan[i]);
        }
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(Path64 poly)
    {
        return Area(poly) >= 0d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(PathD poly)
    {
        return Area(poly) >= 0d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AppendPoint64(StringBuilder destination, Point64 point)
    {
        destination.Append(point.X).Append(',').Append(point.Y);
#if USINGZ
		destination.Append(',').Append(point.Z);
#endif
        destination.Append(' ');
    }

    public static string Path64ToString(Path64 path)
    {
        var result = new StringBuilder();
        var points = CollectionsMarshal.AsSpan(path);
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            AppendPoint64(result, points[i]);
        }
        return result.Append('\n').ToString();
    }

    public static string Paths64ToString(Paths64 paths)
    {
        var result = new StringBuilder();
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var lenPath = pathSpan.Length;
        for (var i = 0; i < lenPath; ++i)
        {
            var points = CollectionsMarshal.AsSpan(pathSpan[i]);
            var len = points.Length;
            for (var j = 0; j < len; ++j)
            {
                AppendPoint64(result, points[j]);
                result.Append('\n');
            }
        }
        return result.ToString();
    }

    public static string PathDToString(PathD path)
    {
        var result = new StringBuilder();
        var points = CollectionsMarshal.AsSpan(path);
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            result.Append(points[i].ToString());
        }
        return result.Append('\n').ToString();
    }

    public static string PathsDToString(PathsD paths)
    {
        var result = new StringBuilder();
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var lenPath = pathSpan.Length;
        for (var i = 0; i < lenPath; ++i)
        {
            var points = CollectionsMarshal.AsSpan(pathSpan[i]);
            var len = points.Length;
            for (var j = 0; j < len; ++j)
            {
                result.Append(points[j].ToString());
                result.Append('\n');
            }
        }
        return result.ToString();
    }

    private static void TranslatePoints(ReadOnlySpan<Point64> source, Span<Point64> destination, long dx, long dy)
    {
        var len = source.Length;
#if USINGZ
		for (var i = 0; i < len; ++i)
		{
			destination[i] = new Point64(source[i].X + dx, source[i].Y + dy);
		}
#else
        var pointIndex = 0;
        if (Avx2.IsSupported && len >= 2)
        {
            var sourceValues = MemoryMarshal.Cast<Point64, long>(source);
            var destinationValues = MemoryMarshal.Cast<Point64, long>(destination);
            var translation = Vector256.Create(dx, dy, dx, dy);
            ref var sourceRef = ref MemoryMarshal.GetReference(sourceValues);
            ref var destinationRef = ref MemoryMarshal.GetReference(destinationValues);
            var countV = Vector256<long>.Count;
            var simdValueCount = sourceValues.Length & -countV;
            for (var i = 0; i < simdValueCount; i += countV)
            {
                Avx2.Add(Vector256.LoadUnsafe(ref sourceRef, (nuint)i), translation).StoreUnsafe(ref destinationRef, (nuint)i);
            }
            pointIndex = simdValueCount >> 1;
        }
        for (; pointIndex < len; ++pointIndex)
        {
            destination[pointIndex] = new Point64(source[pointIndex].X + dx, source[pointIndex].Y + dy);
        }
#endif
    }

    private static void TranslatePoints(ReadOnlySpan<PointD> source, Span<PointD> destination, double dx, double dy)
    {
        var len = source.Length;
#if USINGZ
		for (var i = 0; i < len; ++i)
		{
			destination[i] = new PointD(source[i].x + dx, source[i].y + dy);
		}
#else
        var pointIndex = 0;
        if (Avx.IsSupported && len >= 2)
        {
            var sourceValues = MemoryMarshal.Cast<PointD, double>(source);
            var destinationValues = MemoryMarshal.Cast<PointD, double>(destination);
            var translation = Vector256.Create(dx, dy, dx, dy);
            ref var sourceRef = ref MemoryMarshal.GetReference(sourceValues);
            ref var destinationRef = ref MemoryMarshal.GetReference(destinationValues);
            var countV = Vector256<double>.Count;
            var simdValueCount = sourceValues.Length & -countV;
            for (var i = 0; i < simdValueCount; i += countV)
            {
                Avx.Add(Vector256.LoadUnsafe(ref sourceRef, (nuint)i), translation).StoreUnsafe(ref destinationRef, (nuint)i);
            }
            pointIndex = simdValueCount >> 1;
        }
        for (; pointIndex < len; ++pointIndex)
        {
            destination[pointIndex] = new PointD(source[pointIndex].x + dx, source[pointIndex].y + dy);
        }
#endif
    }

    public static Path64 OffsetPath(Path64 path, long dx, long dy)
    {
        var result = AllocatePath64(path.Count);
        TranslatePoints(CollectionsMarshal.AsSpan(path), CollectionsMarshal.AsSpan(result), dx, dy);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point64 ScalePoint64(Point64 pt, double scale)
    {
        var result = new Point64()
        {
            X = (long)Math.Round(pt.X * scale, MidpointRounding.AwayFromZero),
            Y = (long)Math.Round(pt.Y * scale, MidpointRounding.AwayFromZero),
#if USINGZ
			Z = pt.Z
#endif
        };
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointD ScalePointD(Point64 pt, double scale)
    {
        var result = new PointD()
        {
            x = pt.X * scale,
            y = pt.Y * scale,
#if USINGZ
			z = pt.Z,
#endif
        };
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rect64 ScaleRect(RectD rec, double scale)
    {
        var result = new Rect64()
        {
            left = (long)(rec.left * scale),
            top = (long)(rec.top * scale),
            right = (long)(rec.right * scale),
            bottom = (long)(rec.bottom * scale)
        };
        return result;
    }

    public static Path64 ScalePath(Path64 path, double scale)
    {
        if (InternalClipper.IsAlmostZero(scale - 1))
        {
            return path;
        }
        var result = AllocatePath64(path.Count);
        var source = CollectionsMarshal.AsSpan(path);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
#if USINGZ
        for (var i = 0; i < len; ++i)
		{
				destination[i] = new Point64(source[i].X * scale, source[i].Y * scale, source[i].Z);
		}
#else
        for (var i = 0; i < len; ++i)
        {
            destination[i] = new Point64(source[i].X * scale, source[i].Y * scale);
        }
#endif
        return result;
    }

    public static Paths64 ScalePaths(Paths64 paths, double scale)
    {
        if (InternalClipper.IsAlmostZero(scale - 1))
        {
            return paths;
        }
        var result = AllocatePaths64(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = ScalePath(source[i], scale);
        }
        return result;
    }

    private static void ScalePoints(ReadOnlySpan<PointD> source, Span<PointD> destination, double scale)
    {
        var len = source.Length;
#if USINGZ
		for (var i = 0; i < len; ++i)
		{
			destination[i] = new PointD(source[i], scale);
		}
#else
        var pointIndex = 0;
        if (Avx.IsSupported && len >= 2)
        {
            var sourceValues = MemoryMarshal.Cast<PointD, double>(source);
            var destinationValues = MemoryMarshal.Cast<PointD, double>(destination);
            var scaleVector = Vector256.Create(scale);
            ref var sourceRef = ref MemoryMarshal.GetReference(sourceValues);
            ref var destinationRef = ref MemoryMarshal.GetReference(destinationValues);
            var countV = Vector256<double>.Count;
            var simdValueCount = sourceValues.Length & -countV;
            for (var i = 0; i < simdValueCount; i += countV)
            {
                Avx.Multiply(Vector256.LoadUnsafe(ref sourceRef, (nuint)i), scaleVector).StoreUnsafe(ref destinationRef, (nuint)i);
            }
            pointIndex = simdValueCount >> 1;
        }
        for (; pointIndex < len; ++pointIndex)
        {
            destination[pointIndex] = new PointD(source[pointIndex], scale);
        }
#endif
    }

    public static PathD ScalePath(PathD path, double scale)
    {
        if (InternalClipper.IsAlmostZero(scale - 1))
        {
            return path;
        }
        var result = AllocatePathD(path.Count);
        ScalePoints(CollectionsMarshal.AsSpan(path), CollectionsMarshal.AsSpan(result), scale);
        return result;
    }

    public static PathsD ScalePaths(PathsD paths, double scale)
    {
        if (InternalClipper.IsAlmostZero(scale - 1))
        {
            return paths;
        }
        var result = AllocatePathsD(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = ScalePath(source[i], scale);
        }
        return result;
    }

    // Unlike ScalePath, both ScalePath64 & ScalePathD also involve type conversion
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ScalePoints64(ReadOnlySpan<PointD> source, Span<Point64> destination, double scale)
    {
        var len = source.Length;
        if (destination.Length < len)
        {
            throw new ArgumentException("Destination is shorter than source.", nameof(destination));
        }
        ref var sourceRef = ref MemoryMarshal.GetReference(source);
        ref var destinationRef = ref MemoryMarshal.GetReference(destination);
        for (var i = 0; i < len; ++i)
        {
            Unsafe.Add(ref destinationRef, i) = new Point64(Unsafe.Add(ref sourceRef, i), scale);
        }
    }

    public static Path64 ScalePath64(PathD path, double scale)
    {
        var cnt = path.Count;
        var res = AllocatePath64(cnt);
        ScalePoints64(CollectionsMarshal.AsSpan(path), CollectionsMarshal.AsSpan(res), scale);
        return res;
    }

    public static Paths64 ScalePaths64(PathsD paths, double scale)
    {
        var cnt = paths.Count;
        var res = AllocatePaths64(cnt);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(res);
        for (var i = 0; i < cnt; ++i)
        {
            destination[i] = ScalePath64(source[i], scale);
        }
        return res;
    }

    public static PathD ScalePathD(Path64 path, double scale)
    {
        var cnt = path.Count;
        var res = AllocatePathD(cnt);
        ScalePointsD(CollectionsMarshal.AsSpan(path), CollectionsMarshal.AsSpan(res), scale);
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void ScalePointsD(ReadOnlySpan<Point64> source, Span<PointD> destination, double scale)
    {
        var len = source.Length;
        if (destination.Length < len)
        {
            throw new ArgumentException("Destination is shorter than source.", nameof(destination));
        }
        ref var sourceRef = ref MemoryMarshal.GetReference(source);
        ref var destinationRef = ref MemoryMarshal.GetReference(destination);
        for (var i = 0; i < len; ++i)
        {
            Unsafe.Add(ref destinationRef, i) = new PointD(Unsafe.Add(ref sourceRef, i), scale);
        }
    }

    public static PathsD ScalePathsD(Paths64 paths, double scale)
    {
        var cnt = paths.Count;
        var res = AllocatePathsD(cnt);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(res);
        for (var i = 0; i < cnt; ++i)
        {
            destination[i] = ScalePathD(source[i], scale);
        }
        return res;
    }

    // The static functions Path64 and PathD convert path types without scaling
    public static Path64 Path64(PathD path)
    {
        var result = AllocatePath64(path.Count);
        var source = CollectionsMarshal.AsSpan(path);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = new Point64(source[i]);
        }
        return result;
    }

    public static Paths64 Paths64(PathsD paths)
    {
        var result = AllocatePaths64(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = Path64(source[i]);
        }
        return result;
    }

    public static PathsD PathsD(Paths64 paths)
    {
        var result = AllocatePathsD(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = PathD(source[i]);
        }
        return result;
    }

    public static PathD PathD(Path64 path)
    {
        var result = AllocatePathD(path.Count);
        var source = CollectionsMarshal.AsSpan(path);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = new PointD(source[i]);
        }
        return result;
    }

    public static Path64 TranslatePath(Path64 path, long dx, long dy)
    {
        return OffsetPath(path, dx, dy);
    }

    public static Paths64 TranslatePaths(Paths64 paths, long dx, long dy)
    {
        var result = AllocatePaths64(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = OffsetPath(source[i], dx, dy);
        }
        return result;
    }

    public static PathD TranslatePath(PathD path, double dx, double dy)
    {
        var result = AllocatePathD(path.Count);
        TranslatePoints(CollectionsMarshal.AsSpan(path), CollectionsMarshal.AsSpan(result), dx, dy);
        return result;
    }

    public static PathsD TranslatePaths(PathsD paths, double dx, double dy)
    {
        var result = AllocatePathsD(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = TranslatePath(source[i], dx, dy);
        }
        return result;
    }

    public static Path64 ReversePath(Path64 path)
    {
        var result = AllocatePath64(path.Count);
        var source = CollectionsMarshal.AsSpan(path);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = source[len - 1 - i];
        }
        return result;
    }

    public static PathD ReversePath(PathD path)
    {
        var result = AllocatePathD(path.Count);
        var source = CollectionsMarshal.AsSpan(path);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = source[len - 1 - i];
        }
        return result;
    }

    public static Paths64 ReversePaths(Paths64 paths)
    {
        var result = AllocatePaths64(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = ReversePath(source[i]);
        }
        return result;
    }

    public static PathsD ReversePaths(PathsD paths)
    {
        var result = AllocatePathsD(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = ReversePath(source[i]);
        }
        return result;
    }

    public static Rect64 GetBounds(Path64 path)
    {
        var result = InvalidRect64;
        var points = CollectionsMarshal.AsSpan(path);
        var lenP = points.Length;
        for (var i = 0; i < lenP; ++i)
        {
            ref var pt = ref points[i];
            var ptX = pt.X;
            var ptY = pt.Y;
            if (ptX < result.left)
            {
                result.left = ptX;
            }
            if (ptX > result.right)
            {
                result.right = ptX;
            }
            if (ptY < result.top)
            {
                result.top = ptY;
            }
            if (ptY > result.bottom)
            {
                result.bottom = ptY;
            }
        }
        return result.left == long.MaxValue ? new Rect64() : result;
    }

    public static Rect64 GetBounds(Paths64 paths)
    {
        var result = InvalidRect64;
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            var points = CollectionsMarshal.AsSpan(pathSpan[i]);
            var lenP = points.Length;
            for (var j = 0; j < lenP; ++j)
            {
                ref var pt = ref points[j];
                var ptX = pt.X;
                var ptY = pt.Y;
                if (ptX < result.left)
                {
                    result.left = ptX;
                }
                if (ptX > result.right)
                {
                    result.right = ptX;
                }
                if (ptY < result.top)
                {
                    result.top = ptY;
                }
                if (ptY > result.bottom)
                {
                    result.bottom = ptY;
                }
            }
        }
        return result.left == long.MaxValue ? new Rect64() : result;
    }

    public static RectD GetBounds(PathD path)
    {
        var result = InvalidRectD;
        var points = CollectionsMarshal.AsSpan(path);
        var lenP = points.Length;
        for (var i = 0; i < lenP; ++i)
        {
            ref var pt = ref points[i];
            var ptX = pt.x;
            var ptY = pt.y;
            if (ptX < result.left)
            {
                result.left = ptX;
            }
            if (ptX > result.right)
            {
                result.right = ptX;
            }
            if (ptY < result.top)
            {
                result.top = ptY;
            }
            if (ptY > result.bottom)
            {
                result.bottom = ptY;
            }
        }
        return Math.Abs(result.left - double.MaxValue) < InternalClipper.floatingPointTolerance ? new RectD() : result;
    }

    public static RectD GetBounds(PathsD paths)
    {
        var result = InvalidRectD;
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            var points = CollectionsMarshal.AsSpan(pathSpan[i]);
            var lenP = points.Length;
            for (var j = 0; j < lenP; ++j)
            {
                ref var pt = ref points[j];
                var ptX = pt.x;
                var ptY = pt.y;
                if (ptX < result.left)
                {
                    result.left = ptX;
                }
                if (ptX > result.right)
                {
                    result.right = ptX;
                }
                if (ptY < result.top)
                {
                    result.top = ptY;
                }
                if (ptY > result.bottom)
                {
                    result.bottom = ptY;
                }
            }
        }
        return Math.Abs(result.left - double.MaxValue) < InternalClipper.floatingPointTolerance ? new RectD() : result;
    }

    public static Path64 MakePath(int[] arr)
    {
        var len = arr.Length / 2;
        var p = AllocatePath64(len);
        var points = CollectionsMarshal.AsSpan(p);
        for (var i = 0; i < len; ++i)
        {
            points[i] = new Point64(arr[i * 2], arr[i * 2 + 1]);
        }
        return p;
    }

    public static Path64 MakePath(long[] arr)
    {
        var len = arr.Length / 2;
        var p = AllocatePath64(len);
        var points = CollectionsMarshal.AsSpan(p);
        for (var i = 0; i < len; ++i)
        {
            points[i] = new Point64(arr[i * 2], arr[i * 2 + 1]);
        }
        return p;
    }

    public static PathD MakePath(double[] arr)
    {
        var len = arr.Length / 2;
        var p = AllocatePathD(len);
        var points = CollectionsMarshal.AsSpan(p);
        for (var i = 0; i < len; ++i)
        {
            points[i] = new PointD(arr[i * 2], arr[i * 2 + 1]);
        }
        return p;
    }

#if USINGZ
	public static Path64 MakePathZ(long[] arr)
	{
		int len = arr.Length / 3;
		Path64 p = AllocatePath64(len);
		var points = CollectionsMarshal.AsSpan(p);
		for (int i = 0; i < len; ++i)
		{
			points[i] = new Point64(arr[i * 3], arr[i * 3 + 1], arr[i * 3 + 2]);
		}
		return p;
	}

	public static PathD MakePathZ(double[] arr)
	{
		int len = arr.Length / 3;
		PathD p = AllocatePathD(len);
		var points = CollectionsMarshal.AsSpan(p);
		for (int i = 0; i < len; ++i)
		{
			points[i] = new PointD(arr[i * 3], arr[i * 3 + 1], (long)arr[i * 3 + 2]);
		}
		return p;
	}
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Sqr(double val)
    {
        return val * val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Sqr(long val)
    {
        return val * (double)val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DistanceSqr(Point64 pt1, Point64 pt2)
    {
        return Sqr(pt1.X - pt2.X) + Sqr(pt1.Y - pt2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point64 MidPoint(Point64 pt1, Point64 pt2)
    {
        return new Point64((pt1.X + pt2.X) * 0.5d, (pt1.Y + pt2.Y) * 0.5d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointD MidPoint(PointD pt1, PointD pt2)
    {
        return new PointD((pt1.x + pt2.x) * 0.5d, (pt1.y + pt2.y) * 0.5d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InflateRect(ref Rect64 rec, int dx, int dy)
    {
        rec.left -= dx;
        rec.right += dx;
        rec.top -= dy;
        rec.bottom += dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void InflateRect(ref RectD rec, double dx, double dy)
    {
        rec.left -= dx;
        rec.right += dx;
        rec.top -= dy;
        rec.bottom += dy;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool PointsNearEqual(PointD pt1, PointD pt2, double distanceSqrd)
    {
        return Sqr(pt1.x - pt2.x) + Sqr(pt1.y - pt2.y) < distanceSqrd;
    }

    public static PathD StripNearDuplicates(PathD path, double minEdgeLenSqrd, bool isClosedPath)
    {
        var cnt = path.Count;
        PathD result = [with(cnt)];
        if (cnt == 0)
        {
            return result;
        }
        var points = CollectionsMarshal.AsSpan(path);
        CollectionsMarshal.SetCount(result, cnt);
        var output = CollectionsMarshal.AsSpan(result);
        var lastPt = points[0];
        output[0] = lastPt;
        var resultCount = 1;
        for (var i = 1; i < cnt; ++i)
        {
            if (!PointsNearEqual(lastPt, points[i], minEdgeLenSqrd))
            {
                lastPt = points[i];
                output[resultCount++] = lastPt;
            }
        }

        if (isClosedPath && PointsNearEqual(lastPt, output[0], minEdgeLenSqrd))
        {
            --resultCount;
        }
        CollectionsMarshal.SetCount(result, resultCount);

        return result;
    }

    public static Path64 StripDuplicates(Path64 path, bool isClosedPath)
    {
        var cnt = path.Count;
        Path64 result = [with(cnt)];
        if (cnt == 0)
        {
            return result;
        }
        var points = CollectionsMarshal.AsSpan(path);
        CollectionsMarshal.SetCount(result, cnt);
        var output = CollectionsMarshal.AsSpan(result);
        var lastPt = points[0];
        output[0] = lastPt;
        var resultCount = 1;
        for (var i = 1; i < cnt; ++i)
        {
            if (lastPt != points[i])
            {
                lastPt = points[i];
                output[resultCount++] = lastPt;
            }
        }
        if (isClosedPath && lastPt == output[0])
        {
            --resultCount;
        }
        CollectionsMarshal.SetCount(result, resultCount);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddPolyNodeToPaths(PolyPath64 polyPath, Paths64 paths)
    {
        var count = polyPath.Polygon!.Count;
        if (count > 0)
        {
            paths.Add(polyPath.Polygon);
        }
        for (var i = 0; i < count; ++i)
        {
            AddPolyNodeToPaths((PolyPath64)polyPath._childs[i], paths);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Paths64 PolyTreeToPaths64(PolyTree64 polyTree)
    {
        Paths64 result = [];
        var count = polyTree.Count;
        for (var i = 0; i < count; ++i)
        {
            AddPolyNodeToPaths((PolyPath64)polyTree._childs[i], result);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddPolyNodeToPathsD(PolyPathD polyPath, PathsD paths)
    {
        var count = polyPath.Polygon!.Count;
        if (count > 0)
        {
            paths.Add(polyPath.Polygon);
        }
        for (var i = 0; i < count; ++i)
        {
            AddPolyNodeToPathsD((PolyPathD)polyPath._childs[i], paths);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PathsD PolyTreeToPathsD(PolyTreeD polyTree)
    {
        PathsD result = [];
        var count = polyTree.Count;
        for (var i = 0; i < count; ++i)
        {
            AddPolyNodeToPathsD((PolyPathD)polyTree._childs[i], result);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double PerpendicDistFromLineSqrd(PointD pt, PointD line1, PointD line2)
    {
        var a = pt.x - line1.x;
        var b = pt.y - line1.y;
        var c = line2.x - line1.x;
        var d = line2.y - line1.y;
        if (c == 0d && d == 0d)
        {
            return 0d;
        }
        return Sqr(a * d - c * b) / (c * c + d * d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double PerpendicDistFromLineSqrd(Point64 pt, Point64 line1, Point64 line2)
    {
        var a = (double)pt.X - line1.X;
        var b = (double)pt.Y - line1.Y;
        var c = (double)line2.X - line1.X;
        var d = (double)line2.Y - line1.Y;
        if (c == 0d && d == 0d)
        {
            return 0d;
        }
        return Sqr(a * d - c * b) / (c * c + d * d);
    }

    private static void RDPCore(ReadOnlySpan<Point64> path, int begin, int end, double epsSqrd, Span<bool> flags, Span<int> stack)
    {
        var stackCount = 0;
        stack[stackCount++] = begin;
        stack[stackCount++] = end;
        while (stackCount > 0)
        {
            end = stack[--stackCount];
            begin = stack[--stackCount];
            var idx = 0;
            double max_d = 0;
            while (end > begin && path[begin] == path[end])
            {
                flags[end--] = false;
            }
            for (var i = begin + 1; i < end; ++i)
            {
                // PerpendicDistFromLineSqrd - avoids expensive Sqrt()
                var d = PerpendicDistFromLineSqrd(path[i], path[begin], path[end]);
                if (d <= max_d)
                {
                    continue;
                }
                max_d = d;
                idx = i;
            }

            if (max_d <= epsSqrd)
            {
                continue;
            }
            flags[idx] = true;
            // Push right first so the left range is processed first, matching
            // the recursive implementation's observable flag-update order.
            if (idx < end - 1)
            {
                stack[stackCount++] = idx;
                stack[stackCount++] = end;
            }
            if (idx > begin + 1)
            {
                stack[stackCount++] = begin;
                stack[stackCount++] = idx;
            }
        }
    }

    private static void RDP(ReadOnlySpan<Point64> path, int begin, int end,
      double epsSqrd, Span<bool> flags)
    {
        var stackLength = path.Length * 2;
        if (path.Length <= ArrayPoolThreshold)
        {
            Span<int> stack = stackalloc int[stackLength];
            RDPCore(path, begin, end, epsSqrd, flags, stack);
            return;
        }

        var rentedStack = ArrayPool<int>.Shared.Rent(stackLength);
        try
        {
            RDPCore(path, begin, end, epsSqrd, flags, rentedStack.AsSpan(0, stackLength));
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rentedStack);
        }
    }

    private static Path64 RamerDouglasPeuckerCore(Path64 path, double epsilon, Span<bool> flags)
    {
        flags.Clear();
        flags[0] = true;
        flags[^1] = true;
        var points = CollectionsMarshal.AsSpan(path);
        var len = points.Length;
        RDP(points, 0, len - 1, Sqr(epsilon), flags);
        var resultCount = 0;
        for (var i = 0; i < len; ++i)
        {
            if (flags[i])
            {
                ++resultCount;
            }
        }
        var result = AllocatePath64(resultCount);
        var destination = CollectionsMarshal.AsSpan(result);
        for (int i = 0, write = 0; i < len; ++i)
        {
            if (flags[i])
            {
                destination[write++] = points[i];
            }
        }
        return result;
    }

    public static Path64 RamerDouglasPeucker(Path64 path, double epsilon)
    {
        var len = path.Count;
        if (len < 5)
        {
            return path;
        }
        if (len <= ArrayPoolThreshold)
        {
            Span<bool> flags = stackalloc bool[len];
            return RamerDouglasPeuckerCore(path, epsilon, flags);
        }

        var rentedFlags = ArrayPool<bool>.Shared.Rent(len);
        try
        {
            return RamerDouglasPeuckerCore(path, epsilon, rentedFlags.AsSpan(0, len));
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(rentedFlags);
        }
    }

    public static Paths64 RamerDouglasPeucker(Paths64 paths, double epsilon)
    {
        var result = AllocatePaths64(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = RamerDouglasPeucker(source[i], epsilon);
        }
        return result;
    }

    private static void RDPCore(ReadOnlySpan<PointD> path, int begin, int end,
      double epsSqrd, Span<bool> flags, Span<int> stack)
    {
        var stackCount = 0;
        stack[stackCount++] = begin;
        stack[stackCount++] = end;
        while (stackCount > 0)
        {
            end = stack[--stackCount];
            begin = stack[--stackCount];
            var idx = 0;
            double max_d = 0d;
            while (end > begin && path[begin] == path[end])
            {
                flags[end--] = false;
            }
            for (var i = begin + 1; i < end; ++i)
            {
                // PerpendicDistFromLineSqrd - avoids expensive Sqrt()
                var d = PerpendicDistFromLineSqrd(path[i], path[begin], path[end]);
                if (d <= max_d)
                {
                    continue;
                }
                max_d = d;
                idx = i;
            }

            if (max_d <= epsSqrd)
            {
                continue;
            }
            flags[idx] = true;
            if (idx < end - 1)
            {
                stack[stackCount++] = idx;
                stack[stackCount++] = end;
            }
            if (idx > begin + 1)
            {
                stack[stackCount++] = begin;
                stack[stackCount++] = idx;
            }
        }
    }

    private static void RDP(ReadOnlySpan<PointD> path, int begin, int end, double epsSqrd, Span<bool> flags)
    {
        var stackLength = checked(path.Length * 2);
        if (path.Length <= ArrayPoolThreshold)
        {
            Span<int> stack = stackalloc int[stackLength];
            RDPCore(path, begin, end, epsSqrd, flags, stack);
            return;
        }

        var rentedStack = ArrayPool<int>.Shared.Rent(stackLength);
        try
        {
            RDPCore(path, begin, end, epsSqrd, flags, rentedStack.AsSpan(0, stackLength));
        }
        finally
        {
            ArrayPool<int>.Shared.Return(rentedStack);
        }
    }

    private static PathD RamerDouglasPeuckerCore(PathD path, double epsilon, Span<bool> flags)
    {
        flags.Clear();
        flags[0] = true;
        flags[^1] = true;
        var points = CollectionsMarshal.AsSpan(path);
        var len = points.Length;
        RDP(points, 0, len - 1, Sqr(epsilon), flags);
        var resultCount = 0;
        for (var i = 0; i < len; ++i)
        {
            if (flags[i])
            {
                ++resultCount;
            }
        }
        var result = AllocatePathD(resultCount);
        var destination = CollectionsMarshal.AsSpan(result);
        for (int i = 0, write = 0; i < len; ++i)
        {
            if (flags[i])
            {
                destination[write++] = points[i];
            }
        }
        return result;
    }

    public static PathD RamerDouglasPeucker(PathD path, double epsilon)
    {
        var len = path.Count;
        if (len < 5)
        {
            return path;
        }
        if (len <= ArrayPoolThreshold)
        {
            Span<bool> flags = stackalloc bool[len];
            return RamerDouglasPeuckerCore(path, epsilon, flags);
        }

        var rentedFlags = ArrayPool<bool>.Shared.Rent(len);
        try
        {
            return RamerDouglasPeuckerCore(path, epsilon, rentedFlags.AsSpan(0, len));
        }
        finally
        {
            ArrayPool<bool>.Shared.Return(rentedFlags);
        }
    }

    public static PathsD RamerDouglasPeucker(PathsD paths, double epsilon)
    {
        var result = AllocatePathsD(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = RamerDouglasPeucker(source[i], epsilon);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InitializeCircularLinks(Span<int> previous, Span<int> next)
    {
        var high = previous.Length - 1;
        previous[0] = high;
        next[high] = 0;
        for (var i = 0; i < high; ++i)
        {
            next[i] = i + 1;
            previous[i + 1] = i;
        }
    }

    private static Path64 SimplifyPathCore(Path64 path, double epsSqr, bool isClosedPath, Span<double> dsq, Span<int> links)
    {
        int len = path.Count, high = len - 1;
        var previous = links[..len];
        var nextAlive = links.Slice(len, len);
        InitializeCircularLinks(previous, nextAlive);
        var points = CollectionsMarshal.AsSpan(path);
        var curr = 0;
        if (isClosedPath)
        {
            dsq[0] = PerpendicDistFromLineSqrd(points[0], points[high], points[1]);
            dsq[high] = PerpendicDistFromLineSqrd(points[high], points[0], points[high - 1]);
        }
        else
        {
            dsq[0] = double.MaxValue;
            dsq[high] = double.MaxValue;
        }
        for (var i = 1; i < high; ++i)
        {
            dsq[i] = PerpendicDistFromLineSqrd(points[i], points[i - 1], points[i + 1]);
        }

        for (; ; )
        {
            if (dsq[curr] > epsSqr)
            {
                var start = curr;
                do
                {
                    curr = nextAlive[curr];
                }
                while (curr != start && dsq[curr] > epsSqr);
                if (curr == start)
                {
                    break;
                }
            }

            var prev = previous[curr];
            var next = nextAlive[curr];
            if (next == prev)
            {
                break;
            }

            int prior2;
            if (dsq[next] < dsq[curr])
            {
                prior2 = prev;
                prev = curr;
                curr = next;
                next = nextAlive[next];
            }
            else
            {
                prior2 = previous[prev];
            }

            nextAlive[prev] = next;
            previous[next] = prev;
            nextAlive[curr] = -1;
            previous[curr] = -1;
            curr = next;
            next = nextAlive[next];
            if (isClosedPath || curr != high && curr != 0)
            {
                dsq[curr] = PerpendicDistFromLineSqrd(points[curr], points[prev], points[next]);
            }
            if (isClosedPath || prev != 0 && prev != high)
            {
                dsq[prev] = PerpendicDistFromLineSqrd(points[prev], points[prior2], points[curr]);
            }
        }

        var resultCount = 0;
        for (var i = 0; i < len; ++i)
        {
            if (nextAlive[i] >= 0)
            {
                ++resultCount;
            }
        }
        var result = AllocatePath64(resultCount);
        var destination = CollectionsMarshal.AsSpan(result);
        for (int i = 0, write = 0; i < len; ++i)
        {
            if (nextAlive[i] >= 0)
            {
                destination[write++] = points[i];
            }
        }
        return result;
    }

    public static Path64 SimplifyPath(Path64 path, double epsilon, bool isClosedPath = true)
    {
        var len = path.Count;
        if (len < 4)
        {
            return path;
        }

        var epsSqr = Sqr(epsilon);
        var linkCount = len * 2;
        if (len <= ArrayPoolThreshold)
        {
            Span<double> dsq = stackalloc double[len];
            Span<int> links = stackalloc int[linkCount];
            return SimplifyPathCore(path, epsSqr, isClosedPath, dsq, links);
        }

        var rentedDistances = ArrayPool<double>.Shared.Rent(len);
        int[]? rentedLinks = null;
        try
        {
            rentedLinks = ArrayPool<int>.Shared.Rent(linkCount);
            return SimplifyPathCore(path, epsSqr, isClosedPath, rentedDistances.AsSpan(0, len), rentedLinks.AsSpan(0, linkCount));
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rentedDistances);
            if (rentedLinks != null)
            {
                ArrayPool<int>.Shared.Return(rentedLinks);
            }
        }
    }

    public static Paths64 SimplifyPaths(Paths64 paths, double epsilon, bool isClosedPaths = true)
    {
        var result = AllocatePaths64(paths.Count);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = SimplifyPath(source[i], epsilon, isClosedPaths);
        }
        return result;
    }

    private static PathD SimplifyPathCore(PathD path, double epsSqr, bool isClosedPath, Span<double> dsq, Span<int> links)
    {
        int len = path.Count, high = len - 1;
        var previous = links[..len];
        var nextAlive = links.Slice(len, len);
        InitializeCircularLinks(previous, nextAlive);
        var points = CollectionsMarshal.AsSpan(path);
        var curr = 0;
        if (isClosedPath)
        {
            dsq[0] = PerpendicDistFromLineSqrd(points[0], points[high], points[1]);
            dsq[high] = PerpendicDistFromLineSqrd(points[high], points[0], points[high - 1]);
        }
        else
        {
            dsq[0] = double.MaxValue;
            dsq[high] = double.MaxValue;
        }
        for (var i = 1; i < high; ++i)
        {
            dsq[i] = PerpendicDistFromLineSqrd(points[i], points[i - 1], points[i + 1]);
        }

        for (; ; )
        {
            if (dsq[curr] > epsSqr)
            {
                var start = curr;
                do
                {
                    curr = nextAlive[curr];
                }
                while (curr != start && dsq[curr] > epsSqr);
                if (curr == start)
                {
                    break;
                }
            }

            var prev = previous[curr];
            var next = nextAlive[curr];
            if (next == prev)
            {
                break;
            }

            int prior2;
            if (dsq[next] < dsq[curr])
            {
                prior2 = prev;
                prev = curr;
                curr = next;
                next = nextAlive[next];
            }
            else
            {
                prior2 = previous[prev];
            }

            nextAlive[prev] = next;
            previous[next] = prev;
            nextAlive[curr] = -1;
            previous[curr] = -1;
            curr = next;
            next = nextAlive[next];
            if (isClosedPath || curr != high && curr != 0)
            {
                dsq[curr] = PerpendicDistFromLineSqrd(points[curr], points[prev], points[next]);
            }
            if (isClosedPath || prev != 0 && prev != high)
            {
                dsq[prev] = PerpendicDistFromLineSqrd(points[prev], points[prior2], points[curr]);
            }
        }

        var resultCount = 0;
        for (var i = 0; i < len; ++i)
        {
            if (nextAlive[i] >= 0)
            {
                ++resultCount;
            }
        }
        var result = AllocatePathD(resultCount);
        var destination = CollectionsMarshal.AsSpan(result);
        for (int i = 0, write = 0; i < len; ++i)
        {
            if (nextAlive[i] >= 0)
            {
                destination[write++] = points[i];
            }
        }
        return result;
    }

    public static PathD SimplifyPath(PathD path, double epsilon, bool isClosedPath = true)
    {
        var len = path.Count;
        if (len < 4)
        {
            return path;
        }

        var epsSqr = Sqr(epsilon);
        var linkCount = checked(len * 2);
        if (len <= ArrayPoolThreshold)
        {
            Span<double> dsq = stackalloc double[len];
            Span<int> links = stackalloc int[linkCount];
            return SimplifyPathCore(path, epsSqr, isClosedPath, dsq, links);
        }

        var rentedDistances = ArrayPool<double>.Shared.Rent(len);
        int[]? rentedLinks = null;
        try
        {
            rentedLinks = ArrayPool<int>.Shared.Rent(linkCount);
            return SimplifyPathCore(path, epsSqr, isClosedPath, rentedDistances.AsSpan(0, len), rentedLinks.AsSpan(0, linkCount));
        }
        finally
        {
            ArrayPool<double>.Shared.Return(rentedDistances);
            if (rentedLinks != null)
            {
                ArrayPool<int>.Shared.Return(rentedLinks);
            }
        }
    }

    public static PathsD SimplifyPaths(PathsD paths, double epsilon, bool isClosedPath = true)
    {
        var len = paths.Count;
        var result = AllocatePathsD(len);
        var source = CollectionsMarshal.AsSpan(paths);
        var destination = CollectionsMarshal.AsSpan(result);
        for (var i = 0; i < len; ++i)
        {
            destination[i] = SimplifyPath(source[i], epsilon, isClosedPath);
        }
        return result;
    }

    public static Path64 TrimCollinear(Path64 path, bool isOpen = false)
    {
        var len = path.Count;
        var i = 0;
        if (!isOpen)
        {
            while (i < len - 1 && InternalClipper.IsCollinear(path[len - 1], path[i], path[i + 1]))
            {
                ++i;
            }
            while (i < len - 1 && InternalClipper.IsCollinear(path[len - 2], path[len - 1], path[i]))
            {
                --len;
            }
        }

        if (len - i < 3)
        {
            if (!isOpen || len < 2 || path[0] == path[1])
            {
                return [];
            }
            return path;
        }

        Path64 result = [with(len - i)];
        var last = path[i];
        result.Add(last);
        for (++i; i < len - 1; ++i)
        {
            if (InternalClipper.IsCollinear(last, path[i], path[i + 1]))
            {
                continue;
            }
            last = path[i];
            result.Add(last);
        }

        if (isOpen)
        {
            result.Add(path[len - 1]);
        }
        else if (!InternalClipper.IsCollinear(last, path[len - 1], result[0]))
        {
            result.Add(path[len - 1]);
        }
        else
        {
            while (result.Count > 2 && InternalClipper.IsCollinear(result[^1], result[^2], result[0]))
            {
                result.RemoveAt(result.Count - 1);
            }
            if (result.Count < 3)
            {
                result.Clear();
            }
        }
        return result;
    }

    public static PathD TrimCollinear(PathD path, int precision, bool isOpen = false)
    {
        InternalClipper.CheckPrecision(precision);
        var scale = Math.Pow(10d, precision);
        var p = ScalePath64(path, scale);
        p = TrimCollinear(p, isOpen);
        return ScalePathD(p, 1d / scale);
    }

    public static PointInPolygonResult PointInPolygon(Point64 pt, Path64 polygon)
    {
        return InternalClipper.PointInPolygon(pt, polygon);
    }

    public static PointInPolygonResult PointInPolygon(PointD pt, PathD polygon, int precision = 2)
    {
        InternalClipper.CheckPrecision(precision);
        var scale = Math.Pow(10, precision);
        var p = new Point64(pt, scale);
        var path = ScalePath64(polygon, scale);
        return InternalClipper.PointInPolygon(p, path);
    }

    public static Path64 Ellipse(Point64 center, double radiusX, double radiusY = 0d, int steps = 0)
    {
        if (radiusX <= 0d)
        {
            return [];
        }
        if (radiusY <= 0d)
        {
            radiusY = radiusX;
        }
        if (steps <= 2)
        {
            steps = (int)Math.Ceiling(HalfPI * Math.Sqrt(radiusX + radiusY));
        }
        var (si, co) = Math.SinCos(DoublePI / steps);
        double dx = co, dy = si;
        var result = AllocatePath64(steps);
        var points = CollectionsMarshal.AsSpan(result);
        var centerX = center.X;
        var centerY = center.Y;
        points[0] = new Point64(centerX + radiusX, centerY);
        for (var i = 1; i < steps; ++i)
        {
            points[i] = new Point64(centerX + radiusX * dx, centerY + radiusY * dy);
            var x = dx * co - dy * si;
            dy = dy * co + dx * si;
            dx = x;
        }
        return result;
    }

    public static PathD Ellipse(PointD center, double radiusX, double radiusY = 0d, int steps = 0)
    {
        if (radiusX <= 0d)
        {
            return [];
        }
        if (radiusY <= 0d)
        {
            radiusY = radiusX;
        }
        if (steps <= 2)
        {
            steps = (int)Math.Ceiling(HalfPI * Math.Sqrt(radiusX + radiusY));
        }

        var (si, co) = Math.SinCos(DoublePI / steps);

        double dx = co, dy = si;
        var result = AllocatePathD(steps);
        var points = CollectionsMarshal.AsSpan(result);
        var centerX = center.x;
        var centerY = center.y;
        points[0] = new PointD(centerX + radiusX, centerY);
        for (var i = 1; i < steps; ++i)
        {
            points[i] = new PointD(centerX + radiusX * dx, centerY + radiusY * dy);
            var x = dx * co - dy * si;
            dy = dy * co + dx * si;
            dx = x;
        }
        return result;
    }

    private static void ShowPolyPathStructure(PolyPath64 pp, int level)
    {
        var spaces = new string(' ', level * 2);
        var caption = pp.IsHole ? "Hole " : "Outer ";
        if (pp.Count == 0)
        {
            Console.WriteLine(spaces + caption);
        }
        else
        {
            Console.WriteLine(spaces + caption + $"({pp.Count})");
            var count = pp.Count;
            for (var i = 0; i < count; ++i)
            {
                var child = pp[i];
                ShowPolyPathStructure(child, level + 1);
            }
        }
    }

    public static void ShowPolyTreeStructure(PolyTree64 polytree)
    {
        Console.WriteLine("Polytree Root");
        var count = polytree.Count;
        for (var i = 0; i < count; ++i)
        {
            var child = polytree[i];
            ShowPolyPathStructure(child, 1);
        }
    }

    private static void ShowPolyPathStructure(PolyPathD pp, int level)
    {
        var spaces = new string(' ', level * 2);
        var caption = pp.IsHole ? "Hole " : "Outer ";
        if (pp.Count == 0)
        {
            Console.WriteLine(spaces + caption);
        }
        else
        {
            Console.WriteLine(spaces + caption + $"({pp.Count})");
            var count = pp.Count;
            for (var i = 0; i < count; ++i)
            {
                var child = pp[i];
                ShowPolyPathStructure(child, level + 1);
            }
        }
    }

    public static void ShowPolyTreeStructure(PolyTreeD polytree)
    {
        Console.WriteLine("Polytree Root");
        var count = polytree.Count;
        for (var i = 0; i < count; ++i)
        {
            var child = polytree[i];
            ShowPolyPathStructure(child, 1);
        }
    }
}
