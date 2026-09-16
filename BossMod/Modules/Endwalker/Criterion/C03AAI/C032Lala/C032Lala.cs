namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

abstract class ArcaneBlight(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 135f.Degrees()));
sealed class NArcaneBlight(BossModule module) : ArcaneBlight(module, (uint)AID.NArcaneBlightAOE);
sealed class SArcaneBlight(BossModule module) : ArcaneBlight(module, (uint)AID.SArcaneBlightAOE);

public abstract class C032Lala(WorldState ws, Actor primary) : BossModule(ws, primary, new(200f, 0f), new ArenaBoundsSquare(20f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979u, NameID = 12639u, SortOrder = 8, PlanLevel = 90)]
public sealed class C032NLala(WorldState ws, Actor primary) : C032Lala(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980u, NameID = 12639u, SortOrder = 8, PlanLevel = 90)]
public sealed class C032SLala(WorldState ws, Actor primary) : C032Lala(ws, primary);
