namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotWard(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ByregotWard, new AOEShapeCone(10f, 45f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11281, SortOrder = 1, PlanLevel = 90)]
public class A11Byregot(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(default, 700f);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    public static readonly ArenaBoundsSquare DefaultBounds = new(24f);
    public static readonly ArenaBoundsRect StartingHammerBounds = new(15, 25);
}
