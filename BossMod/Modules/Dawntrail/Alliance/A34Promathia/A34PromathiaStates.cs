namespace BossMod.Dawntrail.Alliance.A34Promathia;

sealed class A34PromathiaStates : StateMachineBuilder
{
    public A34PromathiaStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
