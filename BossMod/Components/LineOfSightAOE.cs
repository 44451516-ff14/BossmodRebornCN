namespace BossMod.Components;

// generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
// TODO: add support for multiple AOE sources at the same time (I simplified Hermes from 4 AOEs into one)
// add support for blockers that spawn or get destroyed after cast already started (Hermes: again a cheat here by only using that meteor that exists for the whole mechanic)
public abstract class GenericLineOfSightAOE(BossModule module, uint aid, float maxRange, bool blockersImpassable = false, bool rect = false, bool safeInsideHitbox = true) : GenericAOEs(module, aid, "Hide behind obstacle!")
{
    public DateTime NextExplosion;
    public readonly bool BlockersImpassable = blockersImpassable;
    public readonly bool SafeInsideHitbox = safeInsideHitbox;
    public readonly float MaxRange = maxRange;
    public readonly bool Rect = rect; // if the AOE is a rectangle instead of a circle
    public BitMask IgnoredPlayers;
    public WPos? Origin; // inactive if null
    public readonly List<(WPos Center, float Radius)> Blockers = [];
    public readonly List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility = [];
    public readonly List<AOEInstance> Safezones = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Safezones.Count != 0 && !IgnoredPlayers[slot] ? new AOEInstance[1] { Safezones[0] } : [];

    public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers, DateTime nextExplosion = default)
    {
        NextExplosion = nextExplosion;
        Origin = origin;
        Blockers.Clear();
        Blockers.AddRange(blockers);
        Visibility.Clear();
        if (origin != null)
        {
            var count = Blockers.Count;
            for (var i = 0; i < count; ++i)
            {
                var b = Blockers[i];
                var toBlock = b.Center - origin.Value;
                var dist = toBlock.Length();
                Visibility.Add((dist, Angle.FromDirection(toBlock), b.Radius < dist ? Angle.Asin(b.Radius / dist) : 90f.Degrees()));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
            return;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            if (c.Risky && !c.Check(actor.Position))
            {
                if (Origin != null && ((WPos)Origin - actor.Position).LengthSq() < MaxRange * MaxRange)
                {
                    hints.Add(WarningText);
                    return;
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            AddSafezone(Module.CastFinishAt(spell), spell.Rotation);
    }

    public void AddSafezone(DateTime activation, Angle rotation = default)
    {
        List<Shape> unionShapes = new(4);
        List<Shape> differenceShapes = new(4);
        if (Origin != null)
        {
            if (!Rect)
            {
                for (var i = 0; i < Visibility.Count; ++i)
                {
                    var v = Visibility[i];
                    unionShapes.Add(new DonutSegmentHA(Origin.Value, v.Distance + 0.2f, MaxRange, v.Dir, v.HalfWidth));
                }
            }
            else if (Rect)
            {
                for (var i = 0; i < Blockers.Count; ++i)
                {
                    var b = Blockers[i];
                    var dir = rotation.ToDirection();
                    unionShapes.Add(new RectangleSE(b.Center + 0.2f * dir, b.Center + MaxRange * dir, b.Radius));
                }
            }
            if (BlockersImpassable || !SafeInsideHitbox)
                for (var i = 0; i < Blockers.Count; ++i)
                {
                    var b = Blockers[i];
                    differenceShapes.Add(new Circle(b.Center, !SafeInsideHitbox ? b.Radius : b.Radius + 0.5f));
                }
            Safezones.Add(new(new AOEShapeCustom([.. unionShapes], [.. differenceShapes], InvertForbiddenZone: true), Arena.Center, default, activation, Colors.SafeFromAOE));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Safezones.Count != 0 && spell.Action.ID == WatchedAction)
            Safezones.RemoveAt(0);
    }
}

// simple line-of-sight aoe that happens at the end of the cast
public abstract class CastLineOfSightAOE : GenericLineOfSightAOE
{
    public readonly List<Actor> Casters = [];
    public Actor? ActiveCaster
    {
        get
        {
            var count = Casters.Count;
            if (count == 0)
                return null;
            Actor? activeCaster = null;
            var minRemainingTime = double.MaxValue;
            for (var i = 0; i < count; ++i)
            {
                var caster = Casters[i];
                if (caster.CastInfo != null && caster.CastInfo.RemainingTime < minRemainingTime)
                {
                    minRemainingTime = caster.CastInfo.RemainingTime;
                    activeCaster = caster;
                }
            }
            return activeCaster;
        }
    }

    protected CastLineOfSightAOE(BossModule module, uint aid, float maxRange, bool blockersImpassable = false, bool rect = false, bool safeInsideHitbox = true) : base(module, aid, maxRange, blockersImpassable, rect, safeInsideHitbox)
    {
        Refresh();
    }

    public abstract ReadOnlySpan<Actor> BlockerActors();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(caster);
            Refresh();
        }
        base.OnCastStarted(caster, spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Remove(caster);
            Refresh();
        }
        base.OnCastFinished(caster, spell);
    }

    public void Refresh()
    {
        var caster = ActiveCaster;
        WPos? position = caster != null ? caster.CastInfo!.LocXZ : null;
        var blockers = BlockerActors();
        var len = blockers.Length;
        var blockerData = new (WPos, float)[len];

        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref blockers[i];
            blockerData[i] = (b.Position, b.HitboxRadius);
        }
        Modify(position, blockerData, Module.CastFinishAt(caster?.CastInfo));
    }
}
