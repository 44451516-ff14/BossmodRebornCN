/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Date      :  30 May 2025                                                     *
* Website   :  https://www.angusj.com                                          *
* Copyright :  Angus Johnson 2010-2025                                         *
* Purpose   :  This is the main polygon clipping module                        *
* Thanks    :  Special thanks to Thong Nguyen, Guus Kuiper, Phil Stopford,     *
*           :  and Daniel Gosnell for their invaluable assistance with C#.     *
* License   :  https://www.boost.org/LICENSE_1_0.txt                           *
*******************************************************************************/

namespace Clipper2Lib;

// Vertex: a pre-clipping data structure. It is used to separate polygons
// into ascending and descending 'bounds' (or sides) that start at local
// minima and ascend to a local maxima, before descending again.
public enum PointInPolygonResult
{
    IsOn = 0,
    IsInside = 1,
    IsOutside = 2
}

internal enum VertexFlags
{
    None = 0,
    OpenStart = 1,
    OpenEnd = 2,
    LocalMax = 4,
    LocalMin = 8
}

[SkipLocalsInit]
internal sealed class Vertex(Point64 pt, VertexFlags flags, Vertex? prev)
{
    public Point64 pt = pt;
    public Vertex? next = null;
    public Vertex? prev = prev;
    public VertexFlags flags = flags;
}

[SkipLocalsInit]
internal readonly struct LocalMinima(Vertex vertex, PathType polytype, bool isOpen = false)
{
    public readonly Vertex vertex = vertex;
    public readonly PathType polytype = polytype;
    public readonly bool isOpen = isOpen;

    public static bool operator ==(LocalMinima lm1, LocalMinima lm2) => ReferenceEquals(lm1.vertex, lm2.vertex);

    public static bool operator !=(LocalMinima lm1, LocalMinima lm2) => !(lm1 == lm2);

    public override bool Equals(object? obj)
    {
        return obj is LocalMinima minima && this == minima;
    }

    public override int GetHashCode()
    {
        return vertex.GetHashCode();
    }
}

// IntersectNode: a structure representing 2 intersecting edges.
// Intersections must be sorted so they are processed from the largest
// Y coordinates to the smallest while keeping edges adjacent.
[SkipLocalsInit]
internal readonly struct IntersectNode(Point64 pt, Active edge1, Active edge2)
{
    public readonly Point64 pt = pt;
    public readonly Active edge1 = edge1;
    public readonly Active edge2 = edge2;
}

[SkipLocalsInit]
internal struct LocMinSorter : IComparer<LocalMinima>
{
    public readonly int Compare(LocalMinima locMin1, LocalMinima locMin2)
    {
        return locMin2.vertex.pt.Y.CompareTo(locMin1.vertex.pt.Y);
    }
}

// OutPt: vertex data structure for clipping solutions
[SkipLocalsInit]
internal sealed class OutPt
{
    public Point64 pt;
    public OutPt? next;
    public OutPt prev;
    public OutRec outrec;
    public HorzSegment? horz;

    public OutPt(Point64 pt, OutRec outrec)
    {
        this.pt = pt;
        this.outrec = outrec;
        next = this;
        prev = this;
        horz = null;
    }
}

internal enum JoinWith { None, Left, Right }
internal enum HorzPosition { Bottom, Middle, Top }

// OutRec: path data structure for clipping solutions
[SkipLocalsInit]
internal sealed class OutRec
{
    public int idx;
    public int outPtCount;
    public OutRec? owner;
    public Active? frontEdge;
    public Active? backEdge;
    public OutPt? pts;
    public PolyPathBase? polypath;
    public Rect64 bounds;
    public Path64? path;
    public bool isOpen;
    public List<int>? splits;
    public OutRec? recursiveSplit;
}

[SkipLocalsInit]
internal sealed class HorzSegment(OutPt op)
{
    public OutPt? leftOp = op;
    public OutPt? rightOp = null;
    public bool leftToRight = true;
}

[SkipLocalsInit]
internal sealed class HorzJoin(OutPt ltor, OutPt rtol)
{
    public OutPt? op1 = ltor;
    public OutPt? op2 = rtol;
}

///////////////////////////////////////////////////////////////////
// Important: UP and DOWN here are premised on Y-axis positive down
// displays, which is the orientation used in Clipper's development.
///////////////////////////////////////////////////////////////////
[SkipLocalsInit]
internal sealed class Active
{
    public Point64 bot;
    public Point64 top;
    public long curX; // current (updated at every new scanline)
    public double dx;
    public int windDx; // 1 or -1 depending on winding direction
    public int windCount;
    public int windCount2; // winding count of the opposite polytype
    public OutRec? outrec;

    // AEL: 'active edge list' (Vatti's AET - active edge table)
    //     a linked list of all edges (from left to right) that are present
    //     (or 'active') within the current scanbeam (a horizontal 'beam' that
    //     sweeps from bottom to top over the paths in the clipping operation).
    public Active? prevInAEL;
    public Active? nextInAEL;

    // SEL: 'sorted edge list' (Vatti's ST - sorted table)
    //     linked list used when sorting edges into their new positions at the
    //     top of scanbeams, but also (re)used to process horizontals.
    public Active? prevInSEL;
    public Active? nextInSEL;
    public Active? jump;
    public Vertex? vertexTop;
    public LocalMinima localMin; // the bottom of an edge 'bound' (also Vatti)
    internal bool isLeftBound;
    internal JoinWith joinWith;
}

[SkipLocalsInit]
internal static class ClipperEngine
{
    internal static void AddLocMin(Vertex vert, PathType polytype, bool isOpen,
      List<LocalMinima> minimaList)
    {
        // make sure the vertex is added only once ...
        if ((vert.flags & VertexFlags.LocalMin) != VertexFlags.None)
        {
            return;
        }
        vert.flags |= VertexFlags.LocalMin;

        LocalMinima lm = new LocalMinima(vert, polytype, isOpen);
        minimaList.Add(lm);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void EnsureCapacity<T>(this List<T> list, int minCapacity)
    {
        if (list.Capacity < minCapacity)
        {
            list.Capacity = minCapacity;
        }
    }

    private static void AddPathToVertexListCore(ReadOnlySpan<Point64> points, PathType polytype, bool isOpen, List<LocalMinima> minimaList, VertexPoolList vertexList)
    {
        Vertex? v0 = null, prev_v = null, curr_v;
        var len = points.Length;
        for (var i = 0; i < len; ++i)
        {
            var pt = points[i];
            if (v0 == null)
            {
                v0 = vertexList.Add(pt, VertexFlags.None, null);
                prev_v = v0;
            }
            else if (prev_v!.pt != pt) // ie skips duplicates
            {
                curr_v = vertexList.Add(pt, VertexFlags.None, prev_v);
                prev_v.next = curr_v;
                prev_v = curr_v;
            }
        }
        if (prev_v?.prev == null)
        {
            return;
        }
        if (!isOpen && prev_v.pt == v0!.pt)
        {
            prev_v = prev_v.prev;
        }
        prev_v.next = v0;
        v0!.prev = prev_v;
        if (!isOpen && prev_v.next == prev_v)
        {
            return;
        }

        // OK, we have a valid path
        bool going_up;
        if (isOpen)
        {
            curr_v = v0.next;
            while (curr_v != v0 && curr_v!.pt.Y == v0.pt.Y)
            {
                curr_v = curr_v.next;
            }
            going_up = curr_v.pt.Y <= v0.pt.Y;
            if (going_up)
            {
                v0.flags = VertexFlags.OpenStart;
                AddLocMin(v0, polytype, true, minimaList);
            }
            else
                v0.flags = VertexFlags.OpenStart | VertexFlags.LocalMax;
        }
        else // closed path
        {
            prev_v = v0.prev;
            while (prev_v != v0 && prev_v!.pt.Y == v0.pt.Y)
            {
                prev_v = prev_v.prev;
            }
            if (prev_v == v0)
            {
                return; // only open paths can be completely flat
            }
            going_up = prev_v.pt.Y > v0.pt.Y;
        }

        var going_up0 = going_up;
        prev_v = v0;
        curr_v = v0.next;
        while (curr_v != v0)
        {
            if (curr_v!.pt.Y > prev_v.pt.Y && going_up)
            {
                prev_v.flags |= VertexFlags.LocalMax;
                going_up = false;
            }
            else if (curr_v.pt.Y < prev_v.pt.Y && !going_up)
            {
                going_up = true;
                AddLocMin(prev_v, polytype, isOpen, minimaList);
            }
            prev_v = curr_v;
            curr_v = curr_v.next;
        }

        if (isOpen)
        {
            prev_v.flags |= VertexFlags.OpenEnd;
            if (going_up)
            {
                prev_v.flags |= VertexFlags.LocalMax;
            }
            else
            {
                AddLocMin(prev_v, polytype, isOpen, minimaList);
            }
        }
        else if (going_up != going_up0)
        {
            if (going_up0)
            {
                AddLocMin(prev_v, polytype, false, minimaList);
            }
            else
            {
                prev_v.flags |= VertexFlags.LocalMax;
            }
        }
    }

    internal static void AddPathToVertexList(Path64 path, PathType polytype, bool isOpen, List<LocalMinima> minimaList, VertexPoolList vertexList)
    {
        AddPathToVertexList(CollectionsMarshal.AsSpan(path), polytype, isOpen, minimaList, vertexList);
    }

    internal static void AddPathToVertexList(ReadOnlySpan<Point64> points, PathType polytype, bool isOpen, List<LocalMinima> minimaList, VertexPoolList vertexList)
    {
        vertexList.EnsureCapacity(vertexList.Count + points.Length);
        AddPathToVertexListCore(points, polytype, isOpen, minimaList, vertexList);
    }

    internal static void AddPathsToVertexList(Paths64 paths, PathType polytype, bool isOpen, List<LocalMinima> minimaList, VertexPoolList vertexList)
    {
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var totalVertCnt = 0;
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            totalVertCnt += pathSpan[i].Count;
        }
        vertexList.EnsureCapacity(vertexList.Count + totalVertCnt);

        for (var i = 0; i < len; ++i)
        {
            AddPathToVertexListCore(CollectionsMarshal.AsSpan(pathSpan[i]), polytype, isOpen, minimaList, vertexList);
        }
    }
}

[SkipLocalsInit]
public sealed class ReuseableDataContainer64
{
    private static readonly IComparer<LocalMinima> LocMinComparer = new LocMinSorter();
    internal readonly List<LocalMinima> _minimaList;
    internal readonly VertexPoolList _vertexList;
    private bool _isSortedMinimaList;

    public ReuseableDataContainer64()
    {
        _minimaList = [];
        _vertexList = new VertexPoolList();
        _isSortedMinimaList = true;
    }

    public void Clear()
    {
        _minimaList.Clear();
        _vertexList.Clear();
        _isSortedMinimaList = true;
    }

    public void AddPaths(Paths64 paths, PathType pt, bool isOpen)
    {
        _isSortedMinimaList = false;
        ClipperEngine.AddPathsToVertexList(paths, pt, isOpen, _minimaList, _vertexList);
    }

    internal void AddPath(Path64 path, PathType pt, bool isOpen)
    {
        AddPath(CollectionsMarshal.AsSpan(path), pt, isOpen);
    }

    internal void AddPath(ReadOnlySpan<Point64> path, PathType pt, bool isOpen)
    {
        _isSortedMinimaList = false;
        ClipperEngine.AddPathToVertexList(path, pt, isOpen, _minimaList, _vertexList);
    }

    internal ReadOnlySpan<LocalMinima> GetSortedMinima()
    {
        if (!_isSortedMinimaList)
        {
            _minimaList.Sort(LocMinComparer);
            _isSortedMinimaList = true;
        }
        return CollectionsMarshal.AsSpan(_minimaList);
    }
}

[SkipLocalsInit]
public class ClipperBase
{
    private const int ScanlineHeapThreshold = 32;
    private static readonly IComparer<LocalMinima> LocMinComparer = new LocMinSorter();
    private static readonly IComparer<IntersectNode> IntersectComparer = new IntersectListSort();
    private static readonly IComparer<HorzSegment?> HorzSegmentComparer = Comparer<HorzSegment?>.Create(HorzSegSort);

    private ClipType _cliptype;
    private FillRule _fillrule;
    private Active? _actives;
    private Active? _sel;
    private readonly List<LocalMinima> _minimaList;
    private readonly List<IntersectNode> _intersectList;
    private readonly VertexPoolList _vertexList;
    private readonly OutRecPoolList _outrecList;
    private readonly List<long> _scanlineList;
    private HashSet<long>? _scanlineSet;
    private bool _scanlineIsHeap;
    private readonly HorzSegmentPoolList _horzSegList;
    private readonly HorzJoinPoolList _horzJoinList;
    private readonly OutPtPoolList _outPtPool;
    private Active? _freeActives;
    private int _currentLocMin;
    private long _currentBotY;
    private bool _isSortedMinimaList;
    private bool _hasOpenPaths;
    internal bool _using_polytree;
    internal bool _succeeded;
    public bool PreserveCollinear;
    public bool ReverseSolution;

#if USINGZ
public delegate void ZCallback64(Point64 bot1, Point64 top1,
    Point64 bot2, Point64 top2, ref Point64 intersectPt);

public long DefaultZ;
protected ZCallback64? _zCallback;
#endif
    public ClipperBase()
    {
        _minimaList = [];
        _intersectList = [];
        _vertexList = new VertexPoolList();
        _outrecList = new OutRecPoolList();
        _scanlineList = [];
        _horzSegList = new HorzSegmentPoolList();
        _horzJoinList = new HorzJoinPoolList();
        _outPtPool = new OutPtPoolList();
        PreserveCollinear = true;
    }

#if USINGZ
	private bool XYCoordsEqual(Point64 pt1, Point64 pt2)
	{
		return (pt1.X == pt2.X && pt1.Y == pt2.Y);
	}

