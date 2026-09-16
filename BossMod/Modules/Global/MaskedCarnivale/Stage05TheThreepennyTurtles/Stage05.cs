namespace BossMod.Global.MaskedCarnivale.Stage05;

public enum OID : uint
{
    Boss = 0x25CC, //R=5.0
}

sealed class Stage05States : StateMachineBuilder
{
    public Stage05States(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyed((uint)OID.Boss);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 615u, NameID = 8089u)]
public sealed class Stage05(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }

    protected override bool CheckPull() => IsAnyActorInCombat((uint)OID.Boss);

    private readonly string[] _prePullHints =
    [
        "These turtles have very high defenses. Bring 1000 Needles or Doom to defeat them. Alternatively you can remove their buff with Eerie Soundwave."
    ];

    public override string[] PrePullHints => _prePullHints;
}
