namespace BossMod.Dawntrail.Raid.M01NBlackCat;

class ElevateAndEviscerate(BossModule module) : Components.Knockback(module, ignoreImmunes: true, stopAfterWall: true)
{
    public DateTime Activation;
    public (Actor source, Actor target) Tether;
    public WPos Cache;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (Tether != default && actor == Tether.target)
            yield return new(Tether.source.Position, 10, Activation);
    }

    public override void Update()
    {
        if (Sources(0, Tether.target).Any())
            Cache = CalculateMovements(0, Tether.target).First().to;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElevateAndEviscerate)
            Activation = Module.CastFinishAt(spell);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.ElevateAndEviscerateGood or (uint)TetherID.ElevateAndEviscerateBad)
            Tether = (source, WorldState.Actors.Find(tether.Target)!);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElevateAndEviscerate)
        {
            Tether = default;
            ++NumCasts;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<ElevateAndEviscerateHint>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Arena.InBounds(pos);
}

class ElevateAndEviscerateHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var tether = _kb.Tether;
        if (tether != default && actor == tether.target)
        {
            var damagedCells = Module.FindComponent<ArenaChanges>()!.DamagedCells;
            var tiles = ArenaChanges.Tiles;

            foreach (var index in damagedCells.SetBits())
            {
                var tile = tiles[index];
                yield return new(Mouser.Rect, tile.Center, Color: Colors.FutureVulnerable, Risky: false);
            }
        }
    }
}

class ElevateAndEviscerateImpact(BossModule module) : Components.GenericAOEs(module, default, "GTFO from impact!")
{
    private readonly ElevateAndEviscerate _kb = module.FindComponent<ElevateAndEviscerate>()!;
    private AOEInstance? aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_kb.Tether != default && _kb.Tether.target != actor && Module.InBounds(_kb.Cache))
            yield return new(Mouser.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, _kb.Activation.AddSeconds(3.6f));
        if (aoe != null)
            yield return aoe.Value;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ElevateAndEviscerate && Module.InBounds(_kb.Cache))
            aoe = new(Mouser.Rect, ArenaChanges.CellCenter(ArenaChanges.CellIndex(_kb.Cache)), default, Module.CastFinishAt(spell, 3.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Impact)
            aoe = null;
    }

    public override void Update()
    {
        if (aoe != null && _kb.Tether != default && _kb.Tether.target.IsDead) // impact doesn't happen if player dies between ElevateAndEviscerate and Impact
            aoe = null;
    }
}
