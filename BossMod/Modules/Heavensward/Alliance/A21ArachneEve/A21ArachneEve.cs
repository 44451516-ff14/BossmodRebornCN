namespace BossMod.Heavensward.Alliance.A21ArachneEve;

class DarkSpike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DarkSpike);
class SilkenSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilkenSpray, new AOEShapeCone(24.5f, 30.Degrees()));
class ShadowBurst(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ShadowBurst, 6, 8);
class SpiderThread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SpiderThread, 6);
class Tremblor1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tremblor1, 10.5f);
class Tremblor2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tremblor2, 20.5f);
class Tremblor3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Tremblor3, 30.5f);
class FrondAffeared(BossModule module) : Components.CastGaze(module, (uint)AID.FrondAffeared);
class TheWidowsEmbrace(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TheWidowsEmbrace, 18, kind: Kind.TowardsOrigin, stopAtWall: true);
class TheWidowsKiss(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TheWidowsKiss, 4, kind: Kind.TowardsOrigin, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4871)]
public class A21ArachneEve(WorldState ws, Actor primary) : BossModule(ws, primary, new(20, -60), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Keyknot));
        Arena.Actors(Enemies(OID.Webmaiden));
        Arena.Actors(Enemies(OID.SpittingSpider));
        Arena.Actors(Enemies(OID.SkitteringSpider));
        Arena.Actors(Enemies(OID.EarthAether));
        Arena.Actors(Enemies(OID.DeepEarthAether));
    }
}
