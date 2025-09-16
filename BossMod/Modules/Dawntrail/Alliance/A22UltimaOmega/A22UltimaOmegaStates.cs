namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class A22UltimaOmegaStates : StateMachineBuilder
{
    public A22UltimaOmegaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IonEffluxCitadelBusterHyperPulseChemicalBomb>()
            .ActivateOnEnter<Antimatter>()
            .ActivateOnEnter<OmegaBlaster>()
            .ActivateOnEnter<EnergyRay>()
            .ActivateOnEnter<CitadelSiege>()
            .ActivateOnEnter<CitadelSiegeHint>()
            .ActivateOnEnter<MultiMissileBig>()
            .ActivateOnEnter<MultiMissileSmall>()
            .ActivateOnEnter<Crash>()
            .ActivateOnEnter<TractorBeam>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<ChemicalBombHyperPulse>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<TrajectoryProjection>();
    }
}
