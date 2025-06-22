using ImGuiNET;

namespace BossMod.Components;

// generic gaze/weakpoint component, allows customized 'eye' position
public abstract class GenericGaze(BossModule module, uint aid = default) : CastCounter(module, aid)
{
    public record struct Eye(
        WPos Position,
        DateTime Activation = default,
        Angle Forward = default, // if non-zero, treat specified side as 'forward' for hit calculations
        float Range = 10000f,
        bool Inverted = false,
        ulong ActorID = default);

    private const float _eyeOuterH = 10f, _eyeOuterV = 6f, _eyeInnerR = 4f;
    private const float _eyeOuterR = (_eyeOuterH * _eyeOuterH + _eyeOuterV * _eyeOuterV) / (2f * _eyeOuterV);
    private const float _eyeOffsetV = _eyeOuterR - _eyeOuterV;

    private static readonly float _eyeHalfAngle = (float)Math.Asin(_eyeOuterH / _eyeOuterR);
    private static readonly Vector2 offset = new(default, _eyeOffsetV);
    private static readonly float halfPIHalfAngleP = Angle.HalfPi + _eyeHalfAngle;
    private static readonly float halfPIHalfAngleM = Angle.HalfPi - _eyeHalfAngle;

    public abstract ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (actor.Position.InCircle(eye.Position, eye.Range) && HitByEye(ref actor, eye) != eye.Inverted)
            {
                hints.Add(eye.Inverted ? "Face the eye!" : "Turn away from gaze!");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (actor.Position.InCircle(eye.Position, eye.Range))
            {
                var direction = eye.Inverted ? Angle.FromDirection(actor.Position - eye.Position) - eye.Forward
                    : Angle.FromDirection(eye.Position - actor.Position) - eye.Forward;

                var angle = eye.Inverted ? 135f.Degrees() : 45f.Degrees();
                hints.ForbiddenDirections.Add((direction, angle, eye.Activation));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var eyes = ActiveEyes(pcSlot, pc);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            var danger = HitByEye(ref pc, eye) != eye.Inverted;
            var eyeCenter = IndicatorScreenPos(eye.Position);
            DrawEye(eyeCenter, danger);

            if (pc.Position.InCircle(eye.Position, eye.Range))
            {
                var (min, max) = eye.Inverted ? (45f, 315f) : (-45f, 45f);
                Arena.PathArcTo(pc.Position, 1, (pc.Rotation + eye.Forward + min.Degrees()).Rad, (pc.Rotation + eye.Forward + max.Degrees()).Rad);
                MiniArena.PathStroke(false, Colors.Enemy);
            }
        }
    }

    public static void DrawEye(Vector2 eyeCenter, bool danger)
    {
        var dl = ImGui.GetWindowDrawList();
        dl.PathArcTo(eyeCenter - offset, _eyeOuterR, halfPIHalfAngleP, halfPIHalfAngleM);
        dl.PathArcTo(eyeCenter + offset, _eyeOuterR, -halfPIHalfAngleP, -halfPIHalfAngleM);
        dl.PathFillConvex(danger ? Colors.Enemy : Colors.PC);
        dl.AddCircleFilled(eyeCenter, _eyeInnerR, Colors.Border);
    }

    public static bool HitByEye(ref Actor actor, Eye eye) => (actor.Rotation + eye.Forward).ToDirection().Dot((eye.Position - actor.Position).Normalized()) >= 0.707107f; // 45-degree

    private Vector2 IndicatorScreenPos(WPos eye)
    {
        if (Arena.InBounds(eye) || Arena.Bounds is not ArenaBoundsCircle && Arena.Bounds is ArenaBoundsComplex circle && !circle.IsCircle)
        {
            return Arena.WorldPositionToScreenPosition(eye);
        }
        else
        {
            var dir = (eye - Arena.Center).Normalized();
            return Arena.ScreenCenter + Arena.RotatedCoords(dir.ToVec2()) * (Arena.ScreenHalfSize + Arena.ScreenMarginSize * 0.5f);
        }
    }
}

// gaze that happens on cast end
public class CastGaze(BossModule module, uint aid, bool inverted = false, float range = 10000f, int maxCasts = int.MaxValue) : GenericGaze(module, aid)
{
    public readonly List<Eye> Eyes = [];
    public int MaxCasts = maxCasts; // used for staggered gazes, when showing all active would be pointless

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = Eyes.Count;
        if (count == 0)
            return [];
        var max = count > MaxCasts ? MaxCasts : count;
        return CollectionsMarshal.AsSpan(Eyes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Eyes.Add(new(spell.LocXZ, Module.CastFinishAt(spell), default, range, inverted, caster.InstanceID));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = Eyes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                if (Eyes[i].ActorID == id)
                {
                    Eyes.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

// cast weakpoint component: a number of casts (with supposedly non-intersecting shapes), player should face specific side determined by active status to the caster for aoe he's in
public class CastWeakpoint(BossModule module, uint aid, AOEShape shape, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : GenericGaze(module, aid)
{
    public CastWeakpoint(BossModule module, uint aid, float radius, uint statusForward, uint statusBackward, uint statusLeft, uint statusRight) : this(module, aid, new AOEShapeCircle(radius), statusForward, statusBackward, statusLeft, statusRight) { }
    public AOEShape Shape = shape;
    public readonly uint[] Statuses = [statusForward, statusLeft, statusBackward, statusRight]; // 4 elements: fwd, left, back, right
    private readonly List<Actor> _casters = [];
    private readonly Dictionary<ulong, Angle> _playerWeakpoints = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        var count = _casters.Count;
        if (count == 0)
            return [];
        Actor? caster = null;
        var minRemainingTime = float.MaxValue;
        // if there are multiple casters, take one that finishes first
        for (var i = 0; i < count; ++i)
        {
            var a = _casters[i];
            if (Shape.Check(actor.Position, a.Position, a.CastInfo!.Rotation))
            {
                if (a.CastInfo!.RemainingTime < minRemainingTime)
                {
                    caster = a;
                    minRemainingTime = a.CastInfo.RemainingTime;
                }
            }
        }

        if (caster != null && _playerWeakpoints.TryGetValue(actor.InstanceID, out var angle))
            return new Eye[1] { new(caster.Position, Module.CastFinishAt(caster.CastInfo), angle, Inverted: true) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _casters.Remove(caster);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints[actor.InstanceID] = statusKind * 90f.Degrees();
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var statusKind = Array.IndexOf(Statuses, status.ID);
        if (statusKind >= 0)
            _playerWeakpoints.Remove(actor.InstanceID);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var eyes = ActiveEyes(slot, actor);
        var len = eyes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var eye = ref eyes[i];
            if (!HitByEye(ref actor, eye))
            {
                hints.Add("Face open weakpoint to eye!");
                return;
            }
        }
    }
}
