namespace BossMod.Global.MaskedCarnivale.Stage11.Act2;

public enum OID : uint
{
    Boss = 0x2719 //R=1.2
}

public enum AID : uint
{
    Fulmination = 14583 // 2719->self, 23.0s cast, range 60 circle
}

sealed class Stage11Act2States : StateMachineBuilder
{
    public Stage11Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 621u, NameID = 2280u, SortOrder = 2)]
public sealed class Stage11Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.Layout4Quads)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);

    private readonly string[] _prePullHints =
    [
        "Same as last act, but this time there are 4 bombs. Pull them to the middle with Sticky Tongue and attack them with any AOE to keep them interrupted. They are weak against wind and strong against fire."
    ];

    public override string[] PrePullHints => _prePullHints;
}
