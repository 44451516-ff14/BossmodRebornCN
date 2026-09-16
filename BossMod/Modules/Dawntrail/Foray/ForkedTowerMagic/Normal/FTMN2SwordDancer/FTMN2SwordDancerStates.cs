namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

sealed class FTMN2SwordDancerStates : StateMachineBuilder
{
    public FTMN2SwordDancerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwordStormCast>()
            .ActivateOnEnter<RushShort1>()
            .ActivateOnEnter<RushShort2>()
            .ActivateOnEnter<TurnInner>()
            .ActivateOnEnter<TurnOuter>()
            .ActivateOnEnter<MartialMystique>()
            .ActivateOnEnter<Cyclosword>()
            .ActivateOnEnter<SwordDance>()
            .ActivateOnEnter<Pierce>()
            .ActivateOnEnter<Steelsbreath>()
            .ActivateOnEnter<RushSurgesword>();
    }
}
