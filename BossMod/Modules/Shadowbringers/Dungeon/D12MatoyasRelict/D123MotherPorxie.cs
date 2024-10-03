namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D123MotherPorxie;

public enum OID : uint
{
    Boss = 0x300B, // R3.6
    AeolianCaveSprite = 0x3052, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 19087, // Boss->player, no cast, single-target

    TenderLoinVisual = 22803, // Boss->self, 5.0s cast, single-target
    TenderLoin = 22804, // Helper->self, no cast, range 60 circle

    HuffAndPuff1 = 22809, // Boss->self, 8.0s cast, range 40 width 40 rect, knockback 15, source forward

    MediumRearVisual = 22813, // Boss->self, no cast, single-target
    MediumRear1 = 22814, // Helper->self, 10.5s cast, range 5-40 donut
    MediumRear2 = 22815, // Helper->self, 16.0s cast, range 5-40 donut, after limit break phase success

    MeatMallet = 22806, // Boss->location, 7.0s cast, range 45 circle, damage fall off AOE

    Barbeque = 23331, // Boss->self, 2.5s cast, single-target
    BarbequeRect = 22807, // Helper->self, 3.0s cast, range 50 width 5 rect, knockback 15, source forward
    BarbequeCircle = 22808, // Helper->location, 3.0s cast, range 5 circle
    ToACrispVisual = 22820, // Boss->self, no cast, single-target
    ToACrisp = 22821, // Helper->self, no cast, range 10 width 40 rect

    MincedMeatVisual = 22801, // Boss->player, 4.0s cast, single-target
    MincedMeat = 22802, // Helper->player, no cast, single-target

    Buffet = 22822, // AeolianCaveSprite->self, 3.0s cast, range 40 width 6 rect
    HuffAndPuffVisual = 22810, // Boss->self, 50.0s cast, range 40 width 40 rect, casts during limit break, only visual
    Explosion = 20020, // AeolianCaveSprite->self, 2.5s cast, range 80 circle, knocks player up to see a visual knockback hint
    HuffAndPuff2 = 22811, // Boss->self, no cast, range 40 width 40 rect, knockback 15, source forward

    BlowItAllDown = 22812, // Boss->self, no cast, range 40 width 40 rect, knockback 50, source forward (on limit break fail)
    NeerDoneWell = 20045, // Helper->self, 8.0s cast, range 5-40 donut (on limit break fail)

    OpenFlameVisual = 22818, // Boss->self, 6.0s cast, single-target
    OpenFlame = 22819, // Helper->player, no cast, range 5 circle, spread
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Spreadmarker = 169, // player
}

class TenderLoin(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.TenderLoinVisual), ActionID.MakeSpell(AID.TenderLoin), 0.8f);
class MincedMeat(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.MincedMeatVisual), ActionID.MakeSpell(AID.MincedMeat), 0.9f);
class OpenFlame(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.OpenFlame), 5, 6.7f);
class MeatMallet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeatMallet), 30);
class BarbequeCircle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BarbequeCircle), 5);
class BarbequeRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BarbequeRect), new AOEShapeRect(50, 2.5f));
class Buffet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Buffet), new AOEShapeRect(40, 3));

abstract class MediumRear(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(5, 40))
{
    private readonly HuffAndPuff1 _kb1 = module.FindComponent<HuffAndPuff1>()!;
    private readonly HuffAndPuff2 _kb2 = module.FindComponent<HuffAndPuff2>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_kb1.Sources(slot, actor).Any() && !_kb2.Sources(slot, actor).Any())
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class MediumRear1(BossModule module) : MediumRear(module, AID.MediumRear1);
class MediumRear2(BossModule module) : MediumRear(module, AID.MediumRear2);
class NeerDoneWell(BossModule module) : MediumRear(module, AID.NeerDoneWell);

class HuffAndPuff1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HuffAndPuff1), 15, true, stopAtWall: true, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.Origin, 5));
    }
}

class HuffAndPuff2(BossModule module) : Components.Knockback(module, ignoreImmunes: true, stopAtWall: true)
{
    private Source? _sourceCache;
    private Source? _source;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HuffAndPuffVisual)
            _sourceCache = new(caster.Position, 15, default, null, spell.Rotation, Kind.DirForward);
        else if (_sourceCache != null && (AID)spell.Action.ID == AID.NeerDoneWell)
            _source = _sourceCache.Value with { Activation = WorldState.FutureTime(5.4f), Distance = 50 };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_sourceCache != null && (AID)spell.Action.ID == AID.Explosion)
            _source = _sourceCache.Value with { Activation = Module.CastFinishAt(spell, 10.9f) };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HuffAndPuff2 or AID.BlowItAllDown)
        {
            _source = null;
            _sourceCache = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(source.Origin, 5));
    }
}

class Barbeque(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(10, 20);
    private AOEInstance? _aoe;
    private bool imminent;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x10)
            _aoe = new(rect, new(-19.5f, 0), 89.999f.Degrees()); // activates 22.2s later, but should never be entered anyway, since you must go to the opposite of the arena
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Barbeque)
            imminent = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BarbequeRect)
            imminent = false;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ToACrisp)
            _aoe = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe != null)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(new(imminent ? 18.5f : actor.Position.X + 0.1f, 0), new(19, 0), 20));
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (imminent)
            hints.Add("Go to the opposite side of the arena and work against getting sucked in!");
    }
}

class D123MotherPorxieStates : StateMachineBuilder
{
    public D123MotherPorxieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HuffAndPuff1>()
            .ActivateOnEnter<HuffAndPuff2>()
            .ActivateOnEnter<TenderLoin>()
            .ActivateOnEnter<MincedMeat>()
            .ActivateOnEnter<OpenFlame>()
            .ActivateOnEnter<MeatMallet>()
            .ActivateOnEnter<BarbequeCircle>()
            .ActivateOnEnter<BarbequeRect>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<MediumRear1>()
            .ActivateOnEnter<MediumRear2>()
            .ActivateOnEnter<NeerDoneWell>()
            .ActivateOnEnter<Barbeque>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746, NameID = 9741)]
public class D123MotherPorxie(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.AeolianCaveSprite).Concat([PrimaryActor]));
    }
}
