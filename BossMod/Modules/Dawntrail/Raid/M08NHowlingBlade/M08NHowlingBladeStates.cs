namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

sealed class M08NHowlingBladeStates : StateMachineBuilder
{
    public M08NHowlingBladeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ExtraplanarTitanicPursuit>()
            .ActivateOnEnter<GreatDivide>()
            .ActivateOnEnter<Heavensearth1>()
            .ActivateOnEnter<Heavensearth2>()
            .ActivateOnEnter<WolvesReignRect1>()
            .ActivateOnEnter<WolvesReignRect2>()
            .ActivateOnEnter<WolvesReignCone>()
            .ActivateOnEnter<WolvesReignCircle>()
            .ActivateOnEnter<MoonbeamsBite>()
            .ActivateOnEnter<RoaringWindShadowchase>()
            .ActivateOnEnter<TargetedQuake>()
            .ActivateOnEnter<FangedCharge>()
            .ActivateOnEnter<TerrestrialTitans>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<TrackingTremors>()
            .ActivateOnEnter<RavenousSaber>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<GrowlingWindWealofStone>();
    }
}
