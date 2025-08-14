namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P1Throttle(BossModule module) : BossComponent(module)
{
    public bool Applied;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Throttle)
        {
            Applied = true;
        }
    }
}
