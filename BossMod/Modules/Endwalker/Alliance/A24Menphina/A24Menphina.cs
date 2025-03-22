namespace BossMod.Endwalker.Alliance.A24Menphina;

class BlueMoon(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlueMoonAOE));
class FirstBlush(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FirstBlush), new AOEShapeRect(80, 12.5f));
class SilverMirror(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SilverMirrorAOE), 7);
class Moonset(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.MoonsetAOE), 12);

class LoversBridge(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), 19);
class LoversBridgeShort(BossModule module) : LoversBridge(module, AID.LoversBridgeShort);
class LoversBridgeLong(BossModule module) : LoversBridge(module, AID.LoversBridgeLong);

class CeremonialPillar(BossModule module) : Components.Adds(module, (uint)OID.CeremonialPillar);
class AncientBlizzard(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AncientBlizzard), new AOEShapeCone(45, 22.5f.Degrees()));
class KeenMoonbeam(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.KeenMoonbeamAOE), 6);
class RiseOfTheTwinMoons(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RiseOfTheTwinMoons));
class CrateringChill(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.CrateringChillAOE), 20);
class MoonsetRays(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MoonsetRaysAOE), 6, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12063, PlanLevel = 90)]
public class A24Menphina(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 750), new ArenaBoundsCircle(25));
