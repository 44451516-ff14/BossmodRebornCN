namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class DestructiveStatic(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DestructiveStatic, new AOEShapeCone(50f, 90f.Degrees()));
class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBoltAOE, 6f);
class BoltsFromTheBlue(BossModule module) : Components.CastCounter(module, (uint)AID.BoltsFromTheBlueAOE);
class DestructiveStrike(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DestructiveStrike, new AOEShapeCone(13f, 60f.Degrees()), endsOnCastEvent: true, tankbuster: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11274, SortOrder = 2, PlanLevel = 90)]
public class A10RhalgrEmissary(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(73.941f, 516.059f), 24.5f * CosPI.Pi148th, 148)], [new Rectangle(new(91.918f, 498.082f), 20f, 1.25f, -45f.Degrees()),
    new Rectangle(new(74f, 541.4f), 20f, 1.25f)]);
}
