namespace BossMod.Heavensward.Alliance.A22Forgall;

class BrandOfTheFallen(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BrandOfTheFallen, 6, 8);
class MegiddoFlame2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MegiddoFlame2, new AOEShapeRect(52.6f, 4));
class DarkEruption2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkEruption2, 6);

class MortalRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MortalRay, new AOEShapeCone(20, 22.5f.Degrees()));
class Mow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mow, new AOEShapeCone(13.8f, 60.Degrees()));
class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(30, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4878)]
public class A22Forgall(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -416), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SummonedDahak));
        Arena.Actors(Enemies((uint)OID.SummonedSuccubus));
        Arena.Actors(Enemies((uint)OID.SummonedHaagenti));
    }
}
