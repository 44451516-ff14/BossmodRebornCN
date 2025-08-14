namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class ChildsPlay : Components.GenericForcedMarch
{
    private readonly List<Actor> targets = new(8);
    private Angle direction;
    private readonly Explosion _aoe;
    private RelSimplifiedComplexPolygon poly = new();
    private bool polyInit;
    private readonly List<Square> squares = new(8);

    public ChildsPlay(BossModule module) : base(module, stopAfterWall: true)
    {
        OverrideDirection = true;
        _aoe = module.FindComponent<Explosion>()!;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChildsPlay1:
                direction = 180f.Degrees();
                InitIfReady();
                break;
            case (uint)AID.ChildsPlay2:
                direction = 90f.Degrees();
                InitIfReady();
                break;
            case (uint)AID.Explosion:
                squares.Add(new(spell.LocXZ, 9f));
                if (squares.Count == 8)
                {
                    AOEShapeCustom combine = new(squares);
                    poly = combine.GetCombinedPolygon(Arena.Center);
                    polyInit = true;
                    squares.Clear();
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ChildsPlay1:
            case (uint)AID.ChildsPlay2:
                direction = default;
                break;
            case (uint)AID.Explosion:
                polyInit = false;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == default && tether.ID == (uint)TetherID.ChildsPlay)
        {
            targets.Add(source);
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (direction != default)
        {
            var count = targets.Count - 1;
            for (var i = count; i >= 0; --i)
            {
                AddForcedMovement(targets[i], direction, 4f, WorldState.FutureTime(9.9d));
                targets.RemoveAt(i);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PayingThePiper)
        {
            State.GetOrAdd(actor.InstanceID).PendingMoves.Clear();
            ActivateForcedMovement(actor, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PayingThePiper)
        {
            DeactivateForcedMovement(actor);
        }
    }

    public override void Update()
    {
        foreach (var p in State)
        {
            if (WorldState.Actors.Find(p.Key) is Actor t && t.IsDead)
            {
                DeactivateForcedMovement(t);
            }
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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!polyInit)
        {
            return;
        }
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }
        var center = Arena.Center;
        var polygon = poly;
        var move0 = state.PendingMoves[0];
        var dir = 24f * move0.dir.ToDirection();
        hints.AddForbiddenZone(p =>
        {
            var projected = p + dir;
            if (projected.InSquare(center, 19f) && !polygon.Contains(projected - center)) // we can ignore the center square, no pattern results in a safe path through it
                return 1f;
            return default;
        }, move0.activation);
    }
}
