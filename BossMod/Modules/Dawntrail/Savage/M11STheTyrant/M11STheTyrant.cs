namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class CrownOfArcadia(BossModule module) : Components.RaidwideCast(module, (uint)AID.CrownOfArcadia);
sealed class UltimateTrophyWeapons(BossModule module) : Components.CastHint(module, (uint)AID.UltimateTrophyWeapons, "Ultimate Trophy Weapons");

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.Boss, Contributors = "Topas", GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1073u, NameID = 14305u, SortOrder = 1, PlanLevel = 0)]

public sealed class M11STheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.InitialBounds);