namespace BossMod.Endwalker.Savage.P12S1Athena;

sealed class RayOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RayOfLight, new AOEShapeRect(60f, 5f));
sealed class UltimaBlade(BossModule module) : Components.CastCounter(module, (uint)AID.UltimaBladeAOE);
sealed class Parthenos(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Parthenos, new AOEShapeRect(120f, 8f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 943, NameID = 12377, SortOrder = 1, PlanLevel = 90)]
public sealed class P12S1Athena(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
