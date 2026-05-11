namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

sealed class A35ShinryuParadoxStates : StateMachineBuilder
{
    public A35ShinryuParadoxStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
