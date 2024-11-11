namespace BossMod.Components;

// generic component for mechanics that require baiting some aoe (by proximity, by tether, etc) away from raid
// some players can be marked as 'forbidden' - if any of them is baiting, they are warned
// otherwise we show own bait as as outline (and warn if player is clipping someone) and other baits as filled (and warn if player is being clipped)
public class GenericBaitAway(BossModule module, ActionID aid = default, bool alwaysDrawOtherBaits = true, bool centerAtTarget = false) : CastCounter(module, aid)
{
    public record struct Bait(Actor Source, Actor Target, AOEShape Shape, DateTime Activation = default)
    {
        public Angle? CustomRotation { get; init; }

        public readonly Angle Rotation => CustomRotation ?? (Source != Target ? Angle.FromDirection(Target.Position - Source.Position) : Source.Rotation);

        public Bait(Actor source, Actor target, AOEShape shape, DateTime activation, Angle customRotation)
            : this(source, target, shape, activation)
        {
            CustomRotation = customRotation;
        }
    }

    public bool AlwaysDrawOtherBaits = alwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
    public bool CenterAtTarget = centerAtTarget; // if true, aoe source is at target
    public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
    public bool EnableHints = true;
    public bool IgnoreOtherBaits; // if true, don't show hints/aoes for baits by others
    public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
    public BitMask ForbiddenPlayers; // these players should avoid baiting
    public List<Bait> CurrentBaits = [];
    public const string BaitAwayHint = "Bait away from raid!";

    public IEnumerable<Bait> ActiveBaits => AllowDeadTargets ? CurrentBaits.Where(b => !b.Source.IsDead) : CurrentBaits.Where(b => !b.Source.IsDead && !b.Target.IsDead);
    public IEnumerable<Bait> ActiveBaitsOn(Actor target) => ActiveBaits.Where(b => b.Target == target);
    public IEnumerable<Bait> ActiveBaitsNotOn(Actor target) => ActiveBaits.Where(b => b.Target != target);
    public WPos BaitOrigin(Bait bait) => (CenterAtTarget ? bait.Target : bait.Source).Position;
    public bool IsClippedBy(Actor actor, Bait bait) => bait.Shape.Check(actor.Position, BaitOrigin(bait), bait.Rotation);
    public IEnumerable<Actor> PlayersClippedBy(Bait bait) => Raid.WithoutSlot().Exclude(bait.Target).InShape(bait.Shape, BaitOrigin(bait), bait.Rotation);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;

        if (ForbiddenPlayers[slot])
        {
            if (ActiveBaitsOn(actor).Any())
                hints.Add("Avoid baiting!");
        }
        else
        {
            if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any()))
                hints.Add(BaitAwayHint);
        }

        if (!IgnoreOtherBaits && ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited aoe!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!ActiveBaits.Any())
            return;

        foreach (var bait in ActiveBaitsNotOn(actor))
            hints.AddForbiddenZone(bait.Shape, BaitOrigin(bait), bait.Rotation, bait.Activation);

        foreach (var bait in ActiveBaitsOn(actor))
            AddTargetSpecificHints(actor, bait, hints);
    }

    private void AddTargetSpecificHints(Actor actor, Bait bait, AIHints hints)
    {
        if (bait.Source == bait.Target) // TODO: think about how to handle source == target baits, eg. vomitting mechanics
            return;
        foreach (var a in Raid.WithoutSlot().Exclude(actor))
            switch (bait.Shape)
            {
                case AOEShapeDonut:
                case AOEShapeCircle:
                    hints.AddForbiddenZone(bait.Shape, a.Position, default, bait.Activation);
                    break;
                case AOEShapeCone cone:
                    hints.AddForbiddenZone(ShapeDistance.Cone(bait.Source.Position, 100, bait.Source.AngleTo(a), cone.HalfAngle), bait.Activation);
                    break;

                case AOEShapeRect rect:
                    hints.AddForbiddenZone(ShapeDistance.Cone(bait.Source.Position, 100, bait.Source.AngleTo(a), Angle.Asin(rect.HalfWidth / (a.Position - bait.Source.Position).Length())), bait.Activation);
                    break;
                case AOEShapeCross cross:
                    hints.AddForbiddenZone(cross, a.Position, bait.Rotation, bait.Activation);
                    break;
            }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveBaitsOn(player).Any() ? BaiterPriority : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!IgnoreOtherBaits)
            foreach (var bait in ActiveBaitsNotOn(pc))
                if (AlwaysDrawOtherBaits || IsClippedBy(pc, bait))
                    bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaitsOn(pc))
        {
            bait.Shape.Outline(Arena, BaitOrigin(bait), bait.Rotation);
        }
    }
}

