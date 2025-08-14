namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

sealed class A23HeavyArtilleryUnitStates : StateMachineBuilder
{
    public A23HeavyArtilleryUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManeuverVoltArray>()
            .ActivateOnEnter<LowerLaser>()
            .ActivateOnEnter<UpperLaser>()
            .ActivateOnEnter<EnergyBombardment>()
            .ActivateOnEnter<ManeuverHighPoweredLaser>()
            .ActivateOnEnter<UnconventionalVoltage>()
            .ActivateOnEnter<ManeuverImpactCrusherRevolvingLaser>()
            .ActivateOnEnter<R010Laser>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<ChemicalBurn>()
            .ActivateOnEnter<R030Hammer>();
    }
}
