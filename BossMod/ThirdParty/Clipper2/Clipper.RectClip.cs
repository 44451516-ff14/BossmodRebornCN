/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Date      :  10 October 2024                                                 *
* Website   :  https://www.angusj.com                                          *
* Copyright :  Angus Johnson 2010-2024                                         *
* Purpose   :  FAST rectangular clipping                                       *
* License   :  https://www.boost.org/LICENSE_1_0.txt                           *
*******************************************************************************/

namespace Clipper2Lib;

[SkipLocalsInit]
public sealed class OutPt2(Point64 pt)
{
    public OutPt2? next;
    public OutPt2? prev;

    public Point64 pt = pt;
    public int ownerIdx;
    public List<OutPt2?>? edge;
}

[SkipLocalsInit]
public class RectClip64
{
    protected enum Location
    {
        left, top, right, bottom, inside
    }

    protected readonly Rect64 rect_;
    protected readonly Point64 mp_;
    protected readonly Path64 rectPath_;
    protected Rect64 pathBounds_;
    protected List<OutPt2?> results_;
    protected List<OutPt2?>[] edges_;
    protected int currIdx_;
    private readonly OutPt2PoolList _outPtPool;
    private List<Location>? _startLocs;
    internal RectClip64(Rect64 rect)
    {
        currIdx_ = -1;
        rect_ = rect;
        mp_ = rect.MidPoint();
        rectPath_ = rect_.AsPath();
        results_ = [];
        edges_ = new List<OutPt2?>[8];
        _outPtPool = new OutPt2PoolList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<OutPt2?> GetEdge(int index)
    {
        return edges_[index] ??= [];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void ResetWorkingStorage()
    {
        results_.Clear();
        var len = edges_.Length;
        for (var i = 0; i < len; ++i)
        {
            edges_[i]?.Clear();
        }
        _outPtPool.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool HasBounds(Rect64 rect)
    {
        return rect_.left == rect.left && rect_.top == rect.top &&
          rect_.right == rect.right && rect_.bottom == rect.bottom;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void Reset()
    {
        ResetWorkingStorage();
        _startLocs?.Clear();
    }

    internal OutPt2 Add(Point64 pt, bool startingNewPath = false)
    {  // this method is only called by InternalExecute.
       // Later splitting and rejoining won't create additional op's,
       // though they will change the (non-storage) fResults count.
        var currIdx = results_.Count;
        OutPt2 result;
        if (currIdx == 0 || startingNewPath)
        {
            result = _outPtPool.Add(pt);
            results_.Add(result);
            result.ownerIdx = currIdx;
            result.prev = result;
            result.next = result;
        }
        else
        {
            --currIdx;
            var prevOp = results_[currIdx]!;
            if (prevOp.pt == pt)
            {
                return prevOp;
            }
            result = _outPtPool.Add(pt);
            result.ownerIdx = currIdx;
            result.next = prevOp.next;
            prevOp.next!.prev = result;
            prevOp.next = result;
            result.prev = prevOp;
            results_[currIdx] = result;
        }
        return result;
    }

    private static bool Path1ContainsPath2(Path64 path1, Path64 path2)
    {
        // nb: occasionally, due to rounding, path1 may 
        // appear (momentarily) inside or outside path2.
        var ioCount = 0;
        var points = CollectionsMarshal.AsSpan(path2);
        var lenP = points.Length;
        for (var i = 0; i < lenP; ++i)
        {
            var pip =
              InternalClipper.PointInPolygon(points[i], path1);
            switch (pip)
            {
                case PointInPolygonResult.IsInside:
                    --ioCount;
                    break;
                case PointInPolygonResult.IsOutside:
                    ++ioCount;
                    break;
            }
            if (Math.Abs(ioCount) > 1)
            {
                break;
            }
        }
        return ioCount <= 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsClockwise(Location prev, Location curr,
      Point64 prevPt, Point64 currPt, Point64 rectMidPoint)
    {
        if (AreOpposites(prev, curr))
        {
            return InternalClipper.CrossProduct(prevPt, rectMidPoint, currPt) < 0d;
        }
        return HeadingClockwise(prev, curr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AreOpposites(Location prev, Location curr)
    {
        return Math.Abs((int)prev - (int)curr) == 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HeadingClockwise(Location prev, Location curr)
    {
        return (((int)prev + 1) & 3) == (int)curr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Location GetAdjacentLocation(Location loc, bool isClockwise)
    {
        var delta = isClockwise ? 1 : 3;
        return (Location)(((int)loc + delta) & 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OutPt2? UnlinkOp(OutPt2 op)
    {
        if (op.next == op)
        {
            return null;
        }
        op.prev!.next = op.next;
        op.next!.prev = op.prev;
        return op.next;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static OutPt2? UnlinkOpBack(OutPt2 op)
    {
        if (op.next == op)
        {
            return null;
        }
        op.prev!.next = op.next;
        op.next!.prev = op.prev;
        return op.prev;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint GetEdgesForPt(Point64 pt, Rect64 rec)
    {
        var result = 0u;
        var px = pt.X;
        var py = pt.Y;
        if (px == rec.left)
        {
            result = 1u;
        }
        else if (px == rec.right)
        {
            result = 4u;
        }
        if (py == rec.top)
        {
            result += 2u;
        }
        else if (py == rec.bottom)
        {
            result += 8u;
        }
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHeadingClockwise(Point64 pt1, Point64 pt2, int edgeIdx)
    {
        return edgeIdx switch
        {
            0 => pt2.Y < pt1.Y,
            1 => pt2.X > pt1.X,
            2 => pt2.Y > pt1.Y,
            _ => pt2.X < pt1.X
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasHorzOverlap(Point64 left1, Point64 right1,
      Point64 left2, Point64 right2)
    {
        return left1.X < right2.X && right1.X > left2.X;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasVertOverlap(Point64 top1, Point64 bottom1,
      Point64 top2, Point64 bottom2)
    {
        return top1.Y < bottom2.Y && bottom1.Y > top2.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddToEdge(List<OutPt2?> edge, OutPt2 op)
    {
        if (op.edge != null)
        {
            return;
        }
        op.edge = edge;
        edge.Add(op);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UncoupleEdge(OutPt2 op)
    {
        var edge = op.edge;
        if (edge == null)
        {
            return;
        }
        var count = edge.Count;
        for (var i = 0; i < count; ++i)
        {
            var op2 = edge[i];
            if (op2 != op)
            {
                continue;
            }
            edge[i] = null;
            break;
        }
        op.edge = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetNewOwner(OutPt2 op, int newIdx)
    {
        op.ownerIdx = newIdx;
        var op2 = op.next!;
        while (op2 != op)
        {
            op2.ownerIdx = newIdx;
            op2 = op2.next!;
        }
    }

    private void AddCorner(Location prev, Location curr)
    {
        Add(HeadingClockwise(prev, curr) ? rectPath_[(int)prev] : rectPath_[(int)curr]);
    }

    private void AddCorner(ref Location loc, bool isClockwise)
    {
        if (isClockwise)
        {
            Add(rectPath_[(int)loc]);
            loc = GetAdjacentLocation(loc, true);
        }
        else
        {
            loc = GetAdjacentLocation(loc, false);
            Add(rectPath_[(int)loc]);
        }
    }

    protected static bool GetLocation(Rect64 rec, Point64 pt, out Location loc)
    {
        var ptX = pt.X;
        var ptY = pt.Y;
        var top = rec.top;
        var bottom = rec.bottom;
        var left = rec.left;
        var right = rec.right;
        if (ptX == left && ptY >= top && ptY <= bottom)
        {
            loc = Location.left;
            return false; // pt on rec
        }
        if (ptX == right && ptY >= top && ptY <= bottom)
        {
            loc = Location.right;
            return false; // pt on rec
        }
        if (ptY == top && ptX >= left && ptX <= right)
        {
            loc = Location.top;
            return false; // pt on rec
        }
        if (ptY == bottom && ptX >= left && ptX <= right)
        {
            loc = Location.bottom;
            return false; // pt on rec
        }
        if (ptX < left)
        {
            loc = Location.left;
        }
        else if (ptX > right)
        {
            loc = Location.right;
        }
        else if (ptY < top)
        {
            loc = Location.top;
        }
        else if (ptY > bottom)
        {
            loc = Location.bottom;
        }
        else
        {
            loc = Location.inside;
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsHorizontal(Point64 pt1, Point64 pt2)
    {
        return pt1.Y == pt2.Y;
    }

    private static bool GetSegmentIntersection(Point64 p1,
    Point64 p2, Point64 p3, Point64 p4, out Point64 ip)
    {
        var res1 = InternalClipper.CrossProduct(p1, p3, p4);
        var res2 = InternalClipper.CrossProduct(p2, p3, p4);
        if (res1 == 0d)
        {
            ip = p1;
            if (res2 == 0d)
            {
                return false; // segments are collinear
            }
            if (p1 == p3 || p1 == p4)
            {
                return true;
            }
            //else if (p2 == p3 || p2 == p4) { ip = p2; return true; }
            if (IsHorizontal(p3, p4))
            {
                return (p1.X > p3.X) == (p1.X < p4.X);
            }
            return (p1.Y > p3.Y) == (p1.Y < p4.Y);
        }
        if (res2 == 0d)
        {
            ip = p2;
            if (p2 == p3 || p2 == p4)
            {
                return true;
            }
            if (IsHorizontal(p3, p4))
            {
                return (p2.X > p3.X) == (p2.X < p4.X);
            }
            return (p2.Y > p3.Y) == (p2.Y < p4.Y);
        }

        if ((res1 > 0d) == (res2 > 0d))
        {
            ip = new Point64(0L, 0L);
            return false;
        }

        var res3 = InternalClipper.CrossProduct(p3, p1, p2);
        var res4 = InternalClipper.CrossProduct(p4, p1, p2);
        if (res3 == 0d)
        {
            ip = p3;
            if (p3 == p1 || p3 == p2)
            {
                return true;
            }
            if (IsHorizontal(p1, p2))
            {
                return (p3.X > p1.X) == (p3.X < p2.X);
            }
            return (p3.Y > p1.Y) == (p3.Y < p2.Y);
        }
        if (res4 == 0d)
        {
            ip = p4;
            if (p4 == p1 || p4 == p2)
            {
                return true;
            }
            if (IsHorizontal(p1, p2))
            {
                return (p4.X > p1.X) == (p4.X < p2.X);
            }
            return (p4.Y > p1.Y) == (p4.Y < p2.Y);
        }
        if ((res3 > 0d) == (res4 > 0d))
        {
            ip = new Point64(0L, 0L);
            return false;
        }

        // segments must intersect to get here
        return InternalClipper.GetSegmentIntersectPt(p1, p2, p3, p4, out ip);
    }

    protected static bool GetIntersection(Path64 rectPath, Point64 p, Point64 p2, ref Location loc, out Point64 ip)
    {
        // gets the pt of intersection between rectPath and segment(p, p2) that's closest to 'p'
        // when result == false, loc will remain unchanged
        ip = new Point64();
        switch (loc)
        {
            case Location.left:
                if (GetSegmentIntersection(p, p2, rectPath[0], rectPath[3], out ip))
                {
                    return true;
                }
                if (p.Y < rectPath[0].Y && GetSegmentIntersection(p, p2, rectPath[0], rectPath[1], out ip))
                {
                    loc = Location.top;
                    return true;
                }

                if (!GetSegmentIntersection(p, p2, rectPath[2], rectPath[3], out ip))
                    return false;
                loc = Location.bottom;
                return true;

            case Location.right:
                if (GetSegmentIntersection(p, p2, rectPath[1], rectPath[2], out ip))
                    return true;
                if (p.Y < rectPath[0].Y && GetSegmentIntersection(p, p2, rectPath[0], rectPath[1], out ip))
                {
                    loc = Location.top;
                    return true;
                }

                if (!GetSegmentIntersection(p, p2, rectPath[2], rectPath[3], out ip))
                    return false;
                loc = Location.bottom;
                return true;

            case Location.top:
                if (GetSegmentIntersection(p, p2, rectPath[0], rectPath[1], out ip))
                    return true;
                if (p.X < rectPath[0].X && GetSegmentIntersection(p, p2, rectPath[0], rectPath[3], out ip))
                {
                    loc = Location.left;
                    return true;
                }

                if (p.X <= rectPath[1].X || !GetSegmentIntersection(p, p2, rectPath[1], rectPath[2], out ip))
                    return false;
                loc = Location.right;
                return true;

            case Location.bottom:
                if (GetSegmentIntersection(p, p2, rectPath[2], rectPath[3], out ip))
                    return true;
                if (p.X < rectPath[3].X && GetSegmentIntersection(p, p2, rectPath[0], rectPath[3], out ip))
                {
                    loc = Location.left;
                    return true;
                }

                if (p.X <= rectPath[2].X || !GetSegmentIntersection(p, p2, rectPath[1], rectPath[2], out ip))
                    return false;
                loc = Location.right;
                return true;

            default:
                if (GetSegmentIntersection(p, p2, rectPath[0], rectPath[3], out ip))
                {
                    loc = Location.left;
                    return true;
                }
                if (GetSegmentIntersection(p, p2, rectPath[0], rectPath[1], out ip))
                {
                    loc = Location.top;
                    return true;
                }
                if (GetSegmentIntersection(p, p2, rectPath[1], rectPath[2], out ip))
                {
                    loc = Location.right;
                    return true;
                }

                if (!GetSegmentIntersection(p, p2, rectPath[2], rectPath[3], out ip))
                    return false;
                loc = Location.bottom;
                return true;
        }
    }

    protected void GetNextLocation(Path64 path,
      ref Location loc, ref int i, int highI)
    {
        switch (loc)
        {
            case Location.left:
                {
                    while (i <= highI && path[i].X <= rect_.left)
                        ++i;
                    if (i > highI)
                        break;
                    if (path[i].X >= rect_.right)
                        loc = Location.right;
                    else if (path[i].Y <= rect_.top)
                        loc = Location.top;
                    else if (path[i].Y >= rect_.bottom)
                        loc = Location.bottom;
                    else
                        loc = Location.inside;
                }
                break;

            case Location.top:
                {
                    while (i <= highI && path[i].Y <= rect_.top)
                        ++i;
                    if (i > highI)
                        break;
                    if (path[i].Y >= rect_.bottom)
                        loc = Location.bottom;
                    else if (path[i].X <= rect_.left)
                        loc = Location.left;
                    else if (path[i].X >= rect_.right)
                        loc = Location.right;
                    else
                        loc = Location.inside;
                }
                break;

            case Location.right:
                {
                    while (i <= highI && path[i].X >= rect_.right)
                        ++i;
                    if (i > highI)
                        break;
                    if (path[i].X <= rect_.left)
                        loc = Location.left;
                    else if (path[i].Y <= rect_.top)
                        loc = Location.top;
                    else if (path[i].Y >= rect_.bottom)
                        loc = Location.bottom;
                    else
                        loc = Location.inside;
                }
                break;

            case Location.bottom:
                {
                    while (i <= highI && path[i].Y >= rect_.bottom)
                        ++i;
                    if (i > highI)
                        break;
                    if (path[i].Y <= rect_.top)
                        loc = Location.top;
                    else if (path[i].X <= rect_.left)
                        loc = Location.left;
                    else if (path[i].X >= rect_.right)
                        loc = Location.right;
                    else
                        loc = Location.inside;
                }
                break;

            case Location.inside:
                {
                    while (i <= highI)
                    {
                        if (path[i].X < rect_.left)
                            loc = Location.left;
                        else if (path[i].X > rect_.right)
                            loc = Location.right;
                        else if (path[i].Y > rect_.bottom)
                            loc = Location.bottom;
                        else if (path[i].Y < rect_.top)
                            loc = Location.top;
                        else
                        {
                            Add(path[i]);
                            ++i;
                            continue;
                        }
                        break;
                    }
                }
                break;
        } // switch
    }

    private static bool StartLocsAreClockwise(List<Location> startLocs)
    {
        var result = 0;
        for (var i = 1; i < startLocs.Count; ++i)
        {
            var d = (int)startLocs[i] - (int)startLocs[i - 1];
            switch (d)
            {
                case -1:
                    result -= 1;
                    break;
                case 1:
                    result += 1;
                    break;
                case -3:
                    result += 1;
                    break;
                case 3:
                    result -= 1;
                    break;
            }
        }
        return result > 0;
    }

    private void ExecuteInternal(Path64 path)
    {
        if (path.Count < 3 || rect_.IsEmpty())
        {
            return;
        }
        var startLocs = _startLocs ??= [with(4)];
        startLocs.Clear();

        var firstCross = Location.inside;
        Location crossingLoc = firstCross, prev = firstCross;

        int i, highI = path.Count - 1;
        if (!GetLocation(rect_, path[highI], out var loc))
        {
            i = highI - 1;
            while (i >= 0 && !GetLocation(rect_, path[i], out prev))
            {
                --i;
            }
            if (i < 0)
            {
                var points = CollectionsMarshal.AsSpan(path);
                var len = points.Length;
                for (var j = 0; j < len; ++j)
                {
                    Add(points[j]);
                }
                return;
            }
            if (prev == Location.inside)
            {
                loc = Location.inside;
            }
        }
        var startingLoc = loc;

        ///////////////////////////////////////////////////
        i = 0;
        while (i <= highI)
        {
            prev = loc;
            var prevCrossLoc = crossingLoc;
            GetNextLocation(path, ref loc, ref i, highI);
            if (i > highI)
            {
                break;
            }

            var prevPt = (i == 0) ? path[highI] : path[i - 1];
            crossingLoc = loc;
            if (!GetIntersection(rectPath_,
              path[i], prevPt, ref crossingLoc, out var ip))
            {
                // ie remaining outside
                if (prevCrossLoc == Location.inside)
                {
                    var isClockw = IsClockwise(prev, loc, prevPt, path[i], mp_);
                    do
                    {
                        startLocs.Add(prev);
                        prev = GetAdjacentLocation(prev, isClockw);
                    } while (prev != loc);
                    crossingLoc = prevCrossLoc; // still not crossed 
                }

                else if (prev != Location.inside && prev != loc)
                {
                    var isClockw = IsClockwise(prev, loc, prevPt, path[i], mp_);
                    do
                    {
                        AddCorner(ref prev, isClockw);
                    } while (prev != loc);
                }
                ++i;
                continue;
            }

            ////////////////////////////////////////////////////
            // we must be crossing the rect boundary to get here
            ////////////////////////////////////////////////////

            if (loc == Location.inside) // path must be entering rect
            {
                if (firstCross == Location.inside)
                {
                    firstCross = crossingLoc;
                    startLocs.Add(prev);
                }
                else if (prev != crossingLoc)
                {
                    var isClockw = IsClockwise(prev, crossingLoc, prevPt, path[i], mp_);
                    do
                    {
                        AddCorner(ref prev, isClockw);
                    } while (prev != crossingLoc);
                }
            }
            else if (prev != Location.inside)
            {
                // passing right through rect. 'ip' here will be the second 
                // intersect pt but we'll also need the first intersect pt (ip2)
                loc = prev;
                GetIntersection(rectPath_,
                  prevPt, path[i], ref loc, out var ip2);
                if (prevCrossLoc != Location.inside && prevCrossLoc != loc) //#597
                    AddCorner(prevCrossLoc, loc);

                if (firstCross == Location.inside)
                {
                    firstCross = loc;
                    startLocs.Add(prev);
                }

                loc = crossingLoc;
                Add(ip2);
                if (ip == ip2)
                {
                    // it's very likely that path[i] is on rect
                    GetLocation(rect_, path[i], out loc);
                    AddCorner(crossingLoc, loc);
                    crossingLoc = loc;
                    continue;
                }
            }
            else // path must be exiting rect
            {
                loc = crossingLoc;
                if (firstCross == Location.inside)
                    firstCross = crossingLoc;
            }

            Add(ip);
        } //while i <= highI
          ///////////////////////////////////////////////////

        if (firstCross == Location.inside)
        {
            // path never intersects
            if (startingLoc == Location.inside)
                return;
            if (!pathBounds_.Contains(rect_) ||
                !Path1ContainsPath2(path, rectPath_))
                return;
            var startLocsClockwise = StartLocsAreClockwise(startLocs);
            for (var j = 0; j < 4; ++j)
            {
                var k = startLocsClockwise ? j : 3 - j; // ie reverse result path
                Add(rectPath_[k]);
                AddToEdge(GetEdge(k * 2), results_[0]!);
            }
        }
        else if (loc != Location.inside && (loc != firstCross || startLocs.Count > 2))
        {
            var count = startLocs.Count;
            if (count > 0)
            {
                prev = loc;
                for (var startLocIndex = 0; startLocIndex < count; ++startLocIndex)
                {
                    var loc2 = startLocs[startLocIndex];
                    if (prev == loc2)
                    {
                        continue;
                    }
                    AddCorner(ref prev, HeadingClockwise(prev, loc2));
                    prev = loc2;
                }
                loc = prev;
            }
            if (loc != firstCross)
            {
                AddCorner(ref loc, HeadingClockwise(loc, firstCross));
            }
        }
    }

    private void ExecutePath(Path64 path, Paths64 result)
    {
        if (path.Count < 3)
        {
            return;
        }
        pathBounds_ = Clipper.GetBounds(path);
        if (!rect_.Intersects(pathBounds_))
        {
            return; // the path must be completely outside fRect
        }
        if (rect_.Contains(pathBounds_))
        {
            // the path must be completely inside rect_
            result.Add(path);
            return;
        }

        ResetWorkingStorage();
        ExecuteInternal(path);
        CheckEdges();
        for (var i = 0; i < 4; ++i)
        {
            var cw = edges_[i * 2];
            var ccw = edges_[i * 2 + 1];
            if (cw != null && ccw != null)
            {
                TidyEdgePair(i, cw, ccw);
            }
        }
        var count = results_.Count;
        for (var i = 0; i < count; ++i)
        {
            var tmp = GetPath(results_[i]);
            if (tmp.Count > 0)
            {
                result.Add(tmp);
            }
        }
        ResetWorkingStorage();
    }

    public Paths64 Execute(Paths64 paths)
    {
        Paths64 result = [with(paths.Count)];
        if (rect_.IsEmpty())
        {
            return result;
        }
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            ExecutePath(pathSpan[i], result);
        }
        return result;
    }

    internal Paths64 Execute(Path64 path)
    {
        Paths64 result = [with(1)];
        if (!rect_.IsEmpty())
        {
            ExecutePath(path, result);
        }
        return result;
    }

    private void CheckEdges()
    {
        var count = results_.Count;
        for (var i = 0; i < count; ++i)
        {
            var op = results_[i];
            if (op == null)
            {
                continue;
            }
            var op2 = op;
            do
            {
                var current = op2!;
                if (InternalClipper.IsCollinear(current.prev!.pt, current.pt, current.next!.pt))
                {
                    if (current == op)
                    {
                        op2 = UnlinkOpBack(current);
                        if (op2 == null)
                        {
                            break;
                        }
                        op = op2.prev!;
                    }
                    else
                    {
                        op2 = UnlinkOpBack(current);
                        if (op2 == null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    op2 = current.next;
                }
            } while (op2 != op);

            if (op2 == null)
            {
                results_[i] = null;
                continue;
            }
            results_[i] = op2; // safety first

            var edgeSet1 = GetEdgesForPt(op.prev!.pt, rect_);
            var edgeCursor = op;
            do
            {
                var edgeSet2 = GetEdgesForPt(edgeCursor.pt, rect_);
                if (edgeSet2 != 0 && edgeCursor.edge == null)
                {
                    var combinedSet = edgeSet1 & edgeSet2;
                    for (var j = 0; j < 4; ++j)
                    {
                        if ((combinedSet & (1 << j)) == 0)
                        {
                            continue;
                        }
                        if (IsHeadingClockwise(edgeCursor.prev!.pt, edgeCursor.pt, j))
                        {
                            AddToEdge(GetEdge(j * 2), edgeCursor);
                        }
                        else
                        {
                            AddToEdge(GetEdge(j * 2 + 1), edgeCursor);
                        }
                    }
                }
                edgeSet1 = edgeSet2;
                edgeCursor = edgeCursor.next!;
            } while (edgeCursor != op);
        }
    }

    private void TidyEdgePair(int idx, List<OutPt2?> cw, List<OutPt2?> ccw)
    {
        if (ccw.Count == 0)
        {
            return;
        }
        var isHorz = idx is 1 or 3;
        var cwIsTowardLarger = idx is 1 or 2;
        int i = 0, j = 0;
        var cwcount = cw.Count;
        while (i < cwcount)
        {
            var cwCandidate = cw[i];
            if (cwCandidate == null || cwCandidate.next == cwCandidate.prev)
            {
                cw[i++] = null;
                j = 0;
                continue;
            }

            var jLim = ccw.Count;
            while (j < jLim)
            {
                var candidate = ccw[j];
                if (candidate != null && candidate.next != candidate.prev)
                {
                    break;
                }
                ++j;
            }

            if (j == jLim)
            {
                ++i;
                j = 0;
                continue;
            }

            var ccwCandidate = ccw[j]!;
            OutPt2 p1;
            OutPt2 p2;
            OutPt2 p1a;
            OutPt2 p2a;
            if (cwIsTowardLarger)
            {
                // p1 >>>> p1a;
                // p2 <<<< p2a;
                p1 = cwCandidate.prev!;
                p1a = cwCandidate;
                p2 = ccwCandidate;
                p2a = ccwCandidate.prev!;
            }
            else
            {
                // p1 <<<< p1a;
                // p2 >>>> p2a;
                p1 = cwCandidate;
                p1a = cwCandidate.prev!;
                p2 = ccwCandidate.prev!;
                p2a = ccwCandidate;
            }

            if (isHorz && !HasHorzOverlap(p1.pt, p1a.pt, p2.pt, p2a.pt) || !isHorz && !HasVertOverlap(p1.pt, p1a.pt, p2.pt, p2a.pt))
            {
                ++j;
                continue;
            }

            // to get here we're either splitting or rejoining
            var isRejoining = cwCandidate.ownerIdx != ccwCandidate.ownerIdx;

            if (isRejoining)
            {
                results_[p2.ownerIdx] = null;
                SetNewOwner(p2, p1.ownerIdx);
            }

            // do the split or re-join
            if (cwIsTowardLarger)
            {
                // p1 >> | >> p1a;
                // p2 << | << p2a;
                p1.next = p2;
                p2.prev = p1;
                p1a.prev = p2a;
                p2a.next = p1a;
            }
            else
            {
                // p1 << | << p1a;
                // p2 >> | >> p2a;
                p1.prev = p2;
                p2.next = p1;
                p1a.next = p2a;
                p2a.prev = p1a;
            }

            if (!isRejoining)
            {
                var new_idx = results_.Count;
                results_.Add(p1a);
                SetNewOwner(p1a, new_idx);
            }

            OutPt2 op;
            OutPt2 op2;
            if (cwIsTowardLarger)
            {
                op = p2;
                op2 = p1a;
            }
            else
            {
                op = p1;
                op2 = p2a;
            }
            results_[op.ownerIdx] = op;
            results_[op2.ownerIdx] = op2;

            // and now lots of work to get ready for the next loop

            bool opIsLarger, op2IsLarger;
            if (isHorz) // X
            {
                opIsLarger = op.pt.X > op.prev!.pt.X;
                op2IsLarger = op2.pt.X > op2.prev!.pt.X;
            }
            else       // Y
            {
                opIsLarger = op.pt.Y > op.prev!.pt.Y;
                op2IsLarger = op2.pt.Y > op2.prev!.pt.Y;
            }

            if (op.next == op.prev ||
              op.pt == op.prev!.pt)
            {
                if (op2IsLarger == cwIsTowardLarger)
                {
                    cw[i] = op2;
                    ccw[j++] = null;
                }
                else
                {
                    ccw[j] = op2;
                    cw[i++] = null;
                }
            }
            else if (op2.next == op2.prev ||
              op2.pt == op2.prev!.pt)
            {
                if (opIsLarger == cwIsTowardLarger)
                {
                    cw[i] = op;
                    ccw[j++] = null;
                }
                else
                {
                    ccw[j] = op;
                    cw[i++] = null;
                }
            }
            else if (opIsLarger == op2IsLarger)
            {
                if (opIsLarger == cwIsTowardLarger)
                {
                    cw[i] = op;
                    UncoupleEdge(op2);
                    AddToEdge(cw, op2);
                    ccw[j++] = null;
                }
                else
                {
                    cw[i++] = null;
                    ccw[j] = op2;
                    UncoupleEdge(op);
                    AddToEdge(ccw, op);
                    j = 0;
                }
            }
            else
            {
                if (opIsLarger == cwIsTowardLarger)
                {
                    cw[i] = op;
                }
                else
                {
                    ccw[j] = op;
                }
                if (op2IsLarger == cwIsTowardLarger)
                {
                    cw[i] = op2;
                }
                else
                {
                    ccw[j] = op2;
                }
            }
        }
    }

    private static Path64 GetPath(OutPt2? op)
    {
        if (op == null || op.prev == op.next)
        {
            return [];
        }
        var first = op;
        var cursor = first.next;
        while (cursor != null && cursor != first)
        {
            if (InternalClipper.IsCollinear(cursor.prev!.pt, cursor.pt, cursor.next!.pt))
            {
                first = cursor.prev!;
                cursor = UnlinkOp(cursor);
            }
            else
            {
                cursor = cursor.next;
            }
        }
        if (cursor == null)
        {
            return [];
        }

        var count = 1;
        var pathCursor = first.next!;
        while (pathCursor != first)
        {
            ++count;
            pathCursor = pathCursor.next!;
        }

        Path64 result = [with(count)];
        CollectionsMarshal.SetCount(result, count);
        var points = CollectionsMarshal.AsSpan(result);
        points[0] = first.pt;
        pathCursor = first.next!;
        for (var i = 1; i < count; ++i)
        {
            points[i] = pathCursor.pt;
            pathCursor = pathCursor.next!;
        }
        return result;
    }
} // RectClip class

[SkipLocalsInit]
public sealed class RectClipLines64 : RectClip64
{
    internal RectClipLines64(Rect64 rect) : base(rect) { }

    private void ExecutePath(Path64 path, Paths64 result)
    {
        if (path.Count < 2)
        {
            return;
        }
        pathBounds_ = Clipper.GetBounds(path);
        if (!rect_.Intersects(pathBounds_))
        {
            return; // the path must be completely outside fRect
                    // Apart from that, we can't be sure whether the path
                    // is completely outside or completed inside or intersects
                    // fRect, simply by comparing path bounds with fRect.
        }
        ResetWorkingStorage();
        ExecuteInternal(path);

        var count = results_.Count;
        for (var i = 0; i < count; ++i)
        {
            var tmp = GetPath(results_[i]);
            if (tmp.Count > 0)
            {
                result.Add(tmp);
            }
        }
        ResetWorkingStorage();
    }

    public new Paths64 Execute(Paths64 paths)
    {
        Paths64 result = [with(paths.Count)];
        if (rect_.IsEmpty())
        {
            return result;
        }
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            ExecutePath(pathSpan[i], result);
        }
        return result;
    }

    internal new Paths64 Execute(Path64 path)
    {
        Paths64 result = [with(1)];
        if (!rect_.IsEmpty())
        {
            ExecutePath(path, result);
        }
        return result;
    }

    private static Path64 GetPath(OutPt2? op)
    {
        if (op == null || op == op.next)
        {
            return [];
        }
        var first = op.next!; // starting at path beginning
        var count = 1;
        var cursor = first.next!;
        while (cursor != first)
        {
            ++count;
            cursor = cursor.next!;
        }

        Path64 result = [with(count)];
        CollectionsMarshal.SetCount(result, count);
        var points = CollectionsMarshal.AsSpan(result);
        points[0] = first.pt;
        cursor = first.next!;
        for (var i = 1; i < count; ++i)
        {
            points[i] = cursor.pt;
            cursor = cursor.next!;
        }
        return result;
    }

    private void ExecuteInternal(Path64 path)
    {
        if (path.Count < 2 || rect_.IsEmpty())
            return;

        var prev = Location.inside;
        int i = 1, highI = path.Count - 1;
        if (!GetLocation(rect_, path[0], out var loc))
        {
            while (i <= highI && !GetLocation(rect_, path[i], out prev))
            {
                ++i;
            }
            if (i > highI)
            {
                var points = CollectionsMarshal.AsSpan(path);
                var len = points.Length;
                for (var j = 0; j < len; ++j)
                {
                    Add(points[j]);
                }
                return;
            }
            if (prev == Location.inside)
            {
                loc = Location.inside;
            }
            i = 1;
        }
        if (loc == Location.inside)
        {
            Add(path[0]);
        }

        ///////////////////////////////////////////////////
        while (i <= highI)
        {
            prev = loc;
            GetNextLocation(path, ref loc, ref i, highI);
            if (i > highI)
            {
                break;
            }
            var prevPt = path[i - 1];

            var crossingLoc = loc;
            if (!GetIntersection(rectPath_, path[i], prevPt, ref crossingLoc, out var ip))
            {
                // ie remaining outside (& crossingLoc still == loc)
                ++i;
                continue;
            }

            ////////////////////////////////////////////////////
            // we must be crossing the rect boundary to get here
            ////////////////////////////////////////////////////

            if (loc == Location.inside) // path must be entering rect
            {
                Add(ip, true);
            }
            else if (prev != Location.inside)
            {
                // passing right through rect. 'ip' here will be the second 
                // intersect pt but we'll also need the first intersect pt (ip2)
                crossingLoc = prev;
                GetIntersection(rectPath_, prevPt, path[i], ref crossingLoc, out var ip2);
                Add(ip2, true);
                Add(ip);
            }
            else // path must be exiting rect
            {
                Add(ip);
            }
        } //while i <= highI
          ///////////////////////////////////////////////////      
    } // RectClipLines.ExecuteInternal
}
