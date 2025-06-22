namespace BossMod.Stormblood.Alliance.A22Belias;

class FireIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.FireIV);
class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 8f);
class TimeBomb2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TimeBomb2, new AOEShapeCone(60f, 45f.Degrees()));
class TimeEruption(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.TimeEruptionAOEFirst, (uint)AID.TimeEruptionAOESecond], new AOEShapeRect(20f, 10f), expectedNumCasters: 9);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7223)]
public class A22Belias(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200f, -541f), new ArenaBoundsSquare(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Gigas));
    }
}
