/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Date      :  22 January 2025                                                 *
* Website   :  https://www.angusj.com                                          *
* Copyright :  Angus Johnson 2010-2025                                         *
* Purpose   :  Core structures and functions for the Clipper Library           *
* License   :  https://www.boost.org/LICENSE_1_0.txt                           *
*******************************************************************************/

namespace Clipper2Lib;

[SkipLocalsInit]
public struct Point64 : IEquatable<Point64>
{
    public long X;
    public long Y;

#if USINGZ
	public long Z;
#endif

    public Point64(Point64 pt)
    {
        X = pt.X;
        Y = pt.Y;
#if USINGZ
		Z = pt.Z;
#endif
    }

    public Point64(Point64 pt, double scale)
    {
        X = (long)Math.Round(pt.X * scale, MidpointRounding.AwayFromZero);
        Y = (long)Math.Round(pt.Y * scale, MidpointRounding.AwayFromZero);
#if USINGZ
		Z = (long) Math.Round(pt.Z * scale, MidpointRounding.AwayFromZero);
#endif
    }

    public Point64(long x, long y
#if USINGZ
		, long z = 0
#endif
    )
    {
        X = x;
        Y = y;
#if USINGZ
		Z = z;
#endif
    }

    public Point64(double x, double y
#if USINGZ
		, double z = 0.0
#endif
    )
    {
        X = (long)Math.Round(x, MidpointRounding.AwayFromZero);
        Y = (long)Math.Round(y, MidpointRounding.AwayFromZero);
#if USINGZ
  Z = (long) Math.Round(z, MidpointRounding.AwayFromZero);
#endif
    }

    public Point64(PointD pt)
    {
        X = (long)Math.Round(pt.x, MidpointRounding.AwayFromZero);
        Y = (long)Math.Round(pt.y, MidpointRounding.AwayFromZero);
#if USINGZ
  Z = pt.z;
#endif
    }

    public Point64(PointD pt, double scale)
    {
        X = (long)Math.Round(pt.x * scale, MidpointRounding.AwayFromZero);
        Y = (long)Math.Round(pt.y * scale, MidpointRounding.AwayFromZero);
#if USINGZ
  Z = pt.z;
#endif
    }

    public static bool operator ==(Point64 lhs, Point64 rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;

    public static bool operator !=(Point64 lhs, Point64 rhs) => lhs.X != rhs.X || lhs.Y != rhs.Y;

    public static Point64 operator +(Point64 lhs, Point64 rhs) => new(lhs.X + rhs.X, lhs.Y + rhs.Y
#if USINGZ
    	, lhs.Z + rhs.Z
#endif
        );

    public static Point64 operator -(Point64 lhs, Point64 rhs) => new(lhs.X - rhs.X, lhs.Y - rhs.Y
#if USINGZ
    	, lhs.Z - rhs.Z
#endif
        );

    public override readonly string ToString()
    {
        // nb: trailing space
#if USINGZ
		return $"{X},{Y},{Z} ";
#else
        return $"{X},{Y} ";
#endif
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is Point64 p)
        {
            return this == p;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(Point64 other) => this == other;

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(X, Y); //#599
    }
}

[SkipLocalsInit]
public struct PointD : IEquatable<PointD>
{
    public double x;
    public double y;

#if USINGZ
	public long z;
#endif

    public PointD(PointD pt)
    {
        x = pt.x;
        y = pt.y;
#if USINGZ
	z = pt.z;
#endif
    }

    public PointD(Point64 pt)
    {
        x = pt.X;
        y = pt.Y;
#if USINGZ
	z = pt.Z;
#endif
    }

    public PointD(Point64 pt, double scale)
    {
        x = pt.X * scale;
        y = pt.Y * scale;
#if USINGZ
		z = pt.Z;
#endif
    }

    public PointD(PointD pt, double scale)
    {
        x = pt.x * scale;
        y = pt.y * scale;
#if USINGZ
		z = pt.z;
#endif
    }

