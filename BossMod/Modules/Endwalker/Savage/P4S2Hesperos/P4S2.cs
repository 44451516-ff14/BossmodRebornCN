namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to demigod double mechanic (shared tankbuster)
sealed class DemigodDouble(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.DemigodDouble, 6);

// state related to heart stake mechanic (dual hit tankbuster with bleed)
// TODO: consider showing some tank swap / invul hint...
sealed class HeartStake(BossModule module) : Components.CastCounter(module, (uint)AID.HeartStakeSecond);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 801u, NameID = 10744u, SortOrder = 2, PlanLevel = 90)]
public sealed class P4S2(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    // common wreath of thorns constants
    public const float WreathAOERadius = 20f;
    public const float WreathTowerRadius = 4f;
}
