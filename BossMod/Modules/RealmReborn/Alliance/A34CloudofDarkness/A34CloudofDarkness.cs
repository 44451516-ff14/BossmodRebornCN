namespace BossMod.RealmReborn.Alliance.A34CloudofDarkness;

class ZeroFormParticleBeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ZeroFormParticleBeam, new AOEShapeRect(74, 12));
class ParticleBeam2(BossModule module) : Components.RaidwideCast(module, (uint)AID.ParticleBeam2);

class FeintParticleBeam(BossModule module) : Components.StandardChasingAOEs(module, 10f, (uint)AID.FeintParticleBeam1, (uint)AID.FeintParticleBeam2, 4, 1.5f, 5, true, (uint)IconID.FeintParticleBeam)
{
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3240)]
public class A34CloudofDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -400), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.DarkCloud));
        Arena.Actors(Enemies((uint)OID.DarkStorm));
        Arena.Actors(Enemies((uint)OID.HyperchargedCloud));
    }
}
