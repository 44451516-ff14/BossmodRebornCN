namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

sealed class M03NBruteBomberStates : StateMachineBuilder
{
    public M03NBruteBomberStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KnuckleSandwich>()
            .ActivateOnEnter<BrutalImpact>()
            .ActivateOnEnter<BarbarousBarrageTower>()
            .ActivateOnEnter<BarbarousBarrageKnockback>()
            .ActivateOnEnter<BrutalLariat>()
            .ActivateOnEnter<ExplosiveRainCircle>()
            .ActivateOnEnter<ExplosiveRainConcentric>()
            .ActivateOnEnter<FireSpin>()
            .ActivateOnEnter<LitFuse>()
            .ActivateOnEnter<LariatCombo>()
            .ActivateOnEnter<BrutalBurn>()
            .ActivateOnEnter<MurderousMist>();
    }
}
