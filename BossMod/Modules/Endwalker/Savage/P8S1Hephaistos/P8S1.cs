namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

sealed class VolcanicTorches(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TorchFlame, new AOEShapeRect(10f, 5f));
sealed class AbyssalFires(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalFires, 15); // TODO: verify falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 884u, NameID = 11399u, SortOrder = 1, PlanLevel = 90)]
public sealed class P8S1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
