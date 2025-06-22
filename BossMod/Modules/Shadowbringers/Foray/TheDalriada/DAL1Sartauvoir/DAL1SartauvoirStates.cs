namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

sealed class DAL1SartauvoirStates : StateMachineBuilder
{
    public DAL1SartauvoirStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PyrokinesisAOE>()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<ThermalGustAOE>()
            .ActivateOnEnter<GrandCrossflameAOE>()
            .ActivateOnEnter<Flamedive>()
            .ActivateOnEnter<BurningBlade>()
            .ActivateOnEnter<MannatheihwonFlameRW>()
            .ActivateOnEnter<MannatheihwonFlameRect>()
            .ActivateOnEnter<MannatheihwonFlameCircle>()
            .ActivateOnEnter<Brand>()
            .ActivateOnEnter<Pyroclysm>()
            .ActivateOnEnter<Pyrocrisis>()
            .ActivateOnEnter<Pyrodoxy>();
    }
}
