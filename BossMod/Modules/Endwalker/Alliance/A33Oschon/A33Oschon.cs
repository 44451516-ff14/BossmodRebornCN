namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1SuddenDownpour(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SuddenDownpourAOE));

class TrekShot(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(65f, 60f.Degrees()));
class P1TrekShotN(BossModule module) : TrekShot(module, AID.TrekShotNAOE);
class P1TrekShotS(BossModule module) : TrekShot(module, AID.TrekShotSAOE);

class SoaringMinuet(BossModule module, AID aid) : Components.SimpleAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(65f, 135f.Degrees()));
class P1SoaringMinuet1(BossModule module) : SoaringMinuet(module, AID.SoaringMinuet1);
class P1SoaringMinuet2(BossModule module) : SoaringMinuet(module, AID.SoaringMinuet2);

class P1Arrow(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.ArrowP1AOE), new AOEShapeCircle(6f), true);
class P1Downhill(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.DownhillP1AOE), 6f);
class P2MovingMountains(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.MovingMountains));
class P2PeakPeril(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PeakPeril));
class P2Shockwave(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Shockwave));
class P2SuddenDownpour(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.P2SuddenDownpourAOE));

class P2PitonPull(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PitonPullAOE), 22f);
class P2Altitude(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.AltitudeAOE), 6f);
class P2Arrow(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.ArrowP2AOE), new AOEShapeCircle(10f), true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300, SortOrder = 4, PlanLevel = 90)]
public class A33Oschon(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, 750), new ArenaBoundsSquare(25f))
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.BossP2)[0] : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
