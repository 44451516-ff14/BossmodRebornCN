namespace BossMod.Dawntrail.Alliance.A31AlZahbi;

sealed class A31AlZahbiStates : StateMachineBuilder
{
    public A31AlZahbiStates(BossModule module) : base(module)
    {
        TrivialPhase()
        .Raw.Update = () => AllDeadOrDestroyed(A31AlZahbi.AlZahbiMobs);
    }
}
