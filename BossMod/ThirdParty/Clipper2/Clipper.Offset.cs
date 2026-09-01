/*******************************************************************************
* Author    :  Angus Johnson                                                   *
* Date      :  4 May 2025                                                      *
* Website   :  https://www.angusj.com                                          *
* Copyright :  Angus Johnson 2010-2025                                         *
* Purpose   :  Path Offset (Inflate/Shrink)                                    *
* License   :  https://www.boost.org/LICENSE_1_0.txt                           *
*******************************************************************************/

namespace Clipper2Lib;

public enum JoinType
{
    Miter,
    Square,
    Bevel,
    Round
}

public enum EndType
{
    Polygon,
    Joined,
    Butt,
    Square,
    Round
}

[SkipLocalsInit]
public sealed class ClipperOffset
{
    private sealed class Group
    {
        internal Paths64 inPaths;
        internal JoinType joinType;
        internal EndType endType;
        internal bool pathsReversed;
        internal int lowestPathIdx;

        private Group(JoinType joinType, EndType endType)
        {
            this.joinType = joinType;
            this.endType = endType;
            inPaths = null!;
            lowestPathIdx = -1;
            pathsReversed = false;
        }

        public Group(Path64 path, JoinType joinType, EndType endType) : this(joinType, endType)
        {
            var isJoined = endType is EndType.Polygon or EndType.Joined;
            inPaths = [Clipper.StripDuplicates(path, isJoined)];
            SetPathsReversed();
        }

        public Group(Paths64 paths, JoinType joinType, EndType endType = EndType.Polygon) : this(joinType, endType)
        {
            var isJoined = endType is EndType.Polygon or EndType.Joined;
            var len = paths.Count;
            inPaths = [with(len)];
            var pathSpan = CollectionsMarshal.AsSpan(paths);
            for (var i = 0; i < len; ++i)
            {
                inPaths.Add(Clipper.StripDuplicates(pathSpan[i], isJoined));
            }

            SetPathsReversed();
        }

        private void SetPathsReversed()
        {
            if (endType == EndType.Polygon)
            {
                GetLowestPathInfo(inPaths, out lowestPathIdx, out var isNegArea);
                // the lowermost path must be an outer path, so if its orientation is negative,
                // then flag that the whole group is 'reversed' (will negate delta etc.)
                // as this is much more efficient than reversing every path.
                pathsReversed = (lowestPathIdx >= 0) && isNegArea;
            }
        }
    }

    private const double Tolerance = 1.0E-12;
    private const double DoublePI = 2 * Math.PI;
    private const double InvDoublePI = 1 / DoublePI;
    private const int RoundBulkWriteLimit = 1 << 16;

    // Clipper2 approximates arcs by using series of relatively short straight
    // line segments. And logically, shorter line segments will produce better arc
    // approximations. But very short segments can degrade performance, usually
    // with little or no discernable improvement in curve quality. Very short
    // segments can even detract from curve quality, due to the effects of integer
    // rounding. Since there isn't an optimal number of line segments for any given
    // arc radius (that perfectly balances curve approximation with performance),
    // arc tolerance is user defined. Nevertheless, when the user doesn't define
    // an arc tolerance (ie leaves alone the 0 default value), the calculated
    // default arc tolerance (offset_radius / 500) generally produces good (smooth)
    // arc approximations without producing excessively small segment lengths.
    // See also: https://www.angusj.com/clipper2/Docs/Trigonometry.htm
    private const double arc_const = 0.002; // <-- 1/500

    private readonly List<Group> _groupList = [];
    private Path64 pathOut = null!;
    private readonly PathD _normals = [];
    private readonly Paths64 _solution = [];
    private Paths64? _solutionPaths;
    private PolyTree64? _solutionTree;
    private Clipper64? _cleanupClipper;

    private double _groupDelta; //*0.5 for open paths; *-1.0 for negative areas
    private double _delta;
    private double _mitLimSqr;
    private double _stepsPerRad;
    private double _stepSin;
    private double _stepCos;
    private double _configuredRoundDelta = double.NaN;
    private double _configuredArcTolerance = double.NaN;
    private bool _configuredRoundNegative;
    private JoinType _joinType;
    private EndType _endType;
    public double ArcTolerance;
    public bool MergeGroups;
    public double MiterLimit;
    public bool PreserveCollinear;
    public bool ReverseSolution;

