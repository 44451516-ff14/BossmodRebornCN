namespace BossMod.Endwalker.VariantCriterion.V01SS.V014ZelessGah;

class Burn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burn, 12);
class PureFire2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PureFire2, 6);

class CastShadowFirst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CastShadowFirst, new AOEShapeCone(50, 45.Degrees()));
class CastShadowNext(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CastShadowNext, new AOEShapeCone(50, 45.Degrees()));

class FiresteelFracture(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FiresteelFracture, new AOEShapeCone(50, 45.Degrees()));

class InfernGaleKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Unknown2, 20, shape: new AOEShapeCircle(80));

class ShowOfStrength(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShowOfStrength);
class CastShadow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CastShadowFirst, (uint)AID.CastShadowNext], new AOEShapeCone(50f, 15f.Degrees()), 6, 12);

class BlazingBenifice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingBenifice, new AOEShapeRect(100, 5));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11394)]
public class V014ZelessGah(WorldState ws, Actor primary) : BossModule(ws, primary, new(289, -105), new ArenaBoundsRect(15, 20));
