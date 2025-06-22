namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class SweepingImmolation(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SweepingImmolationSpread, (uint)AID.SweepingImmolationStack], new AOEShapeCone(20f, 90f.Degrees()));
class PartialTotalImmolation(BossModule module) : Components.CastStackSpread(module, (uint)AID.TotalImmolation, (uint)AID.PartialImmolation, 6f, 5f, 8, 8, true);
class ScaldingSignal(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScaldingSignal, 10f);
class ScaldingRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScaldingRing, new AOEShapeDonut(10f, 20f));
class ScaldingFleetFirst(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeRect(40f, 3f), (uint)AID.ScaldingFleetFirst);

// note: it seems to have incorrect target, but acts like self-targeted
class ScaldingFleetSecond(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScaldingFleetSecond, new AOEShapeRect(60f, 3f));