    public delegate double DeltaCallback64(Path64 path,
      PathD path_norms, int currPt, int prevPt);
    public DeltaCallback64? DeltaCallback;

#if USINGZ
internal void ZCB(Point64 bot1, Point64 top1, Point64 bot2, Point64 top2, ref Point64 ip)
{
  if (bot1.Z != 0 && ((bot1.Z == bot2.Z) || (bot1.Z == top2.Z))) 
	  {
		ip.Z = bot1.Z;
	  }
  else if (bot2.Z != 0 && bot2.Z == top1.Z)
	  {
		ip.Z = bot2.Z;
	  }
  else if (top1.Z != 0 && top1.Z == top2.Z)
	  {
		ip.Z = top1.Z;
	  }
  else
	  {
		ZCallback?.Invoke(bot1, top1, bot2, top2, ref ip);
	  }
}
public ClipperBase.ZCallback64? ZCallback;
#endif
    public ClipperOffset(double miterLimit = 2.0,
      double arcTolerance = 0.0, bool
      preserveCollinear = false, bool reverseSolution = false)
    {
        MiterLimit = miterLimit;
        ArcTolerance = arcTolerance;
        MergeGroups = true;
        PreserveCollinear = preserveCollinear;
        ReverseSolution = reverseSolution;
#if USINGZ
  ZCallback = null;
#endif
    }
    public void Clear()
    {
        _groupList.Clear();
        _solution.Clear();
        _solutionPaths = null;
        _solutionTree = null;
    }

    public void AddPath(Path64 path, JoinType joinType, EndType endType)
    {
        var cnt = path.Count;
        if (cnt == 0)
        {
            return;
        }
        _groupList.Add(new Group(path, joinType, endType));
    }

    public void AddPaths(Paths64 paths, JoinType joinType, EndType endType)
    {
        var cnt = paths.Count;
        if (cnt == 0)
        {
            return;
        }
        _groupList.Add(new Group(paths, joinType, endType));
    }

    private int CalcSolutionCapacity()
    {
        var result = 0;
        var groups = CollectionsMarshal.AsSpan(_groupList);
        var len = groups.Length;
        for (var i = 0; i < len; ++i)
        {
            var g = groups[i];
            result += (g.endType == EndType.Joined) ? g.inPaths.Count * 2 : g.inPaths.Count;
        }
        return result;
    }

    internal bool CheckPathsReversed()
    {
        var result = false;
        var groups = CollectionsMarshal.AsSpan(_groupList);
        var len = groups.Length;
        for (var i = 0; i < len; ++i)
        {
            var g = groups[i];
            if (g.endType == EndType.Polygon)
            {
                result = g.pathsReversed;
                break;
            }
        }
        return result;
    }

    private void ExecuteInternal(double delta)
    {
        if (_groupList.Count == 0)
            return;
        var solutionCapacity = CalcSolutionCapacity();

        // make sure the offset delta is significant
        if (Math.Abs(delta) < 0.5)
        {
            var target = _solutionTree == null ? _solutionPaths! : _solution;
            target.EnsureCapacity(solutionCapacity);
            var groups = CollectionsMarshal.AsSpan(_groupList);
            var len = groups.Length;
            for (var i = 0; i < len; ++i)
            {
                var paths = CollectionsMarshal.AsSpan(groups[i].inPaths);
                var lenP = paths.Length;
                for (var j = 0; j < lenP; ++j)
                {
                    target.Add(paths[j]);
                }
            }
            if (_solutionTree == null)
            {
                return;
            }
        }
        else
        {
            _solution.EnsureCapacity(solutionCapacity);
            _delta = delta;
            _mitLimSqr = (MiterLimit <= 1 ?
              2.0 : 2.0 / Clipper.Sqr(MiterLimit));

            var groupSpan = CollectionsMarshal.AsSpan(_groupList);
            var len = groupSpan.Length;
            for (var i = 0; i < len; ++i)
            {
                DoGroupOffset(groupSpan[i]);
            }
        }

        if (_groupList.Count == 0)
        {
            return;
        }

        var pathsReversed = CheckPathsReversed();
        var fillRule = pathsReversed ? FillRule.Negative : FillRule.Positive;

        // clean up self-intersections ...
        var c = _cleanupClipper ??= new Clipper64();
        c.Clear();
        c.PreserveCollinear = PreserveCollinear;
        c.ReverseSolution = ReverseSolution != pathsReversed;
#if USINGZ
  c.ZCallback = ZCB;
#endif
        try
        {
            c.AddSubject(_solution);
            _solution.Clear();
            if (_solutionTree != null)
            {
                c.Execute(ClipType.Union, fillRule, _solutionTree);
            }
            else
            {
                c.Execute(ClipType.Union, fillRule, _solutionPaths!);
            }
        }
        finally
        {
            _solution.Clear();
            c.Clear();
        }
    }

