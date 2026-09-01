namespace BossMod.Dawntrail.Alliance.A30Shantoto;

[SkipLocalsInit]
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
            .ActivateOnEnter<DiagrammaticDoorway>()
            .ActivateOnEnter<LocalizedBlizzard>()
            .ActivateOnEnter<ThunderAndError>()
            .ActivateOnEnter<SmallSpecimen>()
            .ActivateOnEnter<LargeSpecimen>()
            .ActivateOnEnter<StardustSpecimen>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<FallingRubble>()
            .ActivateOnEnter<FallingRubble1>()
            .ActivateOnEnter<FallingRubble2>()
            .ActivateOnEnter<FallingRubble3>()
            .ActivateOnEnter<AeroDynamics>()
            .ActivateOnEnter<FinalExam>()
            .Raw.Update = () => Module.PrimaryActor is var primary && primary.IsDeadOrDestroyed || primary.HPMP.CurHP <= 1u;
    }
}
