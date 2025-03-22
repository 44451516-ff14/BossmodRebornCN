namespace BossMod.Pathfinding;

public class ThetaStar
{
    public enum Score
    {
        JustBad, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe and has same or lower max-g than starting cell
        UltimatelyBetter, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), destination is unsafe but has larger max-g than starting cell
        UltimatelySafe, // the path is unsafe (there are cells along the path with negative leeway, and some cells have lower max-g than starting cell), however destination is safe
        UnsafeAsStart, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is <= 0)
        SemiSafeAsStart, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe with same max-g as starting cell (starting cell will have this score if its max-g is > 0)
        UnsafeImprove, // the path is unsafe (there are cells along the path with negative leeway, but no max-g lower than starting cell), destination is at least better than start
        SemiSafeImprove, // the path is semi-safe (no cell along the path has negative leeway or max-g lower than starting cell), destination is unsafe but better than start
        Safe, // the path reaches safe cell and is fully safe (no cell along the path has negative leeway) (starting cell will have this score if it's safe)
        SafeBetterPrio, // the path reaches safe cell with a higher goal priority than starting cell (but less than max) and is fully safe (no cell along the path has negative leeway)
        SafeMaxPrio, // the path reaches safe cell with max goal priority and is fully safe (no cell along the path has negative leeway)
    }

    public struct Node
    {
        public float GScore;
        public float HScore;
        public int ParentIndex;
        public int OpenHeapIndex; // -1 if in closed list, 0 if not in any lists, otherwise (index+1)
        public float PathLeeway; // min diff along path between node's g-value and cell's g-value
        public float PathMinG; // minimum 'max g' value along path
        public Score Score;

        public readonly float FScore => GScore + HScore;
    }

    private const float Epsilon = 1e-5f;

    private Map _map = new();
    private Node[] _nodes = [];
    private readonly List<int> _openList = [];
    private float _deltaGSide;
    private float _deltaGDiag;

    private float _mapResolution;
    private float _mapHalfResolution;
    private int _startNodeIndex;
    private float _startMaxG;
    private float _startPrio;
    private Score _startScore;

    private int _bestIndex; // node with best score
    private int _fallbackIndex; // best 'fallback' node: node that we don't necessarily want to go to, but might want to move closer to it (to the parent)

    // statistics
    public int NumSteps;
    public int NumReopens;

    public ref Node NodeByIndex(int index) => ref _nodes[index];
    public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

    // gMultiplier is typically inverse speed, which turns g-values into time
    public void Start(Map map, WPos startPos, float gMultiplier)
    {
        _map = map;
        var numPixels = map.Width * map.Height;
        if (_nodes.Length < numPixels)
            _nodes = new Node[numPixels];
        else
            Array.Fill(_nodes, default, 0, numPixels);
        _openList.Clear();
        _deltaGSide = map.Resolution * gMultiplier;
        _deltaGDiag = _deltaGSide * 1.414214f;
        _mapResolution = map.Resolution;
        _mapHalfResolution = map.Resolution * 0.5f;

        PrefillH();

        var startFrac = map.WorldToGridFrac(startPos);
        var start = map.ClampToGrid(Map.FracToGrid(startFrac));
        _startNodeIndex = _bestIndex = _fallbackIndex = _map.GridToIndex(start.x, start.y);
        _startMaxG = _map.PixelMaxG[_startNodeIndex];
        _startPrio = _map.PixelPriority[_startNodeIndex];
        //if (_startMaxG < 0)
        //    _startMaxG = float.MaxValue; // TODO: this is a hack that allows navigating outside the obstacles, reconsider...
        _startScore = CalculateScore(_startMaxG, _startMaxG, _startMaxG, _startNodeIndex);
        NumSteps = NumReopens = 0;

        startFrac.X -= start.x + 0.5f;
        startFrac.Y -= start.y + 0.5f;
        ref var startNode = ref _nodes[_startNodeIndex];
        startNode = new()
        {
            GScore = 0f,
            HScore = startNode.HScore, //HeuristicDistance(start.x, start.y),
            ParentIndex = _startNodeIndex, // start's parent is self
            PathLeeway = _startMaxG,
            PathMinG = _startMaxG,
            Score = _startScore,
        };
        AddToOpen(_startNodeIndex);
    }

    // returns whether search is to be terminated; on success, first node of the open list would contain found goal
    public bool ExecuteStep()
    {
        if (_openList.Count == 0 /*|| _nodes[_openList[0]].HScore <= 0*/)
            return false;

        ++NumSteps;
        var nextNodeIndex = PopMinOpen();
        var nextNodeX = nextNodeIndex % _map.Width;
        var nextNodeY = nextNodeIndex / _map.Width;
        ref var nextNode = ref _nodes[nextNodeIndex];

        // update our best indices
        if (CompareNodeScores(ref nextNode, ref _nodes[_bestIndex]) < 0)
            _bestIndex = nextNodeIndex;
        if (nextNode.Score == Score.UltimatelySafe && (_fallbackIndex == _startNodeIndex || CompareNodeScores(ref nextNode, ref _nodes[_fallbackIndex]) < 0))
            _fallbackIndex = nextNodeIndex;

        var haveN = nextNodeY > 0;
        var haveS = nextNodeY < _map.Height - 1;
        var haveE = nextNodeX > 0;
        var haveW = nextNodeX < _map.Width - 1;
        if (haveN)
        {
            VisitNeighbour(nextNodeIndex, nextNodeX, nextNodeY - 1, nextNodeIndex - _map.Width, _deltaGSide);
            if (haveE)
                VisitNeighbour(nextNodeIndex, nextNodeX - 1, nextNodeY - 1, nextNodeIndex - _map.Width - 1, _deltaGDiag);
            if (haveW)
                VisitNeighbour(nextNodeIndex, nextNodeX + 1, nextNodeY - 1, nextNodeIndex - _map.Width + 1, _deltaGDiag);
        }
        if (haveE)
            VisitNeighbour(nextNodeIndex, nextNodeX - 1, nextNodeY, nextNodeIndex - 1, _deltaGSide);
        if (haveW)
            VisitNeighbour(nextNodeIndex, nextNodeX + 1, nextNodeY, nextNodeIndex + 1, _deltaGSide);
        if (haveS)
        {
            VisitNeighbour(nextNodeIndex, nextNodeX, nextNodeY + 1, nextNodeIndex + _map.Width, _deltaGSide);
            if (haveE)
                VisitNeighbour(nextNodeIndex, nextNodeX - 1, nextNodeY + 1, nextNodeIndex + _map.Width - 1, _deltaGDiag);
            if (haveW)
                VisitNeighbour(nextNodeIndex, nextNodeX + 1, nextNodeY + 1, nextNodeIndex + _map.Width + 1, _deltaGDiag);
        }
        return true;
    }

    public int Execute()
    {
        while (_nodes[_bestIndex].HScore > 0f && _fallbackIndex == _startNodeIndex && ExecuteStep())
            ;
        return BestIndex();
    }

    public int BestIndex()
    {
        if (_nodes[_bestIndex].Score > _startScore)
            return _bestIndex; // we've found something better than start

        if (_fallbackIndex != _startNodeIndex)
        {
            // find first parent of best-among-worst that is at least as good as start
            var destIndex = _fallbackIndex;
            var parentIndex = _nodes[destIndex].ParentIndex;
            while (_nodes[parentIndex].Score < _startScore)
            {
                destIndex = parentIndex;
                parentIndex = _nodes[destIndex].ParentIndex;
            }

            // TODO: this is very similar to LineOfSight, try to unify implementations...
            ref var startNode = ref _nodes[parentIndex];
            ref var destNode = ref _nodes[destIndex];
            var (x2, y2) = _map.IndexToGrid(destIndex);
            var (x1, y1) = _map.IndexToGrid(parentIndex);
            var dx = x2 - x1;
            var dy = y2 - y1;
            var sx = dx > 0 ? 1 : -1;
            var sy = dy > 0 ? 1 : -1;
            var hsx = 0.5f * sx;
            var hsy = 0.5f * sy;
            var indexDeltaX = sx;
            var indexDeltaY = sy * _map.Width;

            var ab = new Vector2(dx, dy);
            ab /= ab.Length();
            var invx = ab.X != 0f ? 1f / ab.X : float.MaxValue; // either can be infinite, but not both; we want to avoid actual infinities here, because 0*inf = NaN (and we'd rather have it be 0 in this case)
            var invy = ab.Y != 0f ? 1f / ab.Y : float.MaxValue;

            while (x1 != x2 || y1 != y2)
            {
                var tx = hsx * invx; // if negative, we'll never intersect it
                var ty = hsy * invy;
                if (tx < 0f || x1 == x2)
                    tx = float.MaxValue;
                if (ty < 0f || y1 == y2)
                    ty = float.MaxValue;

                var nextIndex = parentIndex;
                if (tx < ty)
                {
                    x1 += sx;
                    nextIndex += indexDeltaX;
                }
                else
                {
                    y1 += sy;
                    nextIndex += indexDeltaY;
                }

                if (_nodes[nextIndex].Score < _startScore)
                {
                    return parentIndex;
                }
                parentIndex = nextIndex;
            }
        }

        return _bestIndex;
    }

    public Score CalculateScore(float pixMaxG, float pathMinG, float pathLeeway, int pixelIndex)
    {
        var destSafe = pixMaxG == float.MaxValue;
        var pathSafe = pathLeeway > 0;
        var destBetter = pixMaxG > _startMaxG;

        if (destSafe && pathSafe)
        {
            ref readonly var prio = ref _map.PixelPriority[pixelIndex];
            return prio == _map.MaxPriority ? Score.SafeMaxPrio : prio > _startPrio ? Score.SafeBetterPrio : Score.Safe;
        }

        if (pathMinG == _startMaxG) // TODO: some small threshold? should be solved by preprocessing...
            return pathSafe
                ? (destBetter ? Score.SemiSafeImprove : Score.SemiSafeAsStart) // note: if pix.MaxG is < _startMaxG, then PathMinG will be < too
                : (destBetter ? Score.UnsafeImprove : Score.UnsafeAsStart);

        return destSafe ? Score.UltimatelySafe : destBetter ? Score.UltimatelyBetter : Score.JustBad;
    }

    // return a 'score' difference: 0 if identical, -1 if left is somewhat better, -2 if left is significantly better, +1/+2 when right is better
    public static int CompareNodeScores(ref Node nodeL, ref Node nodeR)
    {
        if (nodeL.Score != nodeR.Score)
            return nodeL.Score > nodeR.Score ? -2 : +2;

        // TODO: should we use leeway here or distance?..
        //return nodeL.PathLeeway > nodeR.PathLeeway;
        var gl = nodeL.GScore;
        var gr = nodeR.GScore;
        var fl = gl + nodeL.HScore;
        var fr = gr + nodeR.HScore;
        if (fl + Epsilon < fr)
            return -1;
        else if (fr + Epsilon < fl)
            return +1;
        else if (gl != gr)
            return gl > gr ? -1 : 1; // tie-break towards larger g-values
        else
            return 0;
    }

    public bool LineOfSight(int x0, int y0, int x1, int y1, float parentGScore, out float lineOfSightLeeway, out float lineOfSightDist, out float lineOfSightMinG)
    {
        lineOfSightLeeway = float.MaxValue;
        lineOfSightMinG = float.MaxValue;
        var cumulativeG = parentGScore;

        var dx = x1 - x0;
        var dy = y1 - y0;

        var shiftdx = dx >> 31;
        var shiftdy = dy >> 31;

        var stepX = dx == 0 ? 0 : (shiftdx | 1); // Sign of dx
        var stepY = dy == 0 ? 0 : (shiftdy | 1); // Sign of dy

        dx = (dx ^ shiftdx) - shiftdx;  // Absolute value of dx
        dy = (dy ^ shiftdy) - shiftdy;  // Absolute value of dy

        lineOfSightDist = MathF.Sqrt(dx * dx + dy * dy);

        var invdx = dx != 0 ? 1f / dx : float.MaxValue;
        var invdy = dy != 0 ? 1f / dy : float.MaxValue;

        var tMaxX = _mapHalfResolution * invdx;
        var tMaxY = _mapHalfResolution * invdy;
        var tDeltaX = _mapResolution * invdx;
        var tDeltaY = _mapResolution * invdy;

        var x = x0;
        var y = y0;

        while (true)
        {
            ref var maxG = ref _map.PixelMaxG[y * _map.Width + x];

            // If this pixel is considered impassable
            if (maxG <= 0)
                return false;

            // Update the minG we have seen on this line
            if (maxG < lineOfSightMinG)
                lineOfSightMinG = maxG;

            // Update the path leeway along this line
            var thisLeeway = maxG - cumulativeG;
            if (thisLeeway < lineOfSightLeeway)
                lineOfSightLeeway = thisLeeway;

            // Check if we're finished
            if (x == x1 && y == y1)
                break;

            // Otherwise pick which direction to step
            if (tMaxX < tMaxY)
            {
                tMaxX += tDeltaX;
                x += stepX;
                cumulativeG += _deltaGSide;
            }
            else if (tMaxY < tMaxX)
            {
                tMaxY += tDeltaY;
                y += stepY;
                cumulativeG += _deltaGSide;
            }
            else
            {
                // stepping diagonally
                tMaxX += tDeltaX;
                tMaxY += tDeltaY;
                x += stepX;
                y += stepY;
                cumulativeG += _deltaGDiag;
            }
        }

        // If we made it out of the loop, line of sight is good
        return true;
    }

    private void VisitNeighbour(int parentIndex, int nodeX, int nodeY, int nodeIndex, float deltaGrid)
    {
        ref var currentParentNode = ref _nodes[parentIndex];
        ref var destNode = ref _nodes[nodeIndex];

        if (destNode.OpenHeapIndex < 0 && destNode.Score >= Score.SemiSafeAsStart)
            return;

        ref var destPixG = ref _map.PixelMaxG[nodeIndex];
        ref var parentPixG = ref _map.PixelMaxG[parentIndex];
        if (destPixG < 0f && parentPixG >= 0f)
            return; // impassable

        var stepCost = deltaGrid; // either _deltaGSide or _deltaGDiag
        var candidateG = currentParentNode.GScore + stepCost;

        var candidateLeeway = MathF.Min(currentParentNode.PathLeeway, Math.Min(destPixG, parentPixG) - candidateG);
        var candidateMinG = MathF.Min(currentParentNode.PathMinG, destPixG);

        var altNode = new Node
        {
            GScore = candidateG,
            HScore = destNode.HScore, // or init if first time
            ParentIndex = parentIndex,
            OpenHeapIndex = destNode.OpenHeapIndex,
            PathLeeway = candidateLeeway,
            PathMinG = candidateMinG,
            Score = CalculateScore(destPixG, candidateMinG, candidateLeeway, nodeIndex)
        };

        if (currentParentNode.Score == Score.Safe && altNode.Score == Score.JustBad) // don't leave safe cells if it requires going through bad cells
            return;

        var grandParentIndex = currentParentNode.ParentIndex;

        if (grandParentIndex != nodeIndex && _nodes[grandParentIndex].PathMinG >= currentParentNode.PathMinG)
        {
            var (gx, gy) = _map.IndexToGrid(grandParentIndex);

            // Attempt to see if we can go directly from grandparent to (nodeX, nodeY)
            if (LineOfSight(gx, gy, nodeX, nodeY, _nodes[grandParentIndex].GScore, out var losLeeway, out var losDist, out var losMinG))
            {
                var losScore = CalculateScore(destPixG, losMinG, losLeeway, nodeIndex);
                if (losScore > altNode.Score || losScore == altNode.Score && losLeeway >= (losScore >= Score.Safe ? 0 : altNode.PathLeeway))
                {
                    parentIndex = grandParentIndex;
                    altNode.GScore = _nodes[parentIndex].GScore + _deltaGSide * losDist;
                    altNode.ParentIndex = grandParentIndex;
                    altNode.PathLeeway = losLeeway;
                    altNode.PathMinG = losMinG;
                    altNode.Score = losScore;
                }
            }
        }

        var visit = destNode.OpenHeapIndex == 0 || CompareNodeScores(ref altNode, ref destNode) < (destNode.OpenHeapIndex < 0 ? -1 : 0);
        if (visit)
        {
            if (destNode.OpenHeapIndex < 0)
                ++NumReopens;
            destNode = altNode;
            AddToOpen(nodeIndex);
        }
    }

    private void PrefillH()
    {
        var width = _map.Width;
        var hight = _map.Height;
        var maxPriority = _map.MaxPriority;
        var iCell = 0;
        for (var y = 0; y < hight; ++y)
        {
            for (var x = 0; x < width; ++x, ++iCell)
            {
                if (_map.PixelPriority[iCell] < maxPriority)
                {
                    ref var node = ref _nodes[iCell];
                    node.HScore = float.MaxValue;
                    if (x > 0)
                        UpdateHNeighbour(x, y, ref node, x - 1, y, iCell - 1);
                    if (y > 0)
                        UpdateHNeighbour(x, y, ref node, x, y - 1, iCell - width);
                }
                // else: leave unfilled (H=0, parent=uninit)
            }
        }
        --iCell;
        for (int y0 = hight - 1, y = y0; y >= 0; --y)
        {
            for (int x0 = width - 1, x = x0; x >= 0; --x, --iCell)
            {
                if (_map.PixelPriority[iCell] < maxPriority)
                {
                    ref var node = ref _nodes[iCell];
                    if (x < x0)
                        UpdateHNeighbour(x, y, ref node, x + 1, y, iCell + 1);
                    if (y < y0)
                        UpdateHNeighbour(x, y, ref node, x, y + 1, iCell + width);
                }
            }
        }
    }

    private void UpdateHNeighbour(int x1, int y1, ref Node node, int x2, int y2, int neighIndex)
    {
        ref var neighbour = ref _nodes[neighIndex];
        if (neighbour.HScore == 0)
        {
            node.HScore = _deltaGSide; // don't bother with min, it can't be lower
            node.ParentIndex = neighIndex;
        }
        else if (neighbour.HScore < float.MaxValue)
        {
            (x2, y2) = _map.IndexToGrid(neighbour.ParentIndex);
            var dx = x2 - x1;
            var dy = y2 - y1;
            var hScore = _deltaGSide * MathF.Sqrt(dx * dx + dy * dy);
            if (hScore < node.HScore)
            {
                node.HScore = hScore;
                node.ParentIndex = neighbour.ParentIndex;
            }
        }
    }

    private void AddToOpen(int nodeIndex)
    {
        if (_nodes[nodeIndex].OpenHeapIndex <= 0)
        {
            _openList.Add(nodeIndex);
            _nodes[nodeIndex].OpenHeapIndex = _openList.Count;
        }
        // update location
        PercolateUp(_nodes[nodeIndex].OpenHeapIndex - 1);
    }

    // remove first (minimal) node from open heap and mark as closed
    private int PopMinOpen()
    {
        var nodeIndex = _openList[0];
        _openList[0] = _openList[^1];
        _nodes[nodeIndex].OpenHeapIndex = -1;
        _openList.RemoveAt(_openList.Count - 1);
        if (_openList.Count > 0)
        {
            _nodes[_openList[0]].OpenHeapIndex = 1;
            PercolateDown(0);
        }
        return nodeIndex;
    }

    private void PercolateUp(int heapIndex)
    {
        var openSpan = _openList.AsSpan();
        int nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];
        while (heapIndex > 0)
        {
            int parentHeapIndex = (heapIndex - 1) >> 1;
            ref int parentNodeIndex = ref openSpan[parentHeapIndex];
            ref var parent = ref _nodes[parentNodeIndex];
            if (CompareNodeScores(ref node, ref parent) >= 0)
                break; // parent is 'less' (same/better), stop

            openSpan[heapIndex] = parentNodeIndex;
            parent.OpenHeapIndex = heapIndex + 1;
            heapIndex = parentHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }

    private void PercolateDown(int heapIndex)
    {
        var openSpan = _openList.AsSpan();
        int nodeIndex = openSpan[heapIndex];
        ref var node = ref _nodes[nodeIndex];

        int maxSize = openSpan.Length;
        while (true)
        {
            // find 'better' child
            int childHeapIndex = (heapIndex << 1) + 1;
            if (childHeapIndex >= maxSize)
                break; // node is already a leaf

            int childNodeIndex = openSpan[childHeapIndex];
            ref var child = ref _nodes[childNodeIndex];
            int altChildHeapIndex = childHeapIndex + 1;
            if (altChildHeapIndex < maxSize)
            {
                int altChildNodeIndex = openSpan[altChildHeapIndex];
                ref var altChild = ref _nodes[altChildNodeIndex];
                if (CompareNodeScores(ref altChild, ref child) < 0)
                {
                    childHeapIndex = altChildHeapIndex;
                    childNodeIndex = altChildNodeIndex;
                    child = ref altChild;
                }
            }

            if (CompareNodeScores(ref node, ref child) < 0)
                break; // node is better than best child, so should remain on top

            openSpan[heapIndex] = childNodeIndex;
            child.OpenHeapIndex = heapIndex + 1;
            heapIndex = childHeapIndex;
        }
        openSpan[heapIndex] = nodeIndex;
        node.OpenHeapIndex = heapIndex + 1;
    }
}
