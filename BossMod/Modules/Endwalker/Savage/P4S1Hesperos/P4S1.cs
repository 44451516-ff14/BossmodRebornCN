namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to elegant evisceration mechanic (dual hit tankbuster)
// TODO: consider showing some tank swap / invul hint...
public sealed class ElegantEvisceration(BossModule module) : Components.CastCounter(module, (uint)AID.ElegantEviscerationSecond);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 801u, NameID = 10744u, SortOrder = 1, PlanLevel = 90)]
public sealed class P4S1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
