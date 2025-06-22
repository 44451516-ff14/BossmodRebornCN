namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1FluidSwing(BossModule module) : Components.Cleave(module, (uint)AID.FluidSwing, new AOEShapeCone(11.5f, 45f.Degrees()));
class P1FluidStrike(BossModule module) : Components.Cleave(module, (uint)AID.FluidStrike, new AOEShapeCone(11.6f, 45f.Degrees()), [(uint)OID.LiquidHand]);
class P1Sluice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sluice, 5);
class P1Splash(BossModule module) : Components.CastCounter(module, (uint)AID.Splash);
class P1Drainage(BossModule module) : Components.TankbusterTether(module, (uint)AID.DrainageP1, (uint)TetherID.Drainage, 6f);
class P2JKick(BossModule module) : Components.CastCounter(module, (uint)AID.JKick);
class P2EyeOfTheChakram(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EyeOfTheChakram, new AOEShapeRect(76f, 3f));
class P2HawkBlasterOpticalSight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HawkBlasterP2, 10f);
class P2Photon(BossModule module) : Components.CastCounter(module, (uint)AID.PhotonAOE);
class P2SpinCrusher(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinCrusher, new AOEShapeCone(10f, 45f.Degrees()));
class P2Drainage(BossModule module) : Components.Voidzone(module, 8f, GetVoidzones) // TODO: verify distance
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.LiquidRage);
}

class P2PropellerWind(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.PropellerWind, 50)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.GelidGaol));
}

class P2DoubleRocketPunch(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.DoubleRocketPunch, 3f);
class P3ChasteningHeat(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ChasteningHeat, 5f);
class P3DivineSpear(BossModule module) : Components.Cleave(module, (uint)AID.DivineSpear, new AOEShapeCone(24.2f, 45f.Degrees()), [(uint)OID.AlexanderPrime]); // TODO: verify angle
class P3DivineJudgmentRaidwide(BossModule module) : Components.CastCounter(module, (uint)AID.DivineJudgmentRaidwide);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 694, PlanLevel = 80)]
public class TEA : BossModule
{
    private readonly List<Actor> _liquidHand;
    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? LiquidHand() => _liquidHand.Count != 0 ? _liquidHand[0] : null;

    private Actor? _bruteJustice;
    private Actor? _cruiseChaser;
    public Actor? BruteJustice() => _bruteJustice;
    public Actor? CruiseChaser() => _cruiseChaser;

    private Actor? _alexPrime;
    private readonly List<Actor> _trueHeart;
    public Actor? AlexPrime() => _alexPrime;
    public Actor? TrueHeart() => _trueHeart.Count != 0 ? _trueHeart[0] : null;

    private Actor? _perfectAlex;
    public Actor? PerfectAlex() => _perfectAlex;

    public TEA(WorldState ws, Actor primary) : base(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
    {
        _liquidHand = Enemies((uint)OID.LiquidHand);
        _trueHeart = Enemies((uint)OID.TrueHeart);
    }

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_bruteJustice == null)
        {
            var b = Enemies((uint)OID.BruteJustice);
            _bruteJustice = b.Count != 0 ? b[0] : null;
        }
        if (_cruiseChaser == null)
        {
            var b = Enemies((uint)OID.CruiseChaser);
            _cruiseChaser = b.Count != 0 ? b[0] : null;
        }
        if (_alexPrime == null)
        {
            var b = Enemies((uint)OID.AlexanderPrime);
            _alexPrime = b.Count != 0 ? b[0] : null;
        }
        if (_perfectAlex == null)
        {
            var b = Enemies((uint)OID.PerfectAlexander);
            _perfectAlex = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(BossP1());
                Arena.Actor(LiquidHand());
                break;
            case 1:
                Arena.Actor(_bruteJustice, allowDeadAndUntargetable: true);
                Arena.Actor(_cruiseChaser, allowDeadAndUntargetable: true);
                break;
            case 2:
                Arena.Actor(_alexPrime);
                Arena.Actor(TrueHeart());
                Arena.Actor(_bruteJustice);
                Arena.Actor(_cruiseChaser);
                break;
            case 3:
                Arena.Actor(_perfectAlex);
                break;
        }
    }
}
