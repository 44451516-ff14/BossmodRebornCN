namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

abstract class LateralSlice(BossModule module, uint aid) : Components.BaitAwayCast(module, aid, new AOEShapeCone(40f, 45f.Degrees())); // TODO: verify angle
class NLateralSlice(BossModule module) : LateralSlice(module, (uint)AID.NLateralSlice);
class SLateralSlice(BossModule module) : LateralSlice(module, (uint)AID.SLateralSlice);

public abstract class C023Moko(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(-200f, default);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.6f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12357, SortOrder = 5, PlanLevel = 90)]
public class C023NMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12357, SortOrder = 5, PlanLevel = 90)]
public class C023SMoko(WorldState ws, Actor primary) : C023Moko(ws, primary);
