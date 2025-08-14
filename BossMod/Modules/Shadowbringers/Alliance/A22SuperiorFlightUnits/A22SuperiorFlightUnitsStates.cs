namespace BossMod.Shadowbringers.Alliance.A22SuperiorFlightUnits;

sealed class A22SuperiorFlightUnitsStates : StateMachineBuilder
{
    public A22SuperiorFlightUnitsStates(A22SuperiorFlightUnits module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShieldProtocol>()
            .ActivateOnEnter<IncendiaryBombing>()
            .ActivateOnEnter<IncendiaryBombingBait>()
            .ActivateOnEnter<StandardSurfaceMissile>()
            .ActivateOnEnter<LethalRevolution>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<SurfaceMissileHighOrderExplosiveBlastCircle>()
            .ActivateOnEnter<HighOrderExplosiveBlastCross>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<PrecisionGuidedMissile>()
            .ActivateOnEnter<ManeuverHighPoweredLaser>()
            .ActivateOnEnter<ManeuverMissileCommand>()
            .ActivateOnEnter<SharpTurn>()
            .ActivateOnEnter<SlidingSwipe>()
            .ActivateOnEnter<IncendiaryBarrage>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.BossBeta?.IsDeadOrDestroyed ?? true) && (module.BossChi?.IsDeadOrDestroyed ?? true);
    }
}
