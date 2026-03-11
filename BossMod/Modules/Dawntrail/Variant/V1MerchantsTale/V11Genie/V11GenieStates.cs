namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V11Genie;

class V11GenieStates : StateMachineBuilder
{
    public V11GenieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FabulousFirecrackersBig>()
            .ActivateOnEnter<FabulousFirecrackersSmall>()
            .ActivateOnEnter<ParadeOfWonders>()
            .ActivateOnEnter<SpectacularSparks>()
            .ActivateOnEnter<Voyage>()
            .ActivateOnEnter<ExplosiveEnding>()
            .ActivateOnEnter<Pyromagicks>()
            .ActivateOnEnter<LampLighting>()
            .ActivateOnEnter<FanningFlame>()
            .ActivateOnEnter<SupernaturalSurprise>()
            .ActivateOnEnter<RubBurn>()
            .ActivateOnEnter<RainbowRoad>()
            .ActivateOnEnter<AetherialBlizzard>()
            .ActivateOnEnter<LampOil>();
    }
}
