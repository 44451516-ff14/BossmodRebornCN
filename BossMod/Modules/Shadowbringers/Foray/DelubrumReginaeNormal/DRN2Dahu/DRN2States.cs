namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN2Dahu;

sealed class DRN2DahuStates : StateMachineBuilder
{
    public DRN2DahuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<HotCharge>()
            .ActivateOnEnter<Firebreathe>()
            .ActivateOnEnter<HeadDown>()
            .ActivateOnEnter<FirebreatheRotation>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<HuntersClaw>()
            .ActivateOnEnter<FeralHowl>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<HeatBreath>()
            .ActivateOnEnter<RipperClaw>();
    }
}
