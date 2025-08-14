namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class WickedJolt(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WickedJolt, new AOEShapeRect(60f, 2.5f), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class WickedBolt(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, (uint)AID.WickedBolt, 5f, 5f, 8, 8, 5);
sealed class SoaringSoulpress(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SoaringSoulpress, (uint)AID.SoaringSoulpress, 6f, 5.4f, 8, 8);
sealed class WrathOfZeus(BossModule module) : Components.RaidwideCast(module, (uint)AID.WrathOfZeus);
sealed class BewitchingFlight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BewitchingFlight, new AOEShapeRect(40f, 2.5f));
sealed class Thunderslam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderslam, 5f);
sealed class Thunderstorm(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Thunderstorm, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 991, NameID = 13057)]
public class M04NWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.DefaultCenter, ArenaChanges.DefaultBounds);
