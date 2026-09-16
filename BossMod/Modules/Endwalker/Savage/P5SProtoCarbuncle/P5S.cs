namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

sealed class ToxicCrunch(BossModule module) : Components.CastCounter(module, (uint)AID.ToxicCrunchAOE); // TODO: improve component?
sealed class DoubleRush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.DoubleRush, 50);
sealed class DoubleRushReturn(BossModule module) : Components.CastCounter(module, (uint)AID.DoubleRushReturn); // TODO: show knockback?
sealed class SonicShatter(BossModule module) : Components.CastCounter(module, (uint)AID.SonicShatterRest);
sealed class DevourBait(BossModule module) : Components.CastCounter(module, (uint)AID.DevourBait);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 873u, NameID = 11440u, PlanLevel = 90)]
public sealed class P5S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(15f));
