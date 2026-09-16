namespace BossMod.Global.MaskedCarnivale.Stage12.Act1;

public enum OID : uint
{
    Boss = 0x271A //R=0.8
}

public enum AID : uint
{
    Seedvolley = 14750 // Boss->player, no cast, single-target
}

sealed class Stage12Act1States : StateMachineBuilder
{
    public Stage12Act1States(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 622u, NameID = 8103u, SortOrder = 1)]
public sealed class Stage12Act1(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    private readonly string[] _prePullHints =
    [
        "For this stage Ice Spikes and Bomb Toss are recommended spells. Use Ice Spikes to instantly kill roselets once they become aggressive. Hydnora in act 2 is weak against water and strong against earth spells."
    ];

    public override string[] PrePullHints => _prePullHints;
}
