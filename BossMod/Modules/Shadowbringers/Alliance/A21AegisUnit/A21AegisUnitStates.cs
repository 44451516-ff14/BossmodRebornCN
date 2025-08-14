namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

sealed class A21AegisUnitStates : StateMachineBuilder
{
    public A21AegisUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManeuverDiffusionCannon>()
            .ActivateOnEnter<SurfaceLaserAOE>()
            .ActivateOnEnter<SurfaceLaserSpread>()
            .ActivateOnEnter<BeamCannons>()
            .ActivateOnEnter<ColliderCannons>()
            .ActivateOnEnter<RefractionCannons>()
            .ActivateOnEnter<AntiPersonnelLaser>()
            .ActivateOnEnter<FlightPath>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<ManeuverSaturationBombing>()
            .ActivateOnEnter<LifesLastSong>();
    }
}
