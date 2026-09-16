namespace BossMod.Endwalker.Savage.P6SHegemone;

sealed class Agonies(BossModule module) : Components.UniformStackSpread(module, 6f, 15f, 3)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AgoniesDarkburst1:
            case (uint)AID.AgoniesDarkburst2:
            case (uint)AID.AgoniesDarkburst3:
                if (WorldState.Actors.Find(spell.TargetID) is var spreadTarget && spreadTarget != null)
                    AddSpread(spreadTarget);
                break;
            case (uint)AID.AgoniesUnholyDarkness1:
            case (uint)AID.AgoniesUnholyDarkness2:
            case (uint)AID.AgoniesUnholyDarkness3:
                if (WorldState.Actors.Find(spell.TargetID) is var stackTarget && stackTarget != null)
                    AddStack(stackTarget);
                break;
            case (uint)AID.AgoniesDarkPerimeter1:
            case (uint)AID.AgoniesDarkPerimeter2:
                // don't really care about donuts, they auto resolve...
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AgoniesDarkburst1:
            case (uint)AID.AgoniesDarkburst2:
            case (uint)AID.AgoniesDarkburst3:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
                break;
            case (uint)AID.AgoniesUnholyDarkness1:
            case (uint)AID.AgoniesUnholyDarkness2:
            case (uint)AID.AgoniesUnholyDarkness3:
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
                break;
            case (uint)AID.AgoniesDarkPerimeter1:
            case (uint)AID.AgoniesDarkPerimeter2:
                // don't really care about donuts, they auto resolve...
                break;
        }
    }
}
