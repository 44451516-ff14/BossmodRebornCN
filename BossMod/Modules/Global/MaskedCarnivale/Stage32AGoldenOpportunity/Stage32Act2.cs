namespace BossMod.Global.MaskedCarnivale.Stage32.Act2;

public enum OID : uint
{
    Boss = 0x3FA7, //R=2.5
    GildedCrystal = 0x3FAC, //R=3.0
    GildedGolem = 0x3FA9, //R=2.1
    GildedMarionette = 0x3FA8, //R=1.2
    GildedCyclops = 0x3FAA, //R=3.2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 34444, // Boss->player, no cast, single-target

    ShiningSummon = 34461, // Boss->self, 4.0s cast, single-target
    Teleport = 34129, // Helper->location, no cast, single-target
    GoldorAeroIII = 34460, // Boss->self, 4.0s cast, range 50 circle, raidwide, knockback 10 away from source
    GoldenCross = 34465, // GildedGolem->self, 7.0s cast, range 100 width 7 cross
    GoldorQuakeVisual = 34456, // Boss->self, 3.0s cast, single-target
    GoldorQuake1 = 34457, // Helper->self, 4.0s cast, range 10 circle
    GoldorQuake2 = 34458, // Helper->self, 5.5s cast, range 10-20 donut
    GoldorQuake3 = 34459, // Helper->self, 7.0s cast, range 20-30 donut
    GoldenBeam = 34464, // GildedMarionette->self, 7.0s cast, range 40 120-degree cone
    GoldorThunderIIIVisual = 34453, // Boss->self, 4.0s cast, single-target
    GoldorThunderIII1 = 34454, // Helper->player, no cast, range 5 circle, applies cleansable electrocution
    GoldorThunderIII2 = 34455, // Helper->location, 2.5s cast, range 6 circle
    TwentyFourCaratInhale = 34466, // GildedCyclops->self, 2.0s cast, range 50 circle, pull 30 between centers
    TwentyFourCaratSwing = 34467, // GildedCyclops->self, 4.0s cast, range 12 circle
    GoldFever = 34469, // Boss->self, 8.0s cast, single-target, Goldor draws power from crystal
    GoldorBlast = 34450, // Boss->self, 3.5s cast, range 60 width 10 rect
    GoldorGravity = 34451, // Boss->self, 5.0s cast, single-target
    GoldorGravity2 = 34452, // Helper->player, no cast, single-target, heavy damage + heavy
    GoldorRush = 34468, // Boss->self, 4.0s cast, range 50 circle, knockback 10 away from source, raidwide
    GoldorFireIII = 34449, // Helper->location, 2.5s cast, range 8 circle
    GoldorBlizzardIIIVisual = 34589, // Boss->self, 6.0s cast, single-target, interruptible, freezes player
    GoldorBlizzardIII = 34590 // Helper->player, no cast, range 6 circle
}

public enum SID : uint
{
    Heavy = 240, // Helper->player, extra=0x63
    Electrocution = 3779, // Helper->player, extra=0x0
    MagicDamageUp = 3707, // none->Boss, extra=0x0
    MagicResistance = 3621 // none->Boss, extra=0x0
}

class GoldorFireIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorFireIII, 8f);
class GoldorBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorBlast, new AOEShapeRect(60f, 5f));
class GoldenCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldenCross, new AOEShapeCross(100f, 3.5f));

class GoldenBeam(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(40f, 60f.Degrees());
    private AOEInstance? _aoe;
    private readonly List<ConeV> cones = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldenBeam)
        {
            if (Module.Enemies((uint)OID.GildedMarionette).Count == 1)
                _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
            else
            {
                cones.Add(new(spell.LocXZ, cone.Radius, spell.Rotation, cone.HalfAngle, 2));
                if (cones.Count == 4)
                {
                    var coneskip1 = new List<ConeV>(3);
                    for (var i = 1; i < 4; ++i)
                    {
                        coneskip1.Add(cones[i]);
                    }
                    ConeV[] conesA = [.. coneskip1];
                    AOEShapeCustom intersect = new([cones[0]], Shapes2: conesA, Operand: OperandType.Intersection);
                    AOEShapeCustom xor = new([cones[0]], Shapes2: conesA, Operand: OperandType.Xor);
                    var clipper = new PolygonClipper();
                    var combinedShapes = clipper.Union(new PolygonClipper.Operand(intersect.GetCombinedPolygon(Arena.Center)),
                    new PolygonClipper.Operand(xor.GetCombinedPolygon(Arena.Center)));
                    _aoe = new(intersect with { Polygon = combinedShapes }, Arena.Center, default, Module.CastFinishAt(spell));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldenBeam)
        {
            _aoe = null;
            cones.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_aoe != null && Module.Enemies((uint)OID.GildedMarionette).Count > 1)
            hints.Add("Use Diamondback outside of marked area!");
    }
}

class TwentyFourCaratSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwentyFourCaratSwing, 12f);

class GoldorQuake(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GoldorQuake1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.GoldorQuake1 => 0,
                (uint)AID.GoldorQuake2 => 1,
                (uint)AID.GoldorQuake3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(1.5d));
        }
    }
}

class GoldorAeroIII(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GoldorAeroIII, 10f)
{
    private readonly GoldenCross _aoe = module.FindComponent<GoldenCross>()!;

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
}

class GoldorAeroIIIRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.GoldorAeroIII);
class GoldorRush(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.GoldorRush, 10f);
class GoldorRushRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.GoldorRush);
class TwentyFourCaratInhale(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TwentyFourCaratInhale, 30f, kind: Kind.TowardsOrigin);
class GoldorGravity(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GoldorGravity, (uint)AID.GoldorGravity2, 0.8f, "Dmg + Heavy debuff");
class GoldorThunderIII(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GoldorThunderIIIVisual, (uint)AID.GoldorThunderIII1, 0.8f, "Prepare to cleanse Electrocution");
class GoldorThunderIII2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoldorThunderIII2, 6f);
class GoldorBlizzardIII(BossModule module) : Components.CastInterruptHint(module, (uint)AID.GoldorBlizzardIIIVisual);

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.FindStatus((uint)SID.MagicResistance) != null)
            hints.Add($"{Module.PrimaryActor.Name} is immune to magic damage! (Destroy crystal to remove buff)");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor.FindStatus((uint)SID.Electrocution) != null)
            hints.Add($"Cleanse Electrocution!");
        if (actor.FindStatus((uint)SID.Heavy) != null)
            hints.Add("Use Loom to dodge AOEs!");
    }
}

class Stage31Act2States : StateMachineBuilder
{
    public Stage31Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GoldenCross>()
            .ActivateOnEnter<GoldorFireIII>()
            .ActivateOnEnter<GoldenBeam>()
            .ActivateOnEnter<GoldorBlast>()
            .ActivateOnEnter<TwentyFourCaratInhale>()
            .ActivateOnEnter<GoldorQuake>()
            .ActivateOnEnter<GoldorAeroIII>()
            .ActivateOnEnter<GoldorAeroIIIRaidwide>()
            .ActivateOnEnter<GoldorRush>()
            .ActivateOnEnter<GoldorRushRaidwide>()
            .ActivateOnEnter<TwentyFourCaratSwing>()
            .ActivateOnEnter<GoldorGravity>()
            .ActivateOnEnter<GoldorThunderIII>()
            .ActivateOnEnter<GoldorThunderIII2>()
            .ActivateOnEnter<GoldorBlizzardIII>()
            .ActivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 948, NameID = 12471, SortOrder = 2)]
public class Stage31Act2(WorldState ws, Actor primary) : BossModule(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall);