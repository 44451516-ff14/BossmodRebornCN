namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class SplittingCry(BossModule module, AID aid) : Components.BaitAwayCast(module, ActionID.MakeSpell(aid), new AOEShapeRect(60, 7));
class NSplittingCry(BossModule module) : SplittingCry(module, AID.NSplittingCry);
class SSplittingCry(BossModule module) : SplittingCry(module, AID.SSplittingCry);

abstract class ThunderVortex(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(8, 30));
class NThunderVortex(BossModule module) : ThunderVortex(module, AID.NThunderVortex);
class SThunderVortex(BossModule module) : ThunderVortex(module, AID.SThunderVortex);

public abstract class C021Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(0, -100);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    public static readonly ArenaBoundsCircle CircleBounds = new(20);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12428, SortOrder = 4, PlanLevel = 90)]
public class C021NShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12428, SortOrder = 4, PlanLevel = 90)]
public class C021SShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);
