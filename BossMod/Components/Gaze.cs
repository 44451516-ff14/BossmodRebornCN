namespace BossMod.Components;

// generic gaze/weakpoint component, allows customized 'eye' position
public abstract class GenericGaze(BossModule module, uint aid = default) : CastCounter(module, aid)
{
    public readonly struct Eye(
        WPos position,
        DateTime activation = default,
        Angle forward = default, // if non-zero, treat specified side as 'forward' for hit calculations
        float range = 10000f,
        bool inverted = false,
        ulong actorID = default,
        WPos? eyeCenter = null,
        int? arenaProjectionLayer = null,
        bool? restrictToArenaProjectionLayer = false)
    {
        public readonly WPos Position = position;
        public readonly DateTime Activation = activation;
        public readonly Angle Forward = forward;
        public readonly float Range = range;
        public readonly bool Inverted = inverted;
        public readonly ulong ActorID = actorID;
        public readonly WPos? EyeCenter = eyeCenter; // optional world position where the eye should be drawn
        public readonly int? ArenaProjectionLayer = arenaProjectionLayer;
        public readonly bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    }

    public abstract ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor);
    public bool EnableHints = true;
    public bool DrawEyeRange = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
        {
            return;
        }
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (ArenaProjectionLayerParticipantApplies(actor, eye.ArenaProjectionLayer, eye.RestrictToArenaProjectionLayer)
                && actor.Position.InCircle(eye.Position, eye.Range) && HitByEye(actor, eye) != eye.Inverted)
            {
                hints.Add(eye.Inverted ? "面对视线！" : "背对视线！");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!EnableHints)
        {
            return;
        }
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        if (len == 0)
        {
            return;
        }

        var pos = actor.Position;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            var eyepos = eye.Position;
            if (pos.InCircle(eyepos, eye.Range) && ArenaProjectionLayerApplies(actor, eye.ArenaProjectionLayer, eye.RestrictToArenaProjectionLayer))
            {
                var inv = eye.Inverted;
                var direction = inv ? Angle.FromDirection(pos - eyepos) - eye.Forward : Angle.FromDirection(eyepos - pos) - eye.Forward;

                var angle = inv ? 135f.Degrees() : 45f.Degrees();
                hints.ForbiddenDirections.Add((direction, angle, eye.Activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var eyes = ActiveEyes(pcSlot, pc);
        var len = eyes.Length;
        if (len == 0)
        {
            return;
        }
        var rot = pc.Rotation;
        var pcpos = pc.Position;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            var range = eye.Range;
            var eyePos = eye.Position;
            if (DrawEyeRange && range < 100f && pcpos.InCircle(eyePos, range * 1.2f)) // draw eye with 20% extra distance for safety
            {
                continue;
            }
            var participantApplies = ArenaProjectionLayerParticipantApplies(pc, eye.ArenaProjectionLayer, eye.RestrictToArenaProjectionLayer);
            var inverted = eye.Inverted;
            var danger = participantApplies && HitByEye(pc, eye) != inverted;
            using (Arena.WorldProjectionLayer(eye.ArenaProjectionLayer, eye.RestrictToArenaProjectionLayer))
            {
                Arena.DrawEye(eye.EyeCenter ?? IndicatorWorldPos(eyePos), danger, inverted);
            }

            if (participantApplies)
            {
                // The eye belongs to its authored mechanic layer, but this facing indicator belongs
                // to the participant. For unrestricted gazes those can be different physical floors.
                using (Arena.WorldProjectionLayer(Module.ResolveArenaProjectionLayer(pc)))
                {
                    var eyeF = eye.Forward;
                    var (min, max) = inverted ? (45f, 315f) : (-45f, 45f);
                    Arena.PathArcTo(pcpos, 1f, (rot + eyeF + min.Degrees()).Rad, (rot + eyeF + max.Degrees()).Rad);
                    Arena.PathStroke(false, Colors.Enemy);
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HitByEye(Actor actor, in Eye eye) => (actor.Rotation + eye.Forward).ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0.707107f; // 45-degree

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal WPos IndicatorWorldPos(WPos position)
    {
        if (Arena.InBounds(position))
        {
            return position;
        }

        var center = Arena.Center;
        var delta = position - center;
        var lenSq = delta.LengthSq();
        if (lenSq < 1e-8f)
        {
            return center;
        }

        var dir = delta / MathF.Sqrt(lenSq);
        var t = Arena.IntersectRayBounds(center, dir);

        var screenScale = Arena.ScreenHalfSize * Arena.Bounds.InvRadius;
        var marginWorld = Arena.ScreenMarginSize * 0.5f / screenScale;
        return center + (t + marginWorld) * dir;
    }
}

// gaze that happens on cast end
public class CastGaze(BossModule module, uint aid, bool inverted = false, float range = 10000f, int maxCasts = int.MaxValue, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null) : GenericGaze(module, aid)
{
    public readonly List<Eye> Eyes = [];
    public int MaxCasts = maxCasts; // used for staggered gazes, when showing all active would be pointless
    public float[]? ArenaProjectionLayers = arenaProjectionLayers;
    public int? ArenaProjectionLayer = arenaProjectionLayer; // explicit ID takes precedence over height-based selection
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;

    protected int? ResolveArenaProjectionLayer(float y)
        => !RestrictToArenaProjectionLayer.HasValue ? null : ArenaProjectionLayer
            ?? (ArenaProjectionLayers is { Length: > 0 } layers ? GenericAOEs.IndexOfClosestLayer(layers, y) : null);

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = Eyes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > MaxCasts ? MaxCasts : count;
        return CollectionsMarshal.AsSpan(Eyes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var loc = spell.LocXZ;
            Eyes.Add(new(loc, Module.CastFinishAt(spell), default, range, inverted, caster.InstanceID, IndicatorWorldPos(loc),
                ResolveArenaProjectionLayer(spell.Location.Y), RestrictToArenaProjectionLayer));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = Eyes.Count;
            var id = caster.InstanceID;
            var eyes = CollectionsMarshal.AsSpan(Eyes);
            for (var i = 0; i < count; ++i)
            {
                if (eyes[i].ActorID == id)
                {
                    Eyes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

public class CastGazes(BossModule module, uint[] aids, bool inverted = false, float range = 10000f, int maxCasts = int.MaxValue, int expectedNumCasters = 99, float[]? arenaProjectionLayers = null, bool? restrictToArenaProjectionLayer = true, int? arenaProjectionLayer = null)
    : CastGaze(module, default, maxCasts: maxCasts, arenaProjectionLayers: arenaProjectionLayers, restrictToArenaProjectionLayer: restrictToArenaProjectionLayer, arenaProjectionLayer: arenaProjectionLayer)
{
    protected readonly uint[] AIDs = aids;
    protected readonly int ExpectedNumCasters = expectedNumCasters;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = AIDs.Length;
        var aid = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (aid == AIDs[i])
            {
                var loc = spell.LocXZ;
                Eyes.Add(new(loc, Module.CastFinishAt(spell), default, range, inverted, caster.InstanceID, IndicatorWorldPos(loc),
                    ResolveArenaProjectionLayer(spell.Location.Y), RestrictToArenaProjectionLayer));
                if (Eyes.Count == ExpectedNumCasters)
                {
                    SortHelpers.SortEyesByActivation(Eyes);
                }
                return;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // we probably dont need to check for AIDs here since actorID should already be unique to any active spell
        var count = Eyes.Count;
        var id = caster.InstanceID;
        var eyes = CollectionsMarshal.AsSpan(Eyes);
        for (var i = 0; i < count; ++i)
        {
            if (eyes[i].ActorID == id)
            {
                Eyes.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var len = AIDs.Length;
        var aid = spell.Action.ID;
        for (var i = 0; i < len; ++i)
        {
            if (aid == AIDs[i])
            {
                ++NumCasts;
                return;
            }
        }
    }
}

// cast weakpoint component: a number of casts (with supposedly non-intersecting shapes), player should face specific side determined by active status to the caster for aoe he's in
public class CastWeakpoint(BossModule module, uint aid, AOEShape shape, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight,
    int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : GenericGaze(module, aid)
{
    public CastWeakpoint(BossModule module, uint aid, float radius, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight,
        int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false)
        : this(module, aid, new AOEShapeCircle(radius), statusForward, statusBackward, statusLeft, statusRight, arenaProjectionLayer, restrictToArenaProjectionLayer) { }
    public AOEShape Shape = shape;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight]; // 4 elements: fwd, left, back, right
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    protected readonly List<Actor> _casters = [];
    private readonly Dictionary<ulong, Angle> _playerWeakpoints = [];
    protected float fallbackTime;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
        {
            return [];
        }

        Actor? caster = null;
        var minRemainingTime = float.MaxValue;
        // if there are multiple casters, take one that finishes first
        for (var i = 0; i < count; ++i)
        {
            var a = _casters[i];
            if (!ArenaProjectionLayerParticipantApplies(a, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }
            if (Shape.Check(actor.Position, a.Position, a.CastInfo?.Rotation ?? a.Rotation))
            {
                if ((a.CastInfo?.RemainingTime ?? fallbackTime) < minRemainingTime)
                {
                    caster = a;
                    minRemainingTime = a.CastInfo?.RemainingTime ?? fallbackTime;
                }
            }
        }

        if (caster != null && _playerWeakpoints.TryGetValue(actor.InstanceID, out var angle))
        {
            var loc = caster.Position.Quantized();
            return new Eye[1] { new(loc, Module.CastFinishAt(caster.CastInfo), angle, inverted: true, eyeCenter: IndicatorWorldPos(loc),
                arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer) };
        }

        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _casters.Remove(caster);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
        {
            _playerWeakpoints[actor.InstanceID] = statusKind * 90f.Degrees();
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
        {
            _playerWeakpoints.Remove(actor.InstanceID);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (ArenaProjectionLayerParticipantApplies(actor, eye.ArenaProjectionLayer, eye.RestrictToArenaProjectionLayer) && !HitByEye(actor, eye))
            {
                hints.Add("将破绽面向视线！");
                return;
            }
        }
    }
}
