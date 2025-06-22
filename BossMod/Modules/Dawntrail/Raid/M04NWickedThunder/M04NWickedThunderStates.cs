namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class M04NWickedThunderStates : StateMachineBuilder
{
    public M04NWickedThunderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<WickedHypercannon>()
            .ActivateOnEnter<WickedJolt>()
            .ActivateOnEnter<WickedBolt>()
            .ActivateOnEnter<WickedCannon>()
            .ActivateOnEnter<WrathOfZeus>()
            .ActivateOnEnter<SidewiseSpark>()
            .ActivateOnEnter<SoaringSoulpress>()
            .ActivateOnEnter<StampedingThunder>()
            .ActivateOnEnter<BewitchingFlight>()
            .ActivateOnEnter<Thunderslam>()
            .ActivateOnEnter<Thunderstorm>()
            .ActivateOnEnter<WitchHunt>();
    }
}
