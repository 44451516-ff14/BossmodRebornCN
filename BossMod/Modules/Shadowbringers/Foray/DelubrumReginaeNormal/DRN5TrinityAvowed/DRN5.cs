namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class WrathOfBozja(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WrathOfBozja, new AOEShapeCone(60f, 45f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class GleamingArrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GleamingArrow, new AOEShapeRect(60f, 5f));
sealed class GloryOfBozja(BossModule module) : Components.RaidwideCast(module, (uint)AID.GloryOfBozja);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9853)]
public sealed class DRN5TrinityAvowed(WorldState ws, Actor primary) : TrinityAvowed(ws, primary);
