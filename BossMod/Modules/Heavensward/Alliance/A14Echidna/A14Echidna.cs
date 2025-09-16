namespace BossMod.Heavensward.Alliance.A14Echidna;

class SickleStrike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SickleStrike);

abstract class SickleSlash(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(18.5f, 30));
class SickleSlash1(BossModule module) : SickleSlash(module, (uint)AID.SickleSlash1);
class SickleSlash2(BossModule module) : SickleSlash(module, (uint)AID.SickleSlash2);

class AbyssalReaper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalReaper, 14);
class AbyssalReaperKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.AbyssalReaper, 5, stopAtWall: true);
class Petrifaction1(BossModule module) : Components.CastGaze(module, (uint)AID.Petrifaction1);
class Petrifaction2(BossModule module) : Components.CastGaze(module, (uint)AID.Petrifaction2);
class Gehenna(BossModule module) : Components.RaidwideCast(module, (uint)AID.Gehenna);
class BloodyHarvest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BloodyHarvest, 12);
class Deathstrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Deathstrike, new AOEShapeRect(62, 3));
class FlameWreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlameWreath, 18);
class SerpentineStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SerpentineStrike, 20);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4631)]
public class A14Echidna(WorldState ws, Actor primary) : BossModule(ws, primary, new(288, -126), new ArenaBoundsCircle(29.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Dexter));
        Arena.Actors(Enemies((uint)OID.Sinister));
        Arena.Actors(Enemies((uint)OID.Bloodguard));
    }
}