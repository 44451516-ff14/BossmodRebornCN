namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

sealed class ShatteringHeatBoss(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShatteringHeatBoss, 4f);
sealed class BlazingRapture(BossModule module) : Components.CastCounter(module, (uint)AID.BlazingRaptureAOE);
sealed class InfernoSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.InfernoSpreadAOE, 5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 924u, NameID = 12057u, PlanLevel = 90)]
public sealed class Ex5Rubicante : BossModule
{
    public Ex5Rubicante(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Ex5Rubicante(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 20f, 64)]);
        return (arena.Center, arena);
    }
}
