namespace BossMod.Endwalker.Savage.P8S2;

sealed class EgoDeath(BossModule module) : BossComponent(module)
{
    public BitMask InEventMask;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.InEvent)
            InEventMask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.InEvent)
            InEventMask.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
