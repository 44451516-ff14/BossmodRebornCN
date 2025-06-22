namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

sealed class M02NHoneyBLovelyStates : StateMachineBuilder
{
    public M02NHoneyBLovelyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CallMeHoney>()
            .ActivateOnEnter<SweetheartsN>()
            .ActivateOnEnter<TemptingTwist>()
            .ActivateOnEnter<HoneyBeeline>()
            .ActivateOnEnter<HoneyedBreeze>()
            .ActivateOnEnter<HoneyBLive>()
            .ActivateOnEnter<Heartsore>()
            .ActivateOnEnter<Heartsick>()
            .ActivateOnEnter<Fracture>()
            .ActivateOnEnter<Loveseeker>()
            .ActivateOnEnter<BlowKiss>()
            .ActivateOnEnter<HoneyBFinale>()
            .ActivateOnEnter<DropOfVenom>()
            .ActivateOnEnter<SplashOfVenom>()
            .ActivateOnEnter<Splinter>()
            .ActivateOnEnter<BlindingLove1>()
            .ActivateOnEnter<BlindingLove2>()
            .ActivateOnEnter<HeartStruck1>()
            .ActivateOnEnter<HeartStruck2>()
            .ActivateOnEnter<HeartStruck3>();
    }
}
