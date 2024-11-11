namespace BossMod.Dawntrail.Raid.M01NBlackCat;

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
        // index per tile, starting north
        // 0x00, 0x01, 0x02, 0x03
        // 0x04, 0x05, 0x06, 0x07
        // 0x08, 0x09, 0x0A, 0x0B
        // 0x0C, 0x0D, 0x0E, 0x0F
        if (index > 0x0F)
            return;
        if (state == 0x00020001) // tile gets damaged
            DamagedCells.Set(index);
        else if (state == 0x00200010) // tile gets broken
        {
            DamagedCells.Clear(index);
            DestroyedCells.Set(index);
        }
        else if (state is 0x01000004 or 0x00800004) // tile gets repaired
        {
            DamagedCells.Clear(index);
            DestroyedCells.Clear(index);
        }
        UpdateArenaBounds();
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
        ArenaBoundsComplex arena = new(defaultSquare, brokenTiles, Offset: -0.5f);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}

class Mouser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public static readonly AOEShapeRect Rect = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        var aoeCount = Math.Clamp(count, 0, NumCasts > 2 ? 2 : 3);
        var aoeCount2 = Math.Clamp(count, 0, 4);
        var totalAoeCount = Math.Min(count, aoeCount + aoeCount2);
        if (_aoes.Count >= totalAoeCount)
            for (var i = aoeCount; i < totalAoeCount; ++i)
                yield return _aoes[i];
        if (_aoes.Count > 0)
            for (var i = 0; i < aoeCount; ++i)
                yield return _aoes[i] with { Color = Colors.Danger };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MouserTelegraphFirst or AID.MouserTelegraphSecond)
            _aoes.Add(new(Rect, caster.Position, spell.Rotation, WorldState.FutureTime(9.7f)));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index <= 0x0F && state is 0x00020001 or 0x00200010 && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Mouser)
            if (++NumCasts == 19)
                NumCasts = 0;
    }
}
