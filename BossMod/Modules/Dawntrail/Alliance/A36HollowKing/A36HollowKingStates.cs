namespace BossMod.Dawntrail.Alliance.A36HollowKing;

sealed class A36HollowKingStates : StateMachineBuilder
{
    public A36HollowKingStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
