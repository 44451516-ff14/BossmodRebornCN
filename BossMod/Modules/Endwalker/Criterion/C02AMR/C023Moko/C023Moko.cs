namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

abstract class LateralSlice(BossModule module, AID aid) : Components.BaitAwayCast(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 45.Degrees())); // TODO: verify angle
class NLateralSlice(BossModule module) : LateralSlice(module, AID.NLateralSlice);
class SLateralSlice(BossModule module) : LateralSlice(module, AID.SLateralSlice);

public abstract class C023Moko(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-200, 0);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.6f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12357, SortOrder = 8, PlanLevel = 90)]
public class C023NMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn, Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12357, SortOrder = 8, PlanLevel = 90)]
public class C023SMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);
