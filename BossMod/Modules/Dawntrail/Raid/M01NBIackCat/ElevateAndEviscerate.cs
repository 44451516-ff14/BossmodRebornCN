namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class ElevateAndEviscerate(BossModule module) : Components.GenericKnockback(module)
{
    public DateTime Activation;
    public (Actor source, Actor target) Tether;
    private ElevateAndEviscerateHint? _hint = module.FindComponent<ElevateAndEviscerateHint>();
    private ElevateAndEviscerateImpact? _aoe = module.FindComponent<ElevateAndEviscerateImpact>();
    public WPos Cache;
    private ArenaBounds? bounds;
    private RelSimplifiedComplexPolygon poly = new();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Tether != default && actor == Tether.target)
            return new Knockback[1] { new(Tether.source.Position, 10f, Activation, ignoreImmunes: true) };
        return [];
    }

    public override void Update()
    {
        var kb = ActiveKnockbacks(0, Tether.target);
        if (kb.Length != 0)
        {
            var movements = CalculateMovements(0, Tether.target);
            if (movements.Count != 0)
                Cache = movements[0].to;
            if (bounds != Arena.Bounds)
            {
                bounds = Arena.Bounds;
                if (bounds is ArenaBoundsComplex arena)
                {
                    poly = arena.poly.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate)
        {
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ElevateAndEviscerateGood or (uint)TetherID.ElevateAndEviscerateBad)
        {
            if (WorldState.Actors.Find(tether.Target) is Actor t)
            {
                Tether = (source, t);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate)
        {
            Tether = default;
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        _hint ??= Module.FindComponent<ElevateAndEviscerateHint>();
        var aoes = _hint!.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        _aoe ??= Module.FindComponent<ElevateAndEviscerateImpact>();
        var aoes2 = _aoe!.ActiveAOEs(slot, actor);
        var len2 = aoes2.Length;
        for (var i = 0; i < len2; ++i)
        {
            ref readonly var aoe = ref aoes2[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Module.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kb = ActiveKnockbacks(slot, actor);
        if (kb.Length != 0)
        {
            _hint ??= Module.FindComponent<ElevateAndEviscerateHint>();
            _aoe ??= Module.FindComponent<ElevateAndEviscerateImpact>();
            var aoes = _hint!.ActiveAOEs(slot, actor);
            var aoes2 = _aoe!.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var len2 = aoes2.Length;
            var total = len + len2;
            var temp = new WPos[total];
            for (var i = 0; i < len; ++i)
            {
                temp[i] = aoes[i].Origin;
            }
            for (var i = 0; i < len2; ++i)
            {
                temp[len + i] = aoes2[i].Origin;
            }
            var center = Arena.Center;
            var origin = Tether.source.Position;
            var polygon = poly;
            hints.AddForbiddenZone(p =>
            {
                var offsetSource = (p - origin).Normalized();
                var offsetCenter = p - center;
                if (polygon.Contains(offsetCenter + 10f * offsetSource.Normalized()))
                {
                    var offset = p + 10f * offsetSource;
                    for (var i = 0; i < total; ++i)
                    {
                        if (offset.InSquare(temp[i], 5f))
                        {
                            return default;
                        }
                    }
                    return 1f;
                }
                return default;
            }, kb[0].Activation);
        }
    }
}

sealed class ElevateAndEviscerateHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    public static readonly AOEShapeRect Rect = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var tether = _kb.Tether;
        if (tether != default && actor == tether.target)
        {
            var damagedCells = _arena.DamagedCells.SetBits();
            var tiles = ArenaChanges.Tiles;
            var aoes = new List<AOEInstance>();
            var len = damagedCells.Length;
            for (var i = 0; i < len; ++i)
            {
                var tile = tiles[damagedCells[i]];
                aoes.Add(new(Rect, tile.Center, color: Colors.FutureVulnerable, risky: false));
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        return [];
    }
}

sealed class ElevateAndEviscerateImpact(BossModule module) : Components.GenericAOEs(module, default, "GTFO from impact!")
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    private AOEInstance? aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        if (_kb.Tether != default && _kb.Tether.target != actor && Module.InBounds(_kb.Cache))
        {
            aoes.Add(new(ElevateAndEviscerateHint.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, _kb.Activation.AddSeconds(3.6d)));
        }
        if (aoe != null)
        {
            aoes.Add(aoe.Value);
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate && Module.InBounds(_kb.Cache))
        {
            aoe = new(ElevateAndEviscerateHint.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, Module.CastFinishAt(spell, 3.6f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Impact)
        {
            aoe = null;
        }
    }

    public override void Update()
    {
        if (aoe != null && _kb.Tether != default && _kb.Tether.target.IsDead) // impact doesn't happen if player dies between ElevateAndEviscerate and Impact
        {
            aoe = null;
        }
    }
}
