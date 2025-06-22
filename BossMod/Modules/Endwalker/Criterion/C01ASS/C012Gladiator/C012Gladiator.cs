namespace BossMod.Endwalker.VariantCriterion.C01ASS.C012Gladiator;

abstract class RushOfMightFront(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 90f.Degrees()));
sealed class NRushOfMightFront(BossModule module) : RushOfMightFront(module, (uint)AID.NRushOfMightFront);
sealed class SRushOfMightFront(BossModule module) : RushOfMightFront(module, (uint)AID.SRushOfMightFront);

abstract class RushOfMightBack(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 90f.Degrees()));
sealed class NRushOfMightBack(BossModule module) : RushOfMightBack(module, (uint)AID.NRushOfMightBack);
sealed class SRushOfMightBack(BossModule module) : RushOfMightBack(module, (uint)AID.SRushOfMightBack);

public abstract class C012Gladiator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35f, -271f), new ArenaBoundsSquare(19.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11387, SortOrder = 4, PlanLevel = 90)]
public sealed class C012NGladiator(WorldState ws, Actor primary) : C012Gladiator(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11387, SortOrder = 4, PlanLevel = 90)]
public sealed class C012SGladiator(WorldState ws, Actor primary) : C012Gladiator(ws, primary);
