namespace BossMod.Global.PVP.HiddenGorge.GoblinMercenary;

public enum OID : uint
{
    Boss = 0x25FA, //R=2.0
    BossHelper = 0x233C,
}

public enum AID : uint
{
    IronKiss = 14562, // 233C->location, 5.0s cast, range 7 circle
    GobfireShootypopsStart = 14563, // 25FA->self, 5.0s cast, range 30+R width 6 rect
    GobfireShootypops = 14564, // 25FA->self, no cast, range 30+R width 6 rect
    GobspinWhooshdropsTelegraph = 14567, // 233C->self, 1.0s cast, single-target
    Plannyplot = 14558, // 25FA->self, 4.0s cast, single-target
    GobspinWhooshdrops = 14559, // 25FA->self, no cast, range 8 circle, knockback 15 away from source
    GobswipeConklopsTelegraph = 14568, // BossHelper->self, 1.0s cast, single-target
    GobswipeConklops = 14560, // Boss->self, no cast, range 5-30 donut, knockback 15 away from source
    Discharge = 14561, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167, // Boss
}

class GobspinSwipe(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private static readonly AOEShapeDonut donut = new(5, 30);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 4);
        if ((AID)spell.Action.ID == AID.GobspinWhooshdropsTelegraph)
            _aoe = new(circle, Module.PrimaryActor.Position, default, activation);
        if ((AID)spell.Action.ID == AID.GobswipeConklopsTelegraph)
            _aoe = new(donut, Module.PrimaryActor.Position, default, activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GobspinWhooshdrops or AID.GobswipeConklops)
            _aoe = null;
    }
}

class Knockbacks(BossModule module) : Components.GenericKnockback(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private static readonly AOEShapeDonut donut = new(5f, 30f);
    private Knockback? _knockback;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 4);
        if (spell.Action.ID == (uint)AID.GobspinWhooshdropsTelegraph)
            _knockback = new(Module.PrimaryActor.Position, 15f, activation, circle);
        else if (spell.Action.ID == (uint)AID.GobswipeConklopsTelegraph)
            _knockback = new(Module.PrimaryActor.Position, 15f, activation, donut);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GobspinWhooshdrops or (uint)AID.GobswipeConklops)
            _knockback = null;
    }
}

class GobfireShootypops(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(32f, 3f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -60f.Degrees(),
            (uint)IconID.RotateCCW => 60f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GobfireShootypopsStart)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
            InitIfReady(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GobfireShootypopsStart or (uint)AID.GobfireShootypops)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, WPos.ClampToGrid(source.Position), _rotation, _increment, _activation, 1, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class IronKiss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronKiss, 7);

class GoblinMercenaryStates : StateMachineBuilder
{
    public GoblinMercenaryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronKiss>()
            .ActivateOnEnter<GobspinSwipe>()
            .ActivateOnEnter<Knockbacks>()
            .ActivateOnEnter<GobfireShootypops>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || !Module.PrimaryActor.IsTargetable;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 599, NameID = 7906)]
public class GoblinMercenary(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, primary.Position.Z < 0 ? -124.5f : 144.5f), primary.Position.Z < 0 ? new ArenaBoundsSquare(16) : new ArenaBoundsCircle(30));
