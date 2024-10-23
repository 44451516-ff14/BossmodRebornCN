namespace BossMod.Dawntrail.Quest.JobQuests.Viper.VengeanceOfTheViper;

public enum OID : uint
{
    Boss = 0x42A7, // R7.5
    RightWing = 0x4423, // R2.5
    LeftWing = 0x4424, // R2.5
    BallOfFire = 0x42A8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player/Keshkwa, no cast, single-target
    Teleport = 38737, // Boss->location, no cast, single-target

    BitingScratch1 = 38721, // Boss->self, 6.0s cast, range 40 90-degree cone
    BitingScratch2 = 39552, // Boss->self, 11.0s cast, range 40 90-degree cone

    FervidImpactVisual = 38716, // Boss->self, no cast, single-target
    FrigidPulseVisual = 38718, // Boss->self, no cast, single-target
    FervidImpact = 38715, // Boss->self, 5.0s cast, range 12 circle
    FrigidPulse = 38717, // Boss->self, 5.0s cast, range 12-60 donut
    StormkissedFlamesIn = 38795, // Boss->self, 4.0s cast, range 12-60 donut
    StormkissedFlamesOut = 38719, // Helper->self, 6.4s cast, range 12 circle
    FirestormCycleOut = 38794, // Boss->self, 4.0s cast, range 12 circle
    FirestormCycleIn = 38720, // Helper->self, 6.4s cast, range 12-60 donut

    SwoopingFrenzy1 = 38714, // Boss->location, 5.0s cast, range 15 circle
    SwoopingFrenzy2 = 38728, // Boss->location, 8.0s cast, range 15 circle
    SwoopingFrenzyTelegraph = 38730, // Helper->self, 9.0s cast, range 15 circle
    SwoopingFrenzy3 = 38729, // Boss->location, no cast, range 15 circle

    FervidPulseFirstCW1 = 38723, // Boss->self, 5.0s cast, range 50 width 14 cross, rotation, 5 hits, step 22.5°
    FervidPulseFirstCW2 = 38724, // Boss->self, 8.0s cast, range 50 width 14 cross
    FervidPulseFirstCCW = 38725, // Boss->self, 8.0s cast, range 50 width 14 cross
    FervidPulseRest = 38726, // Boss->self, no cast, range 50 width 14 cross
    FervidPulseTelegraph = 38727, // Helper->self, 2.5s cast, range 50 width 14 cross

    GaleripperVisual = 38731, // Boss->self, 4.0+0,3s cast, single-target
    Galeripper = 38732, // Helper->self, 4.3s cast, range 60 90-degree cone

    CatchingChaos = 38733, // Boss->self, 12.0s cast, range 60 circle
    CatchingChaosEnrage = 38734, // Boss->self, 59.0s cast, range 60 circle

    GreatBallOfFire = 38722, // Boss->self, 3.0s cast, single-target
    Explosion = 38745, // BallOfFire->self, 16.0s cast, range 15 circle

    BrutalStroke = 38735, // Boss->location, 6.0s cast, range 45 circle, damage fall off AOE
    Razorwind = 38736 // Helper->Keshkwa, 5.0s cast, range 7 circle, stack
}

class Razorwind(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Razorwind), 7, 2, 2);
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Explosion), new AOEShapeCircle(15));

class SwoopingFrenzy1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SwoopingFrenzy1), 15);
class SwoopingFrenzy2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SwoopingFrenzy2 || (AID)spell.Action.ID == AID.SwoopingFrenzyTelegraph && !_aoes.Any(x => x.Origin.AlmostEqual(spell.LocXZ, 1)))
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 1.5f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.SwoopingFrenzy2 or AID.SwoopingFrenzy3)
            _aoes.RemoveAt(0);
    }
}

class BrutalStroke(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BrutalStroke), 25);
class CatchingChaos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CatchingChaos));
class Galeripper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Galeripper), new AOEShapeCone(60, 45.Degrees()));

abstract class BitingScratch(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 45.Degrees()));
class BitingScratch1(BossModule module) : BitingScratch(module, AID.BitingScratch1);
class BitingScratch2(BossModule module) : BitingScratch(module, AID.BitingScratch2);

class FervidImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FervidImpact), new AOEShapeCircle(12));
class FrigidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidPulse), new AOEShapeDonut(12, 60));

class FirestormCycle(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(12), new AOEShapeDonut(12, 60)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FirestormCycleOut)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.FirestormCycleOut => 0,
                AID.FirestormCycleIn => 1,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.4f));
        }
    }
}

class StormkissedFlames(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeDonut(12, 60), new AOEShapeCircle(12)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StormkissedFlamesIn)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.StormkissedFlamesIn => 0,
                AID.StormkissedFlamesOut => 1,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.4f));
        }
    }
}

class FervidPulse(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCross cross = new(50, 7);
    private static readonly Angle increment = 22.5f.Degrees();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FervidPulseFirstCCW:
                Sequences.Add(new(cross, Module.PrimaryActor.Position, spell.Rotation, increment, Module.CastFinishAt(spell), 2.9f, 5));
                break;
            case AID.FervidPulseFirstCW1:
            case AID.FervidPulseFirstCW2:
                Sequences.Add(new(cross, Module.PrimaryActor.Position, spell.Rotation, -increment, Module.CastFinishAt(spell), 2.9f, 5));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FervidPulseFirstCCW or AID.FervidPulseFirstCW1 or AID.FervidPulseFirstCW2 or AID.FervidPulseRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class VengeanceOfTheViperStates : StateMachineBuilder
{
    public VengeanceOfTheViperStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BitingScratch1>()
            .ActivateOnEnter<BitingScratch2>()
            .ActivateOnEnter<Razorwind>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<BrutalStroke>()
            .ActivateOnEnter<SwoopingFrenzy1>()
            .ActivateOnEnter<SwoopingFrenzy2>()
            .ActivateOnEnter<CatchingChaos>()
            .ActivateOnEnter<Galeripper>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidImpact>()
            .ActivateOnEnter<StormkissedFlames>()
            .ActivateOnEnter<FirestormCycle>()
            .ActivateOnEnter<FervidPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70389, NameID = 12829)]
public class VengeanceOfTheViper(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(-402, 738), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.RightWing).Concat(Enemies(OID.LeftWing)).Concat([PrimaryActor]));
    }
}