// bait on all players, requiring everyone to spread out
public class BaitAwayEveryone : GenericBaitAway
{
    public BaitAwayEveryone(BossModule module, Actor? source, AOEShape shape, ActionID aid = default) : base(module, aid)
    {
        AllowDeadTargets = false;
        if (source != null)
            CurrentBaits.AddRange(Raid.WithoutSlot(true).Select(p => new Bait(source, p, shape)));
    }
}

// component for mechanics requiring tether targets to bait their aoe away from raid
public class BaitAwayTethers(BossModule module, AOEShape shape, uint tetherID, ActionID aid = default, uint enemyOID = default, float activationDelay = default) : GenericBaitAway(module, aid)
{
    public AOEShape Shape = shape;
    public uint TID = tetherID;
    public bool DrawTethers = true;
    public readonly IReadOnlyList<Actor> _enemies = module.Enemies(enemyOID);
    public float ActivationDelay = activationDelay;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (DrawTethers)
        {
            foreach (var b in ActiveBaits)
                Arena.AddLine(b.Source.Position, b.Target.Position);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null && (enemyOID == default || _enemies.Contains(source)))
            CurrentBaits.Add(new(enemy, player, Shape, WorldState.FutureTime(ActivationDelay)));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            CurrentBaits.RemoveAll(b => b.Source == enemy && b.Target == player);
        }
    }

    // we support both player->enemy and enemy->player tethers
    public (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return (null, null);

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return (null, null);

        var (player, enemy) = source.Type is ActorType.Player or ActorType.Buddy ? (source, target) : (target, source);
        if (player.Type is not ActorType.Player and not ActorType.Buddy || enemy.Type is ActorType.Player or ActorType.Buddy)
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return (null, null);
        }

        return (player, enemy);
    }
}

// component for mechanics requiring icon targets to bait their aoe away from raid
public class BaitAwayIcon(BossModule module, AOEShape shape, uint iconID, ActionID aid = default, float activationDelay = 5.1f, bool centerAtTarget = false, Actor? source = null) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget)
{
    public AOEShape Shape = shape;
    public uint IID = iconID;
    public float ActivationDelay = activationDelay;

    public virtual Actor? BaitSource(Actor target) => source ?? Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
            CurrentBaits.Add(new(source, actor, Shape, WorldState.FutureTime(ActivationDelay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

// component for mechanics requiring cast targets to gtfo from raid (aoe tankbusters etc)
public class BaitAwayCast(BossModule module, ActionID aid, AOEShape shape, bool centerAtTarget = false, bool endsOnCastEvent = false) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget)
{
    public AOEShape Shape = shape;
    public bool EndsOnCastEvent = endsOnCastEvent;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && !EndsOnCastEvent)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction && EndsOnCastEvent)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

// a variation of BaitAwayCast for charges that end at target
public class BaitAwayChargeCast(BossModule module, ActionID aid, float halfWidth) : GenericBaitAway(module, aid)
{
    private readonly AOEShapeRect rect = new(default, halfWidth);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, rect, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }

    public override void Update()
    {
        for (var i = 0; i < CurrentBaits.Count; ++i)
        {
            var b = CurrentBaits[i];
            CurrentBaits[i] = b with { Shape = rect with { LengthFront = (b.Target.Position - b.Source.Position).Length() } };
        }
    }
}

// a variation of baits with tethers for charges that end at target
public class BaitAwayChargeTether(BossModule module, float halfWidth, float activationDelay, ActionID aidGood, ActionID aidBad = default, uint tetherIDBad = 57, uint tetherIDGood = 1, uint enemyOID = default, float minimumDistance = default)
: StretchTetherDuo(module, minimumDistance, activationDelay, tetherIDBad, tetherIDGood, new AOEShapeRect(default, halfWidth), default, enemyOID)
{
    public ActionID AidGood = aidGood;
    public ActionID AidBad = aidBad; // supports 2nd AID incase the AID changes between good and bad tethers
    public uint TetherIDBad = tetherIDBad;
    public uint TetherIDGood = tetherIDGood;
    public float HalfWidth = halfWidth;

    public override void Update()
    {
        base.Update();
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != length)
                {
                    b.Shape = shape with { LengthFront = length };
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == AidGood || spell.Action == AidBad)
        {
            ++NumCasts;
            CurrentBaits.RemoveAll(x => x.Target == WorldState.Actors.Find(spell.MainTargetID));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ActiveBaits.Any())
            return;
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any()))
            hints.Add(BaitAwayHint);
    }
}
