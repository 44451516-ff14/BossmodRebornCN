namespace BossMod.Endwalker.Savage.P12S1Athena;

class RayOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RayOfLight, new AOEShapeRect(60, 5));
class UltimaBlade(BossModule module) : Components.CastCounter(module, (uint)AID.UltimaBladeAOE);
class Parthenos(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Parthenos, new AOEShapeRect(60, 8, 60));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 943, NameID = 12377, SortOrder = 1, PlanLevel = 90)]
public class P12S1Athena(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
