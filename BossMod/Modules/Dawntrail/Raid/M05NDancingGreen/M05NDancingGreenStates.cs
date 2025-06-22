namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class M05NDancingGreenStates : StateMachineBuilder
{
    public M05NDancingGreenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DoTheHustle>()
            .ActivateOnEnter<Spotlight>()
            .ActivateOnEnter<Moonburn>()
            .ActivateOnEnter<DeepCut>()
            .ActivateOnEnter<FullBeat>()
            .ActivateOnEnter<CelebrateGoodTimesDiscoInfernalLetsPose>()
            .ActivateOnEnter<EighthBeats>()
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<LetsDance>()
            .ActivateOnEnter<RideTheWaves>()
            .ActivateOnEnter<TwoFourSnapTwist>();
    }
}
