namespace BossMod.Stormblood.Alliance.A34UltimaP1;

class HolyIVBait(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyIVBait, 6f);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HolyIVSpread, 6f);
class AuralightAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuralightAOE, 20f);
class AuralightRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuralightRect, new AOEShapeRect(70f, 5f));
class GrandCrossAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossAOE, new AOEShapeCross(60f, 7.5f));
class TimeEruption(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.TimeEruptionAOEFirst, (uint)AID.TimeEruptionAOESecond], new AOEShapeRect(20f, 10f), expectedNumCasters: 9);

class Eruption2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption2, 8f);
class ControlTower2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ControlTower2, 6f);

abstract class ExtremeEdge(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60f, 18f));
class ExtremeEdge1(BossModule module) : ExtremeEdge(module, (uint)AID.ExtremeEdge1);
class ExtremeEdge2(BossModule module) : ExtremeEdge(module, (uint)AID.ExtremeEdge2);

class CrushWeapon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrushWeapon, 6f);
class Searchlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Searchlight, 6f);
class HallowedBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HallowedBolt, 6f);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A34UltimaP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(600f, -600f), new ArenaBoundsSquare(30f));
