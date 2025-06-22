namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

sealed class BrutalImpact(BossModule module) : Components.CastCounter(module, (uint)AID.BrutalImpactAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 990, NameID = 13356, PlanLevel = 100)]
public sealed class M03SBruteBomber(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, DefaultBounds)
{
    private static readonly WPos arenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(15f);
    public static readonly ArenaBoundsComplex FuseFieldBounds = new([new Square(arenaCenter, 15f)], [new Polygon(arenaCenter, 5f, 80)]);
}
