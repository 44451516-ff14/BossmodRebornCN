namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

sealed class DRN6QueenStates : StateMachineBuilder
{
    public DRN6QueenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<NorthswainsGlowPawnOff>()
            .ActivateOnEnter<GodsSaveTheQueen>()
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            .ActivateOnEnter<JudgmentBlade>()
            .ActivateOnEnter<HeavensWrathAOE>()
            .ActivateOnEnter<HeavensWrathKnockback>()
            .ActivateOnEnter<Chess>()
            .ActivateOnEnter<MeansEnds>()
            .ActivateOnEnter<TurretsTour>()
            .ActivateOnEnter<AboveBoard>()
            .ActivateOnEnter<CleansingSlash>();
    }
}
