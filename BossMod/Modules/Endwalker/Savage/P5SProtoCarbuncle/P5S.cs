namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class ToxicCrunch(BossModule module) : Components.CastCounter(module, (uint)AID.ToxicCrunchAOE); // TODO: improve component?
class DoubleRush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.DoubleRush, 50);
class DoubleRushReturn(BossModule module) : Components.CastCounter(module, (uint)AID.DoubleRushReturn); // TODO: show knockback?
class SonicShatter(BossModule module) : Components.CastCounter(module, (uint)AID.SonicShatterRest);
class DevourBait(BossModule module) : Components.CastCounter(module, (uint)AID.DevourBait);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 873, NameID = 11440, PlanLevel = 90)]
public class P5S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));
