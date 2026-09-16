namespace BossMod.Stormblood.Ultimate.UWU;

// select best safespot for all predation patterns
sealed class P4UltimatePredation(BossModule module) : BossComponent(module)
{
    public enum State { Inactive, Predicted, First, Second, Done }

    public State CurState;
    private readonly List<WPos> _hints = [];
    private readonly ArcList _first = new(new(), _dodgeRadius);
    private readonly ArcList _second = new(new(), _dodgeRadius);

    private const float _dodgeRadius = 19f;
    private static readonly Angle _dodgeCushion = 2.5f.Degrees();

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var h in EnumerateHints(actor.Position))
            movementHints.Add(h);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var h in EnumerateHints(pc.Position))
            Arena.AddLine(h.from, h.to, h.color);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (CurState == State.Inactive && id == 0x1E43)
        {
            RecalculateHints();
            CurState = State.Predicted;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.Predicted && spell.Action.ID == (uint)AID.CrimsonCyclone)
            CurState = State.First;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CrimsonCyclone:
                if (CurState == State.First)
                {
                    CurState = State.Second;
                    if (_hints.Count > 0)
                        _hints.RemoveAt(0);
                }
                break;
            case (uint)AID.CrimsonCycloneCross:
                if (CurState == State.Second)
                {
                    CurState = State.Done;
                    _hints.Clear();
                }
                break;
        }
    }

    private IEnumerable<(WPos from, WPos to, uint color)> EnumerateHints(WPos starting)
    {
        var color = Colors.Safe;
        foreach (var p in _hints)
        {
            yield return (starting, p, color);
            starting = p;
            color = Colors.Danger;
        }
    }

    private void RecalculateHints()
    {
        _first.Center = _second.Center = Arena.Center;
        _first.Forbidden.Clear();
        _second.Forbidden.Clear();
        _hints.Clear();

        var castModule = (UWU)Module;
        var garuda = castModule.Garuda();
        var titan = castModule.Titan();
        var ifrit = castModule.Ifrit();
        var ultima = castModule.Ultima();
        if (garuda == null || titan == null || ifrit == null || ultima == null)
            return;

        _first.ForbidInfiniteRect(titan.Position, titan.Rotation, 3f);
        _first.ForbidInfiniteRect(titan.Position, titan.Rotation + 45f.Degrees(), 3f);
        _first.ForbidInfiniteRect(titan.Position, titan.Rotation - 45f.Degrees(), 3f);
        _second.ForbidInfiniteRect(titan.Position, titan.Rotation + 22.5f.Degrees(), 3f);
        _second.ForbidInfiniteRect(titan.Position, titan.Rotation - 22.5f.Degrees(), 3f);
        _second.ForbidInfiniteRect(titan.Position, titan.Rotation + 90f.Degrees(), 3f);
        _first.ForbidInfiniteRect(ifrit.Position, ifrit.Rotation, 9f);
        _second.ForbidInfiniteRect(Arena.Center - new WDir(Arena.Bounds.Radius, default), 90f.Degrees(), 5f);
        _second.ForbidInfiniteRect(Arena.Center - new WDir(default, Arena.Bounds.Radius), default, 5f);
        _first.ForbidCircle(garuda.Position, 20f);
        _second.ForbidCircle(garuda.Position, 20f);
        _second.ForbidCircle(ultima.Position, 14f);

        var safespots = EnumeratePotentialSafespots();
        var (a1, a2) = safespots.MinBy(AngularDistance);
        _hints.Add(GetSafePositionAtAngle(a1));
        _hints.Add(GetSafePositionAtAngle(a2));
    }

    private WPos GetSafePositionAtAngle(Angle angle) => Arena.Center + _dodgeRadius * angle.ToDirection();

    private IEnumerable<(Angle, Angle)> EnumeratePotentialSafespots()
    {
        var safeFirst = _first.Allowed(_dodgeCushion);
        var safeSecond = _second.Allowed(_dodgeCushion);
        foreach (var (min1, max1) in safeFirst)
        {
            foreach (var (min2, max2) in safeSecond)
            {
                var intersectMin = Math.Max(min1.Rad, min2.Rad).Radians();
                var intersectMax = Math.Min(max1.Rad, max2.Rad).Radians();
                if (intersectMin.Rad < intersectMax.Rad)
                {
                    var midpoint = ((intersectMin + intersectMax) * 0.5f).Normalized();
                    yield return (midpoint, midpoint);
                }
                else
                {
                    yield return (max1.Normalized(), min2.Normalized());
                    yield return (min1.Normalized(), max2.Normalized());
                }
            }
        }
    }

    private static float AngularDistance((Angle, Angle) p)
    {
        var dist = Math.Abs(p.Item1.Rad - p.Item2.Rad);
        return dist < MathF.PI ? dist : Angle.DoublePI - dist;
    }
}
