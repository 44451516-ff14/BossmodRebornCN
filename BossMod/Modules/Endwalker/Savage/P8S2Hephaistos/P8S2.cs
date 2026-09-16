namespace BossMod.Endwalker.Savage.P8S2;

sealed class TyrantsFlare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TyrantsFlareAOE, 6f);

// TODO: autoattack component
// TODO: HC components
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 884u, NameID = 11399u, SortOrder = 2, PlanLevel = 90)]
public sealed class P8S2(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
