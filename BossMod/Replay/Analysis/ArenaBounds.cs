﻿using Clipper2Lib;
using ImGuiNET;
using System.Globalization;

namespace BossMod.ReplayAnalysis;

class ArenaBounds
{
    private readonly List<(Replay, Replay.Participant, DateTime, Vector3, uint)> _points = [];
    private readonly UIPlot _plot = new();

    public ArenaBounds(List<Replay> replays, uint oid)
    {
        _plot.DataMin = new(float.MaxValue, float.MaxValue);
        _plot.DataMax = new(float.MinValue, float.MinValue);
        _plot.MainAreaSize = new(500, 500);
        _plot.TickAdvance = new(10, 10);

        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var ps in enc.ParticipantsByOID.Values)
                {
                    foreach (var p in ps)
                    {
                        var iStart = p.PosRotHistory.UpperBound(enc.Time.Start);
                        if (iStart > 0)
                            --iStart;
                        var iEnd = p.PosRotHistory.UpperBound(enc.Time.End);
                        var iNextDead = p.DeadHistory.UpperBound(enc.Time.Start);
                        for (var i = iStart; i < iEnd; ++i)
                        {
                            var t = p.PosRotHistory.Keys[i];
                            var pos = p.PosRotHistory.Values[i].XYZ();
                            if (iNextDead < p.DeadHistory.Count && p.DeadHistory.Keys[iNextDead] <= t)
                                ++iNextDead;
                            var dead = iNextDead > 0 && p.DeadHistory.Values[iNextDead - 1];
                            var color = dead ? Colors.TextColor13 : p.Type is ActorType.Enemy ? Colors.Danger : Colors.PlayerGeneric;
                            _points.Add((replay, p, t, pos, color));
                            _plot.DataMin.X = Math.Min(_plot.DataMin.X, pos.X);
                            _plot.DataMin.Y = Math.Min(_plot.DataMin.Y, pos.Z);
                            _plot.DataMax.X = Math.Max(_plot.DataMax.X, pos.X);
                            _plot.DataMax.Y = Math.Max(_plot.DataMax.Y, pos.Z);
                        }
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        _plot.Begin();
        foreach (var (replay, participant, time, pos, color) in _points)
            _plot.Point(new(pos.X, pos.Z), color, () => $"{ReplayUtils.ParticipantString(participant, time)} {replay.Path} @ {time:O}");
        _plot.End();
    }

    public void DrawContextMenu()
    {
        if (ImGui.MenuItem("Generate complex arena bounds from player movement"))
        {
            Task.Run(() =>
            {
                var playerPoints = _points.Where(p => p.Item2.OID == 0).Select(x => new WPos(x.Item4.XZ())).ToList();
                var points = ConcaveHull.GenerateConcaveHull(playerPoints, 0.5f, 0.1f);
                var center = CalculateCentroid(points);
                var sb = new StringBuilder("private static readonly List<WPos> vertices = [");

                for (var i = 0; i < points.Count; i++)
                {
                    if (i % 5 == 0 && i != 0)
                        sb.Append("\n  ");
                    var p = points[i];
                    sb.Append($"new({p.X.ToString("F2", CultureInfo.InvariantCulture)}f, {p.Z.ToString("F2", CultureInfo.InvariantCulture)}f)");
                    if (i != points.Count - 1)
                    {
                        sb.Append(',');
                        if ((i + 1) % 5 != 0)
                            sb.Append(' ');
                    }
                }

                sb.Append("];");
                sb.Append($"\n// Centroid of the polygon is at: ({center.X.ToString("F2", CultureInfo.InvariantCulture)}f, {center.Z.ToString("F2", CultureInfo.InvariantCulture)}f)");
                ImGui.SetClipboardText(sb.ToString());
            });
        }
    }

    private static WPos CalculateCentroid(List<WPos> points)
    {
        if (points == null || points.Count < 3)
            return default;

        float sumX = 0, sumZ = 0;
        float area = 0;

        for (var i = 0; i < points.Count; i++)
        {
            var current = points[i];
            var next = points[(i + 1) % points.Count];
            var crossProduct = current.X * next.Z - next.X * current.Z;
            area += crossProduct;
            sumX += (current.X + next.X) * crossProduct;
            sumZ += (current.Z + next.Z) * crossProduct;
        }
        area *= 0.5f;

        var centroidX = sumX / (6 * area);
        var centroidZ = sumZ / (6 * area);

        return new(centroidX, centroidZ);
    }
}

public static class ConcaveHull
{
    private static double Distance(WPos a, WPos b) => (b - a).Length();

