namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class ShatteringHeatBoss(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ShatteringHeatBoss, 4f);
class BlazingRapture(BossModule module) : Components.CastCounter(module, (uint)AID.BlazingRaptureAOE);
class InfernoSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.InfernoSpreadAOE, 5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 924, NameID = 12057, PlanLevel = 90)]
public class Ex5Rubicante(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(100f, 100f), 20f, 64)]);
}
