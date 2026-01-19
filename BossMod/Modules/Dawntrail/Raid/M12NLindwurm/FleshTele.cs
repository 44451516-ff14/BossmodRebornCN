namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class FleshTele(BossModule module) : BossComponent(module)
{
    private readonly RavenousReach _reach = module.FindComponent<RavenousReach>()!;
    private readonly Burst _burst = module.FindComponent<Burst>()!;
    private DateTime _activation;
    private readonly Dictionary<ulong, bool> _jumps = [];

    private RelSimplifiedComplexPolygon _poly;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _activation = default;
            _jumps.Clear();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FleshTimer)
        {
            if (Arena.Bounds is ArenaBoundsCustom arena)
            {
                _poly = arena.Polygon.Offset(-1.5f); // pretend polygon is 1.5y smaller than real for less suspect knockbacks
            }
            else
            {
                _poly = Arena.Bounds.ShapeSimplified.Offset(-1.5f);
            }
            _activation = WorldState.FutureTime(5d);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.FleshForward or (uint)SID.FleshBack)
        {
            _jumps[actor.InstanceID] = status.ID is (uint)SID.FleshForward;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.FleshForward or (uint)SID.FleshBack)
        {
            if (_jumps.ContainsKey(actor.InstanceID))
            {
                _jumps.Remove(actor.InstanceID);
            }
        }
    }

    public override void OnActorUntargetable(Actor actor)
    {
        if (actor.OID == Module.PrimaryActor.OID)
        {
            _activation = default;
            _jumps.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation == default)
            return;

        if (!_jumps.ContainsKey(pc.InstanceID))
            return;

        var tp = GetTeleport(pc, 15f, _jumps[pc.InstanceID]);
        Arena.ActorProjected(tp.from, tp.to, tp.rotation, Colors.Danger);
        Arena.AddLine(tp.from, tp.to);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default)
            return;

        if (!_jumps.ContainsKey(actor.InstanceID))
            return;

        var tp = GetTeleport(actor, 15f, _jumps[actor.InstanceID]);
        if (DestinationUnsafe(tp.to))
            hints.Add("About to teleport into danger!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default)
            return;

        if (!_jumps.ContainsKey(actor.InstanceID))
            return;

        var rot = actor.Rotation;
        var dir = rot.ToDirection() * 15f * (_jumps[actor.InstanceID] ? 1f : -1f);

        var tp = GetTeleport(actor, 15f, _jumps[actor.InstanceID]);

        // check if tp is in bounds; if not, need to rotate
        // teleport just barely safe; -1 offset not enough, or just coincidence it was safe?
        if (!Arena.InBounds(tp.to))
        {
            hints.AddForbiddenZone(new SDKnockbackInComplexPolygonFixedDirection(Arena.Center, dir, _poly), _activation);
        }

        var reach = _reach.ActiveCasters;
        var reachLen = reach.Length;
        if (reachLen > 0)
        {
            // offset cone by teleport direction?
            hints.AddForbiddenZone(reach[0].Shape, reach[0].Origin - dir, reach[0].Rotation, _activation);
        }

        var burst = CollectionsMarshal.AsSpan(_burst.AOEs);
        var burstLen = burst.Length;
        if (burstLen > 0)
        {
            hints.AddForbiddenZone(new SDKnockbackInAABBRectFixedDirectionPlusAOECircles(Arena.Center, dir, 20f, 15f, [.. _burst.AOEs.Select(x => x.Origin)], 12f, burstLen), _activation);
        }
    }

    private bool DestinationUnsafe(WPos pos)
    {
        if (!Arena.InBounds(pos))
            return true;

        var reach = _reach.ActiveCasters;
        var reachLen = reach.Length;
        for (var i = 0; i < reachLen; ++i)
        {
            if (reach[i].Check(pos))
                return true;
        }

        var burst = CollectionsMarshal.AsSpan(_burst.AOEs);
        var burstLen = burst.Length;
        for (var i = 0; i < burstLen; ++i)
        {
            if (burst[i].Check(pos))
                return true;
        }

        return false;
    }

    private const float approxHitBoxRadius = 0.499f;
    private const float maxIntersectionError = 0.5f - approxHitBoxRadius;

    private (WPos from, WPos to, Angle rotation) GetTeleport(Actor actor, float distance, bool isForward)
    {
        var from = actor.Position;
        var rot = actor.Rotation;
        var dir = rot.ToDirection() * (isForward ? 1f : -1f);

        // stopAfterWall
        var dist = Math.Min(distance, Arena.IntersectRayBounds(from, dir) + maxIntersectionError);
        var to = from + dist * dir;

        return (from, to, rot);
    }
}
