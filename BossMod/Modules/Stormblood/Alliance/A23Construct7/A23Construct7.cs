namespace BossMod.Stormblood.Alliance.A23Construct7;

class Destroy1(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Destroy1);
class Destroy2(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Destroy2);
class Accelerate(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Accelerate, 6);
class Compress1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Compress1, new AOEShapeRect(104.5f, 3.5f));
class Compress2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Compress2, new AOEShapeCross(100, 3.5f));

class Pulverize2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pulverize2, 12);
class Dispose1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dispose1, new AOEShapeCone(100, 45.Degrees()));
class Dispose3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dispose3, new AOEShapeCone(100, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Construct, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7237)]
public class A23Construct7(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -141), new ArenaBoundsSquare(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Construct711));
        Arena.Actors(Enemies((uint)OID.Construct712));
        Arena.Actors(Enemies((uint)OID.Construct713));
    }
}
