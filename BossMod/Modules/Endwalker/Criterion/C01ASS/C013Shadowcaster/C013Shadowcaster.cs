namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

abstract class FiresteelFracture(BossModule module, AID aid) : Components.Cleave(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 30.Degrees()));
class NFiresteelFracture(BossModule module) : FiresteelFracture(module, AID.NFiresteelFracture);
class SFiresteelFracture(BossModule module) : FiresteelFracture(module, AID.SFiresteelFracture);

abstract class PureFire(BossModule module, AID aid) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(aid), 6);
class NPureFire(BossModule module) : PureFire(module, AID.NPureFireAOE);
class SPureFire(BossModule module) : PureFire(module, AID.SPureFireAOE);

public abstract class C013Shadowcaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(289, -105), new ArenaBoundsRect(15, 20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11393, SortOrder = 9, PlanLevel = 90)]
public class C013NShadowcaster(WorldState ws, Actor primary) : C013Shadowcaster(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11393, SortOrder = 9, PlanLevel = 90)]
public class C013SShadowcaster(WorldState ws, Actor primary) : C013Shadowcaster(ws, primary);
