namespace BossMod.Shadowbringers.Alliance.A34XunZiMengZi;

sealed class A34XunZiMengZiStates : StateMachineBuilder
{
    public A34XunZiMengZiStates(A34XunZiMengZi module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeployArmaments>()
            .ActivateOnEnter<UniversalAssault>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<HighPoweredLaser>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && (module.BossMengZi?.IsDeadOrDestroyed ?? true);
    }
}
