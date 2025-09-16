namespace BossMod.Heavensward.Alliance.A11Cetus;

class ElectricSwipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricSwipe, new AOEShapeCone(25, 30.Degrees()));
class BodySlam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BodySlam, 10);
class Immersion(BossModule module) : Components.RaidwideCast(module, (uint)AID.Immersion);
class ElectricWhorl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricWhorl, new AOEShapeDonut(7, 60));
class ExpulsionAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Expulsion, 14);
class ExpulsionKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Expulsion, 30, stopAtWall: true);
class BiteAndRun(BossModule module) : Components.ChargeAOEs(module, (uint)AID.BiteAndRun, 2.5f);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4613)]
public class A11Cetus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-288, 0), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.HybodusPup));
        Arena.Actors(Enemies((uint)OID.Hybodus));
        Arena.Actors(Enemies((uint)OID.Hydrosphere));
        Arena.Actors(Enemies((uint)OID.Hydrocore));
    }
}
