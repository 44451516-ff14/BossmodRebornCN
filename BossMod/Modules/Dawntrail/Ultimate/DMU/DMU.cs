namespace BossMod.Dawntrail.Ultimate.DMU;

class LightOfJudgment(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightOfJudgment);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(DMUStates),
    ConfigType = typeof(DMUConfig),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Kefka,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Ultimate,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1094u,
    NameID = 7131u,
    SortOrder = 1,
    PlanLevel = 100)]
[SkipLocalsInit]
public sealed class DMU(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f)) {
    public override bool ShouldPrioritizeAllEnemies => true;

    //private Actor? bossP1;
    public Actor? BossP1() => PrimaryActor;

    private Actor? bossP2;
    public Actor? BossP2() => bossP2;

    protected override void UpdateModule()
    {
        bossP2 ??= Enemies((uint)OID.BossP2).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actor(bossP2);
    }
}
