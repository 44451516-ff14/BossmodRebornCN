namespace BossMod.Endwalker.Ultimate.TOP;

sealed class SolarRayM(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SolarRayM, 5f);
sealed class SolarRayF(BossModule module) : Components.BaitAwayCast(module, (uint)AID.SolarRayF, 5f);
sealed class P4BlueScreen(BossModule module) : Components.CastCounter(module, (uint)AID.BlueScreenAOE);
sealed class P5BlindFaith(BossModule module) : Components.CastHint(module, (uint)AID.BlindFaithSuccess, "Intermission");

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 908, PlanLevel = 90)]
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
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_opticalUnit == null)
        {
            if (StateMachine.ActivePhaseIndex == 0)
            {
                var b = Enemies((uint)OID.OpticalUnit);
                _opticalUnit = b.Count != 0 ? b[0] : null;
            }
        }
        if (_omegaM == null)
        {
            if (StateMachine.ActivePhaseIndex == 1)
            {
                var b = Enemies((uint)OID.OmegaM);
                _omegaM = b.Count != 0 ? b[0] : null;
            }
        }
        if (_omegaF == null)
        {
            if (StateMachine.ActivePhaseIndex == 1)
            {
                var b = Enemies((uint)OID.OmegaF);
                _omegaF = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP3 == null)
        {
            if (StateMachine.ActivePhaseIndex == 2)
            {
                var b = Enemies((uint)OID.BossP3);
                _bossP3 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP5 == null)
        {
            if (StateMachine.ActivePhaseIndex == 4)
            {
                var b = Enemies((uint)OID.BossP5);
                _bossP5 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP6 == null)
        {
            if (StateMachine.ActivePhaseIndex >= 4)
            {
                var b = Enemies((uint)OID.BossP6);
                _bossP6 = b.Count != 0 ? b[0] : null;
            }
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
