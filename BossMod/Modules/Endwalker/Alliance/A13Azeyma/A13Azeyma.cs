namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WardensWarmth(BossModule module) : Components.BaitAwayCast(module, (uint)AID.WardensWarmthAOE, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
class FleetingSpark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FleetingSpark, new AOEShapeCone(60f, 135f.Degrees()));
class SolarFold(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SolarFoldAOE, new AOEShapeCross(30f, 5f));
class Sunbeam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sunbeam, 9f, 14);
class SublimeSunset(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SublimeSunsetAOE, 40); // TODO: check falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11277, SortOrder = 5, PlanLevel = 90)]
public class A13Azeyma(WorldState ws, Actor primary) : BossModule(ws, primary, NormalBounds.Center, NormalBounds)
{
    public static readonly WPos NormalCenter = new(-750f, -750f);
    public static readonly ArenaBoundsCustom NormalBounds = new([new Polygon(NormalCenter, 29.5f, 180)], [new Rectangle(new(-750f, -719.981f), 20f, 1.25f),
    new Rectangle(new(-750f, -779.985f), 20f, 1.25f)]);
}
