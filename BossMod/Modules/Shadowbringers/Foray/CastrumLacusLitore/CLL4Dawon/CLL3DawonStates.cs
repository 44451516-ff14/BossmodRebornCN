namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL4Dawon;

sealed class CLL4DawonStates : StateMachineBuilder
{
    public CLL4DawonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Pentagust>()
            .ActivateOnEnter<FervidPulse>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<SwoopingFrenzy>()
            .ActivateOnEnter<MoltingPlumage>()
            .ActivateOnEnter<Scratch>()
            .ActivateOnEnter<CrackleHiss>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<SpikeFlail>()
            .ActivateOnEnter<LeftRightHammer>()
            .ActivateOnEnter<VerdantScarletPlume>()
            .ActivateOnEnter<Obey>()
            .ActivateOnEnter<TasteOfBlood>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<NaturesPulse>()
            .ActivateOnEnter<TwinAgonies>()
            .ActivateOnEnter<TheKingsNotice>()
            .ActivateOnEnter<HeartOfNature>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<WindsPeakKB>();
    }
}
