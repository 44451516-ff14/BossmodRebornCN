namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class M01NBlackCatStates : StateMachineBuilder
{
    public M01NBlackCatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ElevateAndEviscerate>()
            .ActivateOnEnter<ElevateAndEviscerateHint>()
            .ActivateOnEnter<ElevateAndEviscerateImpact>()
            .ActivateOnEnter<BloodyScratch>()
            .ActivateOnEnter<Mouser>()
            .ActivateOnEnter<OneTwoPaw>()
            .ActivateOnEnter<BlackCatCrossing>()
            .ActivateOnEnter<BiscuitMaker>()
            .ActivateOnEnter<Clawful>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<PredaceousPounce>()
            .ActivateOnEnter<GrimalkinGale>()
            .ActivateOnEnter<Overshadow>();
    }
}
