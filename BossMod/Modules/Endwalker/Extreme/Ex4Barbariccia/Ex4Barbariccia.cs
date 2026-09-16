namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

sealed class RagingStorm(BossModule module) : Components.CastCounter(module, (uint)AID.RagingStorm);
sealed class HairFlayUpbraid(BossModule module) : Components.CastStackSpread(module, (uint)AID.Upbraid, (uint)AID.HairFlay, 3f, 10f, maxStackSize: 2);
sealed class CurlingIron(BossModule module) : Components.CastCounter(module, (uint)AID.CurlingIronAOE);
sealed class Catabasis(BossModule module) : Components.CastCounter(module, (uint)AID.Catabasis);
sealed class VoidAeroTankbuster(BossModule module) : Components.Cleave(module, (uint)AID.VoidAeroTankbuster, new AOEShapeCircle(5f), originAtTarget: true);
sealed class SecretBreezeCones(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SecretBreezeAOE, new AOEShapeCone(40f, 22.5f.Degrees()));
sealed class SecretBreezeProteans(BossModule module) : Components.SimpleProtean(module, (uint)AID.SecretBreezeProtean, new AOEShapeCone(40f, 22.5f.Degrees()));

sealed class WarningGale(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WarningGale, 6f);
sealed class WindingGaleCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.WindingGaleCharge, 2f);
sealed class BoulderBreak(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.BoulderBreak, 5f);
sealed class Boulder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Boulder, 10f);
sealed class BrittleBoulder(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BrittleBoulder, 5f);
sealed class TornadoChainInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TornadoChainInner, 11f);
sealed class TornadoChainOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TornadoChainOuter, new AOEShapeDonut(11f, 20f));
sealed class KnuckleDrum(BossModule module) : Components.CastCounter(module, (uint)AID.KnuckleDrum);
sealed class KnuckleDrumLast(BossModule module) : Components.CastCounter(module, (uint)AID.KnuckleDrumLast);
sealed class BlowAwayRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.BlowAwayRaidwide);
sealed class BlowAwayPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlowAwayPuddle, 6f);
sealed class ImpactAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpactAOE, 6f);
sealed class ImpactKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ImpactKnockback, 6f);
sealed class BlusteryRuler(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlusteryRuler, 6f);
sealed class DryBlowsRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.DryBlowsRaidwide);
sealed class DryBlowsPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DryBlowsPuddle, 3f);
sealed class IronOut(BossModule module) : Components.CastCounter(module, (uint)AID.IronOutAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 871u, NameID = 11398u, PlanLevel = 90)]
public sealed class Ex4Barbariccia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
