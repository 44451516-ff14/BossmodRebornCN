namespace BossMod.Heavensward.Dungeon.D05GreatGubalLibrary.D051DemonTome;

public enum OID : uint
{
    Boss = 0xE82, // R7.84
    IceFloor = 0x1E9944, // R0.5
    Helper = 0xED6
}

public enum AID : uint
{
    Blizzard = 3522, // Boss->player, no cast, single-target
    Repel = 3519, // Boss->self, 3.0s cast, range 40+R 180-degree cone, knockback 20, source forward
    WordsOfWinterVisual = 3517, // Boss->self, 4.0s cast, single-target
    WordsOfWinter = 3961, // Helper->self, 4.5s cast, range 40+R width 22 rect

    LiquefyCenter = 3520, // Helper->self, 3.0s cast, range 50+R width 8 rect
    LiquefySides = 3521, // Helper->self, 3.0s cast, range 50+R width 7 rect

    DisclosureVisual = 3518, // Boss->self, 8.0s cast, single-target
    Disclosure = 4818, // Helper->self, 8.0s cast, range 20+R width 22 rect
    DisclosureSpin = 3989, // Helper->self, no cast, range 12+R circle, knockback 20 away from source
    BetweenTheLines = 3926, // Boss->player, no cast, single-target, didn't switch sides in time, 25k dmg per hit
}

class Repel(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Repel, 20, true, kind: Kind.DirForward, stopAtWall: true);
class LiquefyCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LiquefyCenter, new AOEShapeRect(57.84f, 4));
class LiquefySides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LiquefySides, new AOEShapeRect(57.84f, 3.5f));
class Disclosure(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Disclosure, new AOEShapeRect(20.5f, 11f));

class DisclosureSpin(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCircle circle = new(12.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Disclosure)
        {
            Arena.Bounds = D051DemonTome.SpinArena;
            _aoe = [new(circle, Module.PrimaryActor.Position.Quantized(), default, Module.CastFinishAt(spell, 2.4d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DisclosureSpin)
        {
            _aoe = [];
            Arena.Bounds = D051DemonTome.DefaultArena;
        }
    }
}

class ThinIce(BossModule module) : Components.ThinIce(module, 15, stopAtWall: true);

class D051DemonTomeStates : StateMachineBuilder
{
    public D051DemonTomeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThinIce>()
            .ActivateOnEnter<LiquefyCenter>()
            .ActivateOnEnter<LiquefySides>()
            .ActivateOnEnter<Repel>()
            .ActivateOnEnter<Disclosure>()
            .ActivateOnEnter<DisclosureSpin>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 31, NameID = 3923)]
public class D051DemonTome(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultArena.Center, DefaultArena)
{
    private static readonly Rectangle midRect = new(new(-0.182f, 0.014f), 3.475f, 4.5f, 0.02f.Degrees());
    private static readonly Rectangle entranceRect = new(new(-20.486f, 0.109f), 1.25f, 8f);
    private static readonly Rectangle exitRect = new(new(20.388f, -0.18f), 1.25f, 8f);
    private static readonly Rectangle[] baseArena = [new Rectangle(default, 19.5f, 9.5f)];
    public static readonly ArenaBoundsCustom DefaultArena = new(baseArena, [entranceRect, exitRect, midRect, new Rectangle(new(-0.166f, 8.588f), 3.487f, 5f),
     new Rectangle(new(-0.185f, -8.554f), 3.475f, 5f)]);
    public static readonly ArenaBoundsCustom SpinArena = new(baseArena, [entranceRect, exitRect, midRect]);
}
