namespace BossMod.Dawntrail.Alliance.A33Awaern;

sealed class A33AwaernStates : StateMachineBuilder
{
    public A33AwaernStates(BossModule module) : base(module)
    {
        TrivialPhase()
        .Raw.Update = () => AllDeadOrDestroyed(A33Awaern.GardenofRuHmetMobs);
    }
}
