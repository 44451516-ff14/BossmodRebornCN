namespace BossMod.Endwalker.Savage.P11SThemis;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 941u, NameID = 12388u, PlanLevel = 90)]
public sealed class P11SThemis(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20u));
