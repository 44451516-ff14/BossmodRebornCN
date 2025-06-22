namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class PuddingParty(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    private int numCasts;
    private bool first = true;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.PuddingParty)
            AddStack(actor, WorldState.FutureTime(5.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.PuddingParty)
        {
            ++numCasts;
            if (first && numCasts == 5 || numCasts == 6)
            {
                Stacks.Clear();
                numCasts = 0;
                first = false;
            }
        }
    }
}
