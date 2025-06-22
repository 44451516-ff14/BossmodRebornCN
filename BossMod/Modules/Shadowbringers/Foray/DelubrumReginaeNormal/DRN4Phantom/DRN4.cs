namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN4Phantom;

sealed class VileWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VileWave, new AOEShapeCone(45f, 60f.Degrees()));
sealed class CreepingMiasma(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CreepingMiasma, new AOEShapeRect(50f, 6f));
sealed class MaledictionOfAgony(BossModule module) : Components.RaidwideCast(module, (uint)AID.MaledictionOfAgony);
sealed class SwirlingMiasma(BossModule module) : Components.SimpleExaflare(module, new AOEShapeDonut(5f, 19f), (uint)AID.SwirlingMiasmaFirst, (uint)AID.SwirlingMiasmaRest, 6f, 1.6f, 8, 2, locationBased: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9755)]
public sealed class DRN4Phantom(WorldState ws, Actor primary) : Phantom(ws, primary);
