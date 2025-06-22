namespace BossMod.Heavensward.Alliance.A33ProtoUltima;

class AetherialPool(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AetherialPool, 40, kind: Kind.TowardsOrigin, stopAtWall: true);
class AetherochemicalFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.AetherochemicalFlare);

abstract class AetherochemicalLaser(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(71, 4));
class AetherochemicalLaser1(BossModule module) : AetherochemicalLaser(module, (uint)AID.AetherochemicalLaser1);
class AetherochemicalLaser2(BossModule module) : AetherochemicalLaser(module, (uint)AID.AetherochemicalLaser2);
class AetherochemicalLaser3(BossModule module) : AetherochemicalLaser(module, (uint)AID.AetherochemicalLaser3);

class CitadelBuster2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CitadelBuster2, new AOEShapeRect(65.5f, 5));
class FlareStar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlareStar, 31);

class Rotoswipe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rotoswipe, new AOEShapeCone(11, 60.Degrees()));

class WreckingBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WreckingBall, 8);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 3780)]
public class A33ProtoUltima(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -50), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AllaganDreadnaught));
        Arena.Actors(Enemies((uint)OID.AetherCollector));
    }
}