    public PointD(long x, long y
#if USINGZ
		, long z = 0
#endif
    )
    {
        this.x = x;
        this.y = y;
#if USINGZ
		this.z = z;
#endif
    }

    public PointD(double x, double y
#if USINGZ
		, long z = 0
#endif
    )
    {
        this.x = x;
        this.y = y;
#if USINGZ
		this.z = z;
#endif
    }

    public readonly string ToString(int precision = 2)
    {
#if USINGZ
		return string.Format($"{{0:F{precision}}},{{1:F{precision}}},{{2:D}}", x,y,z);
#else
        return string.Format($"{{0:F{precision}}},{{1:F{precision}}}", x, y);
#endif
    }

    public static bool operator ==(PointD lhs, PointD rhs) => InternalClipper.IsAlmostZero(lhs.x - rhs.x) && InternalClipper.IsAlmostZero(lhs.y - rhs.y);

    public static bool operator !=(PointD lhs, PointD rhs) => !InternalClipper.IsAlmostZero(lhs.x - rhs.x) || !InternalClipper.IsAlmostZero(lhs.y - rhs.y);

    public override readonly bool Equals(object? obj)
    {
        if (obj is PointD p)
        {
            return this == p;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Equals(PointD other) => this == other;

    public void Negate()
    {
        x = -x;
        y = -y;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(x, y); //#599
    }
}

[SkipLocalsInit]
public struct Rect64
{
    public long left;
    public long top;
    public long right;
    public long bottom;

    public Rect64(long l, long t, long r, long b)
    {
        left = l;
        top = t;
        right = r;
        bottom = b;
    }

    public Rect64(bool isValid)
    {
        if (isValid)
        {
            left = 0;
            top = 0;
            right = 0;
            bottom = 0;
        }
        else
        {
            left = long.MaxValue;
            top = long.MaxValue;
            right = long.MinValue;
            bottom = long.MinValue;
        }
    }

    public Rect64(Rect64 rec)
    {
        left = rec.left;
        top = rec.top;
        right = rec.right;
        bottom = rec.bottom;
    }

    public long Width
    {
        readonly get => right - left;
        set => right = left + value;
    }

    public long Height
    {
        readonly get => bottom - top;
        set => bottom = top + value;
    }

    public readonly bool IsEmpty()
    {
        return bottom <= top || right <= left;
    }

    public readonly bool IsValid()
    {
        return left < long.MaxValue;
    }

    public readonly Point64 MidPoint()
    {
        return new Point64((left + right) * 0.5d, (top + bottom) * 0.5d);
    }

    public readonly bool Contains(Point64 pt)
    {
        return pt.X > left && pt.X < right && pt.Y > top && pt.Y < bottom;
    }

    public readonly bool Contains(Rect64 rec)
    {
        return rec.left >= left && rec.right <= right && rec.top >= top && rec.bottom <= bottom;
    }

    public readonly bool Intersects(Rect64 rec)
    {
        return Math.Max(left, rec.left) <= Math.Min(right, rec.right) && Math.Max(top, rec.top) <= Math.Min(bottom, rec.bottom);
    }

    public readonly Path64 AsPath()
    {
        var result = new Path64(4);
        CollectionsMarshal.SetCount(result, 4);
        var points = CollectionsMarshal.AsSpan(result);
        points[0] = new Point64(left, top);
        points[1] = new Point64(right, top);
        points[2] = new Point64(right, bottom);
        points[3] = new Point64(left, bottom);
        return result;
    }
}

[SkipLocalsInit]
public struct RectD
{
    public double left;
    public double top;
    public double right;
    public double bottom;

    public RectD(double l, double t, double r, double b)
    {
        left = l;
        top = t;
        right = r;
        bottom = b;
    }

    public RectD(RectD rec)
    {
        left = rec.left;
        top = rec.top;
        right = rec.right;
        bottom = rec.bottom;
    }

    public RectD(bool isValid)
    {
        if (isValid)
        {
            left = 0d;
            top = 0d;
            right = 0d;
            bottom = 0d;
        }
        else
        {
            left = double.MaxValue;
            top = double.MaxValue;
            right = -double.MaxValue;
            bottom = -double.MaxValue;
        }
    }
    public double Width
    {
        readonly get => right - left;
        set => right = left + value;
    }

    public double Height
    {
        readonly get => bottom - top;
        set => bottom = top + value;
    }

    public readonly bool IsEmpty()
    {
        return bottom <= top || right <= left;
    }

    public readonly PointD MidPoint()
    {
        return new PointD((left + right) * 0.5d, (top + bottom) * 0.5d);
    }

    public readonly bool Contains(PointD pt)
    {
        return pt.x > left && pt.x < right && pt.y > top && pt.y < bottom;
    }

    public readonly bool Contains(RectD rec)
    {
        return rec.left >= left && rec.right <= right && rec.top >= top && rec.bottom <= bottom;
    }

    public readonly bool Intersects(RectD rec)
    {
        return Math.Max(left, rec.left) < Math.Min(right, rec.right) && Math.Max(top, rec.top) < Math.Min(bottom, rec.bottom);
    }

    public readonly PathD AsPath()
    {
        var result = new PathD(4);
        CollectionsMarshal.SetCount(result, 4);
        var points = CollectionsMarshal.AsSpan(result);
        points[0] = new PointD(left, top);
        points[1] = new PointD(right, top);
        points[2] = new PointD(right, bottom);
        points[3] = new PointD(left, bottom);
        return result;
    }
}

[SkipLocalsInit]
public sealed class Path64 : List<Point64>
{
    public Path64() : base() { }
    public Path64(int capacity = 0) : base(capacity) { }
    public Path64(IEnumerable<Point64> path) : base(path) { }
    public override string ToString()
    {
        if (Count == 0)
        {
            return string.Empty;
        }
        var result = new StringBuilder(Count * 16);
        var points = CollectionsMarshal.AsSpan(this);
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var point = ref points[i];
            result.Append(point.X).Append(',').Append(point.Y);
#if USINGZ
			result.Append(',').Append(point.Z);
#endif
            result.Append(' ');
            if (i + 1 < len)
            {
                result.Append(", ");
            }
        }
        return result.ToString();
    }
}

[SkipLocalsInit]
public sealed class Paths64 : List<Path64>
{
    public Paths64() : base() { }
    public Paths64(int capacity = 0) : base(capacity) { }

