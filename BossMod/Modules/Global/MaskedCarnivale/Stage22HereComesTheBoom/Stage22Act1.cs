namespace BossMod.Global.MaskedCarnivale.Stage22.Act1;

public enum OID : uint
{
    Boss = 0x26FC, //R=1.2
    BossAct2 = 0x26FE //R=3.75, needed for pullcheck, otherwise it activates additional modules in act2
}

public enum AID : uint
{
    Fulmination = 14901 // Boss->self, no cast, range 50+R circle, wipe if failed to kill grenade in one hit
}

sealed class Stage22Act1States : StateMachineBuilder
{
    public Stage22Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 632u, NameID = 8122u, SortOrder = 1)]
public sealed class Stage22Act1(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull()
    {
        return !IsAnyActorTargetable((uint)OID.BossAct2) && Raid.Player()!.LastFrameMovement != default && IsAnyActorTargetable((uint)OID.Boss); // they die in one hit
    }

    private readonly string[] _prePullHints =
    [
        "The first act is easy. Kill the grenades in one hit each or they will wipe you. They got 543 HP.",
        "If you gear is bad consider using 1000 Needles. For the 2nd act you should bring Sticky Tongue.",
        "In the 2nd act you can start the Final Sting combination at about 50% health left. (Off-guard->Bristle->Moonflute->Final Sting)"
    ];

    public override string[] PrePullHints => _prePullHints;
}
