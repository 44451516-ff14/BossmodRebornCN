namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

abstract class TidalRoar(BossModule module, uint aid) : Components.CastCounter(module, aid);
class NTidalRoar(BossModule module) : TidalRoar(module, (uint)AID.NTidalRoarAOE);
class STidalRoar(BossModule module) : TidalRoar(module, (uint)AID.STidalRoarAOE);

abstract class BubbleNet(BossModule module, uint aid) : Components.CastCounter(module, aid);
class NBubbleNet1(BossModule module) : BubbleNet(module, (uint)AID.NBubbleNet1AOE);
class SBubbleNet1(BossModule module) : BubbleNet(module, (uint)AID.SBubbleNet1AOE);
class NBubbleNet2(BossModule module) : BubbleNet(module, (uint)AID.NBubbleNet2AOE);
class SBubbleNet2(BossModule module) : BubbleNet(module, (uint)AID.SBubbleNet2AOE);

abstract class Hydrobomb(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 5f);
class NHydrobomb(BossModule module) : Hydrobomb(module, (uint)AID.NHydrobombAOE);
class SHydrobomb(BossModule module) : Hydrobomb(module, (uint)AID.SHydrobombAOE);

public abstract class C031Ketuduke(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(20f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979, NameID = 12605, SortOrder = 5, PlanLevel = 90)]
public class C031NKetuduke(WorldState ws, Actor primary) : C031Ketuduke(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980, NameID = 12605, SortOrder = 5, PlanLevel = 90)]
public class C031SKetuduke(WorldState ws, Actor primary) : C031Ketuduke(ws, primary);