    public override string ToString()
    {
        var result = new StringBuilder();
        var paths = CollectionsMarshal.AsSpan(this);
        var len = paths.Length;
        for (var i = 0; i < len; ++i)
        {
            result.Append(paths[i]).Append('\n');
        }
        return result.ToString();
    }
}

[SkipLocalsInit]
public sealed class PathD : List<PointD>
{
    public PathD() : base() { }
    public PathD(int capacity = 0) : base(capacity) { }

    public string ToString(int precision = 2)
    {
        var result = new StringBuilder();
        var points = CollectionsMarshal.AsSpan(this);
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            if (i > 0)
            {
                result.Append(", ");
            }
            result.Append(points[i].ToString(precision));
        }
        return result.ToString();
    }
}

[SkipLocalsInit]
public sealed class PathsD : List<PathD>
{
    public PathsD() : base() { }
    public PathsD(int capacity = 0) : base(capacity) { }

    public string ToString(int precision = 2)
    {
        var result = new StringBuilder();
        var paths = CollectionsMarshal.AsSpan(this);
        var len = paths.Length;
        for (var i = 0; i < len; ++i)
        {
            result.Append(paths[i].ToString(precision)).Append('\n');
        }
        return result.ToString();
    }
}

// Note: all clipping operations except for Difference are commutative.
public enum ClipType
{
    NoClip,
    Intersection,
    Union,
    Difference,
    Xor
}

public enum PathType
{
    Subject,
    Clip
}

// By far the most widely used filling rules for polygons are EvenOdd
// and NonZero, sometimes called Alternate and Winding respectively.
// https://en.wikipedia.org/wiki/Nonzero-rule
public enum FillRule
{
    EvenOdd,
    NonZero,
    Positive,
    Negative
}

// PointInPolygon
internal enum PipResult
{
    Inside,
    Outside,
    OnEdge
}

[SkipLocalsInit]
public static class InternalClipper
{
    internal const long MaxInt64 = 9223372036854775807;
    internal const long MaxCoord = MaxInt64 / 4;
    internal const double max_coord = MaxCoord;
    internal const double min_coord = -MaxCoord;
    internal const long Invalid64 = MaxInt64;

