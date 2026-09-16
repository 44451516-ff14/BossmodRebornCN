namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

sealed class DestructiveStatic(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DestructiveStatic, new AOEShapeCone(50f, 90f.Degrees()));
sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBoltAOE, 6f);
sealed class BoltsFromTheBlue(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.BoltsFromTheBlue, (uint)AID.BoltsFromTheBlueAOE, 1d);
sealed class DestructiveStrike(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DestructiveStrike, new AOEShapeCone(13f, 60f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866u, NameID = 11274u, SortOrder = 2, PlanLevel = 90)]
public sealed class A10RhalgrEmissary : BossModule
{
    public A10RhalgrEmissary(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A10RhalgrEmissary(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(73.941f, 516.059f), 24.5f * CosPI.Pi148th, 148)], [new Rectangle(new(91.918f, 498.082f), 20f, 1.25f, -45f.Degrees()),
            new Rectangle(new(74f, 541.4f), 20f, 1.25f)]);
        return (arena.Center, arena);
    }
}
