namespace BossMod.Global.MaskedCarnivale.Stage16.Act1;

public enum OID : uint
{
    Boss = 0x26F2 //R=3.2
}

public enum AID : uint
{
    AutoAttack = 6497 // Boss->player, no cast, single-target
}

sealed class Stage16Act1States : StateMachineBuilder
{
    public Stage16Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 626u, NameID = 8112u, SortOrder = 1)]
public sealed class Stage16Act1(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);

    private readonly string[] _prePullHints =
    [
        "The cyclops are very slow, but will instantly kill you, if they catch you. Kite them or kill them with the self-destruct combo. (Toad Oil->Bristle->Moonflute->Swiftcast->Self-destruct)",
        "If you don't use the self-destruct combo in act 1, you can bring the Final Sting combo for act 2. (Off-guard->Bristle->Moonflute->Final Sting) Diamondback is highly recommended in act 2."
    ];

    public override string[] PrePullHints => _prePullHints;
}
