namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class A32AlexanderStates : StateMachineBuilder
{
    public A32AlexanderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BanishgaIV>()
            .ActivateOnEnter<DivineArrowCone>()
            .ActivateOnEnter<DivineArrowCircles>()
            .ActivateOnEnter<DivineArrowLines>()
            .ActivateOnEnter<BanishgaIVSpread>()
            .ActivateOnEnter<HolyII>()
            .ActivateOnEnter<ImpartialRuling>()
            .ActivateOnEnter<RadiantSacrament>()
            .ActivateOnEnter<DivineSpear>()
            .ActivateOnEnter<MegaHoly>()
            .ActivateOnEnter<Activate>()
            .ActivateOnEnter<PerfectDefense>()
            .ActivateOnEnter<HolyFlame>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<CircuitShock>()
            .ActivateOnEnter<DivineJudgment>()
            .ActivateOnEnter<Electrify>()
            .ActivateOnEnter<DivineBolt>();
    }
}
