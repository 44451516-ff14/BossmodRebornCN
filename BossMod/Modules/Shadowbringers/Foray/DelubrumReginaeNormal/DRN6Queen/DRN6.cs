namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN6Queen;

sealed class NorthswainsGlowPawnOff(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.NorthswainsGlowAOE, (uint)AID.PawnOffReal], 20f);
sealed class GodsSaveTheQueen(BossModule module) : Components.RaidwideCast(module, (uint)AID.GodsSaveTheQueen);
sealed class CleansingSlash(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CleansingSlash, "Tankbuster with doom");
sealed class JudgmentBlade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.JudgmentBladeL, (uint)AID.JudgmentBladeR], new AOEShapeRect(70f, 15f));
sealed class OptimalPlaySword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlaySword, 10f);
sealed class OptimalPlayShield(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimalPlayShield, new AOEShapeDonut(5f, 60f));
sealed class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9863)]
public sealed class DRN6Queen(WorldState ws, Actor primary) : Queen(ws, primary);
