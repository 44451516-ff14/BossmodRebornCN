namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1SuddenDownpour(BossModule module) : Components.CastCounter(module, (uint)AID.SuddenDownpourAOE);

class TrekShot(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(65f, 60f.Degrees()));
class P1TrekShotN(BossModule module) : TrekShot(module, (uint)AID.TrekShotNAOE);
class P1TrekShotS(BossModule module) : TrekShot(module, (uint)AID.TrekShotSAOE);

class SoaringMinuet(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(65f, 135f.Degrees()));
class P1SoaringMinuet1(BossModule module) : SoaringMinuet(module, (uint)AID.SoaringMinuet1);
class P1SoaringMinuet2(BossModule module) : SoaringMinuet(module, (uint)AID.SoaringMinuet2);

class P1Arrow(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ArrowP1AOE, 6f);
class P1Downhill(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DownhillP1AOE, 6f);
class P2MovingMountains(BossModule module) : Components.CastCounter(module, (uint)AID.MovingMountains);
class P2PeakPeril(BossModule module) : Components.CastCounter(module, (uint)AID.PeakPeril);
class P2Shockwave(BossModule module) : Components.CastCounter(module, (uint)AID.Shockwave);
class P2SuddenDownpour(BossModule module) : Components.CastCounter(module, (uint)AID.P2SuddenDownpourAOE);

class P2PitonPull(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PitonPullAOE, 22f);
class P2Altitude(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AltitudeAOE, 6f);
class P2Arrow(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ArrowP2AOE, 10f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300, SortOrder = 4, PlanLevel = 90)]
public class A33Oschon(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, 750f), new ArenaBoundsSquare(25f))
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies((uint)OID.BossP2)[0] : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
