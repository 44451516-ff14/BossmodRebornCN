namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class BloodyScratch(BossModule module) : Components.RaidwideCast(module, (uint)AID.BloodyScratch);
sealed class BiscuitMaker(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BiscuitMaker);
sealed class Clawful(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Clawful, 5f, 8, 8);
sealed class Shockwave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Shockwave, 18, stopAfterWall: true);
sealed class GrimalkinGale(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GrimalkinGale, 5f);
sealed class Overshadow(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.OverShadowMarker, (uint)AID.Overshadow, 5.3f, 60f, 2.5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 985, NameID = 12686)]
public class M01NBlackCat(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.DefaultBounds);
