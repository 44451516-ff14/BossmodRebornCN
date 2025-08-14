namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

sealed class DAL3SaunionDawonStates : StateMachineBuilder
{
    public DAL3SaunionDawonStates(DAL3SaunionDawon module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MobileHaloCrossray>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MagitekHalo>()
            .ActivateOnEnter<MagitekCrossray>()
            .ActivateOnEnter<MissileSalvo>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<HighPoweredMagitekRay>()
            .Raw.Update = () => module.PrimaryActor.IsDestroyed || (module.BossDawon?.IsTargetable ?? false);
        TrivialPhase(1u)
            .ActivateOnEnter<VerdantPlumeVermilionFlame>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<SwoopingFrenzy>()
            .ActivateOnEnter<Pentagust>()
            .ActivateOnEnter<ToothAndTalon>()
            .ActivateOnEnter<WildfireWinds>()
            .ActivateOnEnter<Obey>()
            .ActivateOnEnter<SpiralScourge>()
            .ActivateOnEnter<OneMind>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.BossDawon?.IsDeadOrDestroyed ?? true);
    }
}
