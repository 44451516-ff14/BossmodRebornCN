namespace BossMod.Heavensward.Alliance.A25Calofisteri;

class AuraBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.AuraBurst);
class DepthCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.DepthCharge, 5);
class Extension2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Extension2, 6);
class FeintParticleBeam1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeintParticleBeam1, 3);
class Penetration(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Penetration, 50, kind: Kind.TowardsOrigin);
class Graft(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Graft, 5);

abstract class Haircut(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(65.5f, 90.Degrees()));
class Haircut1(BossModule module) : Haircut(module, (uint)AID.Haircut1);
class Haircut2(BossModule module) : Haircut(module, (uint)AID.Haircut2);

abstract class SplitEnd(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(25.5f, 22.5f.Degrees()));
class SplitEnd1(BossModule module) : SplitEnd(module, (uint)AID.SplitEnd1);
class SplitEnd2(BossModule module) : SplitEnd(module, (uint)AID.SplitEnd2);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4897)]
public class A25Calofisteri(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -35), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Bijou1));
        Arena.Actors(Enemies(OID.Bijou2));
        Arena.Actors(Enemies(OID.GrandBijou));
        Arena.Actors(Enemies(OID.LivingLock1));
        Arena.Actors(Enemies(OID.LivingLock2));
        Arena.Actors(Enemies(OID.LivingLock3));
        Arena.Actors(Enemies(OID.LurkingLock));
        Arena.Actors(Enemies(OID.Entanglement));
    }
}
