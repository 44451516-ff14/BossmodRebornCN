using System.Threading;

namespace BossMod.Pathfinding;

// utility for selecting player's navigation target
// there are several goals that navigation has to meet, in following rough priority
// 1. stay away from aoes; tricky thing is that sometimes it is ok to temporarily enter aoe, if we're sure we'll exit it in time
// 2. maintain uptime - this is represented by being in specified range of specified target, and not moving to interrupt casts unless needed
// 3. execute positionals - this is strictly less important than points above, we only do that if we can meet other conditions
// 4. be in range of healers - even less important, but still nice to do
public struct NavigationDecision
{
    // context that allows reusing large memory allocations
    public sealed class Context
    {
        public float[] Scratch = [];
        public Map Map = new();
        public ThetaStar ThetaStar = new();
    }

    public WPos? Destination;
    public WPos? NextWaypoint;
    public float LeewaySeconds; // can be used for finishing casts / slidecasting etc.
    public float TimeToGoal;

    public const float ActivationTimeCushion = 1f; // reduce time between now and activation by this value in seconds; increase for more conservativeness

    public static NavigationDecision Build(Context ctx, WorldState ws, AIHints hints, Actor player, float playerSpeed = 6f, float forbiddenZoneCushion = default)
    {
        // build a pathfinding map: rasterize all forbidden zones and goals
        hints.InitPathfindMap(ctx.Map);
        // local copies of forbidden zones and goals to ensure no race conditions during async pathfinding
        (Func<WPos, float>, DateTime, ulong)[] localForbiddenZones = [.. hints.ForbiddenZones];
        Func<WPos, float>[] localGoalZones = [.. hints.GoalZones];
        if (hints.ForbiddenZones.Count != 0)
        {
            RasterizeForbiddenZones(ctx.Map, localForbiddenZones, ws.CurrentTime, ctx.Scratch);
        }
        if (player.CastInfo == null) // don't rasterize goal zones if casting or if inside a very dangerous pixel
        {
            var index = ctx.Map.GridToIndex(ctx.Map.WorldToGrid(player.Position));
            if (ctx.Map.PixelMaxG.Length > index && ctx.Map.PixelMaxG[index] is >= 1f or < 0f) // prioritize safety over uptime, still needs to be active for below 0 MaxG to go back inside arena bounds if needed
            {
                if (hints.GoalZones.Count != 0)
                {
                    RasterizeGoalZones(ctx.Map, localGoalZones);
                }
                if (forbiddenZoneCushion > 0)
                {
                    AvoidForbiddenZone(ctx.Map, forbiddenZoneCushion);
                }
            }
        }
        // execute pathfinding
        ctx.ThetaStar.Start(ctx.Map, player.Position, 1.0f / playerSpeed);
        var bestNodeIndex = ctx.ThetaStar.Execute();
        ref var bestNode = ref ctx.ThetaStar.NodeByIndex(bestNodeIndex);
        var waypoints = GetFirstWaypoints(ctx.ThetaStar, ctx.Map, bestNodeIndex, player.Position);
        return new() { Destination = waypoints.first, NextWaypoint = waypoints.second, LeewaySeconds = bestNode.PathLeeway, TimeToGoal = bestNode.GScore };
    }

