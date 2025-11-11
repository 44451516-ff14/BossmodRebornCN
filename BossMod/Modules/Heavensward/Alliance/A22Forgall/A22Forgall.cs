namespace BossMod.Heavensward.Alliance.A22Forgall;

class BrandOfTheFallen(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BrandOfTheFallen, 6, 8);
class MegiddoFlame2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame2, new AOEShapeRect(52.6f, 4));
class DarkEruption2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkEruption2, 6);

class MortalRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MortalRay, new AOEShapeCone(20, 22.5f.Degrees()));
class Mow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mow, new AOEShapeCone(13.8f, 60.Degrees()));
class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(30, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4878)]
[SkipLocalsInit]
public sealed class A22Forgall : BossModule
{
    public A22Forgall(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A22Forgall(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-300.00003f, -416.49481f), 29.5f, 96)],
        [new Rectangle(new(-300f, -386.82932f), 22.9f, 1.25f), new Rectangle(new(-300f, -446.47095f), 10.5f, 1f)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SummonedDahak));
        Arena.Actors(Enemies((uint)OID.SummonedSuccubus));
        Arena.Actors(Enemies((uint)OID.SummonedHaagenti));
    }
}
