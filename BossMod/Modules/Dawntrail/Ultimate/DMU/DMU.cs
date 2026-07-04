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

    private Actor? bossP3;
    public Actor? BossP3() => bossP3;
    private Actor? chaosP3;
    public Actor? ChaosP3() => chaosP3;
    private Actor? exdeathP3;
    public Actor? ExdeathP3() => exdeathP3;

    private Actor? kefkaP4;
    public Actor? KefkaP4() => kefkaP4;
    private Actor? neoExdeath;
    public Actor? NeoExdeath() => neoExdeath;
    private Actor? chaosP4;
    public Actor? ChaosP4() => chaosP4;

    private Actor? kefkaP5;
    public Actor? KefkaP5() => kefkaP5;

    protected override void UpdateModule() {
        bossP2 ??= Enemies((uint)OID.BossP2).FirstOrDefault();
        chaosP3 ??= Enemies((uint)OID.Chaos).FirstOrDefault();
        exdeathP3 ??= Enemies((uint)OID.Exdeath).FirstOrDefault();
        chaosP4 ??= Enemies((uint)OID.ChaosP4).FirstOrDefault();
        neoExdeath ??= Enemies((uint)OID.NeoExdeath).FirstOrDefault();

        if (StateMachine.ActivePhaseIndex == 2) {
            bossP3 ??= Enemies((uint)OID.Kefka).FirstOrDefault();
        }

        if (StateMachine.ActivePhaseIndex == 3) {
            kefkaP4 ??= Enemies((uint)OID.KefkaP4).FirstOrDefault();
        }

        if (StateMachine.ActivePhaseIndex == 4) {
            kefkaP5 ??= Enemies((uint)OID.KefkaP5).FirstOrDefault();
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actor(bossP2);
        Arena.Actor(chaosP3);
        Arena.Actor(exdeathP3);
        Arena.Actor(bossP3);
        Arena.Actor(kefkaP4);
        Arena.Actor(kefkaP5);
    }
}
