namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class DestructiveStatic(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DestructiveStatic), new AOEShapeCone(50f, 90f.Degrees()));
class LightningBolt(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.LightningBoltAOE), 6f);
class BoltsFromTheBlue(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BoltsFromTheBlueAOE));
class DestructiveStrike(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.DestructiveStrike), new AOEShapeCone(13f, 60f.Degrees()), endsOnCastEvent: true, tankbuster: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11274, SortOrder = 2, PlanLevel = 90)]
public class A10RhalgrEmissary(WorldState ws, Actor primary) : BossModule(ws, primary, new(74f, 516f), new ArenaBoundsCircle(25));
