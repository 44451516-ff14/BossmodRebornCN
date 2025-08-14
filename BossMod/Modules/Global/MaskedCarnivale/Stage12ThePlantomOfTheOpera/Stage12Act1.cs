namespace BossMod.Global.MaskedCarnivale.Stage12.Act1;

public enum OID : uint
{
    Boss = 0x271A //R=0.8
}

public enum AID : uint
{
    Seedvolley = 14750 // Boss->player, no cast, single-target
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("For this stage Ice Spikes and Bomb Toss are recommended spells.\nUse Ice Spikes to instantly kill roselets once they become aggressive.\nHydnora in act 2 is weak against water and strong against earth spells.");
    }
}

sealed class Stage12Act1States : StateMachineBuilder
{
    public Stage12Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 622, NameID = 8103, SortOrder = 1)]
public sealed class Stage12Act1 : BossModule
{
    public Stage12Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }
}
