namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

sealed class Lunge(BossModule module) : Components.GenericKnockback(module, (uint)AID.Lunge)
{
    private Knockback? _kb;
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly ColossalImpact _aoe = module.FindComponent<ColossalImpact>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _kb);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            List<SafeWall> safewalls = new(4);
            var count = _arena.Squares.Count;
            var rot = spell.Rotation;
            var (vertex1, vertex2) = (int)rot.Deg switch
            {
                -90 => (3, 0),
                180 => (2, 3),
                0 => (0, 1),
                _ => (1, 2) // should be 89
            };
            var dir = -0.5f * rot.Round(90f).ToDirection();
            var center = Arena.Center;
            var adj = center + dir;
            for (var i = 0; i < count; ++i)
            {
                var s = _arena.Squares[i].Contour(center);
                safewalls.Add(new(s[vertex1] + adj, s[vertex2] + adj)); // get the edge that intersects the current knockback, adjusted by hitbox radius
            }
            _arena.Squares.Clear();
            _kb = new(spell.LocXZ, 60f, Module.CastFinishAt(spell), null, rot, Kind.DirForward, safeWalls: safewalls);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = null;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_kb is Knockback knockback)
        {
            ref readonly var kb = ref knockback;
            var act = kb.Activation;
            if (!IsImmune(pcSlot, act))
            {
                var walls = kb.SafeWalls;
                var len = walls.Length;
                var aoesC = _aoe.Casters;
                var count = aoesC.Count;
                var color = Colors.SafeFromAOE;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var w = ref walls[i];
                    ref readonly var v1 = ref w.Vertex1;
                    ref readonly var v2 = ref w.Vertex2;
                    var midpoint = new WPos((v1.X + v2.X) * 0.5f, (v1.Z + v2.Z) * 0.5f);
                    var aoes = CollectionsMarshal.AsSpan(aoesC);
                    var blocked = false;
                    for (var j = 0; j < count; ++j)
                    {
                        ref readonly var aoe = ref aoes[j];
                        if (aoe.Check(midpoint))
                        {
                            blocked = true;
                            break;
                        }
                    }
                    if (blocked)
                    {
                        continue;
                    }
                    Arena.ZoneRect(midpoint, new WDir(1f, default), 4f, 4f, 4f, color);
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb is Knockback knockback)
        {
            ref readonly var kb = ref knockback;
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var dir = kb.Direction.ToDirection();
                var walls = kb.SafeWalls;
                var len = walls.Length;
                var count = _aoe.Casters.Count;
                if (count != 0)
                {
                    var aoesC = _aoe.Casters;
                    hints.AddForbiddenZone(p =>
                    {
                        var distanceToWall = float.MaxValue;
                        for (var i = 0; i < len; ++i)
                        {
                            ref readonly var w = ref walls[i];
                            var intersect = Intersect.RaySegment(p, dir, w.Vertex1, w.Vertex2);
                            if (intersect < distanceToWall)
                            {
                                distanceToWall = intersect;
                            }
                        }
                        if (distanceToWall < 60f)
                        {
                            var aoes = CollectionsMarshal.AsSpan(aoesC);
                            for (var i = 0; i < count; ++i)
                            {
                                ref readonly var aoe = ref aoes[i];
                                if (aoe.Check(p + distanceToWall * dir))
                                {
                                    return default;
                                }
                            }
                            return 1f;
                        }
                        return default;
                    }, act);
                }
                else
                {
                    hints.AddForbiddenZone(p =>
                    {
                        for (var i = 0; i < len; ++i)
                        {
                            ref readonly var w = ref walls[i];
                            if (Intersect.RaySegment(p, dir, w.Vertex1, w.Vertex2) < 60f)
                            {
                                return 1f;
                            }
                        }
                        return default;
                    }, act);
                }
            }
        }
    }
}
