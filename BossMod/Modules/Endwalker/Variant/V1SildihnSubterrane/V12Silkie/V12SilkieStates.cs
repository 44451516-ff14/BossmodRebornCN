namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V12Silkie;

sealed class V12SilkieStates : StateMachineBuilder
{
    public V12SilkieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DustBluster>()
            .ActivateOnEnter<WashOut>()
            .ActivateOnEnter<SilkenPuff>()
            .ActivateOnEnter<BracingDuster>()
            .ActivateOnEnter<ChillingDuster>()
            .ActivateOnEnter<SlipperySoap>()
            .ActivateOnEnter<SpotRemover>()
            .ActivateOnEnter<SqueakyCleanConeSmall>()
            .ActivateOnEnter<SqueakyCleanConeBig>()
            .ActivateOnEnter<PuffAndTumble>()
            .ActivateOnEnter<CarpetBeater>()
            .ActivateOnEnter<EasternEwers>()
            .ActivateOnEnter<TotalWashDustBluster>();
    }
}
