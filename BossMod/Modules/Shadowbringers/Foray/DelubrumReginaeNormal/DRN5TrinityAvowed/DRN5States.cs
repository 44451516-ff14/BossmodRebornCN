namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class DRN5TrinityAvowedStates : StateMachineBuilder
{
    public DRN5TrinityAvowedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<PlayerTemperatures>()
            .ActivateOnEnter<GloryOfBozja>()
            .ActivateOnEnter<WrathOfBozja>()
            .ActivateOnEnter<ElementalImpact>()
            .ActivateOnEnter<ElementalImpactTemperature>()
            .ActivateOnEnter<ShimmeringShot>()
            .ActivateOnEnter<AllegiantArsenal>()
            .ActivateOnEnter<BladeOfEntropy>()
            .ActivateOnEnter<GleamingArrow>();
    }
}