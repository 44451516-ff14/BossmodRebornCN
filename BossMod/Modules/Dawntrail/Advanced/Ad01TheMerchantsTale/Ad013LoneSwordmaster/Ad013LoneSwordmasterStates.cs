namespace BossMod.Dawntrail.Advanced.Ad01MerchantsTale.Ad013LoneSwordmaster;

[SkipLocalsInit]
sealed class Ad013LoneSwordmasterStates : StateMachineBuilder
{
    public Ad013LoneSwordmasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DebuffTracker>()
            .ActivateOnEnter<SteelsbreathRelease>()
            .ActivateOnEnter<MaleficPortent>()
            .ActivateOnEnter<LashOfLight>()
            .ActivateOnEnter<UnyieldingWill>()
            .ActivateOnEnter<HeavensConfluence>()
            .ActivateOnEnter<NearFarFromHeaven>()
            .ActivateOnEnter<WolfsCrossing>()
            .ActivateOnEnter<EchoingHush>()
            .ActivateOnEnter<EchoingHushPuddle>()
            .ActivateOnEnter<EchoingEight>()
            .ActivateOnEnter<StingOfTheScorpion>()
            .ActivateOnEnter<MaleficAlignment>()
            .ActivateOnEnter<WillOfTheUnderworld>()
            .ActivateOnEnter<WillOfTheUnderworld1>()
            .ActivateOnEnter<WaitingWounds>()
            .ActivateOnEnter<SilentEight>()
            .ActivateOnEnter<SteelsbreathReleaseArena>()
            .ActivateOnEnter<ChainTether>();
    }
}
