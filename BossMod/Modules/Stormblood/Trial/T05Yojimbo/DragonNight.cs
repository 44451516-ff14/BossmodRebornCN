namespace BossMod.Stormblood.Trial.T05Yojimbo;

class DragonNight(BossModule module) : BossComponent(module)
{
    private DateTime _hintExpire;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x008F && actor.OID == (uint)OID.DragonsHead)
            _hintExpire = WorldState.FutureTime(5.5d);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (WorldState.CurrentTime < _hintExpire)
            hints.Add("Raidwide after dragon heads leave arena");
    }
}
