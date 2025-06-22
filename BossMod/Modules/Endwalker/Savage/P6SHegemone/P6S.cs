namespace BossMod.Endwalker.Savage.P6SHegemone;

class UnholyDarkness(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.UnholyDarknessAOE, 6, 8, 8);
class DarkDome(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkDomeAOE, 5);
class DarkAshes(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkAshesAOE, 6);
class DarkSphere(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkSphereAOE, 10);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 881, NameID = 11381, PlanLevel = 90)]
public class P6S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
