namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class Sweltering(BossModule module) : BossComponent(module)
{
    public BitMask SwelteringStatus;
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Sweltering)
        {
            SwelteringStatus[Raid.FindSlot(actor.InstanceID)] = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Sweltering)
        {
            SwelteringStatus[Raid.FindSlot(actor.InstanceID)] = false;
        }
    }
}
