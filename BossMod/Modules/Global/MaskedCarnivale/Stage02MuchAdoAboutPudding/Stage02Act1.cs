namespace BossMod.Global.MaskedCarnivale.Stage02.Act1;

public enum OID : uint
{
    Boss = 0x25C0, //R=1.8
    Marshmallow = 0x25C2, //R1.8
    Bavarois = 0x25C4 //R1.8
}

public enum AID : uint
{
    Fire = 14266, // Boss->player, 1.0s cast, single-target
    Aero = 14269, // Marshmallow->player, 1.0s cast, single-target
    Thunder = 14268, // Bavarois->player, 1.0s cast, single-target
    GoldenTongue = 14265 // Boss/Marshmallow/Bavarois->self, 5.0s cast, single-target
}

class GoldenTongue(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GoldenTongue);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("To beat this stage in a timely manner,\nyou should have at least one spell of each element.\n(Water, Fire, Ice, Lightning, Earth and Wind)");
    }
}

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Pudding is weak to wind spells.\nMarshmallow is weak to ice spells.\nBavarois is weak to earth spells.");
    }
}

class Stage02Act1States : StateMachineBuilder
{
    public Stage02Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoldenTongue>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Stage02Act1.Trash);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 612, NameID = 8078, SortOrder = 1)]
public class Stage02Act1 : BossModule
{
    public Stage02Act1(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleBig)
    {
        ActivateComponent<Hints>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Marshmallow, (uint)OID.Bavarois];

    protected override bool CheckPull()
    {
        var enemies = Enemies(Trash);
        var count = enemies.Count;
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.InCombat)
                return true;
        }
        return false;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Marshmallow));
        Arena.Actors(Enemies((uint)OID.Bavarois));
    }
}
