namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

class BewitchingFlight(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.BewitchingFlightAOE), new AOEShapeRect(40, 2.5f));
class WickedJolt(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.WickedJolt), ActionID.MakeSpell(AID.WickedJolt), ActionID.MakeSpell(AID.WickedJoltSecond), 3.2f, new AOEShapeRect(60, 2.5f), false);
class Soulshock(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Soulshock));
class Impact(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Impact));
class Cannonbolt(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Cannonbolt));

class CannonboltKB(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.PrimaryActor.Position, 50);
    }
}

class CrossTailSwitch(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CrossTailSwitchAOE));
class CrossTailSwitchLast(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CrossTailSwitchLast));
class WickedSpecialCenter(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WickedSpecialCenterAOE), new AOEShapeRect(40, 10));
class WickedSpecialSides(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WickedSpecialSidesAOE), new AOEShapeRect(40, 7.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 992, NameID = 13057, PlanLevel = 100)]
public class M04SWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, P1DefaultCenter, P1DefaultBounds)
{
    public static readonly WPos P1DefaultCenter = new(100, 100);
    public static readonly WPos P2Center = new(100, 165);
    public static readonly ArenaBoundsSquare P1DefaultBounds = new(20);
    public static readonly ArenaBoundsRect IonClusterBounds = new(5, 20);
    public static readonly ArenaBoundsRect P2DefaultBounds = new(20, 15);
    public static readonly ArenaBoundsComplex TransitionBounds = new([new Square(P1DefaultCenter, 20), new Rectangle(P2Center, 20, 15)]);
    public static readonly ArenaBoundsComplex P2CircleBounds = new([new Polygon(P2Center, 15, 50, 3.6f.Degrees())]);
    public static readonly ArenaBoundsComplex P2TowersBounds = new([new Rectangle(new(115, 100), 5, 15), new Rectangle(new(85, 100), 5, 15)]);

    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? BossP2() => _bossP2;

    private Actor? _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
