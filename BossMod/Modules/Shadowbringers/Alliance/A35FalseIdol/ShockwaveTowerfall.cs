namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class Towerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(70f, 7f);
    private int numKnockbacks;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => numKnockbacks == 2 ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(30.2d), risky: false));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ShockwaveKB:
                ++numKnockbacks;
                break;
            case (uint)AID.TowerfallVisual:
                var count = _aoes.Count;
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    aoe.Risky = true;
                }
                break;
            case (uint)AID.Towerfall:
                numKnockbacks = 0;
                _aoes.Clear();
                break;
        }
    }
}

sealed class ShockwaveKB(BossModule module) : Components.GenericKnockback(module, (uint)AID.ShockwaveKB)
{
    private readonly List<Knockback> _kbs = new(2);
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly List<SafeWall> walls = new(20);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kbs.Count != 0 ? CollectionsMarshal.AsSpan(_kbs)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kbs.Add(new(spell.LocXZ, 35f, Module.CastFinishAt(spell), safeWalls: walls));
            if (walls.Count == 0)
            {
                NumCasts = 0;
                var center = Arena.Center;
                var edges = PolygonUtil.EnumerateEdges(CollectionsMarshal.AsSpan(new Polygon(caster.Position, 7.5f, 20).Contour(center)));
                var len = edges.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var edge = ref edges[i];
                    walls.Add(new(edge.Item1 + center, edge.Item2 + center));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_kbs.Count != 0 && spell.Action.ID == WatchedAction)
        {
            _kbs.RemoveAt(0);
            if (_kbs.Count == 0)
            {
                walls.Clear();
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_arena.AOEs);
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs.Count != 0)
        {
            ref readonly var kb = ref _kbs.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var loc = kb.Origin;
                var center = Arena.Center;
                var dir = (-45f).Degrees().ToDirection();
                var locAdj = (loc - center).Rotate(180f.Degrees()) + center;
                // square intentionally smaller for optimal knockbacks
                hints.AddForbiddenZone(p =>
                {
                    var proj = p + 35f * (p - loc).Normalized();
                    if (NumCasts == 0)
                    {
                        if (proj.InRect(locAdj, dir, 20f, 20f, 7f))
                        {
                            return default;
                        }
                    }
                    if (proj.InSquare(center, 20f))
                    {
                        return 1f;
                    }
                    return default;
                }, act);
            }
        }
    }
}
