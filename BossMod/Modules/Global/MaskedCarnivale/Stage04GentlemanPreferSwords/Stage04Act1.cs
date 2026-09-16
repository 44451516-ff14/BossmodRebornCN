namespace BossMod.Global.MaskedCarnivale.Stage04.Act1;

public enum OID : uint
{
    Boss = 0x25C8, //R=1.65
    Bat = 0x25D2, //R=0.4
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    AutoAttack2 = 6499, // Bat->player, no cast, single-target
    BloodDrain = 14360, // Bat->player, no cast, single-target
    SanguineBite = 14361, // Boss->self, no cast, range 3+R width 2 rect
}

sealed class Stage04Act1States : StateMachineBuilder
{
    public Stage04Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed(Stage04Act1.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 614u, NameID = 8086u, SortOrder = 1)]
public sealed class Stage04Act1(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Bat];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Bat));
    }

    private readonly string[] _prePullHints =
    [
        "Trivial act. Enemies here are weak to lightning and fire. In Act 2 the Ram's Voice and Ultravibration combo can be useful. Flying Sardine for interrupts can be beneficial.",
        "Bats are weak to lightning. The wolf is weak to fire."
    ];

    public override string[] PrePullHints => _prePullHints;
}
