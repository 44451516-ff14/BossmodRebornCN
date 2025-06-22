namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class PuddingParty(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PuddingParty)
            AddStack(actor, WorldState.FutureTime(5.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PuddingParty)
        {
            ++NumCasts;
        }
    }
}
