namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

[SkipLocalsInit]
sealed class FTME4IndexStates : StateMachineBuilder
{
    public FTME4IndexStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
