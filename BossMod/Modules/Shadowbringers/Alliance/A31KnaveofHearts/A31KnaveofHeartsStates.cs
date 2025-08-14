namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

sealed class A31KnaveofHeartsStates : StateMachineBuilder
{
    public A31KnaveofHeartsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ColossalImpact>()
            .ActivateOnEnter<MagicArtilleryBeta>()
            .ActivateOnEnter<MagicArtilleryAlpha>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<LightLeap>()
            .ActivateOnEnter<BoxSpawn>()
            .ActivateOnEnter<MagicBarrage>()
            .ActivateOnEnter<Lunge>();
    }
}
