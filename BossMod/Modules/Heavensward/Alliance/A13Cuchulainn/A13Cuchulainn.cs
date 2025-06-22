namespace BossMod.Heavensward.Alliance.A13Cuchulainn;

class CorrosiveBile1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CorrosiveBile1, new AOEShapeCone(25, 45.Degrees()));
class FlailingTentacles2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlailingTentacles2, new AOEShapeRect(32.5f, 3.5f));
class Beckon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Beckon, new AOEShapeCone(36.875f, 30.Degrees()));
class BileBelow(BossModule module) : Components.RaidwideCast(module, (uint)AID.BileBelow);
class Pestilence(BossModule module) : Components.RaidwideCast(module, (uint)AID.Pestilence);
class BlackLung(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlackLung);
class GrandCorruption(BossModule module) : Components.RaidwideCast(module, (uint)AID.GrandCorruption);
class FlailingTentacles2Knockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.FlailingTentacles2, 30, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 120, NameID = 4626)]
public class A13Cuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, new(288, 138.5f), new ArenaBoundsCircle(29.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Foobar));
    }
}