namespace BossMod.Stormblood.Alliance.A24Yiazmat;

class RakeTB(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RakeTB);
class RakeSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.RakeSpread, 5);
class RakeAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RakeAOE, 10);
class RakeLoc1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RakeLoc1, 10);
class RakeLoc2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RakeLoc2, 10);
class StoneBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StoneBreath, new AOEShapeCone(60, 22.5f.Degrees()));
class DustStorm2(BossModule module) : Components.RaidwideCast(module, (uint)AID.DustStorm2);
class WhiteBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhiteBreath, new AOEShapeDonut(10, 60));

class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero, new AOEShapeRect(40, 3));
class Karma(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Karma, new AOEShapeCone(30, 45.Degrees()));
class UnholyDarkness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnholyDarkness, 8);

class SolarStorm1(BossModule module) : Components.RaidwideCast(module, (uint)AID.SolarStorm1);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7070)]
public class A24Yiazmat(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -400), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.HeartOfTheDragon));
        Arena.Actors(Enemies((uint)OID.Archaeodemon));
        Arena.Actors(Enemies((uint)OID.WindAzer));
    }
}
