namespace BossMod.Endwalker.Savage.P6SHegemone;

sealed class UnholyDarkness(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.UnholyDarknessAOE, 6f, 8, 8);
sealed class DarkDome(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkDomeAOE, 5f);
sealed class DarkAshes(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkAshesAOE, 6f);
sealed class DarkSphere(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkSphereAOE, 10f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 881u, NameID = 11381u, PlanLevel = 90)]
public sealed class P6S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
