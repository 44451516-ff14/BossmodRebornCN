namespace BossMod.Endwalker.Alliance.A30Trash2;

public enum OID : uint
{
    AngelosPack1 = 0x4013, // R3.6
    AngelosMikros = 0x4014, // R2.0

    AngelosPack2 = 0x40E1 // R3.6
}

public enum AID : uint
{
    AutoAttack = 870, // AngelosPack1/AngelosMikros/AngelosPack2->player, no cast, single-target
    RingOfSkylight = 35444, // AngelosPack1/AngelosPack2->self, 5.0s cast, range ?-30 donut
    SkylightCross = 35445, // AngelosPack1/AngelosPack2->self, 5.0s cast, range 60 width 8 cross
    Skylight = 35446 // AngelosMikros->self, 3.0s cast, range 6 circle
}

class RingOfSkylight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RingOfSkylight, new AOEShapeDonut(8f, 30f));
class RingOfSkylightInterruptHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.RingOfSkylight);
class SkylightCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SkylightCross, new AOEShapeCross(60f, 4f));
class SkylightCrossInterruptHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.SkylightCross);
class Skylight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Skylight, 6f);

public class A30Trash2Pack1States : StateMachineBuilder
{
    public A30Trash2Pack1States(A30Trash2Pack1 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<RingOfSkylightInterruptHint>()
            .ActivateOnEnter<SkylightCross>()
            .ActivateOnEnter<SkylightCrossInterruptHint>()
            .ActivateOnEnter<Skylight>()
            .Raw.Update = () =>
            {
                var allDeadOrDestroyed = true;
                var enemies = module.Enemies((uint)OID.AngelosMikros);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                    {
                        allDeadOrDestroyed = false;
                        break;
                    }
                }
                return module.PrimaryActor.IsDeadOrDestroyed && allDeadOrDestroyed;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 5)]
public class A30Trash2Pack1(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly Shape[] union = [new Rectangle(new(800f, 786f), 21f, 13.5f), new Rectangle(new(800f, 767f), 7.5f, 10f), new Rectangle(new(800f, 758f), 10f, 4f)];
    private static readonly Shape[] difference = [new Square(new(811.25f, 787f), 1.5f), new Square(new(811.25f, 777.4f), 1.5f), new Square(new(788.75f, 787f), 1.5f), new Square(new(788.75f, 777.4f), 1.5f),
    new Circle(new(793.4f, 762.75f), 1.25f), new Circle(new(806.6f, 762.75f), 1.25f)];
    private static readonly ArenaBoundsCustom arena = new(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AngelosMikros));
    }
}

public class A30Trash2Pack2States : StateMachineBuilder
{
    public A30Trash2Pack2States(A30Trash2Pack2 module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RingOfSkylight>()
            .ActivateOnEnter<SkylightCross>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies((uint)OID.AngelosPack2);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.AngelosPack2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 12481, SortOrder = 6)]
public class A30Trash2Pack2(WorldState ws, Actor primary) : BossModule(ws, primary, new(800f, 909.75f), new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.AngelosPack2));
    }
}
