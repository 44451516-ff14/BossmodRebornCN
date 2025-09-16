namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class DividingWings(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60f, 60f.Degrees()), (uint)TetherID.DividingWings, (uint)AID.DividingWingsAOE);
class PandaemonsHoly(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PandaemonsHoly, 36f);

// note: origin seems to be weird?
class CirclesOfPandaemonium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CirclesOfPandaemonium, new AOEShapeDonut(12f, 40f));

class Imprisonment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImprisonmentAOE, 4f);
class Cannonspawn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CannonspawnAOE, new AOEShapeDonut(3f, 8f));
class PealOfDamnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PealOfDamnation, new AOEShapeRect(50f, 3.5f));
class PandaemoniacPillars(BossModule module) : Components.CastTowers(module, (uint)AID.Bury, 2f);
class Touchdown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TouchdownAOE, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 939, NameID = 12354, PlanLevel = 90)]
public class P10SPandaemonium(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    private static readonly Rectangle[] union = [new(new(100f, 100f), 13f, 15f), new(new(125f, 85f), 4f, 15f), new(new(75f, 85f), 4f, 15f)];
    private static readonly Rectangle[] bridgeL = [new(new(83f, 92.5f), 4f, 1f)];
    private static readonly Rectangle[] bridgeR = [new(new(117f, 92.5f), 4f, 1f)];
    public static readonly ArenaBoundsCustom DefaultArena = new(union);
    public static readonly ArenaBoundsCustom ArenaL = new([.. union, .. bridgeL]);
    public static readonly ArenaBoundsCustom ArenaR = new([.. union, .. bridgeR]);
    public static readonly ArenaBoundsCustom ArenaLR = new([.. union, .. bridgeL, .. bridgeR]);
}
