namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class StockBreak(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            Stacks.Add(new(actor, 6f, 8, 8, WorldState.FutureTime(8.2d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.StockBreak1 or (uint)AID.StockBreak4)
            ++NumCasts;
    }
}
