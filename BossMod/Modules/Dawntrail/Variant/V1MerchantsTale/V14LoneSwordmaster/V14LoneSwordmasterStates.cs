namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V14LoneSwordmaster;

class V14LoneSwordmasterStates : StateMachineBuilder
{
    public V14LoneSwordmasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DebuffTracker>()
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<WillOfTheUnderworld>()
            .ActivateOnEnter<EarthRendingEight>()
            .ActivateOnEnter<EarthRendingEightCross>()
            .ActivateOnEnter<WaitingWounds>()
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<HeavensConfluenceIcon>()
            .ActivateOnEnter<CrusherOfLions>()
            .ActivateOnEnter<UnyieldingWill>()
            .ActivateOnEnter<SteelsbreathRelease>()
            .ActivateOnEnter<SteelsbreathRelease1>()
            .ActivateOnEnter<Concentrativity>()
            .ActivateOnEnter<ConcentrativityKnockback>()
            .ActivateOnEnter<ConcentravityMagnetFloor>()
            .ActivateOnEnter<ConcentrativityRocks>()
            .ActivateOnEnter<MawOfTheWolf>()
            .ActivateOnEnter<StingOfTheScorpion>()
            .ActivateOnEnter<PlummetSmall>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<PlummetBig>()
            .ActivateOnEnter<PlummetProximity>()
            .ActivateOnEnter<WillOfTheUnderworldRocks>();
    }
}
