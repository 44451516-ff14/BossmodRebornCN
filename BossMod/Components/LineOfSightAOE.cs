namespace BossMod.Components;

// generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
// TODO: add support for multiple AOE sources at the same time (I simplified Hermes from 4 AOEs into one)
// add support for blockers that spawn or get destroyed after cast already started (Hermes: again a cheat here by only using that meteor that exists for the whole mechanic)
public abstract class GenericLineOfSightAOE(BossModule module, ActionID aid, float maxRange, bool blockersImpassable = false, bool rect = false, bool safeInsideHitbox = true) : GenericAOEs(module, aid, "Hide behind obstacle!")
{
    public DateTime NextExplosion;
    public bool BlockersImpassable = blockersImpassable;
    public bool SafeInsideHitbox = safeInsideHitbox;
    public float MaxRange { get; private set; } = maxRange;
    public bool Rect { get; private set; } = rect; // if the AOE is a rectangle instead of a circle
    public WPos? Origin { get; private set; } // inactive if null
    public List<(WPos Center, float Radius)> Blockers { get; private set; } = [];
    public List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility { get; private set; } = [];
    public List<AOEInstance> Safezones = [];
    public List<Shape> UnionShapes = [];
    public List<Shape> DifferenceShapes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Safezones.Take(1);
    public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers, DateTime nextExplosion = default)
    {
        NextExplosion = nextExplosion;
        Origin = origin;
        Blockers.Clear();
        Blockers.AddRange(blockers);
        Visibility.Clear();
        if (origin != null)
        {
            foreach (var b in Blockers)
            {
                var toBlock = b.Center - origin.Value;
                var dist = toBlock.Length();
                Visibility.Add((dist, Angle.FromDirection(toBlock), b.Radius < dist ? Angle.Asin(b.Radius / dist) : 90.Degrees()));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => c.Risky && !c.Check(actor.Position)) && Origin != null && ((WPos)Origin - actor.Position).Length() < MaxRange)
            hints.Add(WarningText);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            AddSafezone(Module.CastFinishAt(spell), caster.Rotation);
    }

    public void AddSafezone(DateTime activation, Angle rotation = default)
    {
        if (Origin != null)
        {
            if (!Rect)
            {
                foreach (var v in Visibility)
                    UnionShapes.Add(new DonutSegmentHA(Origin.Value, v.Distance + 0.2f, MaxRange, v.Dir, v.HalfWidth));
            }
            else if (Rect)
            {
                foreach (var b in Blockers)
                {
                    var dir = rotation.ToDirection();
                    UnionShapes.Add(new RectangleSE(b.Center + 0.2f * dir, b.Center + MaxRange * dir, b.Radius));
                }
            }
            if (BlockersImpassable || !SafeInsideHitbox)
                foreach (var b in Blockers)
                    DifferenceShapes.Add(new Circle(b.Center, !SafeInsideHitbox ? b.Radius : b.Radius + 0.5f));
            Safezones.Add(new(new AOEShapeCustom(CopyShapes(UnionShapes), CopyShapes(DifferenceShapes), InvertForbiddenZone: true), Arena.Center, default, activation, Colors.SafeFromAOE));
            UnionShapes.Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Safezones.Count > 0 && spell.Action == WatchedAction)
            Safezones.RemoveAt(0);
    }

    private static List<Shape> CopyShapes(List<Shape> shapes)
    {
        var copy = new List<Shape>();
        copy.AddRange(shapes);
        return copy;
    }
}

// simple line-of-sight aoe that happens at the end of the cast
public abstract class CastLineOfSightAOE : GenericLineOfSightAOE
{
    public readonly List<Actor> Casters = [];
    public Actor? ActiveCaster => Casters.MinBy(c => c.CastInfo!.RemainingTime);

    protected CastLineOfSightAOE(BossModule module, ActionID aid, float maxRange, bool blockersImpassable = false, bool rect = false, bool safeInsideHitbox = true) : base(module, aid, maxRange, blockersImpassable, rect, safeInsideHitbox)
    {
        Refresh();
    }

    public abstract IEnumerable<Actor> BlockerActors();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Casters.Add(caster);
            Refresh();
        }
        base.OnCastStarted(caster, spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            Casters.Remove(caster);
            Refresh();
        }
        base.OnCastFinished(caster, spell);
    }

    public void Refresh()
    {
        var caster = ActiveCaster;
        WPos? position = caster != null ? (WorldState.Actors.Find(caster.CastInfo!.TargetID)?.Position ?? caster.CastInfo!.LocXZ) : null;
        Modify(position, BlockerActors().Select(b => (b.Position, b.HitboxRadius)), Module.CastFinishAt(caster?.CastInfo));
    }
}
