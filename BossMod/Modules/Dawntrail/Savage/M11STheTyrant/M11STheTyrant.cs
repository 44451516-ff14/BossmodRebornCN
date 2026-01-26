namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class CrownOfArcadia(BossModule module) : Components.RaidwideCast(module, (uint)AID.CrownOfArcadia);
sealed class UltimateTrophyWeapons(BossModule module) : Components.CastHint(module, (uint)AID.UltimateTrophyWeapons, "Ultimate Trophy Weapons");

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(M11STheTyrantStates),
ConfigType = null, // replace null with typeof(TheTyrantConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
StatusIDType = null, // replace null with typeof(SID) if applicable
TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
PrimaryActorOID = (uint)OID.Boss,
Contributors = "Topas",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Savage,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1073u,
NameID = 14305u,
SortOrder = 1,
PlanLevel = 0)]

public sealed class M11STheTyrant(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.InitialBounds);