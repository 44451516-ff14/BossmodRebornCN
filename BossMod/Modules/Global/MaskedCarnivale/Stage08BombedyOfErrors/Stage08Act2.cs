namespace BossMod.Global.MaskedCarnivale.Stage08.Act2;

public enum OID : uint
{
    Boss = 0x270B, //R=3.75
    Bomb = 0x270C, //R=0.6
    Snoll = 0x270D //R=0.9
}

public enum AID : uint
{
    AutoAttack = 6499, // Bomb/Boss->player, no cast, single-target

    SelfDestruct = 14730, // Bomb->self, no cast, range 6 circle
    HypothermalCombustion = 14731, // Snoll->self, no cast, range 6 circle
    Sap = 14708, // Boss->location, 5.0s cast, range 8 circle
    Burst = 14680 // Boss->self, 6.0s cast, range 50 circle
}

class Sap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sap, 8);
class Burst(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Burst);

class Selfdetonations(BossModule module) : BossComponent(module)
{
    private const string hint = "In bomb explosion radius!";

    private static readonly uint[] _bombs = [(uint)OID.Bomb, (uint)OID.Snoll];

    private static List<Actor> GetBombs(BossModule module)
    {
        var enemies = module.Enemies(_bombs);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var bombs = new List<Actor>(count);
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                bombs.Add(z);
        }
        return bombs;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var bombs = GetBombs(Module);
        var count = bombs.Count;
        for (var i = 0; i < count; ++i)
            Arena.AddCircle(bombs[i].Position, 6f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var bombs = GetBombs(Module);
        var count = bombs.Count;
        for (var i = 0; i < count; ++i)
            if (actor.Position.InCircle(bombs[i].Position, 6f))
            {
                hints.Add(hint);
                return;
            }
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add("Clever activation of cherry bombs will freeze the Progenitrix.\nInterrupt its burst skill or wipe. The Progenitrix is weak to wind spells.");
    }
}

class Stage08Act2States : StateMachineBuilder
{
    public Stage08Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Sap>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(Stage08Act2.Trash);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 618, NameID = 8098, SortOrder = 2)]
public class Stage08Act2 : BossModule
{
    public Stage08Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.Layout2Corners)
    {
        ActivateComponent<Hints>();
        ActivateComponent<Selfdetonations>();
    }
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Bomb, (uint)OID.Snoll];

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
        Arena.Actors(Enemies((uint)OID.Bomb));
        Arena.Actors(Enemies((uint)OID.Snoll));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
}
