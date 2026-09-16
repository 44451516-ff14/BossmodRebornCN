namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5Phantom;

sealed class MaledictionOfAgony(BossModule module) : Components.CastCounter(module, (uint)AID.MaledictionOfAgonyAOE);
sealed class BloodyWraith(BossModule module) : Components.Adds(module, (uint)OID.BloodyWraith);
sealed class MistyWraith(BossModule module) : Components.Adds(module, (uint)OID.MistyWraith);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761u, NameID = 9755u, PlanLevel = 80)]
public class DRS5Phantom(WorldState ws, Actor primary) : Phantom(ws, primary);
