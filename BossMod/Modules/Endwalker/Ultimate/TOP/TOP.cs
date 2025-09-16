namespace BossMod.Endwalker.Ultimate.TOP;

sealed class SolarRayM(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SolarRayM, 5f);
sealed class SolarRayF(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SolarRayF, 5f);
sealed class P4BlueScreen(BossModule module) : Components.CastCounter(module, (uint)AID.BlueScreenAOE);
sealed class P5BlindFaith(BossModule module) : Components.CastHint(module, (uint)AID.BlindFaithSuccess, "Intermission");

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 908u, NameID = 12256u, PlanLevel = 90)]
public sealed class TOP(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private Actor? _opticalUnit;
    private Actor? _omegaM;
    private Actor? _omegaF;
    private Actor? _bossP3;
    private Actor? _bossP5;
    private Actor? _bossP6;
    public Actor? BossP1() => PrimaryActor;
    public Actor? OpticalUnit() => _opticalUnit; // we use this to distinguish P1 wipe vs P1 kill - primary actor can be destroyed before P2 bosses spawn
    public Actor? BossP2M() => _omegaM;
    public Actor? BossP2F() => _omegaF;
    public Actor? BossP3() => _bossP3;
    public Actor? BossP5() => _bossP5;
    public Actor? BossP6() => _bossP6;

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case 0:
                _opticalUnit ??= GetActor((uint)OID.OpticalUnit);
                break;
            case 1:
                _omegaF ??= GetActor((uint)OID.OmegaF);
                _omegaM ??= GetActor((uint)OID.OmegaM);
                break;
            case 2:
                _bossP3 ??= GetActor((uint)OID.BossP3);
                break;
            case 4:
                _bossP5 ??= GetActor((uint)OID.BossP5);
                break;
            case >= 4:
                _bossP6 ??= GetActor((uint)OID.BossP6);
                break;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_omegaM);
        Arena.Actor(_omegaF);
        Arena.Actor(_bossP3);
        Arena.Actor(_bossP5);
        Arena.Actor(_bossP6);
    }
}
