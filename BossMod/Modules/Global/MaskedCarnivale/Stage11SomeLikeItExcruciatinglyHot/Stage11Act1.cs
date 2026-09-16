namespace BossMod.Global.MaskedCarnivale.Stage11.Act1;

public enum OID : uint
{
    Boss = 0x2718, //R=1.2
}

public enum AID : uint
{
    Fulmination = 14583, // Boss->self, 23.0s cast, range 60 circle
}

sealed class Stage11Act1States : StateMachineBuilder
{
    public Stage11Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 621u, NameID = 2280u, SortOrder = 1)]
public sealed class Stage11Act1(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);

    private readonly string[] _prePullHints =
    [
        "These bombs start self-destruction on combat start. Pull them together with Sticky Tongue and attack them with anything to interrupt them. They are weak against wind and strong against fire."
    ];

    public override string[] PrePullHints => _prePullHints;
}
