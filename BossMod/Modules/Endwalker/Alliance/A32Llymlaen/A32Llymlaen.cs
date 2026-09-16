namespace BossMod.Endwalker.Alliance.A32Llymlaen;

sealed class WindRose(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindRose, 12f);
sealed class SeafoamSpiral(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeafoamSpiral, new AOEShapeDonut(6f, 70f));
sealed class DeepDiveNormal(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DeepDiveNormal, 6f, 8);
sealed class Stormwhorl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Stormwhorl, 6f);
sealed class Stormwinds(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Stormwinds, 6f);
sealed class Maelstrom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Maelstrom, 6f);
sealed class Godsbane(BossModule module) : Components.CastCounter(module, (uint)AID.GodsbaneAOE);
sealed class DeepDiveHardWater(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DeepDiveHardWater, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962u, NameID = 11299u, SortOrder = 3, PlanLevel = 90)]
public sealed class A32Llymlaen(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -900f), new ArenaBoundsRect(19f, 29f));
