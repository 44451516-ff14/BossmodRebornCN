namespace BossMod.Endwalker.Alliance.A14Naldthal;

class GoldenTenet(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.GoldenTenetAOE), 6);
class StygianTenet(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.StygianTenetAOE), 6);

class HellOfFire(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(60, 90.Degrees()));
class HellOfFireFront(BossModule module) : HellOfFire(module, AID.HellOfFireFrontAOE);
class HellOfFireBack(BossModule module) : HellOfFire(module, AID.HellOfFireBackAOE);

class WaywardSoul(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WaywardSoulAOE), 18, 3);
class SoulVessel(BossModule module) : Components.Adds(module, (uint)OID.SoulVesselReal);
class Twingaze(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Twingaze), new AOEShapeCone(60, 15.Degrees()));
class MagmaticSpell(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MagmaticSpellAOE), 6, 8);
class TippedScales(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TippedScalesAOE));

// TODO: balancing counter
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11286, SortOrder = 6, PlanLevel = 90)]
public class A14Naldthal(WorldState ws, Actor primary) : BossModule(ws, primary, new(750, -750), new ArenaBoundsCircle(30));
