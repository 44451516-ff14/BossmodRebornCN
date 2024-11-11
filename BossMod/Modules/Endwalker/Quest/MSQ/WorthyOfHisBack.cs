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

class Kleos(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Kleos));
class TrueHolyRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TrueHoly));
class TrueAeroIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TrueAeroIV));

class ParhelionCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone cone = new(20, 22.5f.Degrees());
    private Angle increment;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ParhelionRotationFirst)
            Sequences.Add(new(cone, caster.Position, spell.Rotation, increment, Module.CastFinishAt(spell), 2.6f, 9));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ParhelionRotationFirst or AID.ParhelionRotationRest)
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.RotateCCW)
            increment = 45.Degrees();
        else if (iconID == (uint)IconID.RotateCW)
            increment = -45.Degrees();
        for (var i = 0; i < Sequences.Count; i++)
            Sequences[i] = Sequences[i] with { Increment = increment };
    }
}

class ParhelionDonut(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 15), new AOEShapeDonut(15, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Parhelion1)
            AddSequence(Arena.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.Parhelion1 => 0,
            AID.Parhelion2 => 1,
            AID.Parhelion3 => 2,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(3));
    }
}

class EpeaPteroenta(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(20, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _aoes.Count; ++i)
        {
            if (i == 0)
                yield return _aoes[i] with { Color = Colors.Danger };
            else if (i == 1)
                yield return _aoes[i];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EpeaPteroentaFirst or AID.EpeaPteroentaRest)
            _aoes.Add(new(cone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.EpeaPteroentaFirst or AID.EpeaPteroentaRest)
            _aoes.RemoveAt(0);
    }
}

class CrepuscularRay(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.CrepuscularRay), 4);
class CircumzenithalArc(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircumzenithalArcFirst), new AOEShapeCone(40, 90.Degrees()));
class CircumzenithalArcSecond(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircumzenithalArcSecond), new AOEShapeCone(40, 90.Degrees()))
{
    private CrepuscularRay? ray;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ray ??= Module.FindComponent<CrepuscularRay>();
        if (ray?.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class CircleOfBrilliance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CircleOfBrilliance), new AOEShapeCircle(5));
class Enomotos(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6), ActionID.MakeSpell(AID.EnomotosFirst))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 5 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 9, MaxShownExplosions = 3 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EnomotosFirst or AID.EnomotosRest)
        {
            var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
            if (line != null)
                AdvanceLine(line, caster.Position);
        }
    }
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Kleos))
{
    private AOEInstance? _aoe;
    private readonly AOEShapeDonut donut = new(20, 25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.3f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DeathWall)
        {
            _aoe = null;
            Arena.Bounds = WorthyOfHisBack.DefaultBounds;
        }
    }
}

class Windage(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Windage), new AOEShapeCircle(5));
class AfflatusAzem(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(5), ActionID.MakeSpell(AID.AfflatusAzemFirst), ActionID.MakeSpell(AID.AfflatusAzemChase), 5, 2.1f, 5);
class WindageSlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WindageSlow), new AOEShapeCircle(5));
class TrueHoly(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.TrueHoly), 20)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(ClassShared.AID.Surecast) : ActionID.MakeSpell(ClassShared.AID.ArmsLength);
        if (Casters.FirstOrDefault()?.CastInfo?.NPCRemainingTime is var t && t < 5)
            hints.ActionsToExecute.Push(action, actor, ActionQueue.Priority.High);
    }
}
class TrueStoneIV(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TrueStoneIV), 10, maxCasts: 7);
class EnomotosSmall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.EnomotosSmall), 4);

class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Thelema, (uint)OID.ThelemaAgape])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = OIDs.Contains(e.Actor.OID) ? 1 : 0;
    }
}

public class WorthyOfHisBackStates : StateMachineBuilder
{
    public WorthyOfHisBackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Kleos>()
            .ActivateOnEnter<TrueAeroIV>()
            .ActivateOnEnter<TrueHolyRaidwide>()
            .ActivateOnEnter<CircumzenithalArc>()
            .ActivateOnEnter<CircumzenithalArcSecond>()
            .ActivateOnEnter<CrepuscularRay>()
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan, Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69968, NameID = 10586)]
public class WorthyOfHisBack(WorldState ws, Actor primary) : BossModule(ws, primary, new(-630, 72), new ArenaBoundsCircle(24.5f))
{
    public static readonly ArenaBoundsCircle DefaultBounds = new(20);
}
