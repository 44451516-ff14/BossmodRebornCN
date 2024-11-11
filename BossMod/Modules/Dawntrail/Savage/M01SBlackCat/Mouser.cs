namespace BossMod.Dawntrail.Savage.M01SBlackCat;

// note: grid cell indices are same as ENVC indices: 0 for NW, then increasing along x, then increasing along z (so NE is 3, SW is 12, SE is 15)
// normally, boss does 8 sets of 3 jumps then 2 sets of 2 jumps, destroying 12 cells and damaging remaining 4
// on enrage, boss does 8 sets of 4 jumps, destroying all cells
class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsSquare DefaultBounds = new(20);
    private static readonly Square[] defaultSquare = [new(ArenaCenter, 20)];
    public BitMask DamagedCells;
    public BitMask DestroyedCells;
    public static readonly Square[] Tiles = Enumerable.Range(0, 16)
        .Select(index => new Square(CellCenter(index), 5)).ToArray();

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index > 0x0F)
            return;
        switch (state)
        {
            case 0x00020001: // damage tile (first jump)
                DamagedCells.Set(index);
                DestroyedCells.Clear(index);
                break;
            case 0x00200010: // destroy tile (second jump)
                DamagedCells.Clear(index);
                DestroyedCells.Set(index);
                break;
            case 0x01000004: // repair destroyed tile (after initial jumps)
            case 0x00800004: // repair damaged tile (mechanic end)
            case 0x00080004: // start short repair (will finish before kb)
                DamagedCells.Clear(index);
                DestroyedCells.Clear(index);
                break;
            case 0x00400004: // start long repair (won't finish before kb)
                break;
        }
        UpdateArenaBounds();
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GrimalkinGaleSpreadAOE)
        {
            DamagedCells.Reset();
            DestroyedCells.Reset();
            UpdateArenaBounds();
        }
    }

    public static int CellIndex(WPos pos)
    {
        var off = pos - ArenaCenter;
        return (CoordinateIndex(off.Z) << 2) | CoordinateIndex(off.X);
    }

    private static int CoordinateIndex(float coord) => coord switch
    {
        < -10 => 0,
        < 0 => 1,
        < 10 => 2,
        _ => 3
    };

    public static WPos CellCenter(int index)
    {
        var x = -15 + 10 * (index & 3);
        var z = -15 + 10 * (index >> 2);
        return ArenaCenter + new WDir(x, z);
    }

    private void UpdateArenaBounds()
    {
        Shape[] brokenTiles = Tiles.Where((tile, index) => DestroyedCells[index]).ToArray();
        var brokenTilesCount = brokenTiles.Length == 16 ? [] : brokenTiles; // prevents empty sequence error at end of enrage
        ArenaBoundsComplex arena = new(defaultSquare, brokenTiles, Offset: -0.5f);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }

    // public BitMask IntactCells => new BitMask(0xffff) ^ DamagedCells ^ DestroyedCells;
}

class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private bool enrage;
    public static readonly AOEShapeRect Rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        var aoeCount = Math.Clamp(count, 0, enrage ? 4 : NumCasts > 23 ? 2 : 3);
        var aoeCount2 = Math.Clamp(count, 0, enrage ? 4 : NumCasts > 20 ? 4 : 6);
        var totalAoeCount = Math.Min(count, aoeCount + aoeCount2);
        if (count >= totalAoeCount)
            for (var i = aoeCount; i < totalAoeCount; ++i)
                yield return _aoes[i];
        if (count > 0)
            for (var i = 0; i < aoeCount; ++i)
                yield return _aoes[i] with { Color = Colors.Danger };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MouserTelegraphFirst or AID.MouserTelegraphSecond)
            _aoes.Add(new(Rect, caster.Position, spell.Rotation, WorldState.FutureTime(9.7f)));
        else if ((AID)spell.Action.ID == AID.MouserEnrage)
            enrage = true;
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 0x0F && state is 0x00020001 or 0x00200010 && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Mouser)
            ++NumCasts;
    }
}

class GrimalkinGaleShockwave(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GrimalkinGaleShockwaveAOE), 21, true, stopAfterWall: true);
class GrimalkinGaleSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.GrimalkinGaleSpreadAOE), 5);

class SplinteringNails(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SplinteringNailsAOE))
{
    private readonly ElevateAndEviscerate? _jumps = module.FindComponent<ElevateAndEviscerate>();
    private Actor? _source;

    private static readonly AOEShapeCone _shape = new(100, 25.Degrees()); // TODO: verify angle

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source != null && _jumps?.CurrentTarget != actor)
        {
            var pcRole = EffectiveRole(actor);
            var pcDir = Angle.FromDirection(actor.Position - _source.Position);
            if (Raid.WithoutSlot().Exclude(_jumps?.CurrentTarget).Any(a => EffectiveRole(a) != pcRole && _shape.Check(a.Position, _source.Position, pcDir)))
                hints.Add("Spread by roles!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _source == null || _jumps?.CurrentTarget == pc || _jumps?.CurrentTarget == player ? PlayerPriority.Irrelevant : EffectiveRole(player) == EffectiveRole(pc) ? PlayerPriority.Normal : PlayerPriority.Interesting;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_source != null && _jumps?.CurrentTarget != pc)
        {
            var pcDir = Angle.FromDirection(pc.Position - _source.Position);
            _shape.Outline(Arena, _source.Position, pcDir);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SplinteringNails)
        {
            _source = caster;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _source = null;
        }
    }

    private Role EffectiveRole(Actor a) => a.Role == Role.Ranged ? Role.Melee : a.Role;
}
