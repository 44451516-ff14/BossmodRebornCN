namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D055ForgivenObscenity;

public enum OID : uint
{
    Boss = 0x27CE, //R=5.0
    BossClones = 0x27CF, //R=5.0
    Orbs = 0x27D0, //R=1.0
    Rings = 0x1EAB62,
    Helper2 = 0x2E8, //R=0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    VauthrysBlessing = 15639, // Boss->self, no cast, single-target
    OrisonFortissimo = 15636, // Boss->self, 4.0s cast, single-target
    OrisonFortissimo2 = 15637, // Helper->self, no cast, range 50 circle

    DivineDiminuendoCircle1 = 15638, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendoCircle2 = 15640, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendoCircle3 = 15649, // BossClones->self, 4.0s cast, range 8 circle
    DivineDiminuendoDonut1 = 15641, // Helper->self, 4.0s cast, range 10-16 donut
    DivineDiminuendoDonut2 = 18025, // Helper->self, 4.0s cast, range 18-32 donut

    ConvictionMarcato1 = 15642, // Boss->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato2 = 15643, // Helper->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato3 = 15648, // BossClones->self, 4.0s cast, range 40 width 5 rect
    unknown = 16846, // Helper->self, 4.0s cast, single-target
    PenancePianissimo = 15644, // Boss->self, 3.0s cast, single-target, inverted circle voidzone appears
    FeatherMarionette = 15645, // Boss->self, 3.0s cast, single-target
    SolitaireRing = 17066, // Boss->self, 3.5s cast, single-target
    Ringsmith = 15652, // Boss->self, no cast, single-target
    GoldChaser = 15653, // Boss->self, 4.0s cast, single-target
    VenaAmoris = 15655, // Orbs->self, no cast, range 40 width 5 rect
    SacramentSforzando = 15634, // Boss->self, 4.0s cast, single-target
    SacramentSforzando2 = 15635, // Helper->player, no cast, single-target
    SanctifiedStaccato = 15654 // Helper->self, no cast, range 3 circle, sort of a voidzone around the light orbs, only triggers if you get too close
}

class Orbs(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly List<Actor> _orbs = [];
    private const int Radius = 3;
    private static readonly AOEShapeCircle circle = new(Radius);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _orbs.Select(a =>
        {
            var position = a.Position;
            var inRect = Module.Enemies(OID.Rings).FirstOrDefault(x => x.Position.InRect(position, 20 * a.Rotation.ToDirection(), Radius));
            if (inRect != null)
            {
                var shapes = new Shape[]
                {
                    new Circle(a.Position, Radius),
                    new Circle(inRect.Position, Radius),
                    new RectangleSE(a.Position, inRect.Position, Radius)
                };
                return new AOEInstance(new AOEShapeCustom(shapes), Arena.Center);
            }
            else
                return new(circle, a.Position);
        });
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_orbs.Count == 0)
            return;
        var forbidden = new List<Func<WPos, float>>();
        foreach (var o in _orbs)
        {
            var position = o.Position;
            var inRect = Module.Enemies(OID.Rings).FirstOrDefault(x => x.Position.InRect(position, 20 * o.Rotation.ToDirection(), Radius));
            if (inRect != null)
                forbidden.Add(ShapeDistance.Capsule(o.Position, o.Rotation.ToDirection(), (position - inRect.Position).Length(), Radius));
            else
                forbidden.Add(ShapeDistance.Circle(o.Position, Radius));
        }
        hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Min());
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Orbs)
            _orbs.Add(actor);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            _orbs.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 4));
    }
}

