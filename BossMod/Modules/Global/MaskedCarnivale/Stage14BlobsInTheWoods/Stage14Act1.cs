namespace BossMod.Global.MaskedCarnivale.Stage14.Act1;

public enum OID : uint
{
    Boss = 0x271D //R=2.0
}

public enum AID : uint
{
    TheLastSong = 14756 // Boss->self, 6.0s cast, range 60 circle
}

sealed class LastSong(BossModule module) : Components.CastLineOfSightAOEComplex(module, (uint)AID.TheLastSong, Layouts.Layout2CornersBlockers);

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

sealed class Stage14Act1States : StateMachineBuilder
{
    public Stage14Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LastSong>()
            .ActivateOnEnter<LastSongHint>()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss) && module.FindComponent<LastSongHint>()!.Casting == 0;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 624u, NameID = 8108u, SortOrder = 1)]
public sealed class Stage14Act1(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.Layout2Corners)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);

    private readonly string[] _prePullHints =
    [
        "These slimes start casting Final Song after death. While Final Song is not deadly, it does heavy damage and applies silence to you. Take cover!",
        "For act 2 the spell Loom is strongly recommended. The slimes are strong against blunt melee damage such as J Kick."
    ];

    public override string[] PrePullHints => _prePullHints;
}
