namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P2QuadrupleSlap(BossModule module) : Components.TankSwap(module, (uint)AID.QuadrupleSlapFirst, (uint)AID.QuadrupleSlapFirst, (uint)AID.QuadrupleSlapSecond, 4.1f, null, true);
sealed class P3Junction(BossModule module) : Components.CastCounter(module, (uint)AID.Junction);
sealed class P3BlackHalo(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.BlackHalo, new AOEShapeCone(60f, 45f.Degrees())); // TODO: verify angle

sealed class P4HallowedWings(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HallowedWingsL, (uint)AID.HallowedWingsR], new AOEShapeRect(80f, 20f));

sealed class P5ParadiseLost(BossModule module) : Components.CastCounter(module, (uint)AID.ParadiseLostP5AOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1006, NameID = 9707, PlanLevel = 100)]
public sealed class FRU(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(100f, 100f), 20f, 64)]) { IsCircle = true };
    public static readonly ArenaBoundsSquare PathfindHugBorderBounds = new(20f); // this is a hack to allow precise positioning near border by some mechanics, TODO reconsider

    private Actor? _bossP2;
    private Actor? _iceVeil;
    private Actor? _bossP3;
    private Actor? _bossP4Usurper;
    private Actor? _bossP4Oracle;
    private Actor? _bossP5;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;
    public Actor? IceVeil() => _iceVeil;
    public Actor? BossP3() => _bossP3;
    public Actor? BossP4Usurper() => _bossP4Usurper;
    public Actor? BossP4Oracle() => _bossP4Oracle;
    public Actor? BossP5() => _bossP5;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_bossP2 == null)
        {
            if (StateMachine.ActivePhaseIndex == 1)
            {
                var b = Enemies((uint)OID.BossP2);
                _bossP2 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_iceVeil == null)
        {
            if (StateMachine.ActivePhaseIndex == 1)
            {
                var b = Enemies((uint)OID.IceVeil);
                _iceVeil = b.Count != 0 ? b[0] : null;
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
        if (_bossP4Usurper == null)
        {
            if (StateMachine.ActivePhaseIndex == 2)
            {
                var b = Enemies((uint)OID.UsurperOfFrostP4);
                _bossP4Usurper = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP4Oracle == null)
        {
            if (StateMachine.ActivePhaseIndex == 2)
            {
                var b = Enemies((uint)OID.OracleOfDarknessP4);
                _bossP4Oracle = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP5 == null)
        {
            if (StateMachine.ActivePhaseIndex == 3)
            {
                var b = Enemies((uint)OID.BossP5);
                _bossP5 = b.Count != 0 ? b[0] : null;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
        Arena.Actor(_bossP3);
        Arena.Actor(_bossP4Usurper);
        Arena.Actor(_bossP4Oracle);
        Arena.Actor(_bossP5);
    }
}
