namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class A32AlexanderStates : StateMachineBuilder
{
    public A32AlexanderStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
