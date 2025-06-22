namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

abstract class SplittingCry(BossModule module, uint aid) : Components.BaitAwayCast(module, aid, new AOEShapeRect(60f, 7f));
class NSplittingCry(BossModule module) : SplittingCry(module, (uint)AID.NSplittingCry);
class SSplittingCry(BossModule module) : SplittingCry(module, (uint)AID.SSplittingCry);

abstract class ThunderVortex(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(8f, 30f));
class NThunderVortex(BossModule module) : ThunderVortex(module, (uint)AID.NThunderVortex);
class SThunderVortex(BossModule module) : ThunderVortex(module, (uint)AID.SThunderVortex);

public abstract class C021Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(default, -100f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20f);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Polygon(ArenaCenter, 20f, 64)]);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12428, SortOrder = 2, PlanLevel = 90)]
public class C021NShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12428, SortOrder = 2, PlanLevel = 90)]
public class C021SShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);
