namespace BossMod;

public sealed class ColumnStateMachineBranch(Timeline timeline, StateMachineTree tree, List<int> phaseBranches) : ColumnStateMachine(timeline, tree)
{
    public override void Update() => Width = PixelsPerBranch;

    public override void Draw()
    {
        var phases = Tree.Phases;
        for (var pi = 0; pi < phases.Count && pi < phaseBranches.Count; ++pi)
        {
            var phase = phases[pi];
            var branch = phaseBranches[pi];
            foreach (var node in phase.BranchNodes(branch))
            {
                if (node.Time >= phase.Duration)
                {
                    break;
                }

                DrawNode(node, true);
            }
        }
    }
}
