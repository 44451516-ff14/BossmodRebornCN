namespace BossMod.Modules.Dawntrail.Raid.M09NVampFatale;

sealed class M09VampFataleStates : StateMachineBuilder
{
    public M09VampFataleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<KillerVoice>()
            .ActivateOnEnter<HalfMoon>()
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<Hardcore>()
            .ActivateOnEnter<Hardcore2>()
            .ActivateOnEnter<FlayingFry>()
            .ActivateOnEnter<CoffinFiller>()
            .ActivateOnEnter<PenetratingPitch>()
            .ActivateOnEnter<BlastBeat>()
            .ActivateOnEnter<DeadWake>()
            .ActivateOnEnter<BrutalRain>()
            .ActivateOnEnter<CrowdKill>()
            .ActivateOnEnter<PulpingPulse>()
            .ActivateOnEnter<AetherlettingHit>()
            .ActivateOnEnter<AetherlettingCross>()
            .ActivateOnEnter<InsatiableThirst>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<NeckBiter>()
            .ActivateOnEnter<CoffinMaker>();
    }
}
