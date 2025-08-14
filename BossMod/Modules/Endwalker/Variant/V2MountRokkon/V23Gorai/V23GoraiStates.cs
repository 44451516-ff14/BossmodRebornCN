namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V23Gorai;

sealed class V23GoraiStates : StateMachineBuilder
{
    public V23GoraiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            // Route 5
            .ActivateOnEnter<PureShock>()
            // Route 6
            .ActivateOnEnter<HumbleHammer>()
            .ActivateOnEnter<Thundercall>()
            // Route 7
            .ActivateOnEnter<WorldlyPursuit>()
            .ActivateOnEnter<FightingSpirits>()
            .ActivateOnEnter<BiwaBreaker>()
            // Standard
            .ActivateOnEnter<ImpurePurgation>()
            .ActivateOnEnter<StringSnap>()
            .ActivateOnEnter<SpikeOfFlameAOE>()
            .ActivateOnEnter<FlameAndSulphur>()
            .ActivateOnEnter<TorchingTorment>()
            .ActivateOnEnter<MalformedPrayer>()
            .ActivateOnEnter<Unenlightenment>();
    }
}
