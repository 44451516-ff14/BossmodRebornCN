namespace BossMod.Dawntrail.Raid.M01NBlackCat;

sealed class ElevateAndEviscerate(BossModule module) : Components.GenericKnockback(module, ignoreImmunes: true, stopAfterWall: true)
{
    public DateTime Activation;
    public (Actor source, Actor target) Tether;
    public WPos Cache;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (Tether != default && actor == Tether.target)
            return new Knockback[1] { new(Tether.source.Position, 10f, Activation) };
        return [];
    }

    public override void Update()
    {
        foreach (var _ in ActiveKnockbacks(0, Tether.target))
        {
            var movements = CalculateMovements(0, Tether.target);
            if (movements.Count != 0)
                Cache = movements[0].to;
            return;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ElevateAndEviscerateGood or (uint)TetherID.ElevateAndEviscerateBad)
            Tether = (source, WorldState.Actors.Find(tether.Target)!);
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
        var comp = Module.FindComponent<ElevateAndEviscerateHint>();
        if (comp != null)
        {
            var aoes = comp.ActiveAOEs(slot, actor);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                if (aoes[i].Check(pos))
                    return true;
            }
        }
        return !Module.InBounds(pos);
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
                aoes.Add(new(Rect, tile.Center, Color: Colors.FutureVulnerable, Risky: false));
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
            aoes.Add(new(ElevateAndEviscerateHint.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, _kb.Activation.AddSeconds(3.6d)));
        if (aoe != null)
            aoes.Add(aoe.Value);
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElevateAndEviscerate && Module.InBounds(_kb.Cache))
            aoe = new(ElevateAndEviscerateHint.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, Module.CastFinishAt(spell, 3.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Impact)
            aoe = null;
    }

    public override void Update()
    {
        if (aoe != null && _kb.Tether != default && _kb.Tether.target.IsDead) // impact doesn't happen if player dies between ElevateAndEviscerate and Impact
            aoe = null;
    }
}
