namespace BossMod.Dawntrail.Trial.T03QueenEternal;

class DivideAndConquer(BossModule module) : Components.GenericBaitAway(module)
{
    // for some reason the icon is detected on the boss instead of the player
    // so we will have to make a hack, line baits can be staggered so we can't use BaitAwayIcon which clears all at the same time
    private static readonly AOEShapeRect rect = new(100, 2.5f);

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.LineBaits && CurrentBaits.Count == 0)
            foreach (var p in Raid.WithoutSlot())
                CurrentBaits.Add(new(Module.PrimaryActor, p, rect, WorldState.FutureTime(3)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count == 0)
            return;
        if ((AID)spell.Action.ID == AID.DivideAndConquer) // this is a hack and could remove a wrong or no line bait...
            CurrentBaits.RemoveAll(x => x.Target == Raid.WithoutSlot().FirstOrDefault(x => x.Position.InRect(Module.PrimaryActor.Position, spell.Rotation, rect.LengthFront, 0, 2)));
        else if ((AID)spell.Action.ID == AID.AutoAttack) // safeguard, might be needed if target dies before cast event or if no target was found in rect
            CurrentBaits.Clear();
    }
}
