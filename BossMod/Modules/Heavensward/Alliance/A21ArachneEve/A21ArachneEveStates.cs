namespace BossMod.Heavensward.Alliance.A21ArachneEve;

[SkipLocalsInit]
sealed class A21ArachneEveStates : StateMachineBuilder
{
    public A21ArachneEveStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SpiderWeb>()
            .ActivateOnEnter<DarkSpike>()
            .ActivateOnEnter<SilkenSpray>()
            .ActivateOnEnter<ShadowBurst>()
            .ActivateOnEnter<SpiderThread>()
            .ActivateOnEnter<FrondAffeared>()
            .ActivateOnEnter<TheWidowsEmbrace>()
            .ActivateOnEnter<TheWidowsKiss>()
            .ActivateOnEnter<Pitfall>()
            .ActivateOnEnter<Tremblor>();
    }
}
