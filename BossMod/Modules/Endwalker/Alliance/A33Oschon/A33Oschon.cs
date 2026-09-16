namespace BossMod.Endwalker.Alliance.A33Oschon;

sealed class P1SuddenDownpour(BossModule module) : Components.CastCounter(module, (uint)AID.SuddenDownpourAOE);

sealed class P1TrekShot(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TrekShotNAOE, (uint)AID.TrekShotSAOE], new AOEShapeCone(65f, 60f.Degrees()));

abstract class SoaringMinuet(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(65f, 135f.Degrees()));
sealed class P1SoaringMinuet1(BossModule module) : SoaringMinuet(module, (uint)AID.SoaringMinuet1);
sealed class P1SoaringMinuet2(BossModule module) : SoaringMinuet(module, (uint)AID.SoaringMinuet2);

sealed class P1Arrow(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ArrowP1AOE, 6f);
sealed class P1Downhill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DownhillP1AOE, 6f);
sealed class P2MovingMountains(BossModule module) : Components.CastCounter(module, (uint)AID.MovingMountains);
sealed class P2PeakPeril(BossModule module) : Components.CastCounter(module, (uint)AID.PeakPeril);
sealed class P2Shockwave(BossModule module) : Components.CastCounter(module, (uint)AID.Shockwave);
sealed class P2SuddenDownpour(BossModule module) : Components.CastCounter(module, (uint)AID.P2SuddenDownpourAOE);

sealed class P2PitonPull(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PitonPullAOE, 22f);
sealed class P2Altitude(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AltitudeAOE, 6f);
sealed class P2Arrow(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ArrowP2AOE, 10f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962u, NameID = 11300u, SortOrder = 4, PlanLevel = 90)]
public class A33Oschon(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, 750f), new ArenaBoundsSquare(25f) { Y = 50f, BorderY = 50f })
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        if (StateMachine.ActivePhaseIndex == 1)
        {
            _bossP2 ??= GetActor((uint)OID.BossP2);
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