    private static void AvoidForbiddenZone(Map map, float forbiddenZoneCushion)
    {
        var d = (int)(forbiddenZoneCushion / map.Resolution);
        map.MaxPriority = -1;
        var pixels = map.EnumeratePixels();
        var len = pixels.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref pixels[i];
            ref readonly var px = ref p.x;
            ref readonly var py = ref p.y;
            var cellIndex = map.GridToIndex(px, py);
            if (map.PixelMaxG[cellIndex] == float.MaxValue)
            {
                var hasDangerousNeighbour = false;
                for (var ox = -1; ox <= 1 && !hasDangerousNeighbour; ++ox)
                {
                    for (var oy = -1; oy <= 1; ++oy)
                    {
                        if (ox == 0 && oy == 0)
                        {
                            continue;
                        }
                        var (nx, ny) = map.ClampToGrid((px + ox * d, py + oy * d));
                        if (map.PixelMaxG[map.GridToIndex(nx, ny)] != float.MaxValue)
                        {
                            hasDangerousNeighbour = true;
                            break;
                        }
                    }
                }

                if (hasDangerousNeighbour)
                {
                    map.PixelPriority[cellIndex] -= 0.125f;
                }
            }
            map.MaxPriority = Math.Max(map.MaxPriority, map.PixelPriority[cellIndex]);
        }
    }

    public static void RasterizeForbiddenZones(Map map, (Func<WPos, float> shapeDistance, DateTime activation, ulong source)[] zones, DateTime current, float[] scratch)
    {
        // 1) Cluster activation times
        // very slight difference in activation times cause issues for pathfinding - cluster them together
        var zonesFixed = new (Func<WPos, float> shapeDistance, float g)[zones.Length];
        DateTime clusterEnd = default, globalStart = current, globalEnd = current.AddSeconds(120d);
        float clusterG = 0;
        var lenZonesFixed = zonesFixed.Length;
        for (var i = 0; i < lenZonesFixed; ++i)
        {
            ref var zone = ref zones[i];
            var activation = zone.activation.Clamp(globalStart, globalEnd);
            if (activation > clusterEnd)
            {
                clusterG = ActivationToG(activation, current);
                clusterEnd = activation.AddSeconds(0.5d);
            }
            zonesFixed[i] = (zone.shapeDistance, clusterG);
        }

        var width = map.Width;
        var height = map.Height;
        var lenPixelMaxG = map.PixelMaxG.Length;

        var resolution = map.Resolution;
        var cushion = resolution * 0.5f;
        map.MaxG = clusterG;

        if (scratch.Length < lenPixelMaxG)
            scratch = new float[lenPixelMaxG];
        Array.Fill(scratch, float.MaxValue);

        var dy = map.LocalZDivRes * resolution * resolution;
        var dx = dy.OrthoL();
        var topLeft = map.Center - (width >> 1) * dx - (height >> 1) * dy;

        // note that a zone can partially intersect a pixel; so what we do is check each corner and set the maxg value of a pixel equal to the minimum of 4 corners
        // to avoid 4x calculations, we do a slightly tricky loop:
        // - outer loop fills row i to with g values corresponding to the 'upper edge' of cell i
        // - inner loop calculates the g value at the left border, then iterates over all right corners and fills minimums of two g values to the cells
        // - second outer loop calculates values at 'bottom' edge and then updates the values of all cells to correspond to the cells rather than edges
        // - third loops checks center and surrounding circle until cell edge to counter small cones not intersecting corners of a cell

        // --------------------------------------------------------------
        // PASS #1 (Parallel over rows): compute min corner G in scratch
        // --------------------------------------------------------------

        // This pass sets: scratch[iCell] = min(G-of-left-corner, G-of-right-corner)
        // for each pixel in row y, from x=0..width-1.
        Parallel.For(0, height, y =>
        {
            var rowStart = y * width;
            var rowCorner = topLeft + y * dy;

            var leftPos = rowCorner;
            var leftG = CalculateMaxG(ref zonesFixed, leftPos);

            for (var x = 0; x < width; ++x)
            {
                var rightPos = leftPos + dx;
                var rightG = CalculateMaxG(ref zonesFixed, rightPos);
                scratch[rowStart + x] = Math.Min(leftG, rightG);
                leftPos = rightPos;
                leftG = rightG;
            }
        });

        // --------------------------------------------------------------
        // PASS #2 (Parallel over columns): combine top corners with bottom
        // --------------------------------------------------------------
        //
        // This takes the 'top' corners from scratch[] and merges them with
        // the 'bottom' corners for each column. We can parallelize
        // by letting each thread handle one column of pixels. Since each
        // column is independent of others, there's no write collision.
        //
        // We'll track how many cells become blocked in a thread-local counter
        // and aggregate it with Interlocked.Add.

        var numBlockedCells = 0;

        Parallel.For(0, width, x =>
        {
            // Each column starts from the same 'bottom corner' approach:
            // But we can compute "bottom corners" for this column now.
            // The bottom row's corner is topLeft + height*dy + x*dx
            // because at the end of pass #1, 'cy' was top-left + (height)*dy.
            var cyBottom = topLeft + height * dy + x * dx;
            var bleftG = CalculateMaxG(ref zonesFixed, cyBottom);

            var columnStart = x;
            var localBlocked = 0; // local aggregator

            var bottomG = bleftG;
            for (var y = height - 1; y >= 0; y--)
            {
                var jCell = columnStart + y * width;
                // top corner from pass #1
                var topG = scratch[jCell];
                ref var pixelMaxG = ref map.PixelMaxG[jCell];
                var cellG = Math.Min(Math.Min(topG, bottomG), pixelMaxG);

                pixelMaxG = cellG;
                if (cellG != float.MaxValue)
                {
                    map.PixelPriority[jCell] = float.MinValue;
                    localBlocked++;
                }
                bottomG = topG;
            }

            // Merge local count
            Interlocked.Add(ref numBlockedCells, localBlocked);
        });

        // --------------------------------------------------------------
        // PASS #3 (Parallel): check each pixel center to catch partial overlaps, this is needed because small cones might not intersect corners
        // with a cushion of cellsize / 2 this ensures the entire inner circle until the edge will be safe
        // --------------------------------------------------------------
        Parallel.For(0, lenPixelMaxG, idx =>
        {
            var (px, py) = map.IndexToGrid(idx);
            var centerPos = map.GridToWorld(px, py, 0.5f, 0.5f);

            var centerG = CalculateMaxG(ref zonesFixed, centerPos, cushion);
            var oldVal = map.PixelMaxG[idx];
            if (centerG < oldVal)
            {
                map.PixelMaxG[idx] = centerG;
                if (oldVal == float.MaxValue)
                {
                    map.PixelPriority[idx] = float.MinValue;
                    Interlocked.Increment(ref numBlockedCells);
                }
            }
        });

        // --------------------------------------------------------------
        // PASS #4: if absolutely everything is blocked, free the "least dangerous"
        // --------------------------------------------------------------
        //  - We need the actual max of map.PixelMaxG to know which ones to free
        //  - First parallel pass: find max
        //  - Second parallel pass: free cells with that max

        if (numBlockedCells == width * height)
        {
            // 4a) find the real max
            var realMaxG = float.MinValue;
            // parallel reduction
            Parallel.For(0, lenPixelMaxG, () => float.MinValue,
                (i, loopState, localMax) =>
                {
                    ref var val = ref map.PixelMaxG[i];
                    return (val > localMax) ? val : localMax;
                },
                localMax =>
                {
                    // Merge local maxima with an atomic
                    float initVal, computedVal;
                    do
                    {
                        initVal = realMaxG;
                        computedVal = Math.Max(initVal, localMax);
                    }
                    while (initVal != Interlocked.CompareExchange(
                        ref realMaxG, computedVal, initVal));
                }
            );

            // 4b) free pixels that match that max
            Parallel.For(0, lenPixelMaxG, i =>
            {
                ref var pixelMaxG = ref map.PixelMaxG[i];
                if (pixelMaxG == realMaxG)
                {
                    pixelMaxG = float.MaxValue;
                    map.PixelPriority[i] = 0f;
                }
            });
        }
    }

    public static void RasterizeGoalZones(Map map, Func<WPos, float>[] goals)
    {
        var resolution = map.Resolution;
        var width = map.Width;
        var height = map.Height;
        var dy = map.LocalZDivRes * resolution * resolution;
        var dx = dy.OrthoL();
        var topLeft = map.Center - (width >> 1) * dx - (height >> 1) * dy;
        var len = goals.Length;

        // We'll do two passes:
        //    Pass #1: row-by-row (parallel over y)
        //    Pass #2: column-by-column (parallel over x)

        //------------------------------------------------------------------------
        // PASS #1 (row-based) - fill in partial priorities in map.PixelPriority
        //------------------------------------------------------------------------
        Parallel.For(0, height, y =>
        {
            // For row y, compute the position of the 'left corner' in world coords
            var cy = topLeft + y * dy;

            // Sum up all goals at the left corner (x=0)
            float leftP = 0;
            for (var i = 0; i < len; ++i)
            {
                leftP += goals[i](cy);
            }

            // Now walk across the row from x=0..(width-1), computing right corner
            var rowStart = y * width;
            var cx = cy;
            for (var x = 0; x < width; ++x)
            {
                // Right corner for this pixel is cx = cy + x*dx
                cx += dx;
                float rightP = 0;
                for (var i = 0; i < len; ++i)
                {
                    rightP += goals[i](cx);
                }

                // Store the min in PixelPriority
                map.PixelPriority[rowStart + x] = Math.Min(leftP, rightP);

                // Shift left -> right
                leftP = rightP;
            }
        });

        //------------------------------------------------------------------------
        // PASS #2 (column-based) - combine top (in PixelPriority) with bottom corners
        //------------------------------------------------------------------------
        // We also update map.MaxPriority here. Each thread will keep a local maximum
        // and we'll merge them in a thread-safe way.
        var globalMaxPriority = float.MinValue;

        // We'll compute the bottom-left corner *once* per column. The bottom row is
        // topLeft + height*dy. Then we move right by x*dx for each column.
        var bottomRowLeft = topLeft + height * dy;  // world coords for left corner of the *bottom* row

        Parallel.For(0, width, () => float.MinValue,
        (x, loopState, localMax) =>
        {
            // For column x, compute the bottom-left corner
            var cyBottom = bottomRowLeft + x * dx;

            // The 'left' bottom corner's priority
            float bleftP = 0;
            for (var i = 0; i < len; ++i)
            {
                bleftP += goals[i](cyBottom);
            }

            var bottomP = bleftP;
            var iCell = (height - 1) * width + x;

            for (var y = height - 1; y >= 0; --y, iCell -= width)
            {
                var topP = map.PixelPriority[iCell];

                // If this pixel is not blocked (PixelMaxG == float.MaxValue),
                // we keep the min of topP and bottomP. Otherwise, we set it to float.MinValue.
                if (map.PixelMaxG[iCell] == float.MaxValue)
                {
                    var cellP = Math.Min(topP, bottomP);
                    map.PixelPriority[iCell] = cellP;

                    // Update local max
                    if (cellP > localMax)
                        localMax = cellP;
                }
                else
                {
                    // Mark blocked areas
                    map.PixelPriority[iCell] = float.MinValue;
                }

                // Shift bottom -> top for next iteration
                bottomP = topP;
            }

            // Return thread-local max for final merge
            return localMax;
        },
        // Final merge across threads:
        localMax =>
        {
            float initVal, newVal;
            do
            {
                initVal = globalMaxPriority;
                newVal = Math.Max(initVal, localMax);
            }
            while (initVal != Interlocked.CompareExchange(
                ref globalMaxPriority, newVal, initVal));
        });

        // Finally store the global maximum in map.MaxPriority
        map.MaxPriority = globalMaxPriority;
    }
    private static float ActivationToG(DateTime activation, DateTime current) => Math.Max(0f, (float)(activation - current).TotalSeconds - ActivationTimeCushion);

    private static float CalculateMaxG(ref (Func<WPos, float> shapeDistance, float g)[] zones, WPos p, float cushion = 0f)
    {
        var len = zones.Length;
        var threshold = cushion;
        for (var i = 0; i < len; ++i)
        {
            ref var z = ref zones[i];
            if (z.shapeDistance(p) < threshold)
                return z.g;
        }
        return float.MaxValue;
    }

    private static (WPos? first, WPos? second) GetFirstWaypoints(ThetaStar pf, Map map, int cell, WPos startingPos)
    {
        ref var startingNode = ref pf.NodeByIndex(cell);
        var iterations = 0; // iteration counter to prevent rare cases of infinite loops
        var maxIterations = map.Width * map.Height;

        if (startingNode.GScore == 0f && startingNode.PathMinG == float.MaxValue)
            return (null, null); // we're already in safe zone

        var nextCell = cell;
        do
        {
            ref var node = ref pf.NodeByIndex(cell);
            if (pf.NodeByIndex(node.ParentIndex).GScore == 0f || ++iterations == maxIterations)
            {
                //var dest = pf.CellCenter(cell);
                // if destination coord matches player coord, do not move along that coordinate, this is used for precise positioning
                var destCoord = map.IndexToGrid(cell);
                var playerCoordFrac = map.WorldToGridFrac(startingPos);
                var playerCoord = Map.FracToGrid(playerCoordFrac);
                var dest = map.GridToWorld(destCoord.x, destCoord.y, destCoord.x == playerCoord.x ? playerCoordFrac.X - playerCoord.x : 0.5f, destCoord.y == playerCoord.y ? playerCoordFrac.Y - playerCoord.y : 0.5f);

                var next = pf.CellCenter(nextCell);
                return (dest, next);
            }
            nextCell = cell;
            cell = node.ParentIndex;
        }
        while (true);
    }
}
