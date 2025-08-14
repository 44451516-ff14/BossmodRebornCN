namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V15ThorneKnight;

sealed class V15ThorneKnightStates : StateMachineBuilder
{
    public V15ThorneKnightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpringToLife>()
            .ActivateOnEnter<BlisteringBlow>()
            .ActivateOnEnter<BlazingBeacon>()
            .ActivateOnEnter<SignalFlare>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<SacredFlay>()
            .ActivateOnEnter<ForeHonor>()
            .ActivateOnEnter<Cogwheel>();
    }
}