    private static List<WPos> FilterClosePoints(IEnumerable<WPos> points, double epsilon)
    {
        List<WPos> filteredPoints = [];
        foreach (var point in points)
            if (filteredPoints.All(p => Distance(p, point) > epsilon))
                filteredPoints.Add(point);
        return filteredPoints;
    }

    private static Path64 ConvertToPath64(List<WPos> points, double scale) => new(points.Select(p => new Point64((long)(p.X * scale), (long)(p.Z * scale))).ToList());
    private static List<WPos> ConvertToPoints(Path64 path, double scale) => path.Select(p => new WPos((float)(p.X / scale), (float)(p.Y / scale))).ToList();

    public static List<WPos> GenerateConcaveHull(List<WPos> points, float alpha, float epsilon)
    {
        if (points.Count < 3)
            return new List<WPos>(points); // Not enough points to form a polygon

        points = FilterClosePoints(points, epsilon);
        var scale = 100000.0;
        var inputPath = ConvertToPath64(points, scale);
        var co = new ClipperOffset();
        co.AddPath(inputPath, JoinType.Miter, EndType.Polygon);

        Paths64 solution = [];

        co.Execute(0.1 * scale, solution);

        if (solution.Count == 0)
            return points;

        var mergedPolygon = MergePaths(solution);
        var hull = ConvertToPoints(mergedPolygon, scale);
        hull = ApplyAlphaFilter(hull, alpha);
        hull = RemoveCollinearPoints(hull);

        return hull;
    }

    private static Path64 MergePaths(Paths64 paths)
    {
        var clipper = new Clipper64();
        clipper.AddPaths(paths, PathType.Subject);
        Paths64 mergedSolution = [];
        clipper.Execute(ClipType.Union, FillRule.NonZero, mergedSolution);
        return mergedSolution.OrderByDescending(Clipper.Area).First();
    }

    private static List<WPos> ApplyAlphaFilter(List<WPos> hull, double alpha)
    {
        List<WPos> filteredHull = [hull[0]];

        for (var i = 1; i < hull.Count; i++)
        {
            var currentPoint = hull[i];
            var lastAddedPoint = filteredHull.Last();
            if (Distance(currentPoint, lastAddedPoint) > alpha)
                filteredHull.Add(currentPoint);
        }

        if (Distance(filteredHull.Last(), filteredHull.First()) > alpha)
            filteredHull.Add(filteredHull.First());
        return filteredHull;
    }

    private static List<WPos> RemoveCollinearPoints(List<WPos> points)
    {
        if (points.Count < 3)
            return points;

        List<WPos> filteredPoints = [];
        for (var i = 0; i < points.Count; i++)
        {
            var prev = points[(i - 1 + points.Count) % points.Count];
            var curr = points[i];
            var next = points[(i + 1) % points.Count];

            if (!AreCollinear(prev, curr, next))
                filteredPoints.Add(curr);
        }
        return filteredPoints;
    }

    private static bool AreCollinear(WPos a, WPos b, WPos c)
    {
        var ab = b - a;
        var bc = c - b;
        var crossProduct = ab.X * bc.Z - ab.Z * bc.X;
        return Math.Abs(crossProduct) < 0.01f;
    }
}

