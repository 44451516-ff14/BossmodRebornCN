namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class VolcanicTorches(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TorchFlame, new AOEShapeRect(10, 5));
class AbyssalFires(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalFires, 15); // TODO: verify falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 884, NameID = 11399, SortOrder = 1, PlanLevel = 90)]
public class P8S1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
