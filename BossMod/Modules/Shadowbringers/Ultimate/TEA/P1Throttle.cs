namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P1Throttle(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Throttle, "Throttle", "throttled")
{
    public bool Applied;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        base.OnStatusGain(actor, ref status);
        if (status.ID == (uint)SID.Throttle)
        {
            Applied = true;
        }
    }
}
