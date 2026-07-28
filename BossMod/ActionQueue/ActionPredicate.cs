namespace BossMod;

public static class ActionPredicate
{
    private static readonly ActionTweaksConfig _config = Service.Config.Get<ActionTweaksConfig>();

    public static bool AllowDashToTarget(WorldState ws, Actor player, ActionQueue.Entry action, AIHints hints)
    {
        var target = action.Target;
        if (target == null || !_config.DashSafety)
        {
            return false;
        }

        // if there are pending knockbacks, god only knows where we would be sent after using a gapcloser
        // note that once the knockback is actually active and not pending, we can probably cancel it with a dash
        if (player.PendingKnockbacks.Count > 0)
        {
            return true;
        }

        var dist = player.DistanceToHitbox(target);
        var dir = player.AngleTo(target);
        var src = player.Position;

        // facing target (to dash) would make us fail gaze, directional bait, etc
        // TODO: only forbid if dash duration is longer than time to deadline?
        var dirs = hints.ForbiddenDirections;
        var count = dirs.Count;
        for (var i = 0; i < count; ++i)
        {
            var d = dirs[i];
            if (dir.AlmostEqual(d.center, d.halfWidth.Rad))
            {
                return true;
            }
        }

        // TODO: check against action's animation lock duration instead of constant 0.8?
        var (mode, deadline, _) = hints.ImminentSpecialMode;
        return mode is AIHints.SpecialMode.Pyretic or AIHints.SpecialMode.NoMovement && deadline <= ws.FutureTime(0.8d) || IsDashSafe(src, src + dir.ToDirection() * Math.Max(0f, dist), hints);
    }

    public static bool AllowDashToPosition(WorldState _, Actor player, ActionQueue.Entry action, AIHints hints)
    {
        if (action.TargetPos == default || !_config.DashSafety || !_config.DashSafetyExtra)
            return true;

        if (player.PendingKnockbacks.Count > 0)
            return false;

        return IsDashSafe(player.Position, new WPos(action.TargetPos.XZ()), hints);
    }

    public static ActionDefinition.ConditionDelegate AllowDashFixed(float range, bool backwards = false)
        => (ws, player, act, hints) =>
        {
            if (!_config.DashSafety || !_config.DashSafetyExtra)
                return true;

            if (player.PendingKnockbacks.Count > 0)
                return false;

            var dir = act.FacingAngle ?? player.Rotation;

            var dest = player.Position + dir.ToDirection() * range * (backwards ? -1f : 1f);

            return IsDashSafe(player.Position, dest, hints);
        };

    public static ActionDefinition.ConditionDelegate AllowBackdash(float range)
         => (ws, player, act, hints) =>
        {
            if (act.Target == null || !_config.DashSafety || !_config.DashSafetyExtra)
                return true;

            if (player.PendingKnockbacks.Count > 0)
                return false;

            var dir = act.Target.DirectionTo(player).Normalized();

            return IsDashSafe(player.Position, player.Position + dir * range, hints);
        };

    // check if dashing to target will put the player inside a forbidden zone
    public static bool IsDashSafe(WPos from, WPos to, AIHints hints)
    {
        var center = hints.PathfindMapCenter;
        if (!hints.PathfindMapBounds.Contains(to - center))
        {
            return false;
        }

        // if arena is a weird shape, try to ensure player won't dash out of it
        if (from != to && hints.PathfindMapBounds is ArenaBoundsCustom)
        {
            var len = (to - from).Length();
            var distToNearestWall = hints.PathfindMapBounds.IntersectRay(from - center, (to - from).Normalized());
            if (distToNearestWall >= 0f && distToNearestWall < len)
            {
                return false;
            }
        }

        var forbiddenZones = CollectionsMarshal.AsSpan(hints.ForbiddenZones);
        var countFZ = forbiddenZones.Length;
        for (var i = 0; i < countFZ; ++i)
        {
            ref var fz = ref forbiddenZones[i];
            if (fz.shapeDistance.Contains(to))
            {
                return false;
            }
        }
        var voidZones = CollectionsMarshal.AsSpan(hints.TemporaryObstacles);
        var countVZ = voidZones.Length;
        for (var i = 0; i < countVZ; ++i)
        {
            if (voidZones[i].Contains(to))
            {
                return false;
            }
        }
        return true;
    }
}
