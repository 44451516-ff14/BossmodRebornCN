namespace BossMod.Endwalker.Quest.MSQ.WorthyOfHisBack;

public enum OID : uint
{
    Boss = 0x342C, // R1.5
    VenatsPhantom = 0x342D, // R0.5
    DeathWall = 0x1EB27A, // R0.5
    TrueAeroIV = 0x342E, // R0.7
    Thelema = 0x342F, // R1.0
    ThelemaAgape = 0x3864, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 28383, // Boss->player, no cast, single-target
    AutoAttackDancer = 28384, // Boss->player, no cast, single-target
    Teleport1 = 25599, // Boss->location, no cast, single-target
    Teleport2 = 25601, // VenatsPhantom->location, no cast, single-target

    TrueStone = 28385, // Boss->player, no cast, single-target, whm form autoattack
    TrueBlink = 25600, // Boss->self, 3.0s cast, single-target, boss dash to some location

    MousasMantle = 25592, // Boss->self, 6.5s cast, single-target, switch jobs to dancer
    MagosMantle = 25593, // Boss->self, 6.5s cast, single-target, switch jobs to whm

    Kleos = 25597, // Boss->self, 3.0s cast, range 40 circle, raidwide

    CircumzenithalArcVisual = 25598, // Boss->self, 5.0+2.2s cast, single-target
    CircumzenithalArcFirst = 28466, // Helper->self, 7.3s cast, range 40 180-degree cone
    CircumzenithalArcSecond = 28376, // Helper->self, 7.3s cast, range 40 180-degree cone

    CrepuscularRay = 25595, // VenatsPhantom->location, 5.0s cast, width 8 rect charge

    CircleOfBrilliance = 25602, // Boss->self, 5.0s cast, range 5 circle

    EnomotosFirst = 25603, // Helper->self, 5.0s cast, range 6 circle
    EnomotosRest = 25604, // Helper->self, no cast, range 6 circle

    EnomotosSmall = 28392, // Helper->location, 3.0s cast, range 4 circle

    EpeaPteroentaVisual = 25606, // Boss->self, no cast, range 20 120-degree cone
    EpeaPteroentaFirst = 25605, // Boss->self, 7.0s cast, range 20 120-degree cone
    EpeaPteroentaRest = 25607, // Helper->self, 8.0s cast, range 20 120-degree cone

    ParhelionVisualCCW = 25608, // Boss->self, 5.0s cast, single-target
    ParhelionVisualCW = 25609, // Boss->self, 5.0s cast, single-target
    ParhelionRotationFirst = 25610, // Helper->self, 5.0s cast, range 20 45-degree cone
    ParhelionRotationRest = 25611, // Helper->self, 1.5s cast, range 20 45-degree cone

    Parhelion1 = 25612, // Helper->self, 4.0s cast, range 10 circle
    Parhelion2 = 25613, // Helper->self, 4.0s cast, range 10-15 donut
    Parhelion3 = 25614, // Helper->self, 4.0s cast, range 15-20 donut

    TrueAeroIV = 25615, // Boss->self, 3.0s cast, range 40 circle
    TrueHoly = 25619, // Boss->self, 5.0s cast, range 40 circle

    AfflatusAzemFirst = 25617, // Boss->location, 4.0s cast, range 5 circle
    AfflatusAzemChase = 25618, // Boss->location, 1.0s cast, range 5 circle

    Windage = 25616, // TrueAeroIV->self, 2.5s cast, range 5 circle
    WindageSlow = 28116, // TrueAeroIV->self, 9.0s cast, range 5 circle

    TrueStoneIVVisual = 25620, // Boss->self, 3.0s cast, single-target
    TrueStoneIV = 25621, // Helper->location, 6.0s cast, range 10 circle

    LimitBreakPhase = 25622, // Boss->self, no cast, single-target
    AetherialBoon = 25624, // ThelemaAgape->Boss, no cast, single-target
    MonomachiaPull = 28539, // Helper->player, no cast, single-target
    Monomachia = 26505, // Boss->player, no cast, single-target
    ThelemaVisual = 25623, // Boss->player, no cast, single-target
    Thelema = 28440, // Helper->player, no cast, single-target
}

public enum IconID : uint
{
    RotateCW = 167, // Boss
    RotateCCW = 168 // Boss
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private readonly AOEShapeDonut donut = new(20f, 25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Kleos)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.3f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DeathWall)
        {
            _aoe = null;
            Arena.Bounds = WorthyOfHisBack.DefaultBounds;
        }
    }
}

