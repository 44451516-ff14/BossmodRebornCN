namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class RagingStorm(BossModule module) : Components.CastCounter(module, (uint)AID.RagingStorm);
class HairFlayUpbraid(BossModule module) : Components.CastStackSpread(module, (uint)AID.Upbraid, (uint)AID.HairFlay, 3, 10, maxStackSize: 2);
class CurlingIron(BossModule module) : Components.CastCounter(module, (uint)AID.CurlingIronAOE);
class Catabasis(BossModule module) : Components.CastCounter(module, (uint)AID.Catabasis);
class VoidAeroTankbuster(BossModule module) : Components.Cleave(module, (uint)AID.VoidAeroTankbuster, new AOEShapeCircle(5), originAtTarget: true);
class SecretBreezeCones(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SecretBreezeAOE, new AOEShapeCone(40, 22.5f.Degrees()));
class SecretBreezeProteans(BossModule module) : Components.SimpleProtean(module, (uint)AID.SecretBreezeProtean, new AOEShapeCone(40, 22.5f.Degrees()));

class WarningGale(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WarningGale, 6);
class WindingGaleCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.WindingGaleCharge, 2);
class BoulderBreak(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.BoulderBreak, 5);
class Boulder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Boulder, 10);
class BrittleBoulder(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BrittleBoulder, 5);
class TornadoChainInner(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TornadoChainInner, 11);
class TornadoChainOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TornadoChainOuter, new AOEShapeDonut(11, 20));
class KnuckleDrum(BossModule module) : Components.CastCounter(module, (uint)AID.KnuckleDrum);
class KnuckleDrumLast(BossModule module) : Components.CastCounter(module, (uint)AID.KnuckleDrumLast);
class BlowAwayRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.BlowAwayRaidwide);
class BlowAwayPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlowAwayPuddle, 6);
class ImpactAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImpactAOE, 6);
class ImpactKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ImpactKnockback, 6);
class BlusteryRuler(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlusteryRuler, 6);
class DryBlowsRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.DryBlowsRaidwide);
class DryBlowsPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DryBlowsPuddle, 3);
class IronOut(BossModule module) : Components.CastCounter(module, (uint)AID.IronOutAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 871, NameID = 11398)]
public class Ex4Barbariccia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
