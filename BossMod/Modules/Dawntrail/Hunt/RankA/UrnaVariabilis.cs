namespace BossMod.Dawntrail.Hunt.RankA.UrnaVariabilis;

public enum OID : uint
{
    Boss = 0x416F // R3.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ProximityPlasma1 = 39106, // Boss->self, 5.0s cast, range 20 circle
    ProximityPlasma2 = 39113, // Boss->self, 1.0s cast, range 20 circle
    RingLightning1 = 39107, // Boss->self, 5.0s cast, range 8-60 donut
    RingLightning2 = 39114, // Boss->self, 1.0s cast, range 8-60 donut
    Magnetron = 39108, // Boss->self, 3.0s cast, range 40 circle, applies positive/negative charges
    Magnetoplasma1 = 39109, // Boss->self, 6.0s cast, range 60 circle, circle
    Magnetoplasma2 = 39111, // Boss->self, 6.0s cast, range 60 circle, circle
    Magnetoring1 = 39112, // Boss->self, 6.0s cast, range 60 circle, donut
    Magnetoring2 = 39110, // Boss->self, 6.0s cast, range 60 circle, donut
    ThunderousShower = 39115, // Boss->player, 5.0s cast, range 6 circle stack
    Electrowave = 39116 // Boss->self, 4.0s cast, range 60 circle
}

public enum SID : uint
{
    PositiveCharge = 4071, // Boss->player, extra=0x0
    NegativeCharge = 4072 // Boss->player, extra=0x0
}

public enum IconID : uint
{
    PositiveChargeBoss = 291, // Boss
    NegativeChargeBoss = 290, // Boss
    Stackmarker = 62 // player
}

class Magnetism(BossModule module) : Components.Knockback(module)
{
    private enum MagneticPole { None, Plus, Minus }
    private MagneticPole CurrentPole { get; set; }
    private enum Shape { None, Donut, Circle, Any }
    private Shape CurrentShape { get; set; }
    private readonly HashSet<(Actor, uint)> statusOnActor = [];
    private DateTime activation;
    private bool done;
    private DateTime activation2;

    private bool IsKnockback(Actor actor, Shape shape, MagneticPole pole)
        => (shape == Shape.Any || CurrentShape == shape) && CurrentPole == pole && statusOnActor.Contains((actor, (uint)(pole == MagneticPole.Plus ? SID.PositiveCharge : SID.NegativeCharge)));

    private bool IsPull(Actor actor, Shape shape, MagneticPole pole)
        => (shape == Shape.Any || CurrentShape == shape) && CurrentPole == pole && statusOnActor.Contains((actor, (uint)(pole == MagneticPole.Plus ? SID.NegativeCharge : SID.PositiveCharge)));

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (IsKnockback(actor, Shape.Any, MagneticPole.Plus) || IsKnockback(actor, Shape.Any, MagneticPole.Minus))
            yield return new(Module.PrimaryActor.Position, 10, activation);
        else if (IsPull(actor, Shape.Any, MagneticPole.Plus) || IsPull(actor, Shape.Any, MagneticPole.Minus))
            yield return new(Module.PrimaryActor.Position, 10, activation, Kind: Kind.TowardsOrigin);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<MagnetismCircleDonut>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch (iconID)
        {
            case (uint)IconID.PositiveChargeBoss:
                CurrentPole = MagneticPole.Plus;
                activation = WorldState.FutureTime(6);
                break;
            case (uint)IconID.NegativeChargeBoss:
                CurrentPole = MagneticPole.Minus;
                activation = WorldState.FutureTime(6);
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Magnetoplasma1:
            case AID.Magnetoplasma2:
                CurrentShape = Shape.Circle;
                break;
            case AID.Magnetoring1:
            case AID.Magnetoring2:
                CurrentShape = Shape.Donut;
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        base.OnStatusGain(actor, status);
        if (status.ID is ((uint)SID.PositiveCharge) or ((uint)SID.NegativeCharge))
            statusOnActor.Add((actor, status.ID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Magnetoring1 or AID.Magnetoring2 or AID.Magnetoplasma1 or AID.Magnetoplasma2)
        {
            done = true;
            activation2 = WorldState.FutureTime(1);
        }
    }

    public override void Update()
    {
        if (activation2 != default && done && WorldState.CurrentTime > activation2) // delay clearing knockbacks/pulls to wait for action effects
        {
            CurrentPole = MagneticPole.None;
            CurrentShape = Shape.None;
            statusOnActor.Clear();
            activation2 = default;
            done = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Sources(slot, actor).Any() && IsImmune(slot, Sources(slot, actor).FirstOrDefault().Activation))
            return;

        var forbidden = new List<Func<WPos, float>>();
        if (IsKnockback(actor, Shape.Circle, MagneticPole.Plus) || IsKnockback(actor, Shape.Circle, MagneticPole.Minus))
            forbidden.Add(ShapeDistance.Circle(Module.PrimaryActor.Position, 10));
        else if (IsPull(actor, Shape.Circle, MagneticPole.Plus) || IsPull(actor, Shape.Circle, MagneticPole.Minus))
            forbidden.Add(ShapeDistance.Circle(Module.PrimaryActor.Position, 30));
        else if (IsKnockback(actor, Shape.Donut, MagneticPole.Plus) || IsKnockback(actor, Shape.Donut, MagneticPole.Minus))
            forbidden.Add(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, 1));
        else if (IsPull(actor, Shape.Donut, MagneticPole.Plus) || IsPull(actor, Shape.Donut, MagneticPole.Minus))
            forbidden.Add(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, 18));
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), activation);
    }
}

class MagnetismCircleDonut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Magnetism _kb = module.FindComponent<Magnetism>()!;
    private static readonly AOEShapeDonut donut = new(8, 60);
    private static readonly AOEShapeCircle circle = new(20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Magnetoplasma1:
            case AID.Magnetoplasma2:
                AddAOE(circle, spell);
                break;
            case AID.Magnetoring1:
            case AID.Magnetoring2:
                AddAOE(donut, spell);
                break;
        }
    }

    private void AddAOE(AOEShape shape, ActorCastInfo spell)
    {
        _aoe = new(shape, Module.PrimaryActor.Position, default, Module.CastFinishAt(spell, 2.5f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ProximityPlasma2 or AID.RingLightning2)
            _aoe = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kb = _kb.Sources(slot, actor).ToList();
        if (kb.Count == 0 || kb.Count != 0 && _kb.IsImmune(slot, kb.FirstOrDefault().Activation))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class ProximityPlasma1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProximityPlasma1), new AOEShapeCircle(20));
class RingLightning1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RingLightning1), new AOEShapeDonut(8, 60));
class ThunderousShower(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ThunderousShower), 6, 8);
class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class Magnetron(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Magnetron));

class UrnaVariabilisStates : StateMachineBuilder
{
    public UrnaVariabilisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Magnetron>()
            .ActivateOnEnter<Magnetism>()
            .ActivateOnEnter<MagnetismCircleDonut>()
            .ActivateOnEnter<ProximityPlasma1>()
            .ActivateOnEnter<RingLightning1>()
            .ActivateOnEnter<ThunderousShower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13158)]
public class UrnaVariabilis(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
