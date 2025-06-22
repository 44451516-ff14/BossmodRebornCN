namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

sealed class Duel3SartauvoirStates : StateMachineBuilder
{
    public Duel3SartauvoirStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flashover>()
            .ActivateOnEnter<FlamingRain>()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<Backdraft>()
            .ActivateOnEnter<ThermalGust>()
            .ActivateOnEnter<Flamedive>()
            .ActivateOnEnter<Meltdown>()
            .ActivateOnEnter<SearingWind>()
            .ActivateOnEnter<ThermalWave>()
            .ActivateOnEnter<Pyrolatry>()
            .ActivateOnEnter<PillarOfFlame>()
            .ActivateOnEnter<BioIV>();
    }
}
