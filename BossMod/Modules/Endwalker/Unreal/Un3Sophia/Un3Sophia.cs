namespace BossMod.Endwalker.Unreal.Un3Sophia;

class ThunderDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderDonut, new AOEShapeDonut(5, 20));
class ExecuteDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecuteDonut, new AOEShapeDonut(5, 20));
class Aero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aero, 10);
class ExecuteAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecuteAero, 10);
class ThunderCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderCone, new AOEShapeCone(20, 45.Degrees()));
class ExecuteCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecuteCone, new AOEShapeCone(20, 45.Degrees()));
class LightDewShort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightDewShort, new AOEShapeRect(55, 9));
class LightDewLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightDewLong, new AOEShapeRect(55, 9));
class Onrush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Onrush, new AOEShapeRect(55, 8));
class Gnosis(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Gnosis, 25);
class Cintamani(BossModule module) : Components.CastCounter(module, (uint)AID.Cintamani); // note: ~4.2s before first cast boss gets model state 5
class QuasarProximity1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuasarProximity1, 15);
class QuasarProximity2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuasarProximity2, 15); // TODO: reconsider distance

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 926, NameID = 5199, PlanLevel = 90)]
public class Un3Sophia(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsRect(20, 15));
