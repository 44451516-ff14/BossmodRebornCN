namespace BossMod.Shadowbringers.Foray.Duel.Duel2Lyon;

sealed class Duel2LyonStates : StateMachineBuilder
{
    public Duel2LyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Enaero>()
            .ActivateOnEnter<HeartOfNature>()
            .ActivateOnEnter<TasteOfBlood>()
            .ActivateOnEnter<TasteOfBloodHint>()
            .ActivateOnEnter<RavenousGale>()
            .ActivateOnEnter<WindsPeakKB>()
            .ActivateOnEnter<WindsPeak>()
            .ActivateOnEnter<SplittingRage>()
            .ActivateOnEnter<TheKingsNotice>()
            .ActivateOnEnter<TwinAgonies>()
            .ActivateOnEnter<NaturesBlood>()
            .ActivateOnEnter<SpitefulFlameCircleVoidzone>()
            .ActivateOnEnter<SpitefulFlameRect>()
            .ActivateOnEnter<DynasticFlame>()
            .ActivateOnEnter<SkyrendingStrike>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 735, NameID = 8)] // bnpcname=9409
public sealed class Duel2Lyon(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(new(211f, 380f), 24.5f, 32)]);
    public static readonly ArenaBoundsCircle DefaultArena = new(20f); // default arena got no extra collision, just a donut aoe

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
