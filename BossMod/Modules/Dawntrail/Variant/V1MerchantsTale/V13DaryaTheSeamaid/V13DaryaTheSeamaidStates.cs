namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V13DaryaTheSeamaid;

class V13DaryaTheSeamaidStates : StateMachineBuilder
{
    public V13DaryaTheSeamaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<EchoedSerenade>()
            .ActivateOnEnter<SurgingCurrent>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<SunkenTreasure>()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<NearFarTide>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<BigWave>()
            .ActivateOnEnter<AlluringOrder>()
            .ActivateOnEnter<AquaSpear>();
    }
}
