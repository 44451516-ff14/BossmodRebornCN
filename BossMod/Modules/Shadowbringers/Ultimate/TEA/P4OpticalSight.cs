namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P4OpticalSight(BossModule module) : Components.UniformStackSpread(module, 6f, 6f, 4, 4)
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
                Spreads.Clear();
                break;
            case (uint)AID.CollectiveReprobation:
                Stacks.Clear();
                break;
        }
    }
}