    public void Execute(double delta, Paths64 solution)
    {
        solution.Clear();
        _solution.Clear();
        _solutionPaths = solution;
        _solutionTree = null;
        try
        {
            ExecuteInternal(delta);
        }
        finally
        {
            _solutionPaths = null;
        }
    }

    public void Execute(double delta, PolyTree64 solutionTree)
    {
        solutionTree.Clear();
        _solutionPaths = null;
        _solutionTree = solutionTree;
        _solution.Clear();
        try
        {
            ExecuteInternal(delta);
        }
        finally
        {
            _solutionTree = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static PointD GetUnitNormal(Point64 pt1, Point64 pt2)
    {
        double dx = pt2.X - pt1.X;
        double dy = pt2.Y - pt1.Y;
        if ((dx == 0d) && (dy == 0d))
        {
            return new PointD();
        }

        var f = 1.0 / Math.Sqrt(dx * dx + dy * dy);
        dx *= f;
        dy *= f;

        return new PointD(dy, -dx);
    }

    public void Execute(DeltaCallback64 deltaCallback, Paths64 solution)
    {
        DeltaCallback = deltaCallback;
        Execute(1.0, solution);
    }

    internal static void GetLowestPathInfo(Paths64 paths, out int idx, out bool isNegArea)
    {
        idx = -1;
        isNegArea = false;
        var botPt = new Point64(long.MaxValue, long.MinValue);
        var pathSpan = CollectionsMarshal.AsSpan(paths);
        var len = pathSpan.Length;
        for (var i = 0; i < len; ++i)
        {
            var a = double.MaxValue;
            var points = CollectionsMarshal.AsSpan(pathSpan[i]);
            var lenP = points.Length;
            for (var j = 0; j < lenP; ++j)
            {
                ref var pt = ref points[j];
                var ptX = pt.X;
                var ptY = pt.Y;
                var botPtY = botPt.Y;
                if (ptY < botPtY || ptY == botPtY && ptX >= botPt.X)
                {
                    continue;
                }
                if (a == double.MaxValue)
                {
                    a = Clipper.Area(pathSpan[i]);
                    if (a == 0d)
                    {
                        break; // invalid closed path so break from inner loop
                    }
                    isNegArea = a < 0d;
                }
                idx = i;
                botPt.X = ptX;
                botPt.Y = ptY;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PointD TranslatePoint(PointD pt, double dx, double dy)
    {
#if USINGZ
        return new PointD(pt.x + dx, pt.y + dy, pt.z);
#else
        return new PointD(pt.x + dx, pt.y + dy);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PointD ReflectPoint(PointD pt, PointD pivot)
    {
#if USINGZ
        return new PointD(pivot.x + (pivot.x - pt.x), pivot.y + (pivot.y - pt.y), pt.z);
#else
        return new PointD(pivot.x + (pivot.x - pt.x), pivot.y + (pivot.y - pt.y));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool AlmostZero(double value, double epsilon = 0.001)
    {
        return Math.Abs(value) < epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Hypotenuse(double x, double y)
    {
        return Math.Sqrt(x * x + y * y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PointD NormalizeVector(PointD vec)
    {
        var h = Hypotenuse(vec.x, vec.y);
        if (AlmostZero(h))
        {
            return new PointD(0L, 0L);
        }
        var inverseHypot = 1d / h;
        return new PointD(vec.x * inverseHypot, vec.y * inverseHypot);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PointD GetAvgUnitVector(PointD vec1, PointD vec2)
    {
        return NormalizeVector(new PointD(vec1.x + vec2.x, vec1.y + vec2.y));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static PointD IntersectPoint(PointD pt1a, PointD pt1b, PointD pt2a, PointD pt2b)
    {
        if (InternalClipper.IsAlmostZero(pt1a.x - pt1b.x)) // vertical
        {
            if (InternalClipper.IsAlmostZero(pt2a.x - pt2b.x))
            {
                return new PointD(0L, 0L);
            }
            var m2 = (pt2b.y - pt2a.y) / (pt2b.x - pt2a.x);
            var b2 = pt2a.y - m2 * pt2a.x;
            return new PointD(pt1a.x, m2 * pt1a.x + b2);
        }

        if (InternalClipper.IsAlmostZero(pt2a.x - pt2b.x)) // vertical
        {
            var m1 = (pt1b.y - pt1a.y) / (pt1b.x - pt1a.x);
            var b1 = pt1a.y - m1 * pt1a.x;
            return new PointD(pt2a.x, m1 * pt2a.x + b1);
        }
        else
        {
            var m1 = (pt1b.y - pt1a.y) / (pt1b.x - pt1a.x);
            var b1 = pt1a.y - m1 * pt1a.x;
            var m2 = (pt2b.y - pt2a.y) / (pt2b.x - pt2a.x);
            var b2 = pt2a.y - m2 * pt2a.x;
            if (InternalClipper.IsAlmostZero(m1 - m2))
            {
                return new PointD(0L, 0L);
            }
            var x = (b2 - b1) / (m1 - m2);
            return new PointD(x, m1 * x + b1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Point64 GetPerpendic(Point64 pt, PointD norm)
    {
#if USINGZ
        return new Point64(pt.X + norm.x * _groupDelta, pt.Y + norm.y * _groupDelta, pt.Z);
#else
        return new Point64(pt.X + norm.x * _groupDelta, pt.Y + norm.y * _groupDelta);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private PointD GetPerpendicD(Point64 pt, PointD norm)
    {
#if USINGZ
        return new PointD(pt.X + norm.x * _groupDelta, pt.Y + norm.y * _groupDelta, pt.Z);
#else
        return new PointD(pt.X + norm.x * _groupDelta, pt.Y + norm.y * _groupDelta);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoBevel(Point64 pt, PointD normalJ, PointD normalK, bool isEnd)
    {
        Point64 pt1, pt2;
        var ptX = pt.X;
        var ptY = pt.Y;
        var normalJx = normalJ.x;
        var normalJy = normalJ.y;

        if (isEnd)
        {
            var absDelta = Math.Abs(_groupDelta);
            var absDeltaNormalJx = absDelta * normalJx;
            var absDeltaNormalJy = absDelta * normalJy;
#if USINGZ
            var ptZ = pt.Z;
            pt1 = new Point64(ptX - absDeltaNormalJx, ptY - absDeltaNormalJy, ptZ);
            pt2 = new Point64(ptX + absDeltaNormalJx, ptY + absDeltaNormalJy, ptZ);
#else
            pt1 = new Point64(ptX - absDeltaNormalJx, ptY - absDeltaNormalJy);
            pt2 = new Point64(ptX + absDeltaNormalJx, ptY + absDeltaNormalJy);
#endif
        }
        else
        {
            var groupDeltaNormalJx = _groupDelta * normalJx;
            var groupDeltaNormalJy = _groupDelta * normalJy;
            var groupDeltaNormalKx = _groupDelta * normalK.x;
            var groupDeltaNormalKy = _groupDelta * normalK.y;
#if USINGZ
			var ptZ = pt.Z;
			pt1 = new Point64(ptX + groupDeltaNormalKx, ptY + groupDeltaNormalKy, ptZ);
			pt2 = new Point64(ptX + groupDeltaNormalJx, ptY + groupDeltaNormalJy, ptZ);
#else
            pt1 = new Point64(ptX + groupDeltaNormalKx, ptY + groupDeltaNormalKy);
            pt2 = new Point64(ptX + groupDeltaNormalJx, ptY + groupDeltaNormalJy);
#endif
        }
        pathOut.Add(pt1);
        pathOut.Add(pt2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoSquare(Point64 pointJ, Point64 pointK, PointD normalJ, PointD normalK, bool isEnd)
    {
        PointD vec;
        if (isEnd)
        {
            vec = new PointD(normalJ.y, -normalJ.x);
        }
        else
        {
            vec = GetAvgUnitVector(new PointD(-normalK.y, normalK.x), new PointD(normalJ.y, -normalJ.x));
        }

        var absDelta = Math.Abs(_groupDelta);
        var vecX = vec.x;
        var vecY = vec.y;
        // now offset the original vertex delta units along unit vector
        var ptQ = new PointD(pointJ);
        ptQ = TranslatePoint(ptQ, absDelta * vecX, absDelta * vecY);

        // get perpendicular vertices
        var pt1 = TranslatePoint(ptQ, _groupDelta * vecY, _groupDelta * -vecX);
        var pt2 = TranslatePoint(ptQ, _groupDelta * -vecY, _groupDelta * vecX);
        // get 2 vertices along one edge offset
        var pt3 = GetPerpendicD(pointK, normalK);

        if (isEnd)
        {
            var pt4 = new PointD(pt3.x + vecX * _groupDelta, pt3.y + vecY * _groupDelta);
            var pt = IntersectPoint(pt1, pt2, pt3, pt4);
#if USINGZ
            pt.z = ptQ.z;
#endif
            //get the second intersect point through reflecion
            pathOut.Add(new Point64(ReflectPoint(pt, ptQ)));
            pathOut.Add(new Point64(pt));
        }
        else
        {
            var pt4 = GetPerpendicD(pointJ, normalK);
            var pt = IntersectPoint(pt1, pt2, pt3, pt4);
#if USINGZ
            pt.z = ptQ.z;
#endif
            pathOut.Add(new Point64(pt));
            //get the second intersect point through reflecion
            pathOut.Add(new Point64(ReflectPoint(pt, ptQ)));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoMiter(Point64 pt, PointD normalJ, PointD normalK, double cosA)
    {
        var q = _groupDelta / (cosA + 1d);
#if USINGZ
        pathOut.Add(new Point64(pt.X + (normalK.x + normalJ.x) * q, pt.Y + (normalK.y + normalJ.y) * q, pt.Z));
#else
        pathOut.Add(new Point64(pt.X + (normalK.x + normalJ.x) * q, pt.Y + (normalK.y + normalJ.y) * q));
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DoRound(Point64 pt, PointD normalJ, PointD normalK, bool isEnd, double angle)
    {
        if (DeltaCallback != null)
        {
            ConfigureRoundSteps(_groupDelta);
        }

        var offsetVec = new PointD(normalK.x * _groupDelta, normalK.y * _groupDelta);
        if (isEnd)
        {
            offsetVec.Negate();
        }
        var ptX = pt.X;
        var ptY = pt.Y;
        var offsetVecX = offsetVec.x;
        var offsetVecY = offsetVec.y;
        var steps = (int)Math.Ceiling(_stepsPerRad * Math.Abs(angle));
        var outputStart = pathOut.Count;
        if (steps > 0 && steps <= RoundBulkWriteLimit && steps < int.MaxValue - outputStart)
        {
            var outputCount = steps + 1;
            pathOut.EnsureCapacity(outputStart + outputCount);
            CollectionsMarshal.SetCount(pathOut, outputStart + outputCount);
            var output = CollectionsMarshal.AsSpan(pathOut).Slice(outputStart, outputCount);
#if USINGZ
            output[0] = new Point64(ptX + offsetVecX, ptY + offsetVecY, pt.Z);
#else
            output[0] = new Point64(ptX + offsetVecX, ptY + offsetVecY);
#endif
            for (var i = 1; i < steps; ++i) // ie 1 less than steps
            {
                offsetVec = new PointD(offsetVecX * _stepCos - _stepSin * offsetVecY, offsetVecX * _stepSin + offsetVecY * _stepCos);
#if USINGZ
                output[i] = new Point64(ptX + offsetVecX, ptY + offsetVecY, pt.Z);
#else
                output[i] = new Point64(ptX + offsetVecX, ptY + offsetVecY);
#endif
            }
            output[steps] = GetPerpendic(pt, normalJ);
            return;
        }

        // Preserve incremental growth for pathological step counts and NaN/overflow conversions.
#if USINGZ
        pathOut.Add(new Point64(ptX + offsetVecX, ptY + offsetVecY, pt.Z));
#else
        pathOut.Add(new Point64(ptX + offsetVecX, ptY + offsetVecY));
#endif
        for (var i = 1; i < steps; ++i)
        {
            offsetVec = new PointD(offsetVecX * _stepCos - _stepSin * offsetVecY, offsetVec.x * _stepSin + offsetVec.y * _stepCos);
#if USINGZ
            pathOut.Add(new Point64(ptX + offsetVecX, ptY + offsetVecY, pt.Z));
#else
            pathOut.Add(new Point64(ptX + offsetVecX, ptY + offsetVecY));
#endif
        }
        pathOut.Add(GetPerpendic(pt, normalJ));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void BuildNormals(Path64 path)
    {
        var points = CollectionsMarshal.AsSpan(path);
        var cnt = points.Length;
        _normals.Clear();
        if (cnt == 0)
        {
            return;
        }
        _normals.EnsureCapacity(cnt);
        CollectionsMarshal.SetCount(_normals, cnt);
        var normals = CollectionsMarshal.AsSpan(_normals);
        for (var i = 0; i < cnt - 1; ++i)
        {
            normals[i] = GetUnitNormal(points[i], points[i + 1]);
        }
        normals[cnt - 1] = GetUnitNormal(points[cnt - 1], points[0]);
    }

    private void ConfigureRoundSteps(double groupDelta)
    {
        var absDelta = Math.Abs(groupDelta);
        var arcTol = ArcTolerance > 0.01 ? ArcTolerance : absDelta * arc_const;
        var isNegative = groupDelta < 0.0;
        if (absDelta == _configuredRoundDelta && arcTol == _configuredArcTolerance && isNegative == _configuredRoundNegative)
        {
            return;
        }

        var stepsPer360 = Math.PI / Math.Acos(1 - arcTol / absDelta);
        (_stepSin, _stepCos) = Math.SinCos(DoublePI / stepsPer360);
        if (isNegative)
        {
            _stepSin = -_stepSin;
        }
        _stepsPerRad = stepsPer360 * InvDoublePI;
        _configuredRoundDelta = absDelta;
        _configuredArcTolerance = arcTol;
        _configuredRoundNegative = isNegative;
    }

    private int EstimateOutputCapacity(int baseCapacity, int roundSweeps)
    {
        if (_joinType != JoinType.Round && _endType != EndType.Round)
        {
            return baseCapacity;
        }
        var extra = Math.Ceiling(_stepsPerRad * DoublePI * roundSweeps);
        // Don't turn an extreme tolerance into an eager multi-gigabyte allocation
        return extra >= int.MaxValue - baseCapacity ? baseCapacity : baseCapacity + (int)extra;
    }

    private void OffsetPointCore(ref Point64 pointJ, ref Point64 pointK, ref PointD normalJ, ref PointD normalK, double sinA, double cosA, int j, ref int k, Path64? callbackPath)
    {
        if (sinA > 1.0)
        {
            sinA = 1.0;
        }
        else if (sinA < -1.0)
        {
            sinA = -1.0;
        }
        if (Math.Abs(_groupDelta) < Tolerance)
        {
            pathOut.Add(pointJ);
            return;
        }

        if (cosA > -0.999 && sinA * _groupDelta < 0) // test for concavity first (#593)
        {
            // is concave
            // by far the simplest way to construct concave joins, especially those joining very
            // short segments, is to insert 3 points that produce negative regions. These regions
            // will be removed later by the finishing union operation. This is also the best way
            // to ensure that path reversals (ie over-shrunk paths) are removed.
            pathOut.Add(GetPerpendic(pointJ, normalK));
            pathOut.Add(pointJ); // (#405, #873, #916)
            pathOut.Add(GetPerpendic(pointJ, normalJ));
        }
        else if (cosA > 0.999 && _joinType != JoinType.Round)
        {
            // almost straight - less than 2.5 degree (#424, #482, #526 & #724)
            DoMiter(pointJ, normalJ, normalK, cosA);
        }
        else
            switch (_joinType)
            {
                // miter unless the angle is sufficiently acute to exceed ML
                case JoinType.Miter when cosA > _mitLimSqr - 1:
                    DoMiter(pointJ, normalJ, normalK, cosA);
                    break;
                case JoinType.Miter:
                    DoSquare(pointJ, callbackPath == null ? pointK : callbackPath[k], normalJ, normalK, false);
                    break;
                case JoinType.Round:
                    DoRound(pointJ, normalJ, normalK, false, Math.Atan2(sinA, cosA));
                    break;
                case JoinType.Bevel:
                    DoBevel(pointJ, normalJ, normalK, false);
                    break;
                default:
                    DoSquare(pointJ, callbackPath == null ? pointK : callbackPath[k], normalJ, normalK, false);
                    break;
            }

        k = j;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OffsetPointFixed(ref Point64 pointJ, ref Point64 pointK, ref PointD normalJ, ref PointD normalK, int j, ref int k)
    {
        if (pointJ.X == pointK.X && pointJ.Y == pointK.Y)
        {
            k = j;
            return;
        }

        // Let A = change in angle where edges join
        // A == 0: ie no change in angle (flat join)
        // A == PI: edges 'spike'
        // sin(A) < 0: right turning
        // cos(A) < 0: change in angle is more than 90 degree
        var sinA = normalJ.y * normalK.x - normalK.y * normalJ.x;
        var cosA = normalJ.x * normalK.x + normalJ.y * normalK.y;
        OffsetPointCore(ref pointJ, ref pointK, ref normalJ, ref normalK, sinA, cosA, j, ref k, null);
    }

    private void OffsetPointCallback(Group group, Path64 path, int j, ref int k)
    {
        Point64 pointJ = path[j], pointK = path[k];
        if (pointJ == pointK)
        {
            k = j;
            return;
        }
        PointD normalJ = _normals[j], normalK = _normals[k];
        var callback = DeltaCallback;
        if (callback == null)
        {
            OffsetPointFixed(ref pointJ, ref pointK, ref normalJ, ref normalK, j, ref k);
            return;
        }
        var sinA = InternalClipper.CrossProduct(normalJ, normalK);
        var cosA = InternalClipper.DotProduct(normalJ, normalK);

        _groupDelta = callback(path, _normals, j, k);
        if (group.pathsReversed)
        {
            _groupDelta = -_groupDelta;
        }
        if (Math.Abs(_groupDelta) < Tolerance)
        {
            pathOut.Add(path[j]);
            return;
        }
        // Callbacks receive the mutable source collections. Refresh the values used
        // below so mutations remain observable exactly as they were with indexers.
        pointJ = path[j];
        normalJ = _normals[j];
        normalK = _normals[k];
        OffsetPointCore(ref pointJ, ref pointK, ref normalJ, ref normalK, sinA, cosA, j, ref k, path);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OffsetPolygon(Group group, Path64 path)
    {
        int cnt = path.Count, prev = cnt - 1;
        pathOut = [with(EstimateOutputCapacity(cnt + 8, 1))];

        if (DeltaCallback == null)
        {
            var points = CollectionsMarshal.AsSpan(path);
            var normals = CollectionsMarshal.AsSpan(_normals);
            for (var i = 0; i < cnt; ++i)
            {
                OffsetPointFixed(ref points[i], ref points[prev], ref normals[i], ref normals[prev], i, ref prev);
            }
        }
        else
        {
            for (var i = 0; i < cnt; ++i)
            {
                OffsetPointCallback(group, path, i, ref prev);
            }
        }
        _solution.Add(pathOut);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OffsetOpenJoined(Group group, Path64 path)
    {
        var usedCallback = DeltaCallback != null;
        OffsetPolygon(group, path);
        if (usedCallback)
        {
            path = Clipper.ReversePath(path);
            BuildNormals(path);
            OffsetPolygon(group, path);
            return;
        }

        var normals = CollectionsMarshal.AsSpan(_normals);
        var closingNormal = normals[^1];
        var len = normals.Length - 1;
        for (var i = len; i > 0; --i)
        {
            normals[i] = new PointD(-normals[i - 1].x, -normals[i - 1].y);
        }
        normals[0] = new PointD(-closingNormal.x, -closingNormal.y);

        pathOut = [with(EstimateOutputCapacity(path.Count + 8, 1))];
        var points = CollectionsMarshal.AsSpan(path);
        normals = CollectionsMarshal.AsSpan(_normals);
        var prev = 0;
        var count = path.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            OffsetPointFixed(ref points[i], ref points[prev], ref normals[i], ref normals[prev], i, ref prev);
        }
        _solution.Add(pathOut);
    }

    private void OffsetOpenPath(Group group, Path64 path)
    {
        var sourceCount = path.Count;
        var capacity = sourceCount <= (int.MaxValue - 8) / 2 ? sourceCount * 2 + 8 : sourceCount;
        pathOut = [with(EstimateOutputCapacity(capacity, 2))];
        var highI = path.Count - 1;

        if (DeltaCallback != null)
        {
            _groupDelta = DeltaCallback(path, _normals, 0, 0);
        }
        var startPoint = path[0];
        var startNormal = _normals[0];

        // do the line start cap
        if (Math.Abs(_groupDelta) < Tolerance)
        {
            pathOut.Add(startPoint);
        }
        else
            switch (_endType)
            {
                case EndType.Butt:
                    DoBevel(startPoint, startNormal, startNormal, true);
                    break;
                case EndType.Round:
                    DoRound(startPoint, startNormal, startNormal, true, Math.PI);
                    break;
                default:
                    DoSquare(startPoint, startPoint, startNormal, startNormal, true);
                    break;
            }

        // offset the left side going forward
        if (DeltaCallback == null)
        {
            var points = CollectionsMarshal.AsSpan(path);
            var normals = CollectionsMarshal.AsSpan(_normals);
            for (int i = 1, k = 0; i < highI; ++i)
            {
                OffsetPointFixed(ref points[i], ref points[k], ref normals[i], ref normals[k], i, ref k);
            }
        }
        else
        {
            for (int i = 1, k = 0; i < highI; ++i)
            {
                OffsetPointCallback(group, path, i, ref k);
            }
        }

        // reverse normals ...
        {
            var reversedNormals = CollectionsMarshal.AsSpan(_normals);
            for (var i = highI; i > 0; --i)
            {
                reversedNormals[i] = new PointD(-reversedNormals[i - 1].x, -reversedNormals[i - 1].y);
            }
            reversedNormals[0] = reversedNormals[highI];
        }

        if (DeltaCallback != null)
        {
            _groupDelta = DeltaCallback(path, _normals, highI, highI);
        }
        var endPoint = path[highI];
        var endNormal = _normals[highI];
        // do the line end cap
        if (Math.Abs(_groupDelta) < Tolerance)
        {
            pathOut.Add(endPoint);
        }
        else
            switch (_endType)
            {
                case EndType.Butt:
                    DoBevel(endPoint, endNormal, endNormal, true);
                    break;
                case EndType.Round:
                    DoRound(endPoint, endNormal, endNormal, true, Math.PI);
                    break;
                default:
                    DoSquare(endPoint, endPoint, endNormal, endNormal, true);
                    break;
            }

        // offset the left side going back
        if (DeltaCallback == null)
        {
            var points = CollectionsMarshal.AsSpan(path);
            var normals = CollectionsMarshal.AsSpan(_normals);
            for (int i = highI - 1, k = highI; i > 0; --i)
            {
                OffsetPointFixed(ref points[i], ref points[k], ref normals[i], ref normals[k], i, ref k);
            }
        }
        else
        {
            for (int i = highI - 1, k = highI; i > 0; --i)
            {
                OffsetPointCallback(group, path, i, ref k);
            }
        }

        _solution.Add(pathOut);
    }

    private void DoGroupOffset(Group group)
    {
        if (group.endType == EndType.Polygon)
        {
            // a straight path (2 points) can now also be 'polygon' offset
            // where the ends will be treated as (180 deg.) joins
            if (group.lowestPathIdx < 0)
            {
                _delta = Math.Abs(_delta);
            }
            _groupDelta = group.pathsReversed ? -_delta : _delta;
        }
        else
        {
            _groupDelta = Math.Abs(_delta);
        }

        var absDelta = Math.Abs(_groupDelta);

        _joinType = group.joinType;
        _endType = group.endType;

        if (group.joinType == JoinType.Round || group.endType == EndType.Round)
        {
            ConfigureRoundSteps(_groupDelta);
        }

        var inputPaths = CollectionsMarshal.AsSpan(group.inPaths);
        var len = inputPaths.Length;
        for (var pathIndex = 0; pathIndex < len; ++pathIndex)
        {
            var p = inputPaths[pathIndex];
            var cnt = p.Count;

            switch (cnt)
            {
                case 1:
                    {
                        var pt = p[0];

                        if (DeltaCallback != null)
                        {
                            _groupDelta = DeltaCallback(p, _normals, 0, 0);
                            if (group.pathsReversed)
                            {
                                _groupDelta = -_groupDelta;
                            }
                            absDelta = Math.Abs(_groupDelta);
                        }

                        // single vertex so build a circle or square ...
                        if (group.endType == EndType.Round)
                        {
                            var steps = (int)Math.Ceiling(_stepsPerRad * DoublePI);
                            pathOut = Clipper.Ellipse(pt, absDelta, absDelta, steps);
#if USINGZ
                            pathOut = InternalClipper.SetZ(pathOut, pt.Z);
#endif
                        }
                        else
                        {
                            var d = (int)Math.Ceiling(_groupDelta);
                            var r = new Rect64(pt.X - d, pt.Y - d, pt.X + d, pt.Y + d);
                            pathOut = r.AsPath();
#if USINGZ
                            pathOut = InternalClipper.SetZ(pathOut, pt.Z);
#endif
                        }
                        _solution.Add(pathOut);
                        continue; // end of offsetting a single point
                    }
                case 2 when group.endType == EndType.Joined:
                    _endType = (group.joinType == JoinType.Round) ? EndType.Round : EndType.Square;
                    break;
            }

            BuildNormals(p);
            switch (_endType)
            {
                case EndType.Polygon:
                    OffsetPolygon(group, p);
                    break;
                case EndType.Joined:
                    OffsetOpenJoined(group, p);
                    break;
                default:
                    OffsetOpenPath(group, p);
                    break;
            }
        }
    }
}
