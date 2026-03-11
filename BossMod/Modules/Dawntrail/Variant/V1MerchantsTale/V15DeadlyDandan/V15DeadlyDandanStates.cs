namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V15DeadlyDandan;

class V15DeadlyDandanStates : StateMachineBuilder
{
    public V15DeadlyDandanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MurkyWaters>()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<Spit>()
            .ActivateOnEnter<Dropsea>()
            .ActivateOnEnter<AiryBubbles>()
            .ActivateOnEnter<MawOfTheDeep>()
            .ActivateOnEnter<TidalGuillotine>()
            .ActivateOnEnter<UnfathomableHorror>()
            .ActivateOnEnter<SwallowedSea>()
            .ActivateOnEnter<StingingTentacle>();
    }
}
