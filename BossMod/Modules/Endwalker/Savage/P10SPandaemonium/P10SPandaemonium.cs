namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class DividingWings(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60f, 60f.Degrees()), (uint)TetherID.DividingWings, (uint)AID.DividingWingsAOE);
sealed class PandaemonsHoly(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PandaemonsHoly, 36f);

// note: origin seems to be weird?
sealed class CirclesOfPandaemonium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CirclesOfPandaemonium, new AOEShapeDonut(12f, 40f));

sealed class Imprisonment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImprisonmentAOE, 4f);
sealed class Cannonspawn(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CannonspawnAOE, new AOEShapeDonut(3f, 8f));
sealed class PealOfDamnation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PealOfDamnation, new AOEShapeRect(50f, 3.5f));
sealed class PandaemoniacPillars(BossModule module) : Components.CastTowers(module, (uint)AID.Bury, 2f);
sealed class Touchdown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TouchdownAOE, 20f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 939u, NameID = 12354u, PlanLevel = 90)]
public sealed class P10SPandaemonium : BossModule
{
    public P10SPandaemonium(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private P10SPandaemonium(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static Rectangle[] GetUnion() => [new(new(100f, 100f), 13f, 15f), new(new(125f, 85f), 4f, 15f), new(new(75f, 85f), 4f, 15f)];
    private static Rectangle[] GetBridgeL() => [new(new(83f, 92.5f), 4f, 1f)];
    private static Rectangle[] GetBridgeR() => [new(new(117f, 92.5f), 4f, 1f)];
    public static ArenaBoundsCustom GetDefaultArena() => new(GetUnion());
    public static ArenaBoundsCustom GetArenaL() => new([.. GetUnion(), .. GetBridgeL()]);
    public static ArenaBoundsCustom GetArenaR() => new([.. GetUnion(), .. GetBridgeR()]);
    public static ArenaBoundsCustom GetArenaLR() => new([.. GetUnion(), .. GetBridgeL(), .. GetBridgeR()]);

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = GetDefaultArena();
        return (arena.Center, arena);
    }
}
