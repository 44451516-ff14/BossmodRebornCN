namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class A21FaithboundKirinStates : StateMachineBuilder
{
    public A21FaithboundKirinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Punishment>()
            .ActivateOnEnter<CrimsonRiddle>()
            .ActivateOnEnter<StonegaIII1>()
            .ActivateOnEnter<StonegaIII2>()
            .ActivateOnEnter<StonegaIVShatteringStomp>()
            .ActivateOnEnter<QuakeSmall>()
            .ActivateOnEnter<QuakeBig>()
            .ActivateOnEnter<EastwindWheel>()
            .ActivateOnEnter<SynchronizedStrikeSmite>()
            .ActivateOnEnter<Wringer>()
            .ActivateOnEnter<StrikingSmiting>()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<Bury>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<KirinCaptivator>()
            .ActivateOnEnter<WallArenaChange>()
            .ActivateOnEnter<GloamingGleam>()
            .ActivateOnEnter<RazorFang>()
            .ActivateOnEnter<VermilionFlight>()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<MoontideFont>()
            .ActivateOnEnter<MidwinterMarchNorthernCurrent>();
    }
}
