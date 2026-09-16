namespace BossMod.Endwalker.Unreal.Un3Sophia;

sealed class ThunderDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderDonut, new AOEShapeDonut(5f, 20f));
sealed class ExecuteDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecuteDonut, new AOEShapeDonut(5f, 20f));
sealed class Aero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aero, 10f);
sealed class ExecuteAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecuteAero, 10f);
sealed class ThunderCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderCone, new AOEShapeCone(20f, 45f.Degrees()));
sealed class ExecuteCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExecuteCone, new AOEShapeCone(20f, 45f.Degrees()));
sealed class LightDewShort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightDewShort, new AOEShapeRect(55f, 9f));
sealed class LightDewLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightDewLong, new AOEShapeRect(55f, 9f));
sealed class Onrush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Onrush, new AOEShapeRect(55f, 8f));
sealed class Gnosis(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Gnosis, 25f);
sealed class Cintamani(BossModule module) : Components.CastCounter(module, (uint)AID.Cintamani); // note: ~4.2s before first cast boss gets model state 5
sealed class QuasarProximity1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuasarProximity1, 15f);
sealed class QuasarProximity2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuasarProximity2, 15f); // TODO: reconsider distance

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 926u, NameID = 5199u, PlanLevel = 90)]
public class Un3Sophia(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsRect(20f, 15f));
