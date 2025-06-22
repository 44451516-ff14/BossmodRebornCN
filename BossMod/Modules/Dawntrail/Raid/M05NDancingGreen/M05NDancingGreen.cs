namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class DoTheHustle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DoTheHustle1, (uint)AID.DoTheHustle2], new AOEShapeCone(50f, 90f.Degrees()));
sealed class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 22.5f.Degrees()), (uint)IconID.DeepCut, (uint)AID.DeepCut, 5f, tankbuster: true);
sealed class FullBeat(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FullBeat, 6f, 8, 8);
sealed class CelebrateGoodTimesDiscoInfernalLetsPose(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.CelebrateGoodTimes, (uint)AID.DiscoInfernal,
(uint)AID.LetsPose1, (uint)AID.LetsPose2]);
sealed class EighthBeats(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.EighthBeats, 5f);
sealed class Moonburn(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Moonburn1, (uint)AID.Moonburn2], new AOEShapeRect(40f, 7.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1019, NameID = 13778)]
public sealed class M05NDancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
