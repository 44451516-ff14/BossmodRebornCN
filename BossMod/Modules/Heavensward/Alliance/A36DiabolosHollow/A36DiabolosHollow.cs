namespace BossMod.Heavensward.Alliance.A36DiabolosHollow;

class Shadethrust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shadethrust, new AOEShapeRect(43, 2.5f));
class HollowCamisado(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HollowCamisado);
class HollowNightmare(BossModule module) : Components.CastGaze(module, (uint)AID.HollowNightmare);
class HollowOmen1(BossModule module) : Components.RaidwideCast(module, (uint)AID.HollowOmen1);
class HollowOmen2(BossModule module) : Components.RaidwideCast(module, (uint)AID.HollowOmen2);
class Blindside(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Blindside, 6, 8);
class EarthShaker2(BossModule module) : Components.SimpleProtean(module, (uint)AID.EarthShaker2, new AOEShapeCone(60f, 15f.Degrees()));
class HollowNight(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HollowNight, 8, 8);
class HollowNightGaze(BossModule module) : Components.CastGaze(module, (uint)AID.HollowNight);
class ParticleBeam2(BossModule module) : Components.CastTowers(module, (uint)AID.ParticleBeam2, 5);
class ParticleBeam4(BossModule module) : Components.CastTowers(module, (uint)AID.ParticleBeam4, 5);

class Nox(BossModule module) : Components.StandardChasingAOEs(module, 10f, (uint)AID.NoxAOEFirst, (uint)AID.NoxAOERest, 5.5f, 1.6f, 5, true, (uint)IconID.Nox)
{
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 5526)]
public class A36DiabolosHollow(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -445), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Deathgate));
        Arena.Actors(Enemies((uint)OID.DiabolicGate));
        Arena.Actors(Enemies((uint)OID.Shadowsphere));
        Arena.Actors(Enemies((uint)OID.NightHound));
    }
}
