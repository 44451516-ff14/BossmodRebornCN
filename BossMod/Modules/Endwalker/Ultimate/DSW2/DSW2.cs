namespace BossMod.Endwalker.Ultimate.DSW2;

class P2AscalonsMercyConcealed(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AscalonsMercyConcealedAOE, new AOEShapeCone(50, 15.Degrees()));

abstract class AscalonMight(BossModule module, uint oid) : Components.Cleave(module, (uint)AID.AscalonsMight, new AOEShapeCone(50, 30.Degrees()), [oid]);
class P2AscalonMight(BossModule module) : AscalonMight(module, (uint)OID.BossP2);

class P2UltimateEnd(BossModule module) : Components.CastCounter(module, (uint)AID.UltimateEndAOE);
class P3Drachenlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DrachenlanceAOE, new AOEShapeCone(13, 45.Degrees()));
class P3SoulTether(BossModule module) : Components.TankbusterTether(module, (uint)AID.SoulTether, (uint)TetherID.HolyShieldBash, 5);
class P4Resentment(BossModule module) : Components.CastCounter(module, (uint)AID.Resentment);
class P5TwistingDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwistingDive, new AOEShapeRect(60, 5));

abstract class Cauterize(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(48, 10));
class P5Cauterize1(BossModule module) : Cauterize(module, (uint)AID.Cauterize1);
class P5Cauterize2(BossModule module) : Cauterize(module, (uint)AID.Cauterize2);

class P5SpearOfTheFury(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpearOfTheFuryP5, new AOEShapeRect(50, 5));
class P5AscalonMight(BossModule module) : AscalonMight(module, (uint)OID.BossP5);
class P5Surrender(BossModule module) : Components.CastCounter(module, (uint)AID.Surrender);
class P6SwirlingBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwirlingBlizzard, new AOEShapeDonut(20, 35));
class P7Shockwave(BossModule module) : Components.CastCounter(module, (uint)AID.ShockwaveP7);
class P7AlternativeEnd(BossModule module) : Components.CastCounter(module, (uint)AID.AlternativeEnd);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP2, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 788, PlanLevel = 90)]
public class DSW2(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), BoundsCircle)
{
    public static readonly ArenaBoundsCircle BoundsCircle = new(21); // p2, intermission
    public static readonly ArenaBoundsSquare BoundsSquare = new(21); // p3, p4

    private Actor? _bossP3;
    private Actor? _leftEyeP4;
    private Actor? _rightEyeP4;
    private Actor? _nidhoggP4;
    private Actor? _serCharibert;
    private Actor? _spear;
    private Actor? _bossP5;
    private Actor? _nidhoggP6;
    private Actor? _hraesvelgrP6;
    private Actor? _bossP7;
    public Actor? ArenaFeatures { get; private set; }
    public Actor? BossP2() => PrimaryActor;
    public Actor? BossP3() => _bossP3;
    public Actor? LeftEyeP4() => _leftEyeP4;
    public Actor? RightEyeP4() => _rightEyeP4;
    public Actor? NidhoggP4() => _nidhoggP4;
    public Actor? SerCharibert() => _serCharibert;
    public Actor? Spear() => _spear;
    public Actor? BossP5() => _bossP5;
    public Actor? NidhoggP6() => _nidhoggP6;
    public Actor? HraesvelgrP6() => _hraesvelgrP6;
    public Actor? BossP7() => _bossP7;

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (ArenaFeatures == null)
        {
            if (StateMachine.ActivePhaseIndex == 0)
            {
                var b = Enemies((uint)OID.ArenaFeatures);
                ArenaFeatures = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP3 == null)
        {
            if (StateMachine.ActivePhaseIndex == 1)
            {
                var b = Enemies((uint)OID.BossP3);
                _bossP3 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_leftEyeP4 == null)
        {
            if (StateMachine.ActivePhaseIndex == 2)
            {
                var b = Enemies((uint)OID.LeftEye);
                _leftEyeP4 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_rightEyeP4 == null)
        {
            if (StateMachine.ActivePhaseIndex == 2)
            {
                var b = Enemies((uint)OID.RightEye);
                _rightEyeP4 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_nidhoggP4 == null)
        {
            if (StateMachine.ActivePhaseIndex == 2)
            {
                var b = Enemies((uint)OID.NidhoggP4);
                _nidhoggP4 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_serCharibert == null)
        {
            if (StateMachine.ActivePhaseIndex == 3)
            {
                var b = Enemies((uint)OID.SerCharibert);
                _serCharibert = b.Count != 0 ? b[0] : null;
            }
        }
        if (_spear == null)
        {
            if (StateMachine.ActivePhaseIndex == 3)
            {
                var b = Enemies((uint)OID.SpearOfTheFury);
                _spear = b.Count != 0 ? b[0] : null;
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
        if (_nidhoggP6 == null)
        {
            if (StateMachine.ActivePhaseIndex == 5)
            {
                var b = Enemies((uint)OID.NidhoggP6);
                _nidhoggP6 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_hraesvelgrP6 == null)
        {
            if (StateMachine.ActivePhaseIndex == 5)
            {
                var b = Enemies((uint)OID.HraesvelgrP6);
                _hraesvelgrP6 = b.Count != 0 ? b[0] : null;
            }
        }
        if (_bossP7 == null)
        {
            if (StateMachine.ActivePhaseIndex == 6)
            {
                var b = Enemies((uint)OID.DragonKingThordan);
                _bossP7 = b.Count != 0 ? b[0] : null;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
        Arena.Actor(_bossP3);
        Arena.Actor(_leftEyeP4);
        Arena.Actor(_rightEyeP4);
        Arena.Actor(_nidhoggP4);
        Arena.Actor(_serCharibert);
        Arena.Actor(_spear);
        Arena.Actor(_bossP5);
        Arena.Actor(_nidhoggP6);
        Arena.Actor(_hraesvelgrP6);
        Arena.Actor(_bossP7);
    }
}
