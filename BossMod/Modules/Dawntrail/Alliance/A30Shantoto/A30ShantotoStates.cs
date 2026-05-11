namespace BossMod.Dawntrail.Alliance.A30Shantoto;

sealed class A30ShantotoStates : StateMachineBuilder
{
    public A30ShantotoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlarePlay>()
            .ActivateOnEnter<Vidohunir>()
            .ActivateOnEnter<EmpiricalResearch>()
            .ActivateOnEnter<SuperiorStoneIITelegraph>()
            .ActivateOnEnter<SuperiorStoneIIArena>()
            .ActivateOnEnter<GroundBreakingQuake>()
            .ActivateOnEnter<CircumscribedFire>();
    }
}
