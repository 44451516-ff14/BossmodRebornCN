namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 943u, NameID = 12382u, SortOrder = 2, PlanLevel = 90)]
public sealed class P12S2PallasAthena(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 95f), new ArenaBoundsRect(20f, 15f));