class Kleos(BossModule module) : Components.RaidwideCast(module, (uint)AID.Kleos);
class TrueHolyRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.TrueHoly);
class TrueAeroIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.TrueAeroIV);

class ParhelionCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private readonly List<Angle> _rotation = new(3);

    private static readonly AOEShapeCone _shape = new(20f, 22.5f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -45f.Degrees(),
            (uint)IconID.RotateCCW => 45f.Degrees(),
            _ => default
        };
        _activation = WorldState.FutureTime(5d);
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (_rotation.Count == 3 && _increment != default)
        {
            for (var i = 0; i < 3; ++i)
                Sequences.Add(new(_shape, Arena.Center.Quantized(), _rotation[i], _increment, _activation, 2.6f, 9));
            _rotation.Clear();
            _increment = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ParhelionRotationFirst)
        {
            _rotation.Add(spell.Rotation);
            InitIfReady();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ParhelionRotationFirst or (uint)AID.ParhelionRotationRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class ParhelionDonut(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 15f), new AOEShapeDonut(15f, 20f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Parhelion1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = spell.Action.ID switch
        {
            (uint)AID.Parhelion1 => 0,
            (uint)AID.Parhelion2 => 1,
            (uint)AID.Parhelion3 => 2,
            _ => -1
        };
        AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(3d));
    }
}

class EpeaPteroenta(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCone cone = new(20f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EpeaPteroentaFirst or (uint)AID.EpeaPteroentaRest)
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.EpeaPteroentaFirst or (uint)AID.EpeaPteroentaRest)
            _aoes.RemoveAt(0);
    }
}

class CrepuscularRay(BossModule module) : Components.ChargeAOEs(module, (uint)AID.CrepuscularRay, 4f);

abstract class CircumzenithalArc(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 90f.Degrees()));
class CircumzenithalArcFirst(BossModule module) : CircumzenithalArc(module, (uint)AID.CircumzenithalArcFirst);
class CircumzenithalArcSecond(BossModule module) : CircumzenithalArc(module, (uint)AID.CircumzenithalArcSecond)
{
    private readonly CrepuscularRay _aoe = module.FindComponent<CrepuscularRay>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class CircleOfBrilliance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircleOfBrilliance, 5f);

class Enomotos(BossModule module) : Components.Exaflare(module, 6f, (uint)AID.EnomotosFirst)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Lines.Add(new(caster.Position, 5f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1d, 9, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EnomotosFirst or (uint)AID.EnomotosRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

class Windage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Windage, 5f);
class AfflatusAzem(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(5f), (uint)AID.AfflatusAzemFirst, (uint)AID.AfflatusAzemChase, 5f, 2.1f, 5, true);
class WindageSlow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindageSlow, 5f);
class TrueHoly(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TrueHoly, 20f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(ClassShared.AID.Surecast) : ActionID.MakeSpell(ClassShared.AID.ArmsLength);
            hints.ActionsToExecute.Push(action, actor, ActionQueue.Priority.High);
        }
    }
}

class TrueStoneIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TrueStoneIV, 10f, maxCasts: 7);
class EnomotosSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EnomotosSmall, 4f);

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Thelema, (uint)OID.ThelemaAgape], 1);

public class WorthyOfHisBackStates : StateMachineBuilder
{
    public WorthyOfHisBackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Kleos>()
            .ActivateOnEnter<TrueAeroIV>()
            .ActivateOnEnter<TrueHolyRaidwide>()
            .ActivateOnEnter<CrepuscularRay>()
            .ActivateOnEnter<CircumzenithalArcFirst>()
            .ActivateOnEnter<CircumzenithalArcSecond>()
            .ActivateOnEnter<CircleOfBrilliance>()
            .ActivateOnEnter<Enomotos>()
            .ActivateOnEnter<EpeaPteroenta>()
            .ActivateOnEnter<ParhelionDonut>()
            .ActivateOnEnter<ParhelionCone>()
            .ActivateOnEnter<Windage>()
            .ActivateOnEnter<AfflatusAzem>()
            .ActivateOnEnter<WindageSlow>()
            .ActivateOnEnter<TrueHoly>()
            .ActivateOnEnter<TrueStoneIV>()
            .ActivateOnEnter<EnomotosSmall>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69968, NameID = 10586)]
public class WorthyOfHisBack(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos arenaCenter = new(-630f, 72f);
    public static readonly ArenaBoundsComplex DefaultBounds = new([new Polygon(arenaCenter, 20f, 20)]);
    private static readonly ArenaBoundsComplex arena = new([new Polygon(arenaCenter, 24.5f, 20)]);
}
