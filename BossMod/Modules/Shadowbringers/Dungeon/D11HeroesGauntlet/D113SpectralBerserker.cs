namespace BossMod.Shadowbringers.Dungeon.D11HeroesGauntlet.D113SpectralBerserker;

public enum OID : uint
{
    Boss = 0x2EFD, // R3.0
    Rubble = 0x2EFE, // R2.5
    Crater = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    BeastlyFury = 21004, // Boss->self, 4.0s cast, range 50 circle

    WildAnguish1 = 21000, // Boss->players, 5.0s cast, range 6 circle
    WildAnguish2 = 21001, // Boss->players, no cast, range 6 circle
    FallingRock = 20997, // Rubble->self, no cast, range 8 circle

    WildRageVisual = 20994, // Boss->location, 5.0s cast, range 8 circle
    WildRage = 20995, // Helper->location, 5.7s cast, range 8 circle
    WildRageKnockback = 20996, // Helper->self, 5.7s cast, range 8-50 donut, raidwide, knockback 15, away fromsource

    WildRampageVisual = 20998, // Boss->self, 5.0s cast, single-target
    WildRampage = 20999, // Helper->self, 5.5s cast, range 50 width 50 rect

    RagingSliceFirst = 21002, // Boss->self, 3.7s cast, range 50 width 6 rect
    RagingSliceRest = 21003 // Boss->self, 2.5s cast, range 50 width 6 rect
}

public enum IconID : uint
{
    Stackmarker = 93, // player
    Spreadmarker = 229 // player
}

class BeastlyFuryArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom cross = new([new Square(D113SpectralBerserker.ArenaCenter, 23)], D113SpectralBerserker.Cross);

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BeastlyFury && Arena.Bounds == D113SpectralBerserker.StartingBounds)
            _aoe = new(cross, Arena.Center, default, Module.CastFinishAt(spell, 1.1f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x0B)
        {
            Arena.Bounds = D113SpectralBerserker.DefaultBounds;
            _aoe = null;
        }
    }
}

class FallingRock(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.WildAnguish2), 8.5f, 6) // 8.5 instead of 6 to prevent aoe from intersecting additional rubble hitboxes
{
    public override void Update()
    {
        if (Spreads.Any(x => x.Target.IsDead))
            Spreads.RemoveAll(x => x.Target.IsDead);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Spreads.Any(x => x.Target == actor) && Module.Enemies(OID.Rubble).Any(x => !x.IsDead))
            hints.Add("Stack alone with rubble!");
    }
}

class WildAnguish1(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.WildAnguish1), 6, 4, 4)
{
    public static bool IsQuadrupleStack(BossModule module) => module.Enemies(OID.Rubble).Any(x => !x.IsDead);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (IsQuadrupleStack(Module))
        { }
        else
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class WildAnguish2(BossModule module) : Components.GenericTowers(module)
{
    private readonly FallingRock _sp = module.FindComponent<FallingRock>()!;

    public override void Update()
    {
        if (Towers.Count > 0 && _sp.Spreads.Count == 0)
            Towers.Clear();
    }

    public override void OnActorCreated(Actor actor)
    {
        // theoretically it would be 8.5 (rubble hitboxradius + aoe hitbox radius), but that makes it harder to spread out correctly, because then we would need to spread rubbles twice as far apart
        if ((OID)actor.OID == OID.Rubble)
            Towers.Add(new(actor.Position, actor.HitboxRadius, activation: WorldState.FutureTime(6.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WildAnguish1 or AID.WildAnguish2)
            Towers.RemoveAll(x => x.Position.InCircle(WorldState.Actors.Find(spell.MainTargetID)!.Position, 8.5f));
        if (_sp.Spreads.Count > 0 && (AID)spell.Action.ID == AID.WildAnguish1)
            _sp.Spreads.RemoveAll(x => x.Target == WorldState.Actors.Find(spell.MainTargetID));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }
}

class WildRageKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WildRageKnockback), 15)
{
    private static readonly Angle a10 = 10.Degrees();
    private static readonly Angle a45 = 45.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();

        if (source != default)
        {
            var forbidden = new List<Func<WPos, float>>();
            var dir = source.Origin.X == 738 ? 1 : -1;
            forbidden.Add(ShapeDistance.InvertedDonutSector(source.Origin, 8, 9, a45 * dir, a10));
            forbidden.Add(ShapeDistance.InvertedDonutSector(source.Origin, 8, 9, 3 * a45 * dir, a10));
            hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), source.Activation);
        }
    }
}

class WildRageRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WildRageKnockback));
class WildRage(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WildRage), 8);
class BeastlyFury(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BeastlyFury));

class CratersWildRampage(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly WPos pos1 = new(738, 482);
    private static readonly WPos pos2 = new(762, 482);
    private static readonly Circle circle1 = new(pos1, 7);
    private static readonly Circle circle2 = new(pos2, 7);
    public readonly List<Circle> Circles = [];
    private bool invert;
    private DateTime activation;
    private const string Hint = "Go inside crater!";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Circles.Count > 0)
            yield return new(new AOEShapeCustom(Circles) with { InvertForbiddenZone = invert }, Arena.Center, default, activation, invert ? Colors.SafeFromAOE : Colors.AOE);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002 && (OID)actor.OID == OID.Crater)
        {
            if (actor.Position == pos1 && !Circles.Any(x => x.Center == pos1))
                Circles.Add(circle1);
            else if (actor.Position == pos2 && !Circles.Any(x => x.Center == pos2))
                Circles.Add(circle2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WildRampage)
        {
            invert = true;
            activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WildRampage)
            invert = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeSafespot = ActiveAOEs(slot, actor).Where(c => c.Color == Colors.SafeFromAOE).ToList();
        if (activeSafespot.Count != 0)
        {
            if (!activeSafespot.Any(c => c.Check(actor.Position)))
                hints.Add(Hint);
            else
                hints.Add(Hint, false);
        }
        else
            base.AddHints(slot, actor, hints);
    }
}

abstract class RagingSlice(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeRect(50, 3));
class RagingSliceFirst(BossModule module) : RagingSlice(module, AID.RagingSliceFirst);
class RagingSliceRest(BossModule module) : RagingSlice(module, AID.RagingSliceRest);

class D113SpectralBerserkerStates : StateMachineBuilder
{
    public D113SpectralBerserkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Components.StayInBounds>()
            .ActivateOnEnter<BeastlyFuryArenaChange>()
            .ActivateOnEnter<BeastlyFury>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<CratersWildRampage>()
            .ActivateOnEnter<WildAnguish1>()
            .ActivateOnEnter<WildAnguish2>()
            .ActivateOnEnter<WildRageKnockback>()
            .ActivateOnEnter<WildRageRaidwide>()
            .ActivateOnEnter<WildRage>()
            .ActivateOnEnter<RagingSliceFirst>()
            .ActivateOnEnter<RagingSliceRest>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 737, NameID = 9511)]
public class D113SpectralBerserker(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(750, 482);
    public static readonly ArenaBoundsSquare StartingBounds = new(22.5f);
    public static readonly Cross[] Cross = [new Cross(ArenaCenter, 20, 10)];
    public static readonly ArenaBounds DefaultBounds = new ArenaBoundsComplex(Cross);
}
