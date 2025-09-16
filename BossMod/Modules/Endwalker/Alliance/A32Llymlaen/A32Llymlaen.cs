namespace BossMod.Endwalker.Alliance.A32Llymlaen;

sealed class WindRose(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindRose, 12f);
sealed class SeafoamSpiral(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeafoamSpiral, new AOEShapeDonut(6f, 70f));
sealed class DeepDiveNormal(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DeepDiveNormal, 6f, 8);
sealed class Stormwhorl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Stormwhorl, 6f);
sealed class Stormwinds(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Stormwinds, 6f);
sealed class Maelstrom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Maelstrom, 6f);
sealed class Godsbane(BossModule module) : Components.CastCounter(module, (uint)AID.GodsbaneAOE);
sealed class DeepDiveHardWater(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.DeepDiveHardWater, 6f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11299, SortOrder = 3, PlanLevel = 90)]
public sealed class A32Llymlaen(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, DefaultBounds)
{
    public static readonly WPos DefaultCenter = new(default, -900f);
    public static readonly ArenaBoundsRect DefaultBounds = new(19f, 29f);
    private static readonly Rectangle defaultRect = new(DefaultCenter, 19f, 29f);
    public static readonly ArenaBoundsCustom EastCorridorBounds = new([defaultRect, new Rectangle(DefaultCenter + new WDir(39f, default), 40f, 10f)], ScaleFactor: 1.5f);
    public static readonly ArenaBoundsCustom WestCorridorBounds = new([defaultRect, new Rectangle(DefaultCenter + new WDir(-39f, default), 40f, 10f)], ScaleFactor: 1.5f);
}
