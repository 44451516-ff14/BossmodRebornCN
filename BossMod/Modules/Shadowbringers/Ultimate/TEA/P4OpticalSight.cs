namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P4OpticalSight(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 4)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.OpticalSightSpread:
                AddSpread(actor, WorldState.FutureTime(5.1d));
                break;
            case (uint)IconID.OpticalSightStack:
                AddStack(actor, WorldState.FutureTime(5.1d));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.IndividualReprobation:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
            case (uint)AID.CollectiveReprobation:
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }
}
