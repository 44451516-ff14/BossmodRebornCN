namespace BossMod;

// tree describing all phases, states and transitions
public sealed class StateMachineTree
{
    public sealed class Node
    {
        public float Time; // time from phase start to state transition, assuming all states last exactly for expected duration
        public int PhaseID;
        public int BranchID;
        public int NumBranches; // how many branches are reachable from this node
        public bool InGroup;
        public bool BossIsCasting;
        public bool IsDowntime;
        public bool IsPositioning;
        public bool IsVulnerable;
        public StateMachine.State State;
        public Node? Predecessor;
        public readonly List<Node> Successors = [];

        internal Node(float t, int phaseID, int branchID, StateMachine.State state, StateMachine.Phase phase, Node? pred)
        {
            Time = t;
            PhaseID = phaseID;
            BranchID = branchID;
            if (pred == null)
            {
                InGroup = true;
                BossIsCasting = IsPositioning = IsVulnerable = false;
                IsDowntime = (phase.Hint & StateMachine.PhaseHint.StartWithDowntime) != 0;
            }
            else
            {
                var predEndHint = pred.State.EndHint;
                InGroup = (predEndHint & StateMachine.StateHint.GroupWithNext) != 0;
                BossIsCasting = (pred.BossIsCasting || (predEndHint & StateMachine.StateHint.BossCastStart) != 0) && (predEndHint & StateMachine.StateHint.BossCastEnd) == 0;
                IsDowntime = (pred.IsDowntime || (predEndHint & StateMachine.StateHint.DowntimeStart) != 0) && (predEndHint & StateMachine.StateHint.DowntimeEnd) == 0;
                IsPositioning = (pred.IsPositioning || (predEndHint & StateMachine.StateHint.PositioningStart) != 0) && (predEndHint & StateMachine.StateHint.PositioningEnd) == 0;
                IsVulnerable = (pred.IsVulnerable || (predEndHint & StateMachine.StateHint.VulnerableStart) != 0) && (predEndHint & StateMachine.StateHint.VulnerableEnd) == 0;
            }
            State = state;
            Predecessor = pred;
        }
    }

    public sealed class Phase
    {
        public string Name;
        public Node StartingNode;
        public float StartTime; // time from pull to phase start
        public float MaxTime; // max state machine duration
        public float Duration; // expected duration

        internal Phase(StateMachine.Phase phase, Node startingNode, float maxTime)
        {
            Name = phase.Name;
            StartingNode = startingNode;
            MaxTime = maxTime;
            Duration = phase.ExpectedDuration >= 0 ? phase.ExpectedDuration : maxTime;
        }

        // return sequential list of nodes belonging to the single branch
        public List<Node> BranchNodes(int branchOffset)
        {
            if (branchOffset < 0 || branchOffset >= StartingNode.NumBranches)
            {
                return [];
            }

            List<Node> nodes = [with(64), StartingNode];
            var n = StartingNode;
            var branchid = StartingNode.BranchID + branchOffset;
            while (n.Successors.Count > 0)
            {
                var successors = n.Successors;
                var count = successors.Count;
                var nextIndex = -1;

                for (var i = 0; i < count; ++i)
                {
                    if (successors[i].BranchID > branchid)
                    {
                        nextIndex = i;
                        break;
                    }
                }

                if (nextIndex == -1)
                {
                    nextIndex = count;
                }

                n = successors[nextIndex - 1];
                nodes.Add(n);
            }
            return nodes;
        }

        public Node TimeToBranchNode(int branchOffset, float t)
        {
            Node? last = null;
            var nodes = BranchNodes(branchOffset);
            var count = nodes.Count;
            for (var i = 0; i < count; ++i)
            {
                var n = nodes[i];
                if (n.Time >= t)
                {
                    return n;
                }

                last = n;
            }
            return last!;
        }
    }

    public readonly Dictionary<uint, Node> Nodes = [];

    public readonly List<Phase> Phases = [];

    public int TotalBranches;
    public float TotalMaxTime;

    public StateMachineTree(StateMachine sm)
    {
        var count = sm.Phases.Count;
        for (var i = 0; i < count; ++i)
        {
            var phase = sm.Phases[i];
            var (startingNode, maxTime) = LayoutNodeAndSuccessors(0f, i, TotalBranches, phase.InitialState, phase, null);
            Phases.Add(new(phase, startingNode, maxTime));
            TotalBranches += startingNode.NumBranches;
            TotalMaxTime = Math.Max(TotalMaxTime, maxTime);
        }
    }

    public void ApplyTimings(List<float>? phaseDurations)
    {
        var count = Phases.Count;
        if (count == 0)
        {
            return;
        }

        if (phaseDurations != null)
        {
            var duraCount = phaseDurations.Count;
            var phasesCount = count < duraCount ? count : duraCount;
            for (var pi = 0; pi < phasesCount; ++pi)
            {
                var p = Phases[pi];
                p.Duration = Math.Min(phaseDurations[pi], p.MaxTime);
            }
        }

        for (var i = 1; i < count; ++i)
        {
            var phasesm1 = Phases[i - 1];
            Phases[i].StartTime = phasesm1.StartTime + phasesm1.Duration;
        }

        var lastPhase = Phases[^1];
        TotalMaxTime = lastPhase.StartTime + lastPhase.Duration;
    }

    // find phase index that corresponds to specified time; assumes ApplyTimings was called before
    public int FindPhaseAtTime(float t)
    {
        var count = Phases.Count;
        var next = -1;
        for (var i = 0; i < count; ++i)
        {
            var p = Phases[i];
            if (p.StartTime > t)
            {
                next = i;
                break;
            }
        }
        return next switch
        {
            < 0 => count - 1,
            0 => 0,
            _ => next - 1
        };
    }

    public (Node, float) PhaseTimeToNodeAndDelay(float t, int phaseIndex, List<int> phaseBranches)
    {
        var node = Phases[phaseIndex].TimeToBranchNode(phaseBranches[phaseIndex], t);
        return (node, t - (node.Predecessor?.Time ?? 0));
    }

    public (Node, float) AbsoluteTimeToNodeAndDelay(float t, List<int> phaseBranches)
    {
        var phaseIndex = FindPhaseAtTime(t);
        return PhaseTimeToNodeAndDelay(t - Phases[phaseIndex].StartTime, phaseIndex, phaseBranches);
    }

    private (Node, float) LayoutNodeAndSuccessors(float t, int phaseID, int branchID, StateMachine.State state, StateMachine.Phase phase, Node? pred)
    {
        var node = Nodes[state.ID] = new Node(t + state.Duration, phaseID, branchID, state, phase, pred);
        float succDuration = 0;
        var states = state.NextStates;
        if (states?.Length > 0)
        {
            var len = states.Length;
            for (var i = 0; i < len; ++i)
            {
                var s = states[i];
                var (succ, dur) = LayoutNodeAndSuccessors(t + state.Duration, phaseID, branchID + node.NumBranches, s, phase, node);
                node.Successors.Add(succ);
                succDuration = Math.Max(succDuration, dur);
                node.NumBranches += succ.NumBranches;
            }
        }
        else
        {
            ++node.NumBranches; // leaf
        }

        return (node, state.Duration + succDuration);
    }
}
