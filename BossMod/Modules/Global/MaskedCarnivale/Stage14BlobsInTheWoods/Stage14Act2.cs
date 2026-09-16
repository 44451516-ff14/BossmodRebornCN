namespace BossMod.Global.MaskedCarnivale.Stage14.Act2;

public enum OID : uint
{
    Boss = 0x271E //R=2.0
}

public enum AID : uint
{
    Syrup = 14757, // Boss->player, no cast, range 4 circle, applies heavy to player
    TheLastSong = 14756 // Boss->self, 6.0s cast, range 60 circle, heavy dmg, applies silence to player
}

sealed class LastSong(BossModule module) : Components.CastLineOfSightAOEComplex(module, (uint)AID.TheLastSong, Layouts.LayoutBigQuadBlockers);
sealed class LastSongHint(BossModule module) : BossComponent(module)
{
    public int Casting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheLastSong)
        {
            ++Casting; // theoretically more than one slime could be casting at the same time
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheLastSong)
        {
            --Casting;
        }
    }
}

sealed class Stage14Act2States : StateMachineBuilder
{
    public Stage14Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LastSong>()
            .ActivateOnEnter<LastSongHint>()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss) && module.FindComponent<LastSongHint>()!.Casting == 0;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 624u, NameID = 8108u, SortOrder = 2)]
public sealed class Stage14Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.LayoutBigQuad)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);

    private readonly string[] _prePullHints =
    [
        "Same as first act, but the slimes will apply heavy to you. Use Loom to get out of line of sight as soon as Final Song gets casted."
    ];

    public override string[] PrePullHints => _prePullHints;
}