	private void SetZ(Active e1, Active e2, ref Point64 intersectPt)
	{
		if (_zCallback == null)
		{
			 return;
		}

		// prioritize subject vertices over clip vertices
		// and pass the subject vertices before clip vertices in the callback
		if (GetPolyType(e1) == PathType.Subject)
		{
			if (XYCoordsEqual(intersectPt, e1.bot))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e1.bot.Z);
			}
			else if (XYCoordsEqual(intersectPt, e1.top))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e1.top.Z);
			}
			else if (XYCoordsEqual(intersectPt, e2.bot))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e2.bot.Z);
			}
			else if (XYCoordsEqual(intersectPt, e2.top))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e2.top.Z);
			}
			else
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, DefaultZ);
			}
			_zCallback(e1.bot, e1.top, e2.bot, e2.top, ref intersectPt);
		}
		else
		{
			if (XYCoordsEqual(intersectPt, e2.bot))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e2.bot.Z);
			}
			else if (XYCoordsEqual(intersectPt, e2.top))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e2.top.Z);
			}
			else if (XYCoordsEqual(intersectPt, e1.bot))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e1.bot.Z);
			}
			else if (XYCoordsEqual(intersectPt, e1.top))
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, e1.top.Z);
			}
			else
			{
			intersectPt = new Point64(intersectPt.X, intersectPt.Y, DefaultZ);
			}
			_zCallback(e2.bot, e2.top, e1.bot, e1.top, ref intersectPt);
		}
	}
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOdd(int val)
    {
        return (val & 1) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHotEdge(Active ae)
    {
        return ae.outrec != null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOpen(Active ae)
    {
        return ae.localMin.isOpen;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOpenEnd(Active ae)
    {
        return ae.localMin.isOpen && IsOpenEnd(ae.vertexTop!);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsOpenEnd(Vertex v)
    {
        return (v.flags & (VertexFlags.OpenStart | VertexFlags.OpenEnd)) != VertexFlags.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Active? GetPrevHotEdge(Active ae)
    {
        var prev = ae.prevInAEL;
        while (prev != null && (IsOpen(prev) || !IsHotEdge(prev)))
        {
            prev = prev.prevInAEL;
        }
        return prev;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFront(Active ae)
    {
        return ae == ae.outrec!.frontEdge;
    }

    /*******************************************************************************
		*  Dx:                             0(90deg)                                    *
		*                                  |                                           *
		*               +inf (180deg) <--- o --. -inf (0deg)                          *
		*******************************************************************************/

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double GetDx(Point64 pt1, Point64 pt2)
    {
        double dy = pt2.Y - pt1.Y;
        return dy != 0d ? (pt2.X - pt1.X) / dy : pt2.X > pt1.X ? double.NegativeInfinity : double.PositiveInfinity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long TopX(Active ae, long currentY)
    {
        if (currentY == ae.top.Y || ae.top.X == ae.bot.X)
        {
            return ae.top.X;
        }
        if (currentY == ae.bot.Y)
        {
            return ae.bot.X;
        }

        // use MidpointRounding.ToEven in order to explicitly match the nearbyint behaviour on the C++ side
        return ae.bot.X + (long)Math.Round(ae.dx * (currentY - ae.bot.Y), MidpointRounding.ToEven);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHorizontal(Active ae)
    {
        return ae.top.Y == ae.bot.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHeadingRightHorz(Active ae)
    {
        return double.IsNegativeInfinity(ae.dx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHeadingLeftHorz(Active ae)
    {
        return double.IsPositiveInfinity(ae.dx);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SwapActives(ref Active ae1, ref Active ae2)
    {
        (ae2, ae1) = (ae1, ae2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PathType GetPolyType(Active ae)
    {
        return ae.localMin.polytype;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSamePolyType(Active ae1, Active ae2)
    {
        return ae1.localMin.polytype == ae2.localMin.polytype;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetDx(Active ae)
    {
        ae.dx = GetDx(ae.bot, ae.top);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vertex NextVertex(Active ae)
    {
        return ae.windDx > 0 ? ae.vertexTop!.next! : ae.vertexTop!.prev!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vertex PrevPrevVertex(Active ae)
    {
        return ae.windDx > 0 ? ae.vertexTop!.prev!.prev! : ae.vertexTop!.next!.next!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsMaxima(Vertex vertex)
    {
        return (vertex.flags & VertexFlags.LocalMax) != VertexFlags.None;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsMaxima(Active ae)
    {
        return IsMaxima(ae.vertexTop!);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Active? GetMaximaPair(Active ae)
    {
        var ae2 = ae.nextInAEL;
        while (ae2 != null)
        {
            if (ae2.vertexTop == ae.vertexTop)
            {
                return ae2; // Found!
            }
            ae2 = ae2.nextInAEL;
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vertex? GetCurrYMaximaVertex_Open(Active ae)
    {
        var result = ae.vertexTop;
        if (ae.windDx > 0)
        {
            while (result!.next!.pt.Y == result.pt.Y && (result.flags & (VertexFlags.OpenEnd | VertexFlags.LocalMax)) == VertexFlags.None)
            {
                result = result.next;
            }
        }
        else
            while (result!.prev!.pt.Y == result.pt.Y && (result.flags & (VertexFlags.OpenEnd | VertexFlags.LocalMax)) == VertexFlags.None)
            {
                result = result.prev;
            }
        if (!IsMaxima(result))
        {
            result = null; // not a maxima
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vertex? GetCurrYMaximaVertex(Active ae)
    {
        var result = ae.vertexTop;
        if (ae.windDx > 0)
        {
            while (result!.next!.pt.Y == result.pt.Y)
            {
                result = result.next;
            }
        }
        else
        {
            while (result!.prev!.pt.Y == result.pt.Y)
            {
                result = result.prev;
            }
        }
        if (!IsMaxima(result))
        {
            result = null; // not a maxima
        }
        return result;
    }

    private struct IntersectListSort : IComparer<IntersectNode>
    {
        public readonly int Compare(IntersectNode a, IntersectNode b)
        {
            return CompareIntersections(in a, in b);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CompareIntersections(in IntersectNode a, in IntersectNode b)
    {
        if (a.pt.Y != b.pt.Y)
        {
            return (a.pt.Y > b.pt.Y) ? -1 : 1;
        }
        if (a.pt.X == b.pt.X)
        {
            return 0;
        }
        return (a.pt.X < b.pt.X) ? -1 : 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetSides(OutRec outrec, Active startEdge, Active endEdge)
    {
        outrec.frontEdge = startEdge;
        outrec.backEdge = endEdge;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SwapOutrecs(Active ae1, Active ae2)
    {
        var or1 = ae1.outrec; // at least one edge has
        var or2 = ae2.outrec; // an assigned outrec
        if (or1 == or2)
        {
            var ae = or1!.frontEdge;
            or1.frontEdge = or1.backEdge;
            or1.backEdge = ae;
            return;
        }

        if (or1 != null)
        {
            if (ae1 == or1.frontEdge)
            {
                or1.frontEdge = ae2;
            }
            else
            {
                or1.backEdge = ae2;
            }
        }

        if (or2 != null)
        {
            if (ae2 == or2.frontEdge)
            {
                or2.frontEdge = ae1;
            }
            else
            {
                or2.backEdge = ae1;
            }
        }

        ae1.outrec = or2;
        ae2.outrec = or1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetOwner(OutRec outrec, OutRec newOwner)
    {
        // precondition1: new_owner is never null
        while (newOwner.owner != null && newOwner.owner.pts == null)
        {
            newOwner.owner = newOwner.owner.owner;
        }

        // make sure that outrec isn't an owner of newOwner
        var tmp = newOwner;
        while (tmp != null && tmp != outrec)
        {
            tmp = tmp.owner;
        }
        if (tmp != null)
        {
            newOwner.owner = outrec.owner;
        }
        outrec.owner = newOwner;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Area(OutPt op)
    {
        // https://en.wikipedia.org/wiki/Shoelace_formula
        var area = 0.0;
        var op2 = op;
        do
        {
            area += (double)(op2.prev.pt.Y + op2.pt.Y) * (op2.prev.pt.X - op2.pt.X);
            op2 = op2.next!;
        } while (op2 != op);
        return area * 0.5;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double AreaTriangle(Point64 pt1, Point64 pt2, Point64 pt3)
    {
        return (double)(pt3.Y + pt1.Y) * (pt3.X - pt1.X) + (double)(pt1.Y + pt2.Y) * (pt1.X - pt2.X) + (double)(pt2.Y + pt3.Y) * (pt2.X - pt3.X);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OutRec? GetRealOutRec(OutRec? outRec)
    {
        while (outRec != null && outRec.pts == null)
        {
            outRec = outRec.owner;
        }
        return outRec;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidOwner(OutRec? outRec, OutRec? testOwner)
    {
        while (testOwner != null && testOwner != outRec)
        {
            testOwner = testOwner.owner;
        }
        return testOwner == null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UncoupleOutRec(Active ae)
    {
        var outrec = ae.outrec;
        if (outrec == null)
        {
            return;
        }
        outrec.frontEdge!.outrec = null;
        outrec.backEdge!.outrec = null;
        outrec.frontEdge = null;
        outrec.backEdge = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool OutrecIsAscending(Active hotEdge)
    {
        return hotEdge == hotEdge.outrec!.frontEdge;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SwapFrontBackSides(OutRec outrec)
    {
        // while this proc. is needed for open paths
        // it's almost never needed for closed paths
        var ae2 = outrec.frontEdge!;
        outrec.frontEdge = outrec.backEdge;
        outrec.backEdge = ae2;
        outrec.pts = outrec.pts!.next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EdgesAdjacentInAEL(in IntersectNode inode)
    {
        return inode.edge1.nextInAEL == inode.edge2 || inode.edge1.prevInAEL == inode.edge2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ClearSolutionOnly()
    {
        while (_actives != null)
        {
            DeleteFromAEL(_actives);
        }
        _sel = null;
        _scanlineList.Clear();
        _scanlineSet?.Clear();
        _scanlineIsHeap = false;
        DisposeIntersectNodes();
        _outrecList.Clear();
        _horzSegList.Clear();
        _horzJoinList.Clear();
        _outPtPool.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        ClearSolutionOnly();
        _minimaList.Clear();
        _vertexList.Clear();
        _currentLocMin = 0;
        _isSortedMinimaList = false;
        _hasOpenPaths = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void Reset()
    {
        if (!_isSortedMinimaList)
        {
            _minimaList.Sort(LocMinComparer);
            _isSortedMinimaList = true;
        }

        var count = _minimaList.Count;
        _scanlineList.EnsureCapacity(count);
        var hasPreviousY = false;
        var previousY = 0L;
        var count1 = count - 1;
        for (var i = count1; i >= 0; --i)
        {
            var y = _minimaList[i].vertex.pt.Y;
            if (hasPreviousY && y == previousY)
            {
                continue;
            }
            _scanlineList.Add(y);
            previousY = y;
            hasPreviousY = true;
        }
        if (_scanlineList.Count > ScanlineHeapThreshold)
        {
            ActivateScanlineHeap();
        }

        _currentBotY = 0L;
        _currentLocMin = 0;
        _actives = null;
        _sel = null;
        _succeeded = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InsertScanline(long y)
    {
        if (_scanlineIsHeap)
        {
            InsertScanlineHeap(y);
            return;
        }

        var index = _scanlineList.BinarySearch(y);
        if (index >= 0)
        {
            return;
        }
        if (_scanlineList.Count < ScanlineHeapThreshold)
        {
            _scanlineList.Insert(~index, y);
            return;
        }

        ActivateScanlineHeap();
        InsertScanlineHeap(y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ActivateScanlineHeap()
    {
        // The small representation is ascending. Reversing it produces a valid
        // max-heap without an O(n) heapify pass.
        _scanlineList.Reverse();
        var scanlineSet = _scanlineSet ??= [];
        scanlineSet.Clear();
        scanlineSet.EnsureCapacity(_scanlineList.Count);
        var count = _scanlineList.Count;
        for (var i = 0; i < count; ++i)
        {
            scanlineSet.Add(_scanlineList[i]);
        }
        _scanlineIsHeap = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InsertScanlineHeap(long y)
    {
        if (!_scanlineSet!.Add(y))
        {
            return;
        }

        var index = _scanlineList.Count;
        _scanlineList.Add(y);
        while (index > 0)
        {
            var parent = (index - 1) >> 1;
            var parentY = _scanlineList[parent];
            if (parentY >= y)
            {
                break;
            }
            _scanlineList[index] = parentY;
            index = parent;
        }
        _scanlineList[index] = y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool PopScanline(out long y)
    {
        if (_scanlineIsHeap)
        {
            return PopScanlineHeap(out y);
        }

        var cnt = _scanlineList.Count - 1;
        if (cnt < 0)
        {
            y = 0;
            return false;
        }

        y = _scanlineList[cnt];
        _scanlineList.RemoveAt(cnt--);
        while (cnt >= 0 && y == _scanlineList[cnt])
        {
            _scanlineList.RemoveAt(cnt--);
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool PopScanlineHeap(out long y)
    {
        var lastIndex = _scanlineList.Count - 1;
        if (lastIndex < 0)
        {
            y = 0;
            return false;
        }

        y = _scanlineList[0];
        _scanlineSet!.Remove(y);
        var lastY = _scanlineList[lastIndex];
        _scanlineList.RemoveAt(lastIndex);
        if (lastIndex == 0)
        {
            return true;
        }

        var index = 0;
        var firstLeaf = lastIndex >> 1;
        while (index < firstLeaf)
        {
            var child = index * 2 + 1;
            var right = child + 1;
            if (right < lastIndex && _scanlineList[right] > _scanlineList[child])
            {
                child = right;
            }
            var childY = _scanlineList[child];
            if (childY <= lastY)
            {
                break;
            }
            _scanlineList[index] = childY;
            index = child;
        }
        _scanlineList[index] = lastY;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasLocMinAtY(long y)
    {
        return _currentLocMin < _minimaList.Count && _minimaList[_currentLocMin].vertex.pt.Y == y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LocalMinima PopLocalMinima()
    {
        return _minimaList[_currentLocMin++];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSubject(Path64 path)
    {
        AddPath(path, PathType.Subject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOpenSubject(Path64 path)
    {
        AddPath(path, PathType.Subject, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddClip(Path64 path)
    {
        AddPath(path, PathType.Clip);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddPath(Path64 path, PathType polytype, bool isOpen = false)
    {
        if (isOpen)
        {
            _hasOpenPaths = true;
        }
        _isSortedMinimaList = false;
        ClipperEngine.AddPathToVertexList(path, polytype, isOpen, _minimaList, _vertexList);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddPathSpan(ReadOnlySpan<Point64> path, PathType polytype, bool isOpen = false)
    {
        if (isOpen)
        {
            _hasOpenPaths = true;
        }
        _isSortedMinimaList = false;
        ClipperEngine.AddPathToVertexList(path, polytype, isOpen, _minimaList, _vertexList);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureVertexCapacity(int capacity)
    {
        _vertexList.EnsureCapacity(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void EnsureAdditionalVertexCapacity(int additionalCapacity)
    {
        _vertexList.EnsureCapacity(checked(_vertexList.Count + additionalCapacity));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddPaths(Paths64 paths, PathType polytype, bool isOpen = false)
    {
        if (isOpen)
        {
            _hasOpenPaths = true;
        }
        _isSortedMinimaList = false;
        ClipperEngine.AddPathsToVertexList(paths, polytype, isOpen, _minimaList, _vertexList);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private LocalMinima CopyReuseableMinima(in LocalMinima minima, bool overrideType, PathType typeOverride)
    {
        if (minima.isOpen)
        {
            _hasOpenPaths = true;
        }
        return overrideType ? new LocalMinima(minima.vertex, typeOverride, minima.isOpen) : minima;
    }

    private void MergeReuseableMinima(ReadOnlySpan<LocalMinima> source, bool overrideType, PathType typeOverride)
    {
        var existingCount = _minimaList.Count;
        var sourceCount = source.Length;
        var totalCount = existingCount + sourceCount;
        _minimaList.EnsureCapacity(totalCount);

        if (existingCount == 0)
        {
            CollectionsMarshal.SetCount(_minimaList, sourceCount);
            var destination = CollectionsMarshal.AsSpan(_minimaList);
            for (var i = 0; i < sourceCount; ++i)
            {
                destination[i] = CopyReuseableMinima(source[i], overrideType, typeOverride);
            }
            _isSortedMinimaList = true;
            return;
        }

        if (!_isSortedMinimaList)
        {
            for (var i = 0; i < sourceCount; ++i)
            {
                _minimaList.Add(CopyReuseableMinima(source[i], overrideType, typeOverride));
            }
            return;
        }

        // Both ranges are sorted by descending Y. Merge from the back so the existing range can be expanded in place
        CollectionsMarshal.SetCount(_minimaList, totalCount);
        var merged = CollectionsMarshal.AsSpan(_minimaList);
        var existingIndex = existingCount - 1;
        var sourceIndex = sourceCount - 1;
        var destinationIndex = totalCount - 1;
        while (sourceIndex >= 0)
        {
            if (existingIndex >= 0 && merged[existingIndex].vertex.pt.Y <= source[sourceIndex].vertex.pt.Y)
            {
                merged[destinationIndex--] = merged[existingIndex--];
            }
            else
            {
                merged[destinationIndex--] = CopyReuseableMinima(source[sourceIndex--], overrideType, typeOverride);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddReuseableData(ReuseableDataContainer64 reuseableData)
    {
        // nb: reuseableData will continue to own the vertices, so it's important that the reuseableData object isn't destroyed before the Clipper object
        // that's using the data
        var minima = reuseableData.GetSortedMinima();
        if (minima.Length != 0)
        {
            MergeReuseableMinima(minima, false, default);
        }
    }

    // BMR edit: a version of AddReusableData that forces polytype; useful if same data is to be reused as subject or clip
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void AddReuseableData(ReuseableDataContainer64 reuseableData, PathType typeOverride)
    {
        var minima = reuseableData.GetSortedMinima();
        if (minima.Length != 0)
            MergeReuseableMinima(minima, true, typeOverride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsContributingClosed(Active ae)
    {
        switch (_fillrule)
        {
            case FillRule.Positive:
                if (ae.windCount != 1)
                {
                    return false;
                }
                break;
            case FillRule.Negative:
                if (ae.windCount != -1)
                {
                    return false;
                }
                break;
            case FillRule.NonZero:
                if (Math.Abs(ae.windCount) != 1)
                {
                    return false;
                }
                break;
        }

        switch (_cliptype)
        {
            case ClipType.Intersection:
                return _fillrule switch
                {
                    FillRule.Positive => ae.windCount2 > 0,
                    FillRule.Negative => ae.windCount2 < 0,
                    _ => ae.windCount2 != 0
                };
            case ClipType.Union:
                return _fillrule switch
                {
                    FillRule.Positive => ae.windCount2 <= 0,
                    FillRule.Negative => ae.windCount2 >= 0,
                    _ => ae.windCount2 == 0
                };
            case ClipType.Difference:
                var result = _fillrule switch
                {
                    FillRule.Positive => ae.windCount2 <= 0,
                    FillRule.Negative => ae.windCount2 >= 0,
                    _ => ae.windCount2 == 0
                };
                return (GetPolyType(ae) == PathType.Subject) ? result : !result;
            case ClipType.Xor:
                return true; // XOr is always contributing unless open
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsContributingOpen(Active ae)
    {
        bool isInClip, isInSubj;
        switch (_fillrule)
        {
            case FillRule.Positive:
                isInSubj = ae.windCount > 0;
                isInClip = ae.windCount2 > 0;
                break;
            case FillRule.Negative:
                isInSubj = ae.windCount < 0;
                isInClip = ae.windCount2 < 0;
                break;
            default:
                isInSubj = ae.windCount != 0;
                isInClip = ae.windCount2 != 0;
                break;
        }

        var result = _cliptype switch
        {
            ClipType.Intersection => isInClip,
            ClipType.Union => !isInSubj && !isInClip,
            _ => !isInClip
        };
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetWindCountForClosedPathEdge(Active ae)
    {
        // Wind counts refer to polygon regions not edges, so here an edge's WindCnt
        // indicates the higher of the wind counts for the two regions touching the
        // edge. (nb: Adjacent regions can only ever have their wind counts differ by
        // one. Also, open paths have no meaningful wind directions or counts.)

        var ae2 = ae.prevInAEL;
        // find the nearest closed path edge of the same PolyType in AEL (heading left)
        var pt = GetPolyType(ae);
        while (ae2 != null && (GetPolyType(ae2) != pt || IsOpen(ae2)))
        {
            ae2 = ae2.prevInAEL;
        }

        if (ae2 == null)
        {
            ae.windCount = ae.windDx;
            ae2 = _actives;
        }
        else if (_fillrule == FillRule.EvenOdd)
        {
            ae.windCount = ae.windDx;
            ae.windCount2 = ae2.windCount2;
            ae2 = ae2.nextInAEL;
        }
        else
        {
            // NonZero, positive, or negative filling here ...
            // when e2's WindCnt is in the SAME direction as its WindDx,
            // then polygon will fill on the right of 'e2' (and 'e' will be inside)
            // nb: neither e2.WindCnt nor e2.WindDx should ever be 0.
            if (ae2.windCount * ae2.windDx < 0)
            {
                // opposite directions so 'ae' is outside 'ae2' ...
                if (Math.Abs(ae2.windCount) > 1)
                {
                    // outside prev poly but still inside another.
                    if (ae2.windDx * ae.windDx < 0)
                    {
                        // reversing direction so use the same WC
                        ae.windCount = ae2.windCount;
                    }
                    else
                    {
                        // otherwise keep 'reducing' the WC by 1 (i.e. towards 0) ...
                        ae.windCount = ae2.windCount + ae.windDx;
                    }
                }
                else
                {
                    // now outside all polys of same polytype so set own WC ...
                    ae.windCount = IsOpen(ae) ? 1 : ae.windDx;
                }
            }
            else
            {
                //'ae' must be inside 'ae2'
                if (ae2.windDx * ae.windDx < 0)
                {
                    // reversing direction so use the same WC
                    ae.windCount = ae2.windCount;
                }
                else
                {
                    // otherwise keep 'increasing' the WC by 1 (i.e. away from 0) ...
                    ae.windCount = ae2.windCount + ae.windDx;
                }
            }

            ae.windCount2 = ae2.windCount2;
            ae2 = ae2.nextInAEL; // i.e. get ready to calc WindCnt2
        }

        // update windCount2 ...
        if (_fillrule == FillRule.EvenOdd)
            while (ae2 != ae)
            {
                if (GetPolyType(ae2!) != pt && !IsOpen(ae2!))
                {
                    ae.windCount2 = ae.windCount2 == 0 ? 1 : 0;
                }
                ae2 = ae2!.nextInAEL;
            }
        else
            while (ae2 != ae)
            {
                if (GetPolyType(ae2!) != pt && !IsOpen(ae2!))
                {
                    ae.windCount2 += ae2!.windDx;
                }
                ae2 = ae2!.nextInAEL;
            }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetWindCountForOpenPathEdge(Active ae)
    {
        var ae2 = _actives;
        if (_fillrule == FillRule.EvenOdd)
        {
            int cnt1 = 0, cnt2 = 0;
            while (ae2 != ae)
            {
                if (GetPolyType(ae2!) == PathType.Clip)
                {
                    ++cnt2;
                }
                else if (!IsOpen(ae2!))
                {
                    ++cnt1;
                }
                ae2 = ae2!.nextInAEL;
            }

            ae.windCount = IsOdd(cnt1) ? 1 : 0;
            ae.windCount2 = IsOdd(cnt2) ? 1 : 0;
        }
        else
        {
            while (ae2 != ae)
            {
                if (GetPolyType(ae2!) == PathType.Clip)
                {
                    ae.windCount2 += ae2!.windDx;
                }
                else if (!IsOpen(ae2!))
                {
                    ae.windCount += ae2!.windDx;
                }
                ae2 = ae2!.nextInAEL;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidAelOrder(Active resident, Active newcomer)
    {
        if (newcomer.curX != resident.curX)
        {
            return newcomer.curX > resident.curX;
        }

        // get the turning direction  a1.top, a2.bot, a2.top
        var d = InternalClipper.CrossProduct(resident.top, newcomer.bot, newcomer.top);
        if (d != 0d)
        {
            return d < 0d;
        }

        // edges must be collinear to get here

        // for starting open paths, place them according to
        // the direction they're about to turn
        if (!IsMaxima(resident) && resident.top.Y > newcomer.top.Y)
        {
            return InternalClipper.CrossProduct(newcomer.bot, resident.top, NextVertex(resident).pt) <= 0d;
        }

        if (!IsMaxima(newcomer) && newcomer.top.Y > resident.top.Y)
        {
            return InternalClipper.CrossProduct(newcomer.bot, newcomer.top, NextVertex(newcomer).pt) >= 0d;
        }

        var y = newcomer.bot.Y;
        var newcomerIsLeft = newcomer.isLeftBound;

        if (resident.bot.Y != y || resident.localMin.vertex.pt.Y != y)
        {
            return newcomer.isLeftBound;
        }
        // resident must also have just been inserted
        if (resident.isLeftBound != newcomerIsLeft)
        {
            return newcomerIsLeft;
        }
        if (InternalClipper.IsCollinear(PrevPrevVertex(resident).pt, resident.bot, resident.top))
        {
            return true;
        }
        // compare turning direction of the alternate bound
        return (InternalClipper.CrossProduct(PrevPrevVertex(resident).pt, newcomer.bot, PrevPrevVertex(newcomer).pt) > 0d) == newcomerIsLeft;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InsertLeftEdge(Active ae)
    {
        if (_actives == null)
        {
            ae.prevInAEL = null;
            ae.nextInAEL = null;
            _actives = ae;
        }
        else if (!IsValidAelOrder(_actives, ae))
        {
            ae.prevInAEL = null;
            ae.nextInAEL = _actives;
            _actives.prevInAEL = ae;
            _actives = ae;
        }
        else
        {
            var ae2 = _actives;
            while (ae2.nextInAEL != null && IsValidAelOrder(ae2.nextInAEL, ae))
            {
                ae2 = ae2.nextInAEL;
            }
            //don't separate joined edges
            if (ae2.joinWith == JoinWith.Right)
            {
                ae2 = ae2.nextInAEL!;
            }
            ae.nextInAEL = ae2.nextInAEL;
            ae2.nextInAEL?.prevInAEL = ae;
            ae.prevInAEL = ae2;
            ae2.nextInAEL = ae;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void InsertRightEdge(Active ae, Active ae2)
    {
        ae2.nextInAEL = ae.nextInAEL;
        ae.nextInAEL?.prevInAEL = ae2;
        ae2.prevInAEL = ae;
        ae.nextInAEL = ae2;
    }

    private void InsertLocalMinimaIntoAEL(long botY)
    {
        // Add any local minima (if any) at BotY ...
        // NB horizontal local minima edges should contain locMin.vertex.prev
        while (HasLocMinAtY(botY))
        {
            var localMinima = PopLocalMinima();
            Active? leftBound;
            if ((localMinima.vertex.flags & VertexFlags.OpenStart) != VertexFlags.None)
            {
                leftBound = null;
            }
            else
            {
                leftBound = NewActive();
                leftBound.bot = localMinima.vertex.pt;
                leftBound.curX = localMinima.vertex.pt.X;
                leftBound.windDx = -1;
                leftBound.vertexTop = localMinima.vertex.prev;
                leftBound.top = localMinima.vertex.prev!.pt;
                leftBound.localMin = localMinima;
                SetDx(leftBound);
            }

            Active? rightBound;
            if ((localMinima.vertex.flags & VertexFlags.OpenEnd) != VertexFlags.None)
            {
                rightBound = null;
            }
            else
            {
                rightBound = NewActive();
                rightBound.bot = localMinima.vertex.pt;
                rightBound.curX = localMinima.vertex.pt.X;
                rightBound.windDx = 1;
                rightBound.vertexTop = localMinima.vertex.next; // i.e. ascending
                rightBound.top = localMinima.vertex.next!.pt;
                rightBound.localMin = localMinima;
                SetDx(rightBound);
            }

            // Currently LeftB is just the descending bound and RightB is the ascending.
            // Now if the LeftB isn't on the left of RightB then we need swap them.
            if (leftBound != null && rightBound != null)
            {
                if (IsHorizontal(leftBound))
                {
                    if (IsHeadingRightHorz(leftBound))
                    {
                        SwapActives(ref leftBound, ref rightBound);
                    }
                }
                else if (IsHorizontal(rightBound))
                {
                    if (IsHeadingLeftHorz(rightBound))
                    {
                        SwapActives(ref leftBound, ref rightBound);
                    }
                }
                else if (leftBound.dx < rightBound.dx)
                {
                    SwapActives(ref leftBound, ref rightBound);
                }
                // so when leftBound has windDx == 1, the polygon will be oriented
                // counter-clockwise in Cartesian coords (clockwise with inverted Y).
            }
            else if (leftBound == null)
            {
                leftBound = rightBound;
                rightBound = null;
            }

            bool contributing;
            leftBound!.isLeftBound = true;
            InsertLeftEdge(leftBound);

            if (IsOpen(leftBound))
            {
                SetWindCountForOpenPathEdge(leftBound);
                contributing = IsContributingOpen(leftBound);
            }
            else
            {
                SetWindCountForClosedPathEdge(leftBound);
                contributing = IsContributingClosed(leftBound);
            }

            if (rightBound != null)
            {
                rightBound.windCount = leftBound.windCount;
                rightBound.windCount2 = leftBound.windCount2;
                InsertRightEdge(leftBound, rightBound); ///////

                if (contributing)
                {
                    AddLocalMinPoly(leftBound, rightBound, leftBound.bot, true);
                    if (!IsHorizontal(leftBound))
                    {
                        CheckJoinLeft(leftBound, leftBound.bot);
                    }
                }

                while (rightBound.nextInAEL != null && IsValidAelOrder(rightBound.nextInAEL, rightBound))
                {
                    IntersectEdges(rightBound, rightBound.nextInAEL, rightBound.bot);
                    SwapPositionsInAEL(rightBound, rightBound.nextInAEL);
                }

                if (IsHorizontal(rightBound))
                {
                    PushHorz(rightBound);
                }
                else
                {
                    CheckJoinRight(rightBound, rightBound.bot);
                    InsertScanline(rightBound.top.Y);
                }
            }
            else if (contributing)
            {
                StartOpenPath(leftBound, leftBound.bot);
            }

            if (IsHorizontal(leftBound))
            {
                PushHorz(leftBound);
            }
            else
            {
                InsertScanline(leftBound.top.Y);
            }
        } // while (HasLocMinAtY())
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushHorz(Active ae)
    {
        ae.nextInSEL = _sel;
        _sel = ae;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool PopHorz(out Active? ae)
    {
        ae = _sel;
        if (_sel == null)
        {
            return false;
        }
        _sel = _sel.nextInSEL;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OutPt AddLocalMinPoly(Active ae1, Active ae2, Point64 pt, bool isNew = false)
    {
        var outrec = NewOutRec();
        ae1.outrec = outrec;
        ae2.outrec = outrec;

        if (IsOpen(ae1))
        {
            outrec.owner = null;
            outrec.isOpen = true;
            if (ae1.windDx > 0)
            {
                SetSides(outrec, ae1, ae2);
            }
            else
            {
                SetSides(outrec, ae2, ae1);
            }
        }
        else
        {
            outrec.isOpen = false;
            var prevHotEdge = GetPrevHotEdge(ae1);
            // e.windDx is the winding direction of the **input** paths
            // and unrelated to the winding direction of output polygons.
            // Output orientation is determined by e.outrec.frontE which is
            // the ascending edge (see AddLocalMinPoly).
            if (prevHotEdge != null)
            {
                if (_using_polytree)
                {
                    SetOwner(outrec, prevHotEdge.outrec!);
                }
                outrec.owner = prevHotEdge.outrec;
                if (OutrecIsAscending(prevHotEdge) == isNew)
                {
                    SetSides(outrec, ae2, ae1);
                }
                else
                {
                    SetSides(outrec, ae1, ae2);
                }
            }
            else
            {
                outrec.owner = null;
                if (isNew)
                {
                    SetSides(outrec, ae1, ae2);
                }
                else
                {
                    SetSides(outrec, ae2, ae1);
                }
            }
        }

        var op = _outPtPool.Add(pt, outrec);
        outrec.pts = op;
        return op;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OutPt? AddLocalMaxPoly(Active ae1, Active ae2, Point64 pt)
    {
        if (IsJoined(ae1))
        {
            Split(ae1, pt);
        }
        if (IsJoined(ae2))
        {
            Split(ae2, pt);
        }

        if (IsFront(ae1) == IsFront(ae2))
        {
            if (IsOpenEnd(ae1))
            {
                SwapFrontBackSides(ae1.outrec!);
            }
            else if (IsOpenEnd(ae2))
            {
                SwapFrontBackSides(ae2.outrec!);
            }
            else
            {
                _succeeded = false;
                return null;
            }
        }

        var result = AddOutPt(ae1, pt);
        if (ae1.outrec == ae2.outrec)
        {
            var outrec = ae1.outrec!;
            outrec.pts = result;

            if (_using_polytree)
            {
                var e = GetPrevHotEdge(ae1);
                if (e == null)
                {
                    outrec.owner = null;
                }
                else
                {
                    SetOwner(outrec, e.outrec!);
                }
                // nb: outRec.owner here is likely NOT the real
                // owner but this will be fixed in DeepCheckOwner()
            }
            UncoupleOutRec(ae1);
        }
        // and to preserve the winding orientation of outrec ...
        else if (IsOpen(ae1))
        {
            if (ae1.windDx < 0)
            {
                JoinOutrecPaths(ae1, ae2);
            }
            else
            {
                JoinOutrecPaths(ae2, ae1);
            }
        }
        else if (ae1.outrec!.idx < ae2.outrec!.idx)
        {
            JoinOutrecPaths(ae1, ae2);
        }
        else
        {
            JoinOutrecPaths(ae2, ae1);
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void JoinOutrecPaths(Active ae1, Active ae2)
    {
        // join ae2 outrec path onto ae1 outrec path and then delete ae2 outrec path
        // pointers. (NB Only very rarely do the joining ends share the same coords.)
        var p1Start = ae1.outrec!.pts!;
        var p2Start = ae2.outrec!.pts!;
        var p1End = p1Start.next!;
        var p2End = p2Start.next!;
        if (IsFront(ae1))
        {
            p2End.prev = p1Start;
            p1Start.next = p2End;
            p2Start.next = p1End;
            p1End.prev = p2Start;
            ae1.outrec.pts = p2Start;
            // nb: if IsOpen(e1) then e1 & e2 must be a 'maximaPair'
            ae1.outrec.frontEdge = ae2.outrec.frontEdge;
            if (ae1.outrec.frontEdge != null)
            {
                ae1.outrec.frontEdge!.outrec = ae1.outrec;
            }
        }
        else
        {
            p1End.prev = p2Start;
            p2Start.next = p1End;
            p1Start.next = p2End;
            p2End.prev = p1Start;

            ae1.outrec.backEdge = ae2.outrec.backEdge;
            if (ae1.outrec.backEdge != null)
            {
                ae1.outrec.backEdge!.outrec = ae1.outrec;
            }
        }

        // after joining, the ae2.OutRec must contains no vertices ...
        ae2.outrec.frontEdge = null;
        ae2.outrec.backEdge = null;
        ae2.outrec.pts = null;
        ae1.outrec.outPtCount += ae2.outrec.outPtCount;
        SetOwner(ae2.outrec, ae1.outrec);

        if (IsOpenEnd(ae1))
        {
            ae2.outrec.pts = ae1.outrec.pts;
            ae2.outrec.outPtCount = ae1.outrec.outPtCount;
            ae1.outrec.pts = null;
        }

        // and ae1 and ae2 are maxima and are about to be dropped from the Actives list.
        ae1.outrec = null;
        ae2.outrec = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OutPt AddOutPt(Active ae, Point64 pt)
    {

        // Outrec.OutPts: a circular doubly-linked-list of POutPt where ...
        // opFront[.Prev]* ~~~> opBack & opBack == opFront.Next
        var outrec = ae.outrec!;
        var toFront = IsFront(ae);
        var opFront = outrec.pts!;
        var opBack = opFront.next!;

        switch (toFront)
        {
            case true when pt == opFront.pt:
                return opFront;
            case false when pt == opBack.pt:
                return opBack;
        }

        var newOp = _outPtPool.Add(pt, outrec);
        opBack.prev = newOp;
        newOp.prev = opFront;
        newOp.next = opBack;
        opFront.next = newOp;
        if (toFront)
        {
            outrec.pts = newOp;
        }
        return newOp;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OutRec NewOutRec()
    {
        var result = _outrecList.Add();
        result.idx = _outrecList.Count - 1;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OutPt StartOpenPath(Active ae, Point64 pt)
    {
        var outrec = NewOutRec();
        outrec.isOpen = true;
        if (ae.windDx > 0)
        {
            outrec.frontEdge = ae;
            outrec.backEdge = null;
        }
        else
        {
            outrec.frontEdge = null;
            outrec.backEdge = ae;
        }

        ae.outrec = outrec;
        var op = _outPtPool.Add(pt, outrec);
        outrec.pts = op;
        return op;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateEdgeIntoAEL(Active ae)
    {
        ae.bot = ae.top;
        ae.vertexTop = NextVertex(ae);
        ae.top = ae.vertexTop!.pt;
        ae.curX = ae.bot.X;
        SetDx(ae);

        if (IsJoined(ae))
        {
            Split(ae, ae.bot);
        }

        if (IsHorizontal(ae))
        {
            if (!IsOpen(ae))
            {
                TrimHorz(ae, PreserveCollinear);
            }
            return;
        }
        InsertScanline(ae.top.Y);

        CheckJoinLeft(ae, ae.bot);
        CheckJoinRight(ae, ae.bot, true); // (#500)
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Active? FindEdgeWithMatchingLocMin(Active e)
    {
        var result = e.nextInAEL;
        while (result != null)
        {
            if (result.localMin == e.localMin)
            {
                return result;
            }
            if (!IsHorizontal(result) && e.bot != result.bot)
            {
                result = null;
            }
            else
            {
                result = result.nextInAEL;
            }
        }
        result = e.prevInAEL;
        while (result != null)
        {
            if (result.localMin == e.localMin)
            {
                return result;
            }
            if (!IsHorizontal(result) && e.bot != result.bot)
            {
                return null;
            }
            result = result.prevInAEL;
        }
        return result;
    }

    private void IntersectEdges(Active ae1, Active ae2, Point64 pt)
    {
        OutPt? resultOp = null;
        // MANAGE OPEN PATH INTERSECTIONS SEPARATELY ...
        if (_hasOpenPaths && (IsOpen(ae1) || IsOpen(ae2)))
        {
            if (IsOpen(ae1) && IsOpen(ae2))
            {
                return;
            }
            // the following line avoids duplicating quite a bit of code
            if (IsOpen(ae2))
            {
                SwapActives(ref ae1, ref ae2);
            }
            if (IsJoined(ae2))
            {
                Split(ae2, pt); // needed for safety
            }

            if (_cliptype == ClipType.Union)
            {
                if (!IsHotEdge(ae2))
                {
                    return;
                }
            }
            else if (ae2.localMin.polytype == PathType.Subject)
            {
                return;
            }

            switch (_fillrule)
            {
                case FillRule.Positive:
                    if (ae2.windCount != 1)
                    {
                        return;
                    }
                    break;
                case FillRule.Negative:
                    if (ae2.windCount != -1)
                    {
                        return;
                    }
                    break;
                default:
                    if (Math.Abs(ae2.windCount) != 1)
                    {
                        return;
                    }
                    break;
            }

            // toggle contribution ...
            if (IsHotEdge(ae1))
            {
                resultOp = AddOutPt(ae1, pt);
#if USINGZ
      			SetZ(ae1, ae2, ref resultOp.pt);
#endif
                if (IsFront(ae1))
                {
                    ae1.outrec!.frontEdge = null;
                }
                else
                {
                    ae1.outrec!.backEdge = null;
                }
                ae1.outrec = null;
            }

            // horizontal edges can pass under open paths at a LocMins
            else if (pt == ae1.localMin.vertex.pt && !IsOpenEnd(ae1.localMin.vertex))
            {
                // find the other side of the LocMin and
                // if it's 'hot' join up with it ...
                var ae3 = FindEdgeWithMatchingLocMin(ae1);
                if (ae3 != null && IsHotEdge(ae3))
                {
                    ae1.outrec = ae3.outrec;
                    if (ae1.windDx > 0)
                    {
                        SetSides(ae3.outrec!, ae1, ae3);
                    }
                    else
                    {
                        SetSides(ae3.outrec!, ae3, ae1);
                    }
                    return;
                }

                resultOp = StartOpenPath(ae1, pt);
            }
            else
            {
                resultOp = StartOpenPath(ae1, pt);
            }

#if USINGZ
    		SetZ(ae1, ae2, ref resultOp.pt);
#endif
            return;
        }

        // MANAGING CLOSED PATHS FROM HERE ON
        if (IsJoined(ae1))
        {
            Split(ae1, pt);
        }
        if (IsJoined(ae2))
        {
            Split(ae2, pt);
        }

        // UPDATE WINDING COUNTS...

        int oldE1WindCount, oldE2WindCount;
        if (ae1.localMin.polytype == ae2.localMin.polytype)
        {
            if (_fillrule == FillRule.EvenOdd)
            {
                oldE1WindCount = ae1.windCount;
                ae1.windCount = ae2.windCount;
                ae2.windCount = oldE1WindCount;
            }
            else
            {
                if (ae1.windCount + ae2.windDx == 0)
                {
                    ae1.windCount = -ae1.windCount;
                }
                else
                {
                    ae1.windCount += ae2.windDx;
                }
                if (ae2.windCount - ae1.windDx == 0)
                {
                    ae2.windCount = -ae2.windCount;
                }
                else
                {
                    ae2.windCount -= ae1.windDx;
                }
            }
        }
        else
        {
            if (_fillrule != FillRule.EvenOdd)
            {
                ae1.windCount2 += ae2.windDx;
            }
            else
            {
                ae1.windCount2 = ae1.windCount2 == 0 ? 1 : 0;
            }
            if (_fillrule != FillRule.EvenOdd)
            {
                ae2.windCount2 -= ae1.windDx;
            }
            else
            {
                ae2.windCount2 = ae2.windCount2 == 0 ? 1 : 0;
            }
        }

        switch (_fillrule)
        {
            case FillRule.Positive:
                oldE1WindCount = ae1.windCount;
                oldE2WindCount = ae2.windCount;
                break;
            case FillRule.Negative:
                oldE1WindCount = -ae1.windCount;
                oldE2WindCount = -ae2.windCount;
                break;
            default:
                oldE1WindCount = Math.Abs(ae1.windCount);
                oldE2WindCount = Math.Abs(ae2.windCount);
                break;
        }

        var e1WindCountIs0or1 = oldE1WindCount is 0 or 1;
        var e2WindCountIs0or1 = oldE2WindCount is 0 or 1;

        if (!IsHotEdge(ae1) && !e1WindCountIs0or1 ||
          !IsHotEdge(ae2) && !e2WindCountIs0or1)
            return;

        // NOW PROCESS THE INTERSECTION ...

        // if both edges are 'hot' ...
        if (IsHotEdge(ae1) && IsHotEdge(ae2))
        {
            if (oldE1WindCount != 0 && oldE1WindCount != 1 || oldE2WindCount != 0 && oldE2WindCount != 1 || _cliptype != ClipType.Xor && ae1.localMin.polytype != ae2.localMin.polytype)
            {
                resultOp = AddLocalMaxPoly(ae1, ae2, pt);
#if USINGZ
				if (resultOp != null)
				{
					SetZ(ae1, ae2, ref resultOp.pt);
				}
#endif
            }
            else if (IsFront(ae1) || ae1.outrec == ae2.outrec)
            {
                // this 'else if' condition isn't strictly needed but
                // it's sensible to split polygons that only touch at
                // a common vertex (not at common edges).
                resultOp = AddLocalMaxPoly(ae1, ae2, pt);
#if USINGZ
				OutPt op2 = AddLocalMinPoly(ae1, ae2, pt);
				if (resultOp != null)
				{
					SetZ(ae1, ae2, ref resultOp.pt);
				}
				SetZ(ae1, ae2, ref op2.pt);
#else
                AddLocalMinPoly(ae1, ae2, pt);
#endif
            }
            else
            {
                // can't treat as maxima & minima
                resultOp = AddOutPt(ae1, pt);
#if USINGZ
				OutPt op2 = AddOutPt(ae2, pt);
				SetZ(ae1, ae2, ref resultOp.pt);
				SetZ(ae1, ae2, ref op2.pt);
#else
                AddOutPt(ae2, pt);
#endif
                SwapOutrecs(ae1, ae2);
            }
        }

        // if one or other edge is 'hot' ...
        else if (IsHotEdge(ae1))
        {
            resultOp = AddOutPt(ae1, pt);
#if USINGZ
			SetZ(ae1, ae2, ref resultOp.pt);
#endif
            SwapOutrecs(ae1, ae2);
        }
        else if (IsHotEdge(ae2))
        {
            resultOp = AddOutPt(ae2, pt);
#if USINGZ
			SetZ(ae1, ae2, ref resultOp.pt);
#endif
            SwapOutrecs(ae1, ae2);
        }

        // neither edge is 'hot'
        else
        {
            long e1Wc2, e2Wc2;
            switch (_fillrule)
            {
                case FillRule.Positive:
                    e1Wc2 = ae1.windCount2;
                    e2Wc2 = ae2.windCount2;
                    break;
                case FillRule.Negative:
                    e1Wc2 = -ae1.windCount2;
                    e2Wc2 = -ae2.windCount2;
                    break;
                default:
                    e1Wc2 = Math.Abs(ae1.windCount2);
                    e2Wc2 = Math.Abs(ae2.windCount2);
                    break;
            }

            if (!IsSamePolyType(ae1, ae2))
            {
                resultOp = AddLocalMinPoly(ae1, ae2, pt);
#if USINGZ
				SetZ(ae1, ae2, ref resultOp.pt);
#endif
            }
            else if (oldE1WindCount == 1 && oldE2WindCount == 1)
            {
                resultOp = null;
                switch (_cliptype)
                {
                    case ClipType.Union:
                        if (e1Wc2 > 0L && e2Wc2 > 0L)
                        {
                            return;
                        }
                        resultOp = AddLocalMinPoly(ae1, ae2, pt);
                        break;

                    case ClipType.Difference:
                        if (GetPolyType(ae1) == PathType.Clip && e1Wc2 > 0L && e2Wc2 > 0L || GetPolyType(ae1) == PathType.Subject && e1Wc2 <= 0L && e2Wc2 <= 0L)
                        {
                            resultOp = AddLocalMinPoly(ae1, ae2, pt);
                        }

                        break;

                    case ClipType.Xor:
                        resultOp = AddLocalMinPoly(ae1, ae2, pt);
                        break;

                    default: // ClipType.Intersection:
                        if (e1Wc2 <= 0L || e2Wc2 <= 0L)
                        {
                            return;
                        }
                        resultOp = AddLocalMinPoly(ae1, ae2, pt);
                        break;
                }
#if USINGZ
			if (resultOp != null) 
			{
				SetZ(ae1, ae2, ref resultOp.pt);
			}
#endif
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DeleteFromAEL(Active ae)
    {
        var prev = ae.prevInAEL;
        var next = ae.nextInAEL;
        if (prev == null && next == null && ae != _actives)
        {
            return; // already deleted
        }
        if (prev != null)
        {
            prev.nextInAEL = next;
        }
        else
        {
            _actives = next;
        }
        next?.prevInAEL = prev;
        RecycleActive(ae);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Active NewActive()
    {
        var result = _freeActives;
        if (result == null)
        {
            return new Active();
        }
        _freeActives = result.nextInAEL;
        result.nextInAEL = null;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RecycleActive(Active ae)
    {
        ae.bot = default;
        ae.top = default;
        ae.curX = 0;
        ae.dx = 0;
        ae.windDx = 0;
        ae.windCount = 0;
        ae.windCount2 = 0;
        ae.outrec = null;
        ae.prevInAEL = null;
        ae.prevInSEL = null;
        ae.nextInSEL = null;
        ae.jump = null;
        ae.vertexTop = null;
        ae.localMin = default;
        ae.isLeftBound = false;
        ae.joinWith = JoinWith.None;
        ae.nextInAEL = _freeActives;
        _freeActives = ae;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdjustCurrXAndCopyToSEL(long topY)
    {
        var ae = _actives;
        _sel = ae;
        while (ae != null)
        {
            ae.prevInSEL = ae.prevInAEL;
            ae.nextInSEL = ae.nextInAEL;
            ae.jump = ae.nextInSEL;
            // it is safe to ignore 'joined' edges here because
            // if necessary they will be split in IntersectEdges()
            ae.curX = TopX(ae, topY);
            // NB don't update ae.curr.Y yet (see AddNewIntersectNode)
            ae = ae.nextInAEL;
        }
    }

    protected void ExecuteInternal(ClipType ct, FillRule fillRule)
    {
        if (ct == ClipType.NoClip)
        {
            return;
        }
        _fillrule = fillRule;
        _cliptype = ct;
        Reset();
        if (!PopScanline(out var y))
        {
            return;
        }
        while (_succeeded)
        {
            InsertLocalMinimaIntoAEL(y);
            Active? ae;
            while (PopHorz(out ae))
            {
                DoHorizontal(ae!);
            }
            if (_horzSegList.Count > 0)
            {
                ConvertHorzSegsToJoins();
                _horzSegList.Clear();
            }
            _currentBotY = y; // bottom of scanbeam
            if (!PopScanline(out y))
            {
                break; // y new top of scanbeam
            }
            DoIntersections(y);
            DoTopOfScanbeam(y);
            while (PopHorz(out ae))
            {
                DoHorizontal(ae!);
            }
        }
        if (_succeeded)
        {
            ProcessHorzJoins();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoIntersections(long topY)
    {
        if (!BuildIntersectList(topY))
        {
            return;
        }
        ProcessIntersectList();
        DisposeIntersectNodes();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DisposeIntersectNodes()
    {
        _intersectList.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNewIntersectNode(Active ae1, Active ae2, long topY)
    {
        if (!InternalClipper.GetSegmentIntersectPt(ae1.bot, ae1.top, ae2.bot, ae2.top, out var ip))
        {
            ip = new Point64(ae1.curX, topY);
        }

        if (ip.Y > _currentBotY || ip.Y < topY)
        {
            var absDx1 = Math.Abs(ae1.dx);
            var absDx2 = Math.Abs(ae2.dx);
            switch (absDx1 > 100d)
            {
                case true when absDx2 > 100d:
                    {
                        if (absDx1 > absDx2)
                        {
                            ip = InternalClipper.GetClosestPtOnSegment(ip, ae1.bot, ae1.top);
                        }
                        else
                        {
                            ip = InternalClipper.GetClosestPtOnSegment(ip, ae2.bot, ae2.top);
                        }
                        break;
                    }
                case true:
                    ip = InternalClipper.GetClosestPtOnSegment(ip, ae1.bot, ae1.top);
                    break;
                default:
                    {
                        if (absDx2 > 100d)
                            ip = InternalClipper.GetClosestPtOnSegment(ip, ae2.bot, ae2.top);
                        else
                        {
                            if (ip.Y < topY)
                            {
                                ip.Y = topY;
                            }
                            else
                            {
                                ip.Y = _currentBotY;
                            }
                            if (absDx1 < absDx2)
                            {
                                ip.X = TopX(ae1, ip.Y);
                            }
                            else
                            {
                                ip.X = TopX(ae2, ip.Y);
                            }
                        }

                        break;
                    }
            }
        }
        IntersectNode node = new IntersectNode(ip, ae1, ae2);
        _intersectList.Add(node);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Active? ExtractFromSEL(Active ae)
    {
        var res = ae.nextInSEL;
        res?.prevInSEL = ae.prevInSEL;
        ae.prevInSEL!.nextInSEL = res;
        return res;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Insert1Before2InSEL(Active ae1, Active ae2)
    {
        ae1.prevInSEL = ae2.prevInSEL;
        ae1.prevInSEL?.nextInSEL = ae1;
        ae1.nextInSEL = ae2;
        ae2.prevInSEL = ae1;
    }

    private bool BuildIntersectList(long topY)
    {
        if (_actives?.nextInAEL == null)
        {
            return false;
        }

        // Calculate edge positions at the top of the current scanbeam, and from this
        // we will determine the intersections required to reach these new positions.
        AdjustCurrXAndCopyToSEL(topY);

        // Find all edge intersections in the current scanbeam using a stable merge
        // sort that ensures only adjacent edges are intersecting. Intersect info is
        // stored in FIntersectList ready to be processed in ProcessIntersectList.
        // Re merge sorts see https://stackoverflow.com/a/46319131/359538

        var left = _sel;

        while (left!.jump != null)
        {
            Active? prevBase = null;
            while (left?.jump != null)
            {
                var currBase = left;
                var right = left.jump;
                var lEnd = right;
                var rEnd = right.jump;
                left.jump = rEnd;
                while (left != lEnd && right != rEnd)
                {
                    if (right!.curX < left!.curX)
                    {
                        var tmp = right.prevInSEL!;
                        for (; ; )
                        {
                            AddNewIntersectNode(tmp, right, topY);
                            if (tmp == left)
                            {
                                break;
                            }
                            tmp = tmp.prevInSEL!;
                        }

                        tmp = right;
                        right = ExtractFromSEL(tmp);
                        lEnd = right;
                        Insert1Before2InSEL(tmp, left);
                        if (left != currBase)
                        {
                            continue;
                        }
                        currBase = tmp;
                        currBase.jump = rEnd;
                        if (prevBase == null)
                        {
                            _sel = currBase;
                        }
                        else
                        {
                            prevBase.jump = currBase;
                        }
                    }
                    else
                    {
                        left = left.nextInSEL;
                    }
                }

                prevBase = currBase;
                left = rEnd;
            }
            left = _sel;
        }

        return _intersectList.Count > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ProcessIntersectList()
    {
        // We now have a list of intersections required so that edges will be
        // correctly positioned at the top of the scanbeam. However, it's important
        // that edge intersections are processed from the bottom up, but it's also
        // crucial that intersections only occur between adjacent edges.
        var countI = _intersectList.Count;
        if (countI <= 16)
        {
            var smallList = CollectionsMarshal.AsSpan(_intersectList);
            var len = smallList.Length;
            for (var i = 1; i < len; ++i)
            {
                var value = smallList[i];
                var j = i - 1;
                while (j >= 0)
                {
                    ref readonly var previous = ref smallList[j];
                    if (CompareIntersections(in value, in previous) >= 0)
                    {
                        break;
                    }
                    smallList[j + 1] = previous;
                    --j;
                }
                smallList[j + 1] = value;
            }
        }
        else
        {
            _intersectList.Sort(IntersectComparer);
        }

        // Now as we process these intersections, we must sometimes adjust the order
        // to ensure that intersecting edges are always adjacent ...
        var intersections = CollectionsMarshal.AsSpan(_intersectList);
        for (var i = 0; i < countI; ++i)
        {
            if (!EdgesAdjacentInAEL(in intersections[i]))
            {
                var j = i + 1;
                while (!EdgesAdjacentInAEL(in intersections[j]))
                {
                    ++j;
                }
                (intersections[i], intersections[j]) = (intersections[j], intersections[i]);
            }

            ref readonly var node = ref intersections[i];
            IntersectEdges(node.edge1, node.edge2, node.pt);
            SwapPositionsInAEL(node.edge1, node.edge2);

            node.edge1.curX = node.pt.X;
            node.edge2.curX = node.pt.X;
            CheckJoinLeft(node.edge2, node.pt, true);
            CheckJoinRight(node.edge1, node.pt, true);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SwapPositionsInAEL(Active ae1, Active ae2)
    {
        // preconditon: ae1 must be immediately to the left of ae2
        var next = ae2.nextInAEL;
        next?.prevInAEL = ae1;
        var prev = ae1.prevInAEL;
        prev?.nextInAEL = ae2;
        ae2.prevInAEL = prev;
        ae2.nextInAEL = ae1;
        ae1.prevInAEL = ae2;
        ae1.nextInAEL = next;
        if (ae2.prevInAEL == null)
        {
            _actives = ae2;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ResetHorzDirection(Active horz, Vertex? vertexMax, out long leftX, out long rightX)
    {
        if (horz.bot.X == horz.top.X)
        {
            // the horizontal edge is going nowhere ...
            leftX = horz.curX;
            rightX = horz.curX;
            var ae = horz.nextInAEL;
            while (ae != null && ae.vertexTop != vertexMax)
            {
                ae = ae.nextInAEL;
            }
            return ae != null;
        }

        if (horz.curX < horz.top.X)
        {
            leftX = horz.curX;
            rightX = horz.top.X;
            return true;
        }
        leftX = horz.top.X;
        rightX = horz.curX;
        return false; // right to left
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TrimHorz(Active horzEdge, bool preserveCollinear)
    {
        var wasTrimmed = false;
        var pt = NextVertex(horzEdge).pt;

        while (pt.Y == horzEdge.top.Y)
        {
            // always trim 180 deg. spikes (in closed paths)
            // but otherwise break if preserveCollinear = true
            if (preserveCollinear && (pt.X < horzEdge.top.X) != (horzEdge.bot.X < horzEdge.top.X))
            {
                break;
            }

            horzEdge.vertexTop = NextVertex(horzEdge);
            horzEdge.top = pt;
            wasTrimmed = true;
            if (IsMaxima(horzEdge))
            {
                break;
            }
            pt = NextVertex(horzEdge).pt;
        }
        if (wasTrimmed)
        {
            SetDx(horzEdge); // +/-infinity
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddToHorzSegList(OutPt op)
    {
        if (op.outrec.isOpen)
        {
            return;
        }
        _horzSegList.Add(op);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OutPt GetLastOp(Active hotEdge)
    {
        var outrec = hotEdge.outrec!;
        return (hotEdge == outrec.frontEdge) ? outrec.pts! : outrec.pts!.next!;
    }

    private void DoHorizontal(Active horz)
    /*******************************************************************************
		 * Notes: Horizontal edges (HEs) at scanline intersections (i.e. at the top or    *
		 * bottom of a scanbeam) are processed as if layered.The order in which HEs     *
		 * are processed doesn't matter. HEs intersect with the bottom vertices of      *
		 * other HEs[#] and with non-horizontal edges [*]. Once these intersections     *
		 * are completed, intermediate HEs are 'promoted' to the next edge in their     *
		 * bounds, and they in turn may be intersected[%] by other HEs.                 *
		 *                                                                              *
		 * eg: 3 horizontals at a scanline:    /   |                     /           /  *
		 *              |                     /    |     (HE3)o ========%========== o   *
		 *              o ======= o(HE2)     /     |         /         /                *
		 *          o ============#=========*======*========#=========o (HE1)           *
		 *         /              |        /       |       /                            *
		 *******************************************************************************/
    {
        var horzIsOpen = IsOpen(horz);
        var Y = horz.bot.Y;

        var vertex_max = horzIsOpen ? GetCurrYMaximaVertex_Open(horz) : GetCurrYMaximaVertex(horz);

        var isLeftToRight =
          ResetHorzDirection(horz, vertex_max, out var leftX, out var rightX);

        if (IsHotEdge(horz))
        {
#if USINGZ
			OutPt op = AddOutPt(horz, new Point64(horz.curX, Y, horz.bot.Z));
#else
            var op = AddOutPt(horz, new Point64(horz.curX, Y));
#endif
            AddToHorzSegList(op);
        }

        for (; ; )
        {
            // loops through consec. horizontal edges (if open)
            var ae = isLeftToRight ? horz.nextInAEL : horz.prevInAEL;

            while (ae != null)
            {
                if (ae.vertexTop == vertex_max)
                {
                    // do this first!!
                    if (IsHotEdge(horz) && IsJoined(ae))
                    {
                        Split(ae, ae.top);
                    }

                    if (IsHotEdge(horz))
                    {
                        while (horz.vertexTop != vertex_max)
                        {
                            AddOutPt(horz, horz.top);
                            UpdateEdgeIntoAEL(horz);
                        }
                        if (isLeftToRight)
                        {
                            AddLocalMaxPoly(horz, ae, horz.top);
                        }
                        else
                        {
                            AddLocalMaxPoly(ae, horz, horz.top);
                        }
                    }
                    DeleteFromAEL(ae);
                    DeleteFromAEL(horz);
                    return;
                }

                // if horzEdge is a maxima, keep going until we reach
                // its maxima pair, otherwise check for break conditions
                Point64 pt;
                if (vertex_max != horz.vertexTop || IsOpenEnd(horz))
                {
                    // otherwise stop when 'ae' is beyond the end of the horizontal line
                    if (isLeftToRight && ae.curX > rightX || !isLeftToRight && ae.curX < leftX)
                    {
                        break;
                    }

                    if (ae.curX == horz.top.X && !IsHorizontal(ae))
                    {
                        pt = NextVertex(horz).pt;
                        var ptX = pt.X;
                        // to maximize the possibility of putting open edges into
                        // solutions, we'll only break if it's past HorzEdge's end
                        var topX = TopX(ae, pt.Y);
                        if (IsOpen(ae) && !IsSamePolyType(ae, horz) && !IsHotEdge(ae))
                        {
                            if (isLeftToRight && topX > ptX || !isLeftToRight && topX < ptX)
                            {
                                break;
                            }
                        }
                        // otherwise for edges at horzEdge's end, only stop when horzEdge's
                        // outslope is greater than e's slope when heading right or when
                        // horzEdge's outslope is less than e's slope when heading left.
                        else if (isLeftToRight && topX >= ptX || !isLeftToRight && topX <= ptX)
                        {
                            break;
                        }
                    }
                }

                pt = new Point64(ae.curX, Y);

                if (isLeftToRight)
                {
                    IntersectEdges(horz, ae, pt);
                    SwapPositionsInAEL(horz, ae);
                    CheckJoinLeft(ae, pt);
                    horz.curX = ae.curX;
                    ae = horz.nextInAEL;
                }
                else
                {
                    IntersectEdges(ae, horz, pt);
                    SwapPositionsInAEL(ae, horz);
                    CheckJoinRight(ae, pt);
                    horz.curX = ae.curX;
                    ae = horz.prevInAEL;
                }

                if (IsHotEdge(horz))
                {
                    AddToHorzSegList(GetLastOp(horz));
                }
            } // we've reached the end of this horizontal

            // check if we've finished looping
            // through consecutive horizontals
            if (horzIsOpen && IsOpenEnd(horz)) // ie open at top
            {
                if (IsHotEdge(horz))
                {
                    AddOutPt(horz, horz.top);
                    if (IsFront(horz))
                    {
                        horz.outrec!.frontEdge = null;
                    }
                    else
                    {
                        horz.outrec!.backEdge = null;
                    }
                    horz.outrec = null;
                }
                DeleteFromAEL(horz);
                return;
            }
            if (NextVertex(horz).pt.Y != horz.top.Y)
            {
                break;
            }

            //still more horizontals in bound to process ...
            if (IsHotEdge(horz))
            {
                AddOutPt(horz, horz.top);
            }

            UpdateEdgeIntoAEL(horz);

            isLeftToRight = ResetHorzDirection(horz, vertex_max, out leftX, out rightX);

        } // end for loop and end of (possible consecutive) horizontals

        if (IsHotEdge(horz))
        {
            var op = AddOutPt(horz, horz.top);
            AddToHorzSegList(op);
        }

        UpdateEdgeIntoAEL(horz); // this is the end of an intermediate horiz.
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoTopOfScanbeam(long y)
    {
        _sel = null; // sel_ is reused to flag horizontals (see PushHorz below)
        var ae = _actives;
        while (ae != null)
        {
            // NB 'ae' will never be horizontal here
            if (ae.top.Y == y)
            {
                ae.curX = ae.top.X;
                if (IsMaxima(ae))
                {
                    ae = DoMaxima(ae); // TOP OF BOUND (MAXIMA)
                    continue;
                }

                // INTERMEDIATE VERTEX ...
                if (IsHotEdge(ae))
                {
                    AddOutPt(ae, ae.top);
                }
                UpdateEdgeIntoAEL(ae);
                if (IsHorizontal(ae))
                {
                    PushHorz(ae); // horizontals are processed later
                }
            }
            else // i.e. not the top of the edge
            {
                ae.curX = TopX(ae, y);
            }

            ae = ae.nextInAEL;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Active? DoMaxima(Active ae)
    {
        var prevE = ae.prevInAEL;
        var nextE = ae.nextInAEL;

        if (IsOpenEnd(ae))
        {
            if (IsHotEdge(ae))
            {
                AddOutPt(ae, ae.top);
            }
            if (IsHorizontal(ae))
            {
                return nextE;
            }
            if (IsHotEdge(ae))
            {
                if (IsFront(ae))
                {
                    ae.outrec!.frontEdge = null;
                }
                else
                {
                    ae.outrec!.backEdge = null;
                }
                ae.outrec = null;
            }
            DeleteFromAEL(ae);
            return nextE;
        }

        var maxPair = GetMaximaPair(ae);
        if (maxPair == null)
        {
            return nextE; // eMaxPair is horizontal
        }

        if (IsJoined(ae))
        {
            Split(ae, ae.top);
        }
        if (IsJoined(maxPair))
        {
            Split(maxPair, maxPair.top);
        }

        // only non-horizontal maxima here.
        // process any edges between maxima pair ...
        while (nextE != maxPair)
        {
            IntersectEdges(ae, nextE!, ae.top);
            SwapPositionsInAEL(ae, nextE!);
            nextE = ae.nextInAEL;
        }

        if (IsOpen(ae))
        {
            if (IsHotEdge(ae))
            {
                AddLocalMaxPoly(ae, maxPair, ae.top);
            }
            DeleteFromAEL(maxPair);
            DeleteFromAEL(ae);
            return prevE != null ? prevE.nextInAEL : _actives;
        }

        // here ae.nextInAel == ENext == EMaxPair ...
        if (IsHotEdge(ae))
        {
            AddLocalMaxPoly(ae, maxPair, ae.top);
        }

        DeleteFromAEL(ae);
        DeleteFromAEL(maxPair);
        return prevE != null ? prevE.nextInAEL : _actives;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsJoined(Active e)
    {
        return e.joinWith != JoinWith.None;
    }

    private void Split(Active e, Point64 currPt)
    {
        if (e.joinWith == JoinWith.Right)
        {
            e.joinWith = JoinWith.None;
            e.nextInAEL!.joinWith = JoinWith.None;
            AddLocalMinPoly(e, e.nextInAEL, currPt, true);
        }
        else
        {
            e.joinWith = JoinWith.None;
            e.prevInAEL!.joinWith = JoinWith.None;
            AddLocalMinPoly(e.prevInAEL, e, currPt, true);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckJoinLeft(Active e,
      Point64 pt, bool checkCurrX = false)
    {
        var prev = e.prevInAEL;
        if (prev == null || !IsHotEdge(e) || !IsHotEdge(prev) || IsHorizontal(e) || IsHorizontal(prev) || IsOpen(e) || IsOpen(prev))
        {
            return;
        }
        var ptY = pt.Y;
        if ((ptY < e.top.Y + 2 || ptY < prev.top.Y + 2) && (e.bot.Y > ptY || prev.bot.Y > ptY)) // avoid trivial joins
        {
            return;  // (#490)
        }

        if (checkCurrX)
        {
            if (Clipper.PerpendicDistFromLineSqrd(pt, prev.bot, prev.top) > 0.25)
            {
                return;
            }
        }
        else if (e.curX != prev.curX)
        {
            return;
        }
        if (!InternalClipper.IsCollinear(e.top, pt, prev.top))
        {
            return;
        }

        if (e.outrec!.idx == prev.outrec!.idx)
        {
            AddLocalMaxPoly(prev, e, pt);
        }
        else if (e.outrec.idx < prev.outrec.idx)
        {
            JoinOutrecPaths(e, prev);
        }
        else
        {
            JoinOutrecPaths(prev, e);
        }
        prev.joinWith = JoinWith.Right;
        e.joinWith = JoinWith.Left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckJoinRight(Active e, Point64 pt, bool checkCurrX = false)
    {
        var next = e.nextInAEL;
        if (next == null || !IsHotEdge(e) || !IsHotEdge(next) || IsHorizontal(e) || IsHorizontal(next) || IsOpen(e) || IsOpen(next))
        {
            return;
        }
        var ptY = pt.Y;
        if ((ptY < e.top.Y + 2 || ptY < next.top.Y + 2) && (e.bot.Y > ptY || next.bot.Y > ptY)) // avoid trivial joins
        {
            return; // (#490)
        }

        if (checkCurrX)
        {
            if (Clipper.PerpendicDistFromLineSqrd(pt, next.bot, next.top) > 0.25)
            {
                return;
            }
        }
        else if (e.curX != next.curX)
        {
            return;
        }
        if (!InternalClipper.IsCollinear(e.top, pt, next.top))
        {
            return;
        }

        if (e.outrec!.idx == next.outrec!.idx)
        {
            AddLocalMaxPoly(e, next, pt);
        }
        else if (e.outrec.idx < next.outrec.idx)
        {
            JoinOutrecPaths(e, next);
        }
        else
        {
            JoinOutrecPaths(next, e);
        }
        e.joinWith = JoinWith.Right;
        next.joinWith = JoinWith.Left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void FixOutRecPts(OutRec outrec)
    {
        var op = outrec.pts!;
        var count = 0;
        do
        {
            op.outrec = outrec;
            ++count;
            op = op.next!;
        } while (op != outrec.pts);
        outrec.outPtCount = count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SetHorzSegHeadingForward(HorzSegment hs, OutPt opP, OutPt opN)
    {
        var ptPX = opP.pt.X;
        var ptNX = opN.pt.X;
        if (ptPX == ptNX)
        {
            return false;
        }
        if (ptPX < ptNX)
        {
            hs.leftOp = opP;
            hs.rightOp = opN;
            hs.leftToRight = true;
        }
        else
        {
            hs.leftOp = opN;
            hs.rightOp = opP;
            hs.leftToRight = false;
        }
        return true;
    }

    private static bool UpdateHorzSegment(HorzSegment hs)
    {
        var op = hs.leftOp!;
        var outrec = GetRealOutRec(op.outrec)!;
        var outrecHasEdges = outrec.frontEdge != null;
        var curr_y = op.pt.Y;
        OutPt opP = op, opN = op;
        if (outrecHasEdges)
        {
            OutPt opA = outrec.pts!, opZ = opA.next!;
            while (opP != opZ && opP.prev.pt.Y == curr_y)
            {
                opP = opP.prev;
            }
            while (opN != opA && opN.next!.pt.Y == curr_y)
            {
                opN = opN.next;
            }
        }
        else
        {
            while (opP.prev != opN && opP.prev.pt.Y == curr_y)
            {
                opP = opP.prev;
            }
            while (opN.next != opP && opN.next!.pt.Y == curr_y)
            {
                opN = opN.next;
            }
        }
        var result = SetHorzSegHeadingForward(hs, opP, opN) && hs.leftOp!.horz == null;

        if (result)
        {
            hs.leftOp!.horz = hs;
        }
        else
        {
            hs.rightOp = null; // (for sorting)
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private OutPt DuplicateOp(OutPt op, bool insert_after)
    {
        var result = _outPtPool.Add(op.pt, op.outrec);
        if (insert_after)
        {
            result.next = op.next;
            result.next!.prev = result;
            result.prev = op;
            op.next = result;
        }
        else
        {
            result.prev = op.prev;
            result.prev.next = result;
            result.next = op;
            op.prev = result;
        }
        return result;
    }

    private static int HorzSegSort(HorzSegment? hs1, HorzSegment? hs2)
    {
        if (hs1 == null || hs2 == null)
        {
            return 0;
        }
        if (hs1.rightOp == null)
        {
            return hs2.rightOp == null ? 0 : 1;
        }
        if (hs2.rightOp == null)
        {
            return -1;
        }
        return hs1.leftOp!.pt.X.CompareTo(hs2.leftOp!.pt.X);
    }

    private void ConvertHorzSegsToJoins()
    {
        var k = 0;
        var count = _horzSegList.Count;
        for (var i = 0; i < count; ++i)
        {
            if (UpdateHorzSegment(_horzSegList[i]))
            {
                ++k;
            }
        }
        if (k < 2)
        {
            return;
        }
        _horzSegList.Sort(HorzSegmentComparer);

        for (var i = 0; i < k - 1; ++i)
        {
            var hs1 = _horzSegList[i];
            // for each HorzSegment, find others that overlap
            for (var j = i + 1; j < k; ++j)
            {
                var hs2 = _horzSegList[j];
                if (hs2.leftOp!.pt.X >= hs1.rightOp!.pt.X)
                {
                    break;
                }
                if (hs2.leftToRight == hs1.leftToRight || hs2.rightOp!.pt.X <= hs1.leftOp!.pt.X)
                {
                    continue;
                }
                var curr_y = hs1.leftOp.pt.Y;
                if (hs1.leftToRight)
                {
                    while (hs1.leftOp.next!.pt.Y == curr_y && hs1.leftOp.next.pt.X <= hs2.leftOp.pt.X)
                    {
                        hs1.leftOp = hs1.leftOp.next;
                    }
                    while (hs2.leftOp.prev.pt.Y == curr_y && hs2.leftOp.prev.pt.X <= hs1.leftOp.pt.X)
                    {
                        hs2.leftOp = hs2.leftOp.prev;
                    }
                    _horzJoinList.Add(DuplicateOp(hs1.leftOp, true), DuplicateOp(hs2.leftOp, false));
                }
                else
                {
                    while (hs1.leftOp.prev.pt.Y == curr_y && hs1.leftOp.prev.pt.X <= hs2.leftOp.pt.X)
                    {
                        hs1.leftOp = hs1.leftOp.prev;
                    }
                    while (hs2.leftOp.next!.pt.Y == curr_y && hs2.leftOp.next.pt.X <= hs1.leftOp.pt.X)
                    {
                        hs2.leftOp = hs2.leftOp.next;
                    }
                    _horzJoinList.Add(DuplicateOp(hs2.leftOp, true), DuplicateOp(hs1.leftOp, false));
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Path64 GetCleanPath(OutPt op)
    {
        Path64 result = [with(op.outrec.outPtCount)];
        var op2 = op;
        while (op2.next != op && (op2.pt.X == op2.next!.pt.X && op2.pt.X == op2.prev.pt.X || op2.pt.Y == op2.next.pt.Y && op2.pt.Y == op2.prev.pt.Y))
        {
            op2 = op2.next;
        }
        result.Add(op2.pt);
        var prevOp = op2;
        op2 = op2.next;
        while (op2 != op)
        {
            if ((op2.pt.X != op2.next!.pt.X || op2.pt.X != prevOp.pt.X) && (op2.pt.Y != op2.next.pt.Y || op2.pt.Y != prevOp.pt.Y))
            {
                result.Add(op2.pt);
                prevOp = op2;
            }
            op2 = op2.next;
        }
        return result;
    }

    private static PointInPolygonResult PointInOpPolygon(Point64 pt, OutPt op)
    {
        if (op == op.next || op.prev == op.next)
        {
            return PointInPolygonResult.IsOutside;
        }
        var ptX = pt.X;
        var ptY = pt.Y;
        var op2 = op;
        do
        {
            if (op.pt.Y != ptY)
            {
                break;
            }
            op = op.next!;
        } while (op != op2);
        if (op.pt.Y == ptY) // not a proper polygon
        {
            return PointInPolygonResult.IsOutside;
        }

        // must be above or below to get here
        bool isAbove = op.pt.Y < ptY, startingAbove = isAbove;
        var val = 0;

        op2 = op.next!;
        while (op2 != op)
        {
            if (isAbove)
            {
                while (op2 != op && op2.pt.Y < ptY)
                {
                    op2 = op2.next!;
                }
            }
            else
            {
                while (op2 != op && op2.pt.Y > ptY)
                {
                    op2 = op2.next!;
                }
            }
            if (op2 == op)
            {
                break;
            }

            // must have touched or crossed the pt.Y horizontal
            // and this must happen an even number of times

            if (op2.pt.Y == ptY) // touching the horizontal
            {
                if (op2.pt.X == ptX || op2.pt.Y == op2.prev.pt.Y && (ptX < op2.prev.pt.X) != (ptX < op2.pt.X))
                {
                    return PointInPolygonResult.IsOn;
                }
                op2 = op2.next!;
                if (op2 == op)
                {
                    break;
                }
                continue;
            }

            if (op2.pt.X <= ptX || op2.prev.pt.X <= ptX)
            {
                if (op2.prev.pt.X < ptX && op2.pt.X < ptX)
                {
                    val = 1 - val; // toggle val
                }
                else
                {
                    var d = InternalClipper.CrossProduct(op2.prev.pt, op2.pt, pt);
                    if (d == 0d)
                    {
                        return PointInPolygonResult.IsOn;
                    }
                    if ((d < 0d) == isAbove)
                    {
                        val = 1 - val;
                    }
                }
            }
            isAbove = !isAbove;
            op2 = op2.next!;
        }

        if (isAbove == startingAbove)
        {
            return val == 0 ? PointInPolygonResult.IsOutside : PointInPolygonResult.IsInside;
        }
        {
            var d = InternalClipper.CrossProduct(op2.prev.pt, op2.pt, pt);
            if (d == 0d)
            {
                return PointInPolygonResult.IsOn;
            }
            if ((d < 0d) == isAbove)
            {
                val = 1 - val;
            }
        }

        return val == 0 ? PointInPolygonResult.IsOutside : PointInPolygonResult.IsInside;
    }

    private static bool Path1InsidePath2(OutPt op1, OutPt op2)
    {
        // we need to make some accommodation for rounding errors
        // so we won't jump if the first vertex is found outside
        var pip = PointInPolygonResult.IsOn;
        var op = op1;
        do
        {
            switch (PointInOpPolygon(op.pt, op2))
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
            op = op.next!;
        } while (op != op1);
        // result is unclear, so try again using cleaned paths
        return InternalClipper.Path2ContainsPath1(GetCleanPath(op1), GetCleanPath(op2)); // (#973)
    }

    private static void MoveSplits(OutRec fromOr, OutRec toOr)
    {
        var fromSplits = fromOr.splits;
        if (fromSplits == null)
        {
            return;
        }
        if (toOr.splits == null)
        {
            toOr.splits = fromSplits;
            fromOr.splits = null;
            return;
        }
        toOr.splits.EnsureCapacity(toOr.splits.Count + fromSplits.Count);
        toOr.splits.AddRange(fromSplits);
        fromOr.splits = null;
    }

    private void ProcessHorzJoins()
    {
        var count = _horzJoinList.Count;
        for (var joinIndex = 0; joinIndex < count; ++joinIndex)
        {
            var j = _horzJoinList[joinIndex];
            var or1 = GetRealOutRec(j.op1!.outrec)!;
            var or2 = GetRealOutRec(j.op2!.outrec)!;

            var op1b = j.op1.next!;
            var op2b = j.op2.prev;
            j.op1.next = j.op2;
            j.op2.prev = j.op1;
            op1b.prev = op2b;
            op2b.next = op1b;

            if (or1 == or2) // 'join' is really a split
            {
                or2 = NewOutRec();
                or2.pts = op1b;
                FixOutRecPts(or2);

                //if or1->pts has moved to or2 then update or1->pts!!
                if (or1.pts!.outrec == or2)
                {
                    or1.pts = j.op1;
                    or1.pts.outrec = or1;
                }

                if (_using_polytree)  //#498, #520, #584, D#576, #618
                {
                    if (Path1InsidePath2(or1.pts, or2.pts))
                    {
                        //swap or1's & or2's pts
                        (or2.pts, or1.pts) = (or1.pts, or2.pts);
                        FixOutRecPts(or1);
                        FixOutRecPts(or2);
                        //or2 is now inside or1
                        or2.owner = or1;
                    }
                    else if (Path1InsidePath2(or2.pts, or1.pts))
                    {
                        or2.owner = or1;
                    }
                    else
                    {
                        or2.owner = or1.owner;
                    }

                    or1.splits ??= [];
                    or1.splits.Add(or2.idx);
                }
                else
                {
                    or2.owner = or1;
                }
            }
            else
            {
                or1.outPtCount += or2.outPtCount;
                or2.pts = null;
                if (_using_polytree)
                {
                    SetOwner(or2, or1);
                    MoveSplits(or2, or1); //#618
                }
                else
                {
                    or2.owner = or1;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool PtsReallyClose(Point64 pt1, Point64 pt2)
    {
        return Math.Abs(pt1.X - pt2.X) < 2 && Math.Abs(pt1.Y - pt2.Y) < 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsVerySmallTriangle(OutPt op)
    {
        return op.next!.next == op.prev && (PtsReallyClose(op.prev.pt, op.next.pt) || PtsReallyClose(op.pt, op.next.pt) || PtsReallyClose(op.pt, op.prev.pt));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidClosedPath(OutPt? op)
    {
        return op != null && op.next != op &&
          (op.next != op.prev || !IsVerySmallTriangle(op));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OutPt? DisposeOutPt(OutPt op)
    {
        var result = op.next == op ? null : op.next;
        op.prev.next = op.next;
        op.next!.prev = op.prev;
        // op == null;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CleanCollinear(OutRec? outrec)
    {
        outrec = GetRealOutRec(outrec);

        if (outrec == null || outrec.isOpen)
        {
            return;
        }

        if (!IsValidClosedPath(outrec.pts))
        {
            outrec.pts = null;
            return;
        }

        var startOp = outrec.pts!;
        var op2 = startOp;
        for (; ; )
        {
            // NB if preserveCollinear == true, then only remove 180 deg. spikes
            if (InternalClipper.IsCollinear(op2!.prev.pt, op2.pt, op2.next!.pt) && (op2.pt == op2.prev.pt || op2.pt == op2.next.pt || !PreserveCollinear
                || InternalClipper.DotProduct(op2.prev.pt, op2.pt, op2.next.pt) < 0d))
            {
                if (op2 == outrec.pts)
                {
                    outrec.pts = op2.prev;
                }
                op2 = DisposeOutPt(op2);
                if (!IsValidClosedPath(op2))
                {
                    outrec.pts = null;
                    return;
                }
                startOp = op2!;
                continue;
            }
            op2 = op2.next;
            if (op2 == startOp)
            {
                break;
            }
        }
        FixSelfIntersects(outrec);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoSplitOp(OutRec outrec, OutPt splitOp)
    {
        // splitOp.prev <=> splitOp &&
        // splitOp.next <=> splitOp.next.next are intersecting
        var prevOp = splitOp.prev;
        var nextNextOp = splitOp.next!.next!;
        outrec.pts = prevOp;

        InternalClipper.GetSegmentIntersectPt(prevOp.pt, splitOp.pt, splitOp.next.pt, nextNextOp.pt, out var ip);

#if USINGZ
		if (_zCallback != null)
		{
			_zCallback(prevOp.pt, splitOp.pt, splitOp.next.pt, nextNextOp.pt, ref ip);
		}
#endif

        var area1 = Area(prevOp);
        var absArea1 = Math.Abs(area1);

        if (absArea1 < 2d)
        {
            outrec.pts = null;
            return;
        }

        var area2 = AreaTriangle(ip, splitOp.pt, splitOp.next.pt);
        var absArea2 = Math.Abs(area2);

        // de-link splitOp and splitOp.next from the path
        // while inserting the intersection point
        if (ip == prevOp.pt || ip == nextNextOp.pt)
        {
            nextNextOp.prev = prevOp;
            prevOp.next = nextNextOp;
        }
        else
        {
            var newOp2 = _outPtPool.Add(ip, outrec);
            newOp2.prev = prevOp;
            newOp2.next = nextNextOp;
            nextNextOp.prev = newOp2;
            prevOp.next = newOp2;
        }

        // nb: area1 is the path's area *before* splitting, whereas area2 is
        // the area of the triangle containing splitOp & splitOp.next.
        // So the only way for these areas to have the same sign is if
        // the split triangle is larger than the path containing prevOp or
        // if there's more than one self=intersection.
        if (!(absArea2 > 1d) || !(absArea2 > absArea1) && (area2 > 0d) != (area1 > 0d))
        {
            return;
        }
        var newOutRec = NewOutRec();
        newOutRec.owner = outrec.owner;
        splitOp.outrec = newOutRec;
        splitOp.next.outrec = newOutRec;

        var newOp = _outPtPool.Add(ip, newOutRec);
        newOp.prev = splitOp.next;
        newOp.next = splitOp;
        newOutRec.pts = newOp;
        newOutRec.outPtCount = 3;
        splitOp.prev = newOp;
        splitOp.next.next = newOp;

        if (!_using_polytree)
        {
            return;
        }
        if (Path1InsidePath2(prevOp, newOp))
        {
            newOutRec.splits ??= [];
            newOutRec.splits.Add(outrec.idx);
        }
        else
        {
            outrec.splits ??= [];
            outrec.splits.Add(newOutRec.idx);
        }
        //else { splitOp = null; splitOp.next = null; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FixSelfIntersects(OutRec outrec)
    {
        var op2 = outrec.pts!;
        if (op2.prev == op2.next!.next)
        {
            return; // because triangles can't self-intersect
        }
        for (; ; )
        {
            if (InternalClipper.SegsIntersect(op2!.prev.pt, op2.pt, op2.next!.pt, op2.next.next!.pt))
            {
                if (InternalClipper.SegsIntersect(op2.prev.pt, op2.pt, op2.next.next!.pt, op2.next.next.next!.pt))
                {
                    // adjacent intersections (ie a micro self-intersection)
                    op2 = DuplicateOp(op2, false);
                    op2.pt = op2.next!.next!.next!.pt;
                    op2 = op2.next;
                }
                else
                {
                    if (op2 == outrec.pts || op2.next == outrec.pts)
                    {
                        outrec.pts = outrec.pts.prev;
                    }
                    DoSplitOp(outrec, op2);
                    if (outrec.pts == null)
                        return;
                    op2 = outrec.pts;
                    // triangles can't self-intersect
                    if (op2.prev == op2.next!.next)
                        break;
                    continue;
                }
            }

            op2 = op2.next!;
            if (op2 == outrec.pts)
            {
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Span<Point64> PreparePathBuffer(Path64 path, int expectedCount)
    {
        path.Clear();
        expectedCount = Math.Max(expectedCount, 4);
        path.EnsureCapacity(expectedCount);
        CollectionsMarshal.SetCount(path, expectedCount);
        return CollectionsMarshal.AsSpan(path);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void GrowPathBuffer(Path64 path, ref Span<Point64> buffer, int count)
    {
        CollectionsMarshal.SetCount(path, count);
        path.EnsureCapacity(count + 1);
        CollectionsMarshal.SetCount(path, path.Capacity);
        buffer = CollectionsMarshal.AsSpan(path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void IncludeInBounds(ref Rect64 bounds, Point64 point)
    {
        if (point.X < bounds.left)
        {
            bounds.left = point.X;
        }
        if (point.X > bounds.right)
        {
            bounds.right = point.X;
        }
        if (point.Y < bounds.top)
        {
            bounds.top = point.Y;
        }
        if (point.Y > bounds.bottom)
        {
            bounds.bottom = point.Y;
        }
    }

    internal static bool BuildPath(OutPt? op, bool reverse, bool isOpen, Path64 path)
    {
        if (op == null || op.next == op || !isOpen && op.next == op.prev)
        {
            return false;
        }

        Point64 lastPt;
        OutPt op2;
        if (reverse)
        {
            lastPt = op.pt;
            op2 = op.prev;
        }
        else
        {
            op = op.next!;
            lastPt = op.pt;
            op2 = op.next!;
        }
        var buffer = PreparePathBuffer(path, op.outrec.outPtCount);
        var count = 0;
        buffer[count++] = lastPt;

        while (op2 != op)
        {
            if (op2.pt != lastPt)
            {
                lastPt = op2.pt;
                if (count == buffer.Length)
                {
                    GrowPathBuffer(path, ref buffer, count);
                }
                buffer[count++] = lastPt;
            }
            if (reverse)
            {
                op2 = op2.prev;
            }
            else
            {
                op2 = op2.next!;
            }
        }

        CollectionsMarshal.SetCount(path, count);
        return count != 3 || isOpen || !IsVerySmallTriangle(op2);
    }

    private static bool BuildPathAndBounds(OutPt? op, bool reverse, Path64 path, out Rect64 bounds)
    {
        bounds = Clipper.InvalidRect64;
        if (op == null || op.next == op || op.next == op.prev)
        {
            return false;
        }

        Point64 lastPt;
        OutPt op2;
        if (reverse)
        {
            lastPt = op.pt;
            op2 = op.prev;
        }
        else
        {
            op = op.next!;
            lastPt = op.pt;
            op2 = op.next!;
        }

        var buffer = PreparePathBuffer(path, op.outrec.outPtCount);
        var count = 0;
        buffer[count++] = lastPt;
        IncludeInBounds(ref bounds, lastPt);
        while (op2 != op)
        {
            if (op2.pt != lastPt)
            {
                lastPt = op2.pt;
                if (count == buffer.Length)
                {
                    GrowPathBuffer(path, ref buffer, count);
                }
                buffer[count++] = lastPt;
                IncludeInBounds(ref bounds, lastPt);
            }
            if (reverse)
            {
                op2 = op2.prev;
            }
            else
            {
                op2 = op2.next!;
            }
        }

        CollectionsMarshal.SetCount(path, count);
        return count != 3 || !IsVerySmallTriangle(op2);
    }

    protected bool BuildPaths(Paths64 solutionClosed, Paths64 solutionOpen)
    {
        solutionClosed.Clear();
        solutionOpen.Clear();
        var count = _outrecList.Count;
        solutionClosed.EnsureCapacity(count);
        solutionOpen.EnsureCapacity(count);

        var i = 0;
        // _outrecList.Count is not static here because
        // CleanCollinear can indirectly add additional OutRec
        while (i < count)
        {
            var outrec = _outrecList[i++];
            if (outrec.pts == null)
            {
                continue;
            }

            Path64 path = [with(outrec.outPtCount)];
            if (outrec.isOpen)
            {
                if (BuildPath(outrec.pts, ReverseSolution, true, path))
                {
                    solutionOpen.Add(path);
                }
            }
            else
            {
                CleanCollinear(outrec);
                // closed paths should always return a Positive orientation
                // except when ReverseSolution == true
                if (BuildPath(outrec.pts, ReverseSolution, false, path))
                {
                    solutionClosed.Add(path);
                }
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CheckBounds(OutRec outrec)
    {
        if (outrec.pts == null)
        {
            return false;
        }
        if (!outrec.bounds.IsEmpty())
        {
            return true;
        }
        CleanCollinear(outrec);
        var path = outrec.path ??= [with(outrec.outPtCount)];
        if (outrec.pts == null || !BuildPathAndBounds(outrec.pts, ReverseSolution, path, out var bounds))
        {
            return false;
        }
        outrec.bounds = bounds;
        return true;
    }

    private bool CheckSplitOwner(OutRec outrec, List<int>? splits)
    {
        var count = splits!.Count;
        for (var idx = 0; idx < count; ++idx)
        {
            var split = _outrecList[splits[idx]];
            if (split.pts == null && split.splits != null && CheckSplitOwner(outrec, split.splits))
            {
                return true; //#942
            }
            split = GetRealOutRec(split);
            if (split == null || split == outrec || split.recursiveSplit == outrec)
            {
                continue;
            }
            split.recursiveSplit = outrec; //#599

            if (split.splits != null && CheckSplitOwner(outrec, split.splits))
            {
                return true;
            }

            if (!CheckBounds(split) || !split.bounds.Contains(outrec.bounds) || !Path1InsidePath2(outrec.pts!, split.pts!))
            {
                continue;
            }

            if (!IsValidOwner(outrec, split)) // split is owned by outrec (#957)
            {
                split.owner = outrec.owner;
            }

            outrec.owner = split; //found in split
            return true;
        }
        return false;
    }
    private void RecursiveCheckOwners(OutRec outrec, PolyPathBase polypath)
    {
        // pre-condition: outrec will have valid bounds
        // post-condition: if a valid path, outrec will have a polypath

        if (outrec.polypath != null || outrec.bounds.IsEmpty())
        {
            return;
        }

        while (outrec.owner != null)
        {
            if (outrec.owner.splits != null && CheckSplitOwner(outrec, outrec.owner.splits))
            {
                break;
            }
            if (outrec.owner.pts != null && CheckBounds(outrec.owner) && Path1InsidePath2(outrec.pts!, outrec.owner.pts!))
            {
                break;
            }
            outrec.owner = outrec.owner.owner;
        }

        if (outrec.owner != null)
        {
            if (outrec.owner.polypath == null)
            {
                RecursiveCheckOwners(outrec.owner, polypath);
            }
            outrec.polypath = outrec.owner.polypath!.AddChild(outrec.path!);
        }
        else
        {
            outrec.polypath = polypath.AddChild(outrec.path!);
        }
    }

    protected void BuildTree(PolyPathBase polytree, Paths64 solutionOpen)
    {
        polytree.Clear();
        solutionOpen.Clear();
        var count = _outrecList.Count;
        if (_hasOpenPaths)
        {
            solutionOpen.EnsureCapacity(count);
        }

        var i = 0;
        // _outrecList.Count is not static here because
        // CheckBounds below can indirectly add additional
        // OutRec (via FixOutRecPts & CleanCollinear)
        while (i < count)
        {
            var outrec = _outrecList[i++];
            if (outrec.pts == null)
            {
                continue;
            }

            if (outrec.isOpen)
            {
                Path64 open_path = [with(outrec.outPtCount)];
                if (BuildPath(outrec.pts, ReverseSolution, true, open_path))
                {
                    solutionOpen.Add(open_path);
                }
                continue;
            }
            if (CheckBounds(outrec))
            {
                RecursiveCheckOwners(outrec, polytree);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rect64 GetBounds()
    {
        var bounds = Clipper.InvalidRect64;
        var count = _vertexList.Count;
        for (var i = 0; i < count; ++i)
        {
            var pt = _vertexList[i].pt;
            if (pt.X < bounds.left)
            {
                bounds.left = pt.X;
            }
            if (pt.X > bounds.right)
            {
                bounds.right = pt.X;
            }
            if (pt.Y < bounds.top)
            {
                bounds.top = pt.Y;
            }
            if (pt.Y > bounds.bottom)
            {
                bounds.bottom = pt.Y;
            }
        }
        return bounds.IsEmpty() ? new Rect64(0L, 0L, 0L, 0L) : bounds;
    }
}

[SkipLocalsInit]
public sealed class Clipper64 : ClipperBase
{
    private readonly Paths64 _discardedOpenPaths = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal new void AddPath(Path64 path, PathType polytype, bool isOpen = false)
    {
        base.AddPath(path, polytype, isOpen);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void AddPath(ReadOnlySpan<Point64> path, PathType polytype, bool isOpen = false)
    {
        AddPathSpan(path, polytype, isOpen);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new void AddReuseableData(ReuseableDataContainer64 reuseableData)
    {
        base.AddReuseableData(reuseableData);
    }

    // BMR edit: a version of AddReusableData that forces polytype; useful if same data is to be reused as subject or clip
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public new void AddReuseableData(ReuseableDataContainer64 reuseableData, PathType typeOverride)
    {
        base.AddReuseableData(reuseableData, typeOverride);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal new void AddPaths(Paths64 paths, PathType polytype, bool isOpen = false)
    {
        base.AddPaths(paths, polytype, isOpen);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSubject(Paths64 paths)
    {
        AddPaths(paths, PathType.Subject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOpenSubject(Paths64 paths)
    {
        AddPaths(paths, PathType.Subject, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddClip(Paths64 paths)
    {
        AddPaths(paths, PathType.Clip);
    }

    public bool Execute(ClipType clipType, FillRule fillRule, Paths64 solutionClosed, Paths64 solutionOpen)
    {
        solutionClosed.Clear();
        solutionOpen.Clear();
        _using_polytree = false;
        try
        {
            ExecuteInternal(clipType, fillRule);
            BuildPaths(solutionClosed, solutionOpen);
        }
        catch
        {
            _succeeded = false;
        }

        ClearSolutionOnly();
        return _succeeded;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Execute(ClipType clipType, FillRule fillRule, Paths64 solutionClosed)
    {
        var succeeded = Execute(clipType, fillRule, solutionClosed, _discardedOpenPaths);
        _discardedOpenPaths.Clear();
        return succeeded;
    }

    public bool Execute(ClipType clipType, FillRule fillRule, PolyTree64 polytree, Paths64 openPaths)
    {
        polytree.Clear();
        openPaths.Clear();
        _using_polytree = true;
        try
        {
            ExecuteInternal(clipType, fillRule);
            BuildTree(polytree, openPaths);
        }
        catch
        {
            _succeeded = false;
        }

        ClearSolutionOnly();
        return _succeeded;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Execute(ClipType clipType, FillRule fillRule, PolyTree64 polytree)
    {
        var succeeded = Execute(clipType, fillRule, polytree, _discardedOpenPaths);
        _discardedOpenPaths.Clear();
        return succeeded;
    }

#if USINGZ
	public ZCallback64? ZCallback
	{
		get 
		{ 
			return this._zCallback;
		}
		set { this._zCallback = value; }
	}
#endif

}

[SkipLocalsInit]
public sealed class ClipperD : ClipperBase
{
    private const string precision_range_error = "Error: Precision is out of range.";
    private const int StackScalePointCapacity = 64;

    private readonly double _scale;
    private readonly double _invScale;
    private readonly Paths64 _solutionClosed64 = [];
    private readonly Paths64 _solutionOpen64 = [];
    private readonly PathsD _discardedOpenPaths = [];
    private Point64[] _scaledPathBuffer = [];

#if USINGZ
	public delegate void ZCallbackD(PointD bot1, PointD top1, PointD bot2, PointD top2, ref PointD intersectPt);

	public ZCallbackD? ZCallback;

	private void CheckZCallback()
	{
	if (ZCallback != null)
		_zCallback = ZCB;
	else
		_zCallback = null;
	}
#endif

    public ClipperD(int roundingDecimalPrecision = 2)
    {
        if (roundingDecimalPrecision is < -8 or > 8)
        {
            throw new ClipperLibException(precision_range_error);
        }
        _scale = Math.Pow(10d, roundingDecimalPrecision);
        _invScale = 1d / _scale;
    }

#if USINGZ
	private void ZCB(Point64 bot1, Point64 top1, Point64 bot2, Point64 top2, ref Point64 intersectPt)
	{
	// de-scale (x & y)
	// temporarily convert integers to their initial float values
	// this will slow clipping marginally but will make it much easier
	// to understand the coordinates passed to the callback function
	PointD tmp = Clipper.ScalePointD(intersectPt, _invScale);
	//do the callback
	ZCallback?.Invoke(Clipper.ScalePointD(bot1, _invScale), Clipper.ScalePointD(top1, _invScale),
		Clipper.ScalePointD(bot2, _invScale), Clipper.ScalePointD(top2, _invScale), ref tmp);
	intersectPt = new Point64(intersectPt.X, intersectPt.Y, tmp.z);
	}
#endif

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowScaledPathBuffer(int minimumCapacity)
    {
        var capacity = _scaledPathBuffer.Length == 0 ? 16 : _scaledPathBuffer.Length;
        while (capacity < minimumCapacity && capacity <= Array.MaxLength / 2)
        {
            capacity *= 2;
        }
        if (capacity < minimumCapacity)
        {
            capacity = minimumCapacity;
        }
        _scaledPathBuffer = GC.AllocateUninitializedArray<Point64>(capacity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddScaledPath(PathD path, PathType polytype, bool isOpen, scoped Span<Point64> scaledBuffer)
    {
        var count = path.Count;
        var scaled = scaledBuffer[..count];
        Clipper.ScalePoints64(CollectionsMarshal.AsSpan(path), scaled, _scale);
        AddPathSpan(scaled, polytype, isOpen);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddScaledPathBuffered(PathD path, PathType polytype, bool isOpen)
    {
        var count = path.Count;
        if (_scaledPathBuffer.Length < count)
        {
            GrowScaledPathBuffer(count);
        }
        AddScaledPath(path, polytype, isOpen, _scaledPathBuffer);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddScaledPathStack(PathD path, PathType polytype, bool isOpen)
    {
        Span<Point64> scaled = stackalloc Point64[path.Count];
        AddScaledPath(path, polytype, isOpen, scaled);
    }

    [SkipLocalsInit]
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddScaledPathsStack(scoped ReadOnlySpan<PathD> paths, int maximumPathCount, PathType polytype, bool isOpen)
    {
        Span<Point64> scaled = stackalloc Point64[maximumPathCount];
        var len = paths.Length;
        for (var i = 0; i < len; ++i)
        {
            AddScaledPath(paths[i], polytype, isOpen, scaled);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddPath(PathD path, PathType polytype, bool isOpen = false)
    {
        if (path.Count <= StackScalePointCapacity)
        {
            AddScaledPathStack(path, polytype, isOpen);
            return;
        }
        AddScaledPathBuffered(path, polytype, isOpen);
    }

    public void AddPaths(PathsD paths, PathType polytype, bool isOpen = false)
    {
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var totalPointCount = 0;
        var maximumPathCount = 0;
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            var count = pathSpan[i].Count;
            totalPointCount += count;
            if (count > maximumPathCount)
            {
                maximumPathCount = count;
            }
        }
        EnsureAdditionalVertexCapacity(totalPointCount);
        if (maximumPathCount <= StackScalePointCapacity)
        {
            AddScaledPathsStack(pathSpan, maximumPathCount, polytype, isOpen);
            return;
        }
        if (_scaledPathBuffer.Length < maximumPathCount)
        {
            GrowScaledPathBuffer(maximumPathCount);
        }
        for (var i = 0; i < len; ++i)
        {
            AddScaledPath(pathSpan[i], polytype, isOpen, _scaledPathBuffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSubject(PathD path)
    {
        AddPath(path, PathType.Subject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOpenSubject(PathD path)
    {
        AddPath(path, PathType.Subject, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddClip(PathD path)
    {
        AddPath(path, PathType.Clip);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddSubject(PathsD paths)
    {
        AddPaths(paths, PathType.Subject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddOpenSubject(PathsD paths)
    {
        AddPaths(paths, PathType.Subject, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddClip(PathsD paths)
    {
        AddPaths(paths, PathType.Clip);
    }

    public bool Execute(ClipType clipType, FillRule fillRule, PathsD solutionClosed, PathsD solutionOpen)
    {
        Paths64 solClosed64 = _solutionClosed64, solOpen64 = _solutionOpen64;
#if USINGZ
		CheckZCallback();
#endif

        var success = true;
        solutionClosed.Clear();
        solutionOpen.Clear();
        _using_polytree = false;
        try
        {
            ExecuteInternal(clipType, fillRule);
            BuildPaths(solClosed64, solOpen64);
        }
        catch
        {
            success = false;
        }

        ClearSolutionOnly();
        if (!success)
        {
            solClosed64.Clear();
            solOpen64.Clear();
            return false;
        }

        solutionClosed.EnsureCapacity(solClosed64.Count);
        CollectionsMarshal.SetCount(solutionClosed, solClosed64.Count);
        var closedSource = CollectionsMarshal.AsSpan(solClosed64);
        var closedDestination = CollectionsMarshal.AsSpan(solutionClosed);
        var lenC = closedSource.Length;
        for (var i = 0; i < lenC; ++i)
        {
            closedDestination[i] = Clipper.ScalePathD(closedSource[i], _invScale);
        }
        solutionOpen.EnsureCapacity(solOpen64.Count);
        CollectionsMarshal.SetCount(solutionOpen, solOpen64.Count);
        var openSource = CollectionsMarshal.AsSpan(solOpen64);
        var openDestination = CollectionsMarshal.AsSpan(solutionOpen);
        var lenO = openSource.Length;
        for (var i = 0; i < lenO; ++i)
        {
            openDestination[i] = Clipper.ScalePathD(openSource[i], _invScale);
        }
        solClosed64.Clear();
        solOpen64.Clear();

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Execute(ClipType clipType, FillRule fillRule, PathsD solutionClosed)
    {
        var succeeded = Execute(clipType, fillRule, solutionClosed, _discardedOpenPaths);
        _discardedOpenPaths.Clear();
        return succeeded;
    }

    public bool Execute(ClipType clipType, FillRule fillRule, PolyTreeD polytree, PathsD openPaths)
    {
        polytree.Clear();
        openPaths.Clear();
        _using_polytree = true;
        (polytree as PolyPathD).Scale = _scale;
#if USINGZ
		CheckZCallback();
#endif
        var oPaths = _solutionOpen64;
        var success = true;
        try
        {
            ExecuteInternal(clipType, fillRule);
            BuildTree(polytree, oPaths);
        }
        catch
        {
            success = false;
        }
        ClearSolutionOnly();
        if (!success)
        {
            oPaths.Clear();
            return false;
        }
        if (oPaths.Count <= 0)
        {
            return true;
        }
        openPaths.EnsureCapacity(oPaths.Count);
        CollectionsMarshal.SetCount(openPaths, oPaths.Count);
        var source = CollectionsMarshal.AsSpan(oPaths);
        var destination = CollectionsMarshal.AsSpan(openPaths);
        var len = source.Length;
        for (var i = 0; i < len; ++i)
        {
            destination[i] = Clipper.ScalePathD(source[i], _invScale);
        }
        oPaths.Clear();

        return true;
    }

    public bool Execute(ClipType clipType, FillRule fillRule, PolyTreeD polytree)
    {
        var succeeded = Execute(clipType, fillRule, polytree, _discardedOpenPaths);
        _discardedOpenPaths.Clear();
        return succeeded;
    }
}

[SkipLocalsInit]
public abstract class PolyPathBase(PolyPathBase? parent = null) : IEnumerable
{
    internal PolyPathBase? _parent = parent;
    internal List<PolyPathBase> _childs = [];

    public IEnumerator GetEnumerator()
    {
        return new NodeEnumerator(_childs);
    }
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    private class NodeEnumerator(List<PolyPathBase> nodes) : IEnumerator
    {
        private int position = -1;
        private readonly List<PolyPathBase> _nodes = nodes;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            ++position;
            return position < _nodes.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            position = -1;
        }

        public object Current
        {
            get
            {
                if (position < 0 || position >= _nodes.Count)
                {
                    throw new InvalidOperationException();
                }
                return _nodes[position];
            }
        }
    }

    public bool IsHole => GetIsHole();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetLevel()
    {
        var result = 0;
        var pp = _parent;
        while (pp != null)
        {
            ++result;
            pp = pp._parent;
        }
        return result;
    }

    public int Level => GetLevel();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool GetIsHole()
    {
        var lvl = GetLevel();
        return lvl != 0 && (lvl & 1) == 0;
    }

    public int Count => _childs.Count;
    public abstract PolyPathBase AddChild(Path64 p);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _childs.Clear();
    }

    internal string ToStringInternal(int idx, int level)
    {
        string result = "", padding = "", plural = "s";
        if (_childs.Count == 1)
        {
            plural = "";
        }
        padding = padding.PadLeft(level * 2);
        if ((level & 1) == 0)
        {
            result += $"{padding}+- hole ({idx}) contains {_childs.Count} nested polygon{plural}.\n";
        }
        else
        {
            result += $"{padding}+- polygon ({idx}) contains {_childs.Count} hole{plural}.\n";
        }

        for (var i = 0; i < Count; ++i)
        {
            if (_childs[i].Count > 0)
            {
                result += _childs[i].ToStringInternal(i, level + 1);
            }
        }
        return result;
    }

    public override string ToString()
    {
        if (Level > 0)
        {
            return ""; //only accept tree root
        }
        var plural = "s";
        if (_childs.Count == 1)
        {
            plural = "";
        }
        var result = $"Polytree with {_childs.Count} polygon{plural}.\n";
        for (var i = 0; i < Count; i++)
        {
            if (_childs[i].Count > 0)
            {
                result += _childs[i].ToStringInternal(i, 1);
            }
        }
        return result + '\n';
    }
}

[SkipLocalsInit]
public class PolyPath64(PolyPathBase? parent = null) : PolyPathBase(parent)
{
    public Path64? Polygon;// polytree root's polygon == null

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override PolyPathBase AddChild(Path64 p)
    {
        PolyPathBase newChild = new PolyPath64(this);
        (newChild as PolyPath64)!.Polygon = p;
        _childs.Add(newChild);
        return newChild;
    }

    public PolyPath64 this[int index]
    {
        get
        {
            if (index < 0 || index >= _childs.Count)
            {
                throw new InvalidOperationException();
            }
            return (PolyPath64)_childs[index];
        }
    }

    public PolyPath64 Child(int index)
    {
        if (index < 0 || index >= _childs.Count)
        {
            throw new InvalidOperationException();
        }
        return (PolyPath64)_childs[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Area()
    {
        var result = Polygon == null ? 0d : Clipper.Area(Polygon);
        var count = _childs.Count;
        for (var i = 0; i < count; ++i)
        {
            var polyPathBase = _childs[i];
            var child = (PolyPath64)polyPathBase;
            result += child.Area();
        }
        return result;
    }
}

[SkipLocalsInit]
public class PolyPathD(PolyPathBase? parent = null) : PolyPathBase(parent)
{
    internal double Scale;
    public PathD? Polygon;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override PolyPathBase AddChild(Path64 p)
    {
        PolyPathBase newChild = new PolyPathD(this);
        (newChild as PolyPathD)!.Scale = Scale;
        (newChild as PolyPathD)!.Polygon = Clipper.ScalePathD(p, 1 / Scale);
        _childs.Add(newChild);
        return newChild;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PolyPathBase AddChild(PathD p)
    {
        PolyPathBase newChild = new PolyPathD(this);
        (newChild as PolyPathD)!.Scale = Scale;
        (newChild as PolyPathD)!.Polygon = p;
        _childs.Add(newChild);
        return newChild;
    }

    [IndexerName("Child")]
    public PolyPathD this[int index]
    {
        get
        {
            if (index < 0 || index >= _childs.Count)
            {
                throw new InvalidOperationException();
            }
            return (PolyPathD)_childs[index];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Area()
    {
        var result = Polygon == null ? 0 : Clipper.Area(Polygon);
        var count = _childs.Count;
        for (var i = 0; i < count; ++i)
        {
            var polyPathBase = _childs[i];
            var child = (PolyPathD)polyPathBase;
            result += child.Area();
        }
        return result;
    }
}

[SkipLocalsInit]
public sealed class PolyTree64 : PolyPath64 { }

[SkipLocalsInit]
public sealed class PolyTreeD : PolyPathD
{
    public new double Scale => base.Scale;
}

[SkipLocalsInit]
public sealed class ClipperLibException(string description) : Exception(description) { }