    internal const double floatingPointTolerance = 1E-12;
    internal const double defaultMinimumEdgeLength = 0.1;

    private static readonly string precision_range_error = "Error: Precision is out of range.";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CrossProduct(Point64 pt1, Point64 pt2, Point64 pt3)
    {
        // typecast to double to avoid potential int overflow
        return (double)(pt2.X - pt1.X) * (pt3.Y - pt2.Y) - (double)(pt2.Y - pt1.Y) * (pt3.X - pt2.X);
    }

#if USINGZ
	public static Path64 SetZ(Path64 path, long Z)
	{
	Path64 result = new Path64(path.Count);
	CollectionsMarshal.SetCount(result, path.Count);
	var source = CollectionsMarshal.AsSpan(path);
	var destination = CollectionsMarshal.AsSpan(result);
	var len = source.Length;
	for (var i = 0; i < len; ++i)
	{
		destination[i] = new Point64(source[i].X, source[i].Y, Z);
	}
	return result;
	}
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void CheckPrecision(int precision)
    {
        if (precision is < -8 or > 8)
        {
            throw new Exception(precision_range_error);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsAlmostZero(double value)
    {
        return Math.Abs(value) <= floatingPointTolerance;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int TriSign(long x) // returns 0, 1 or -1
    {
        return x < 0 ? -1 : x > 0 ? 1 : 0;
    }

    public struct UInt128Struct
    {
        public ulong lo64;
        public ulong hi64;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128Struct MultiplyUInt64(ulong a, ulong b) // #834,#835
    {
        var product = (UInt128)a * b;
        return new UInt128Struct
        {
            lo64 = (ulong)product,
            hi64 = (ulong)(product >> 64)
        };
    }

    // returns true if (and only if) a * b == c * d
    internal static bool ProductsAreEqual(long a, long b, long c, long d)
    {
        return (Int128)a * b == (Int128)c * d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsCollinear(Point64 pt1, Point64 sharedPt, Point64 pt2)
    {
        var a = sharedPt.X - pt1.X;
        var b = pt2.Y - sharedPt.Y;
        var c = sharedPt.Y - pt1.Y;
        var d = pt2.X - sharedPt.X;
        // When checking for collinearity with very large coordinate values
        // then ProductsAreEqual is more accurate than using CrossProduct.
        return ProductsAreEqual(a, b, c, d);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double DotProduct(Point64 pt1, Point64 pt2, Point64 pt3)
    {
        // typecast to double to avoid potential int overflow
        return (double)(pt2.X - pt1.X) * (pt3.X - pt2.X) + (double)(pt2.Y - pt1.Y) * (pt3.Y - pt2.Y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double CrossProduct(PointD vec1, PointD vec2)
    {
        return vec1.y * vec2.x - vec2.y * vec1.x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double DotProduct(PointD vec1, PointD vec2)
    {
        return vec1.x * vec2.x + vec1.y * vec2.y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static long CheckCastInt64(double val)
    {
        if (val is >= max_coord or <= min_coord)
        {
            return Invalid64;
        }
        return (long)Math.Round(val, MidpointRounding.AwayFromZero);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GetSegmentIntersectPt(Point64 ln1a, Point64 ln1b, Point64 ln2a, Point64 ln2b, out Point64 ip)
    {
        double dy1 = ln1b.Y - ln1a.Y;
        double dx1 = ln1b.X - ln1a.X;
        double dy2 = ln2b.Y - ln2a.Y;
        double dx2 = ln2b.X - ln2a.X;
        var det = dy1 * dx2 - dy2 * dx1;
        if (det == 0.0)
        {
            ip = new Point64();
            return false;
        }

        var t = ((ln1a.X - ln2a.X) * dy2 - (ln1a.Y - ln2a.Y) * dx2) / det;
        if (t <= 0.0)
        {
            ip = ln1a;
        }
        else if (t >= 1.0)
        {
            ip = ln1b;
        }
        else
        {
            // avoid using constructor (and rounding too) as they affect performance //664
            ip.X = (long)(ln1a.X + t * dx1);
            ip.Y = (long)(ln1a.Y + t * dy1);
#if USINGZ
			ip.Z = 0L;
#endif
        }
        return true;
    }

    internal static bool SegsIntersect(Point64 seg1a, Point64 seg1b, Point64 seg2a, Point64 seg2b, bool inclusive = false)
    {
        double dy1 = seg1b.Y - seg1a.Y;
        double dx1 = seg1b.X - seg1a.X;
        double dy2 = seg2b.Y - seg2a.Y;
        double dx2 = seg2b.X - seg2a.X;
        var cp = dy1 * dx2 - dy2 * dx1;
        if (cp == 0d)
        {
            return false;
        }

        var t = (seg1a.X - seg2a.X) * dy2 - (seg1a.Y - seg2a.Y) * dx2;
        if (inclusive)
        {
            if (t == 0d)
            {
                return true;
            }
            if (t > 0d)
            {
                if (cp < 0d || t > cp)
                {
                    return false;
                }
            }
            else if (cp > 0d || t < cp)
            {
                return false;
            }

            t = (seg1a.X - seg2a.X) * dy1 - (seg1a.Y - seg2a.Y) * dx1;
            if (t == 0d)
            {
                return true;
            }
            return t > 0d ? cp > 0d && t <= cp : cp < 0d && t >= cp;
        }

        if (t == 0d)
        {
            return false;
        }
        if (t > 0d)
        {
            if (cp < 0d || t >= cp)
            {
                return false;
            }
        }
        else if (cp > 0d || t <= cp)
        {
            return false;
        }

        t = (seg1a.X - seg2a.X) * dy1 - (seg1a.Y - seg2a.Y) * dx1;
        if (t == 0d)
        {
            return false;
        }
        return t > 0d ? cp > 0d && t < cp : cp < 0d && t > cp;
    }

    public static Rect64 GetBounds(Path64 path)
    {
        if (path.Count == 0)
        {
            return new Rect64();
        }
        var result = Clipper.InvalidRect64;
        var points = CollectionsMarshal.AsSpan(path);
        var len = points.Length;

        for (var i = 0; i < len; ++i)
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
        return result;
    }

    public static Point64 GetClosestPtOnSegment(Point64 offPt, Point64 seg1, Point64 seg2)
    {
        if (seg1.X == seg2.X && seg1.Y == seg2.Y)
        {
            return seg1;
        }
        double dx = seg2.X - seg1.X;
        double dy = seg2.Y - seg1.Y;
        var q = ((offPt.X - seg1.X) * dx + (offPt.Y - seg1.Y) * dy) / (dx * dx + dy * dy);
        if (q < 0d)
        {
            q = 0d;
        }
        else if (q > 1d)
        {
            q = 1d;
        }
        return new Point64(seg1.X + Math.Round(q * dx, MidpointRounding.ToEven),
            seg1.Y + Math.Round(q * dy, MidpointRounding.ToEven)); // use MidpointRounding.ToEven in order to explicitly match the nearbyint behaviour on the C++ side
    }

    public static PointInPolygonResult PointInPolygon(Point64 pt, Path64 polygon)
    {
        var points = CollectionsMarshal.AsSpan(polygon);
        int len = points.Length, start = 0;
        if (len < 3)
        {
            return PointInPolygonResult.IsOutside;
        }

        var ptY = pt.Y;
        while (start < len && points[start].Y == ptY)
        {
            ++start;
        }
        if (start == len)
        {
            return PointInPolygonResult.IsOutside;
        }

        double d;
        bool isAbove = points[start].Y < ptY, startingAbove = isAbove;
        int val = 0, i = start + 1, end = len;
        var ptX = pt.X;
        while (true)
        {
            if (i == end)
            {
                if (end == 0 || start == 0)
                {
                    break;
                }
                end = start;
                i = 0;
            }

            if (isAbove)
            {
                while (i < end && points[i].Y < ptY)
                {
                    ++i;
                }
            }
            else
            {
                while (i < end && points[i].Y > ptY)
                {
                    ++i;
                }
            }

            if (i == end)
            {
                continue;
            }

            Point64 curr = points[i], prev;
            if (i > 0)
            {
                prev = points[i - 1];
            }
            else
            {
                prev = points[len - 1];
            }

            if (curr.Y == ptY)
            {
                if (curr.X == ptX || curr.Y == prev.Y && (ptX < prev.X) != (ptX < curr.X))
                {
                    return PointInPolygonResult.IsOn;
                }
                ++i;
                if (i == start)
                {
                    break;
                }
                continue;
            }

            // if (pt.X < curr.X && pt.X < prev.X)
            // {
            // 	// we're only interested in edges crossing on the left
            // }
            // else
            if (ptX > prev.X && ptX > curr.X)
            {
                val = 1 - val; // toggle val
            }
            else
            {
                d = CrossProduct(prev, curr, pt);
                if (d == 0d)
                {
                    return PointInPolygonResult.IsOn;
                }
                if ((d < 0d) == isAbove)
                {
                    val = 1 - val;
                }
            }
            isAbove = !isAbove;
            ++i;
        }

        if (isAbove == startingAbove)
        {
            return val == 0 ? PointInPolygonResult.IsOutside : PointInPolygonResult.IsInside;
        }
        if (i == len)
        {
            i = 0;
        }
        d = i == 0 ? CrossProduct(points[len - 1], points[0], pt) : CrossProduct(points[i - 1], points[i], pt);
        if (d == 0d)
        {
            return PointInPolygonResult.IsOn;
        }
        if ((d < 0d) == isAbove)
        {
            val = 1 - val;
        }

        return val == 0 ? PointInPolygonResult.IsOutside : PointInPolygonResult.IsInside;
    }

    public static bool Path2ContainsPath1(Path64 path1, Path64 path2)
    {
        // we need to make some accommodation for rounding errors
        // so we won't jump if the first vertex is found outside
        var pip = PointInPolygonResult.IsOn;
        var points = CollectionsMarshal.AsSpan(path1);
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var pt = ref points[i];
            switch (PointInPolygon(pt, path2))
            {
                case PointInPolygonResult.IsOutside:
                    if (pip == PointInPolygonResult.IsOutside)
                    {
                        return false;
                    }
                    pip = PointInPolygonResult.IsOutside;
                    break;
                case PointInPolygonResult.IsInside:
                    if (pip == PointInPolygonResult.IsInside)
                    {
                        return true;
                    }
                    pip = PointInPolygonResult.IsInside;
                    break;
                default:
                    break;
            }
        }
        // since path1's location is still equivocal, check its midpoint
        var mp = GetBounds(path1).MidPoint();
        return PointInPolygon(mp, path2) != PointInPolygonResult.IsOutside;
    }
}
