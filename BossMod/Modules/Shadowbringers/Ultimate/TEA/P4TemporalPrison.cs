namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: show prison spots, warn if not taken...
sealed class P4TemporalPrison(BossModule module) : BossComponent(module)
{
    public int NumPrisons;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.TemporalPrison)
            ++NumPrisons;
    }
}