class GoldChaser(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeRect rect = new(100, 2.53f, 100); // halfwidth is 2.5, but +0.03 safety margin because ring position doesn't seem to be exactly caster position
    private static readonly WPos[] positionsSet1 = [new(-227.5f, 253), new(-232.5f, 251.5f)];
    private static readonly WPos[] positionsSet2 = [new(-252.5f, 253), new(-247.5f, 251.5f)];
    private static readonly WPos[] positionsSet3 = [new(-242.5f, 253), new(-237.5f, 253)];
    private static readonly WPos[] positionsSet4 = [new(-252.5f, 253), new(-227.5f, 253)];

    private bool AreCastersInPositions(WPos[] positions)
    {
        return _casters.Count >= 2 && positions.Length == 2 &&
               (_casters[0].Position == positions[0] && _casters[1].Position == positions[1] ||
                _casters[0].Position == positions[1] && _casters[1].Position == positions[0]);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (AreCastersInPositions(positionsSet1) || AreCastersInPositions(positionsSet2))
        {
            if (_casters.Count > 2)
            {
                if (NumCasts == 0)
                    yield return new(rect, _casters[0].Position, default, _activation.AddSeconds(7.1f), Colors.Danger);
                if (NumCasts is 0 or 1)
                    yield return new(rect, _casters[1].Position, default, _activation.AddSeconds(7.6f), Colors.Danger);
            }
            if (_casters.Count > 4 && NumCasts is 0 or 1)
            {
                yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), Risky: false);
                yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.6f), Risky: false);
            }
            if (_casters.Count > 4)
            {
                if (NumCasts == 2)
                    yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), Colors.Danger);
                if (NumCasts is 2 or 3)
                    yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.6f), Colors.Danger);
            }
            if (_casters.Count == 6 && NumCasts is 2 or 3)
            {
                yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(9.1f), Risky: false);
                yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), Risky: false);
            }
            if (_casters.Count == 6)
            {
                if (NumCasts == 4)
                    yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(9.1f), Colors.Danger);
                if (NumCasts is 4 or 5)
                    yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), Colors.Danger);
            }
        }
        else if (AreCastersInPositions(positionsSet3) || AreCastersInPositions(positionsSet4))
        {
            if (_casters.Count > 2)
            {
                if (NumCasts == 0)
                    yield return new(rect, _casters[0].Position, default, _activation.AddSeconds(7.1f), Colors.Danger);
                if (NumCasts is 0 or 1)
                    yield return new(rect, _casters[1].Position, default, _activation.AddSeconds(7.1f), Colors.Danger);
            }
            if (_casters.Count > 4 && NumCasts is 0 or 1)
            {
                yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), Risky: false);
                yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.1f), Risky: false);
            }
            if (_casters.Count > 4)
            {
                if (NumCasts == 2)
                    yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), Colors.Danger);
                if (NumCasts is 2 or 3)
                    yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.1f), Colors.Danger);
            }
            if (_casters.Count == 6 && NumCasts is 2 or 3)
            {
                yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(11.1f), Risky: false);
                yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), Risky: false);
            }
            if (_casters.Count == 6)
            {
                if (NumCasts == 4)
                    yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(11.1f), Colors.Danger);
                if (NumCasts is 4 or 5)
                    yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), Colors.Danger);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Rings)
            _casters.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Ringsmith)
            _activation = WorldState.CurrentTime;

        if ((AID)spell.Action.ID == AID.VenaAmoris)
        {
            if (++NumCasts == 6)
            {
                _casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

class SacramentSforzando(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.SacramentSforzando), ActionID.MakeSpell(AID.SacramentSforzando2), 0.8f);
class OrisonFortissimo(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.OrisonFortissimo), ActionID.MakeSpell(AID.OrisonFortissimo2), 0.8f);

class DivineDiminuendoCircle(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(8));
class DivineDiminuendoCircle1(BossModule module) : DivineDiminuendoCircle(module, AID.DivineDiminuendoCircle1);
class DivineDiminuendoCircle2(BossModule module) : DivineDiminuendoCircle(module, AID.DivineDiminuendoCircle2);
class DivineDiminuendoCircle3(BossModule module) : DivineDiminuendoCircle(module, AID.DivineDiminuendoCircle3);
class DivineDiminuendoDonut1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendoDonut1), new AOEShapeDonut(10, 16));
class DivineDiminuendoDonut2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendoDonut2), new AOEShapeDonut(18, 32));

class ConvictionMarcato(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(40, 2.5f));
class ConvictionMarcato1(BossModule module) : ConvictionMarcato(module, AID.ConvictionMarcato1);
class ConvictionMarcato2(BossModule module) : ConvictionMarcato(module, AID.ConvictionMarcato2);
class ConvictionMarcato3(BossModule module) : ConvictionMarcato(module, AID.ConvictionMarcato3);

class PenancePianissimo(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeDonut donut = new(14.5f, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PenancePianissimo)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            Arena.Bounds = D055ForgivenObscenity.ArenaRect;
        else if (state == 0x00010002)
        {
            _aoe = null;
            Arena.Bounds = D055ForgivenObscenity.ArenaCircle;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Arena.Bounds == D055ForgivenObscenity.ArenaCircle)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
    }
}

class D055ForgivenObscenityStates : StateMachineBuilder
{
    public D055ForgivenObscenityStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SacramentSforzando>()
            .ActivateOnEnter<DivineDiminuendoCircle1>()
            .ActivateOnEnter<DivineDiminuendoCircle2>()
            .ActivateOnEnter<DivineDiminuendoCircle3>()
            .ActivateOnEnter<DivineDiminuendoDonut1>()
            .ActivateOnEnter<DivineDiminuendoDonut2>()
            .ActivateOnEnter<ConvictionMarcato1>()
            .ActivateOnEnter<ConvictionMarcato2>()
            .ActivateOnEnter<ConvictionMarcato3>()
            .ActivateOnEnter<OrisonFortissimo>()
            .ActivateOnEnter<GoldChaser>()
            .ActivateOnEnter<Orbs>()
            .ActivateOnEnter<PenancePianissimo>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8262)]
public class D055ForgivenObscenity(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 237), ArenaRect)
{
    public static readonly ArenaBounds ArenaRect = new ArenaBoundsRect(14.5f, 19.5f);
    public static readonly ArenaBounds ArenaCircle = new ArenaBoundsCircle(15);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, Colors.Enemy, true);
    }
}
