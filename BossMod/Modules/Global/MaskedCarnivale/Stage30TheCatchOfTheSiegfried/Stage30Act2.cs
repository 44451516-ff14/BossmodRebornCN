namespace BossMod.Global.MaskedCarnivale.Stage30.Act2;

public enum OID : uint
{
    Boss = 0x2C6A, //R=2.0
    IceBoulder = 0x2CC6, // R=2.0
    FireVoidzone = 0x1E8D9B,
    Helper = 0x233C
}

public enum AID : uint
{
    LawOfTheTorch1 = 18838, // Boss->self, 3.0s cast, range 34 20-degree cone
    LawOfTheTorch2 = 18839, // Helper->self, 3.0s cast, range 34 20-degree cone
    Teleport = 18848, // Boss->location, no cast, ???
    SwiftsteelKB = 18842, // Boss->self, 5.0s cast, range 100 circle, knockback 10, away from source
    Swiftsteel1 = 18843, // Helper->location, 8.8s cast, range 4 circle
    Swiftsteel2 = 18844, // Helper->self, 8.8s cast, range 8-20 donut
    SparksteelVisual = 18893, // Boss->self, no cast, single-target
    Sparksteel1 = 18840, // Boss->location, 3.0s cast, range 6 circle, spawns voidzone
    Sparksteel2 = 18841, // Helper->location, 4.0s cast, range 8 circle
    Sparksteel3 = 18897, // Helper->location, 6.0s cast, range 8 circle
    Shattersteel = 19027, // Boss->self, 5.0s cast, range 8 circle
    SphereShatter = 18986 // IceBoulder->self, no cast, range 10 circle
}

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
    private static readonly AOEShapeCircle circle = new(10f);
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

class Stage30Act2States : StateMachineBuilder
{
    public Stage30Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LawOfTheTorch1>()
            .ActivateOnEnter<LawOfTheTorch2>()
            .ActivateOnEnter<Swiftsteel1>()
            .ActivateOnEnter<Swiftsteel2>()
            .ActivateOnEnter<SwiftsteelKB>()
            .ActivateOnEnter<Sparksteel1>()
            .ActivateOnEnter<Sparksteel2>()
            .ActivateOnEnter<Sparksteel3>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<Shattersteel>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 699, NameID = 9245, SortOrder = 2)]
public class Stage30Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall);