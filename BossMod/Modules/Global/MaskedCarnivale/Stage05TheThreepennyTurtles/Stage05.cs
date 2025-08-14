namespace BossMod.Global.MaskedCarnivale.Stage05;

public enum OID : uint
{
    Boss = 0x25CC, //R=5.0
}

sealed class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("These turtles have very high defenses.\nBring 1000 Needles or Doom to defeat them.\nAlternatively you can remove their buff with Eerie Soundwave.");
    }
}

sealed class Stage05States : StateMachineBuilder
{
    public Stage05States(BossModule module) : base(module)
    {
        TrivialPhase()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.Boss);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    if (!enemy.IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 615, NameID = 8089)]
public sealed class Stage05 : BossModule
{
    public Stage05(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.Boss));
    }
}
