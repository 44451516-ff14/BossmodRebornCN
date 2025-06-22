namespace BossMod.Endwalker.VariantCriterion.V01SS.V015ThorneKnight;

class BlisteringBlow(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BlisteringBlow);

class BlazingBeacon1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingBeacon1, new AOEShapeRect(50, 16));
class BlazingBeacon2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingBeacon2, new AOEShapeRect(50, 16));
class BlazingBeacon3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingBeacon3, new AOEShapeRect(50, 16));

class SignalFlareAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SignalFlareAOE, 10);

class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeCross(50, 6));

class SacredFlay1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SacredFlay1, new AOEShapeCone(50, 50.Degrees()));
class SacredFlay2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SacredFlay2, new AOEShapeCone(50, 50.Degrees()));
class SacredFlay3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SacredFlay3, new AOEShapeCone(50, 50.Degrees()));

class ForeHonor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ForeHonor, new AOEShapeCone(50, 90.Degrees()));

class Cogwheel(BossModule module) : Components.RaidwideCast(module, (uint)AID.Cogwheel);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11419)]
public class V015ThorneKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new(289, -230), new ArenaBoundsRect(15, 15, 45.Degrees()));