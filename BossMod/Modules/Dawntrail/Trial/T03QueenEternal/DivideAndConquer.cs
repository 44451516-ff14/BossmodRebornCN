namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class DivideAndConquer(BossModule module) : Components.GenericBaitAway(module)
{
    // for some reason the icon is detected on the boss instead of the player
    // so we will have to make a hack, line baits can be staggered so we can't use BaitAwayIcon which clears all at the same time
    // staggered waves always got 8 casts even if some players are dead, simultan waves got line baits on all alive players
    private static readonly AOEShapeRect rect = new(100, 2.5f);
    private int counter;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.LineBaits && CurrentBaits.Count == 0)
        {
            foreach (var p in Raid.WithoutSlot(true))
                CurrentBaits.Add(new(Module.PrimaryActor, p, rect, WorldState.FutureTime(3)));
            counter = 8;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count == 0)
            return;
        if ((AID)spell.Action.ID == AID.DivideAndConquer)
        {
            if (--counter == 0)
                CurrentBaits.Clear();
            if (++NumCasts > 8)
            {
                CurrentBaits.Clear();
                NumCasts = 0;
            }
        }
    }
}
