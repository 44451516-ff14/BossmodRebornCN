namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

sealed class TrackingTremors(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8)
{
    private int numCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TrackingTremors)
            AddStack(actor, WorldState.FutureTime(5d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TrackingTremors)
        {
            if (++numCasts == 5)
            {
                Stacks.Clear();
                numCasts = 0;
            }
        }
    }
}
