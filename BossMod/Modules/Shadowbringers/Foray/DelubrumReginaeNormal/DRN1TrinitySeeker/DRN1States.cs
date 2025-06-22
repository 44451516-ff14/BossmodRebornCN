namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN1TrinitySeeker;

sealed class DRN1TrinitySeekerStates : StateMachineBuilder
{
    public DRN1TrinitySeekerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<MercifulBreeze>()
            .ActivateOnEnter<MercifulBlooms>()
            .ActivateOnEnter<MercifulArc>()
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<IronImpact>()
            .ActivateOnEnter<ActOfMercy>()
            .ActivateOnEnter<BalefulBlade>()
            .ActivateOnEnter<BalefulSwathe>()
            .ActivateOnEnter<IronSplitter>()
            .ActivateOnEnter<MercyFourfold>()
            .ActivateOnEnter<MercifulMoon>()
            .ActivateOnEnter<DeadIron>();
    }
}
