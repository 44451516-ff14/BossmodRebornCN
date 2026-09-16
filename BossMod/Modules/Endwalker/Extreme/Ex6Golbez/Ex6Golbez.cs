namespace BossMod.Endwalker.Extreme.Ex6Golbez;

sealed class Terrastorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrastormAOE, 16f);
sealed class LingeringSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LingeringSparkAOE, 5f);

abstract class PhasesOfTheBlade(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(22f, 90f.Degrees()));
sealed class PhasesOfTheBladeFront(BossModule module) : PhasesOfTheBlade(module, (uint)AID.PhasesOfTheBlade);
sealed class PhasesOfTheBladeBack(BossModule module) : PhasesOfTheBlade(module, (uint)AID.PhasesOfTheBladeBack);
sealed class PhasesOfTheShadowFront(BossModule module) : PhasesOfTheBlade(module, (uint)AID.PhasesOfTheShadow);
sealed class PhasesOfTheShadowBack(BossModule module) : PhasesOfTheBlade(module, (uint)AID.PhasesOfTheShadowBack);

sealed class ArcticAssault(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArcticAssaultAOE, new AOEShapeRect(15f, 7.5f));
sealed class RisingBeacon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RisingBeaconAOE, 10f);
sealed class RisingRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RisingRingAOE, new AOEShapeDonut(6f, 22f));
sealed class BurningShade(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BurningShade, 5f);
sealed class ImmolatingShade(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ImmolatingShade, 6f, 4, 4);
sealed class VoidBlizzard(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VoidBlizzard, 6f, 4, 4);
sealed class VoidAero(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VoidAero, 3f, 2, 2);
sealed class VoidTornado(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VoidTornado, 6f, 4, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 950, NameID = 12365, PlanLevel = 90)]
public sealed class Ex6Golbez(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(15f));
