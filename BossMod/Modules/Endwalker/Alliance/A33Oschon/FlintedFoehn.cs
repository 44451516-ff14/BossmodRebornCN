namespace BossMod.Endwalker.Alliance.A33Oschon;

sealed class P1FlintedFoehn(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FlintedFoehnP1AOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FlintedFoehn)
            AddStack(actor, WorldState.FutureTime(5.1d));
    }
}

sealed class P2FlintedFoehn(BossModule module) : Components.UniformStackSpread(module, 8f, default, 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FlintedFoehnP2AOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FlintedFoehn)
            AddStack(actor, WorldState.FutureTime(5.1d));
    }
}
