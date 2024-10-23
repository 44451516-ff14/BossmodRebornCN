namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D033Eros;

public enum OID : uint
{
    Boss = 0x27B1, //R=7.02
    PoisonVoidzone = 0x1E972C,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Jump = 15519, // Boss->location, no cast, single-target, visual

    Rend = 15513, // Boss->player, 4.0s cast, single-target, tankbuster
    HoundOutOfHeaven = 15514, // Boss->self, 5.0s cast, single-target
    HoundOutOfHeavenSuccess = 17079, // Boss->player, no cast, single-target, tether stretch success
    HoundOutOfHeavenFail = 17080, // Boss->player, no cast, single-target, tether stretch fail
    Glossolalia = 15515, // Boss->self, 3.0s cast, range 50 circle, raidwide
    ViperPoison = 15516, // Boss->self, 6.0s cast, single-target
    ViperPoisonPatterns = 15518, // Helper->location, 6.0s cast, range 6 circle
    ViperPoisonBait = 15517, // Helper->player, 6.0s cast, range 6 circle

    Inhale = 17168, // Boss->self, 4.0s cast, range 50 circle, attract 50 between centers

    HeavingBreathVisual = 16923, // Helper->self, 3.5s cast, range 42 width 30 rect
    HeavingBreath = 15520, // Boss->self, 3.5s cast, range 50 circle, knockback 35 forward

    ConfessionOfFaithVisual1 = 15524, // Boss->self, 5.0s cast, single-target, left/right breath
    ConfessionOfFaithVisual2 = 15521, // Boss->self, 5.0s cast, single-target, center breath
    ConfessionOfFaithLeft = 15526, // Helper->self, 5.5s cast, range 60 60-degree cone
    ConfessionOfFaithRight = 15527, // Helper->self, 5.5s cast, range 60 60-degree cone
    ConfessionOfFaithCenter = 15522, // Helper->self, 5.5s cast, range 60 60-degree cone
    ConfessionOfFaithStack = 15525, // Helper->players, 5.8s cast, range 6 circle, stack
    ConfessionOfFaithSpread = 15523 // Helper->player, 5.8s cast, range 5 circle, spread
}

class HoundOutOfHeaven(BossModule module) : Components.StretchTetherDuo(module, 19, 5.2f);
class ViperPoisonVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.ViperPoisonPatterns), m => m.Enemies(OID.PoisonVoidzone).Where(z => z.EventState != 7), 0);
class ConfessionOfFaithStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ConfessionOfFaithStack), 6, 4, 4);
class ConfessionOfFaithSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ConfessionOfFaithSpread), 5);
class ConfessionOfFaithBreath(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60, 30.Degrees());
    private static readonly AOEShapeCone cone2 = new(60, 30.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ConfessionOfFaithCenter)
            _aoes.Add(new(cone2, caster.Position + 5 * (caster.Rotation + 180.Degrees()).ToDirection(), spell.Rotation, Module.CastFinishAt(spell)));
        else if ((AID)spell.Action.ID is AID.ConfessionOfFaithLeft or AID.ConfessionOfFaithRight)
            _aoes.Add(new(cone, caster.Position + Module.PrimaryActor.HitboxRadius * (caster.Rotation + 180.Degrees()).ToDirection(), spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ConfessionOfFaithCenter or AID.ConfessionOfFaithLeft)
            _aoes.Clear();
    }
}
class ViperPoisonBait(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.ViperPoisonBait), new AOEShapeCircle(6), true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(new(17, -518), new(17, -558), 13));
    }
}

class Inhale(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Inhale), 50, kind: Kind.TowardsOrigin)
{
    private readonly ViperPoisonVoidzone _aoe = module.FindComponent<ViperPoisonVoidzone>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        var component = _aoe.ActiveAOEs(slot, actor).ToList();
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
        {
            foreach (var c in component)
                forbidden.Add(ShapeDistance.Rect(c.Origin, Module.PrimaryActor.Rotation, 40, 0, 6));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Min(), source.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<ViperPoisonVoidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class HeavingBreath(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HeavingBreath), 35, kind: Kind.DirForward, stopAtWall: true)
{
    private readonly ViperPoisonVoidzone _aoe = module.FindComponent<ViperPoisonVoidzone>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        var component = _aoe.ActiveAOEs(slot, actor).ToList();
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
        {
            foreach (var c in component)
                forbidden.Add(ShapeDistance.Rect(c.Origin, new Angle(), 40, 40, 6));
            if (forbidden.Count > 0)
                hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Min(), source.Activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => Module.FindComponent<ViperPoisonVoidzone>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false;
}

class Glossolalia(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Glossolalia));
class Rend(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Rend));

class D033ErosStates : StateMachineBuilder
{
    public D033ErosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ViperPoisonBait>()
            .ActivateOnEnter<ViperPoisonVoidzone>()
            .ActivateOnEnter<Rend>()
            .ActivateOnEnter<HoundOutOfHeaven>()
            .ActivateOnEnter<Glossolalia>()
            .ActivateOnEnter<ConfessionOfFaithBreath>()
            .ActivateOnEnter<ConfessionOfFaithSpread>()
            .ActivateOnEnter<ConfessionOfFaithStack>()
            .ActivateOnEnter<HeavingBreath>()
            .ActivateOnEnter<Inhale>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8233)]
public class D033Eros(WorldState ws, Actor primary) : BossModule(ws, primary, new(17, -538), new ArenaBoundsRect(14.5f, 19.5f));
