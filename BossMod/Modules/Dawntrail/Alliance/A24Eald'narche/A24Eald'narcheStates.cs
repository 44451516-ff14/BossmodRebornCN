namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

sealed class A24EaldnarcheStates : StateMachineBuilder
{
    public A24EaldnarcheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<UranosCascade>()
            .ActivateOnEnter<CronosSlingRect>()
            .ActivateOnEnter<CronosSlingCircle>()
            .ActivateOnEnter<CronosSlingDonut>()
            .ActivateOnEnter<EmpyrealVortexOmegaJavelin>()
            .ActivateOnEnter<EmpyrealVortexSpread>()
            .ActivateOnEnter<EmpyrealVortexRW>()
            .ActivateOnEnter<Sleepga>()
            .ActivateOnEnter<Sleep>()
            .ActivateOnEnter<GaeaStream>()
            .ActivateOnEnter<OmegaJavelinSpread>()
            .ActivateOnEnter<Duplicate>()
            .ActivateOnEnter<StellarBurst>()
            .ActivateOnEnter<QuakeFreeze>()
            .ActivateOnEnter<FloodConcentric>()
            .ActivateOnEnter<FloodProximity>()
            .ActivateOnEnter<TornadoFlareBurst>()
            .ActivateOnEnter<Tornado>()
            .ActivateOnEnter<TornadoPull>()
            .ActivateOnEnter<OrbitalLevin>()
            .ActivateOnEnter<Paralysis>()
            .ActivateOnEnter<FlareRect>();
    }
}
