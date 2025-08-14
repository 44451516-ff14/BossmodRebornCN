namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V25Enenra;

sealed class V25EnenraStates : StateMachineBuilder
{
    public V25EnenraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<PipeCleaner>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<Snuff>()
            .ActivateOnEnter<Smoldering>()
            .ActivateOnEnter<IntoTheFire>()
            .ActivateOnEnter<FlagrantCombustion>()
            .ActivateOnEnter<SmokeRings>()
            .ActivateOnEnter<ClearingSmoke>()
            .ActivateOnEnter<StringRock>();
    }
}
