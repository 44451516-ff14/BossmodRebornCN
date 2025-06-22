namespace BossMod.Global.MaskedCarnivale.Stage30.Act3;

public enum OID : uint
{
    Boss = 0x2C6C, //R=2.0
    SiegfriedCloneIce = 0x2C6E, //R=2.0
    SiegfriedCloneWind = 0x2C6D, //R=2.0
    SiegfriedCloneFire = 0x2C6F, //R=2.0
    IceBoulder = 0x2CC6, // R=2.0
    FireVoidzone = 0x1E8D9B,
    Bomb = 0x2C68, // R=0.4
    Helper = 0x233C
}

public enum AID : uint
{
    MagicDrain = 18890, // Boss->self, 3.0s cast, single-target
    Teleport = 18848, // Boss/SiegfriedCloneIce/SiegfriedCloneWind/SiegfriedCloneFire->location, no cast, ???
    MagitekDecoy = 18850, // Boss->self, no cast, single-target, calls clone (Ice->Wind->Fire weakness)
    HyperdriveFirst = 18836, // Boss->location, 5.0s cast, range 5 circle
    SwiftsteelKB = 18842, // SiegfriedCloneIce/Boss->self, 5.0s cast, range 100 circle
    Swiftsteel1 = 18843, // Helper->location, 8.8s cast, range 4 circle
    Swiftsteel2 = 18844, // Helper->self, 8.8s cast, range 8-20 donut
    LawOfTheTorch1 = 18838, // Boss/SiegfriedCloneIce/SiegfriedCloneWind/SiegfriedCloneFire->self, 3.0s cast, range 34 20-degree cone
    LawOfTheTorch2 = 18839, // Helper->self, 3.0s cast, range 34 20-degree cone
    AnkleGraze = 18846, // Boss->player, 3.0s cast, single-target
    Hyperdrive = 18893, // Boss/SiegfriedCloneWind/SiegfriedCloneFire->self, no cast, single-target
    HyperdriveRest = 18837, // Helper->location, 2.5s cast, range 5 circle
    Sparksteel1 = 18840, // SiegfriedCloneWind/Boss->location, 3.0s cast, range 6 circle, spawns voidzone
    Sparksteel2 = 18841, // Helper->location, 4.0s cast, range 8 circle
    Sparksteel3 = 18897, // Helper->location, 6.0s cast, range 8 circle
    Shattersteel = 19027, // SiegfriedCloneFire/Boss->self, 5.0s cast, range 8 circle
    SphereShatter = 18986, // IceBoulder->self, no cast, range 10 circle
    MagitekExplosive = 18849, // Boss->self, 3.0s cast, single-target
    RubberBullet = 18847, // Boss->player, 4.0s cast, single-target
    Explosion = 18888 // Bomb->self, 3.5s cast, range 8 circle
}

public enum SID : uint
{
    MagitekField = 2166, // Boss->Boss, extra=0x64, reflects magic damage
    MagicVulnerabilityDown = 812, // Boss->Boss, extra=0x0, invulnerable to magic
    Bind = 564, // Boss->player, extra=0x0, dispellable
}

class MagicDrain(BossModule module) : Components.CastHint(module, (uint)AID.MagicDrain, "Reflect magic damage for 30s");
class HyperdriveFirst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HyperdriveFirst, 5f);
class HyperdriveRest(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HyperdriveRest, 5f);
class AnkleGraze(BossModule module) : Components.CastHint(module, (uint)AID.AnkleGraze, "Applies bind, prepare to use Excuviation!");

abstract class LawOfTheTorch(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(34f, 10f.Degrees()));
class LawOfTheTorch1(BossModule module) : LawOfTheTorch(module, (uint)AID.LawOfTheTorch1);
class LawOfTheTorch2(BossModule module) : LawOfTheTorch(module, (uint)AID.LawOfTheTorch2);

class SwiftsteelKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.SwiftsteelKB, 10f)
{
    private readonly Swiftsteel1 _aoe1 = module.FindComponent<Swiftsteel1>()!;
    private readonly Swiftsteel2 _aoe2 = module.FindComponent<Swiftsteel2>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes1 = _aoe1.ActiveAOEs(slot, actor);
        var len1 = aoes1.Length;
        for (var i = 0; i < len1; ++i)
        {
            if (aoes1[i].Check(pos))
                return true;
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len2 = aoes2.Length;
        for (var i = 0; i < len2; ++i)
        {
            if (aoes2[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }
}

class Swiftsteel1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swiftsteel1, 4f);
class Swiftsteel2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swiftsteel2, new AOEShapeDonut(8f, 20f));

class Sparksteel1(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Sparksteel1, GetVoidzones, 0.8f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzone);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

public class Sparksteel2 : Components.SimpleAOEs
{
    public Sparksteel2(BossModule module) : base(module, (uint)AID.Sparksteel2, 8f)
    {
        Color = Colors.Danger;
    }
}

class Sparksteel3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sparksteel3, 8f)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        Color = spell.Action.ID == (uint)AID.Sparksteel2 ? Colors.Danger : 0;
    }
}

class Shattersteel(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shattersteel, 5f);
class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(10);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IceBoulder)
            _aoes.Add(new(circle, WPos.ClampToGrid(actor.Position), default, WorldState.FutureTime(8.4d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
            _aoes.Clear();
    }
}

class RubberBullet(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _knockback;
    private readonly Explosion _aoe = module.FindComponent<Explosion>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
                return true;
        }
        return !Module.InBounds(pos);
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Bomb)
            _knockback = new(Module.PrimaryActor.Position, 20f, WorldState.FutureTime(6.3d));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RubberBullet)
            _knockback = null;
    }
}

class Explosion(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Bomb)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(8.4f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
            _aoes.Clear();
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.MagitekField) != null)
            hints.Add($"{Module.PrimaryActor.Name} will reflect all magic damage!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Bind) != null)
            hints.Add("You were bound! Cleanse it with Exuviation.");
    }
}

class Stage30Act3States : StateMachineBuilder
{
    public Stage30Act3States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HyperdriveFirst>()
            .ActivateOnEnter<HyperdriveRest>()
            .ActivateOnEnter<AnkleGraze>()
            .ActivateOnEnter<LawOfTheTorch1>()
            .ActivateOnEnter<LawOfTheTorch2>()
            .ActivateOnEnter<SwiftsteelKB>()
            .ActivateOnEnter<Swiftsteel1>()
            .ActivateOnEnter<Swiftsteel2>()
            .ActivateOnEnter<MagicDrain>()
            .ActivateOnEnter<Sparksteel1>()
            .ActivateOnEnter<Sparksteel2>()
            .ActivateOnEnter<Sparksteel3>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<Shattersteel>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<RubberBullet>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 3)]
public class Stage30Act3(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
{
    private static readonly uint[] clones = [(uint)OID.SiegfriedCloneIce, (uint)OID.SiegfriedCloneWind, (uint)OID.SiegfriedCloneFire];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(clones), Colors.Object);
    }
}