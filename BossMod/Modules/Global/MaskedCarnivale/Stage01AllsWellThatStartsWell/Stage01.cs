namespace BossMod.Global.MaskedCarnivale.Stage01;

public enum OID : uint
{
    Boss = 0x25BE, //R=1.5
    Slime = 0x25BD //R=1.5
}

public enum AID : uint
{
    AutoAttack = 6499, // Slime->player, no cast, single-target
    AutoAttack2 = 6497, // Boss->player, no cast, single-target

    FluidSpread = 14198, // Slime->player, no cast, single-target
    IronJustice = 14199 // Boss->self, 2.5s cast, range 8+R 120-degree cone
}

sealed class IronJustice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronJustice, new AOEShapeCone(9.5f, 60f.Degrees()));

sealed class Stage01States : StateMachineBuilder
{
    public Stage01States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronJustice>()
            .Raw.Update = () => AllDeadOrDestroyed(Stage01.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 610u, NameID = 8077u)]
public sealed class Stage01(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Slime];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Slime));
    }

    private readonly string[] _prePullHints =
    [
        "This stage is trivial. Use whatever skills you have to defeat these opponents."
    ];

    public override string[] PrePullHints => _prePullHints;
}
