namespace BossMod.Dawntrail.Savage.M01SBlackCat;

sealed class BiscuitMaker(BossModule module) : Components.TankSwap(module, (uint)AID.BiscuitMaker, (uint)AID.BiscuitMaker, (uint)AID.BiscuitMakerSecond, default, 2d);
sealed class QuadrupleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.QuadrupleSwipeBossAOE, 4f, 2, 2);
sealed class DoubleSwipeBoss(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DoubleSwipeBossAOE, 5f, 4, 4);
sealed class QuadrupleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.QuadrupleSwipeShadeAOE, 4f, 2, 2);
sealed class DoubleSwipeShade(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DoubleSwipeShadeAOE, 5f, 4, 4);
sealed class Nailchipper(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.NailchipperAOE, 5f);
sealed class TempestuousTear(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.TempestuousTearTargetSelect, (uint)AID.TempestuousTearAOE, 5d, 100f, 3f, 1, 4);
sealed class Overshadow(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.OvershadowTargetSelect, (uint)AID.OvershadowAOE, 5.1d, 100f, 2.5f, 7, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 986, NameID = 12686, PlanLevel = 100)]
public sealed class M01SBlackCat(WorldState ws, Actor primary) : Raid.M01NBlackCat.M01NBlackCat(ws, primary);
