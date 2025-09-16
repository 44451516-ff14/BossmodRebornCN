namespace BossMod.RealmReborn.Dungeon.D29TheKeeperoftheLake.D293Midgardsormr;

public enum OID : uint
{
    Boss = 0x392A, // R14.0
    MirageDragon1 = 0x392C, // R5.0
    MirageDragon2 = 0x392B, // R5.0
    MirageDragon3 = 0x392E, // R5.0
    MirageDragon4 = 0x392D, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // MirageDragon3/MirageDragon4->player, no cast, single-target
    MirageDragonVisual = 30226, // MirageDragon3/MirageDragon4->self, no cast, single-target

    Admonishment = 29889, // Boss->self, 4.0s cast, range 40 width 12 rect
    InnerTurmoil = 29888, // Boss->self, 4.0s cast, range 22 circle
    PhantomOuterTurmoil = 29279, // Boss->self, 7.0s cast, range 20.5-39 180-degree donut segment
    PhantomInnerTurmoil = 29278, // Boss->self, 7.0s cast, range 22 circle

    PhantomKin = 29277, // Boss->self, 4.0s cast, single-target
    PhantomAdmonishment = 29280, // Boss->self, 7.0s cast, range 40 width 12 rect
    MirageAdmonishment = 29290, // MirageDragon1/MirageDragon2->self, 2.0s cast, range 40 width 12 rect

    DisgustVisual = 29891, // Boss->self, 4.0s cast, single-target
    Disgust = 29892, // Helper->self, no cast, range 20 circle, raidwide

    AntipathyVisual = 29285, // Boss->self, 4.0s cast, single-target
    Antipathy1 = 29286, // Helper->self, 4.0s cast, range 6 circle
    Antipathy2 = 29287, // Helper->self, 4.0s cast, range 6-12 donut
    Antipathy3 = 29288, // Helper->self, 4.0s cast, range 12-20 donut
    Animadversion = 29281, // Boss->self, 7.0s cast, range 50 circle
    AnimadversionEnrage = 29282, // Boss->self, 7.0s cast, range 50 circle
    AkhMornFirst = 29283, // Boss->players, 5.0s cast, range 6 circle
    AkhMornRest = 29284, // Boss->players, no cast, range 6 circle
    Condescension = 29602 // Boss->player, 5.0s cast, single-target
}

abstract class TurmoilInner(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 22f);
class InnerTurmoil(BossModule module) : TurmoilInner(module, (uint)AID.InnerTurmoil);
class PhantomInnerTurmoil(BossModule module) : TurmoilInner(module, (uint)AID.PhantomInnerTurmoil);

class PhantomOuterTurmoil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhantomOuterTurmoil, new AOEShapeDonutSector(20.5f, 39f, 90f.Degrees()));
class Animadversion(BossModule module) : Components.RaidwideCast(module, (uint)AID.Animadversion);
class Condescension(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Condescension);
class Disgust(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DisgustVisual, (uint)AID.Disgust, 0.5f);

abstract class Admonishments(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(40f, 6f));
class Admonishment(BossModule module) : Admonishments(module, (uint)AID.Admonishment);
class PhantomAdmonishment(BossModule module) : Admonishments(module, (uint)AID.PhantomAdmonishment);

class MirageAdmonishment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(40f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D3 && actor.OID is (uint)OID.MirageDragon1 or (uint)OID.MirageDragon2)
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(12d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MirageAdmonishment)
            _aoes.Clear();
    }
}

class Antipathy(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6f), new AOEShapeDonut(6f, 12f), new AOEShapeDonut(12f, 20f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Antipathy1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Antipathy1 => 0,
                (uint)AID.Antipathy2 => 1,
                (uint)AID.Antipathy3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class AkhMorn(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    private int numCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornFirst)
            AddStack(WorldState.Actors.Find(spell.TargetID)!, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornFirst or (uint)AID.AkhMornRest)
        {
            if (++numCasts == 4)
            {
                Stacks.Clear();
                numCasts = 0;
            }
        }
    }
}

class D293MidgardsormrStates : StateMachineBuilder
{
    public D293MidgardsormrStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InnerTurmoil>()
            .ActivateOnEnter<PhantomInnerTurmoil>()
            .ActivateOnEnter<PhantomOuterTurmoil>()
            .ActivateOnEnter<Admonishment>()
            .ActivateOnEnter<Animadversion>()
            .ActivateOnEnter<Condescension>()
            .ActivateOnEnter<Disgust>()
            .ActivateOnEnter<MirageAdmonishment>()
            .ActivateOnEnter<PhantomAdmonishment>()
            .ActivateOnEnter<Antipathy>()
            .ActivateOnEnter<AkhMorn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 32, NameID = 3374)]
public class D293Midgardsormr(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(-40.8f, -78.2f), 18.8f)], [new Rectangle(new(-40.787f, -59.416f), 20, 1.25f)]);
    private static readonly uint[] dragons = [(uint)OID.MirageDragon3, (uint)OID.MirageDragon4];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, dragons);
    }
}
