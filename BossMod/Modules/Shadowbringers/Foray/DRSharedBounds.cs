namespace BossMod.Shadowbringers.Foray.DelubrumReginae;

public abstract class TrinitySeeker(WorldState ws, Actor primary) : BossModule(ws, primary, StartingArena.Center, StartingArena)
{
    private static readonly WPos ArenaCenter = new(default, 278f);
    private static readonly DonutSegmentV[] barricades = [.. GenerateBarricades()];

    private static DonutSegmentV[] GenerateBarricades()
    {
        var barricades = new DonutSegmentV[4];
        var a22 = 22.5f.Degrees();
        var a45 = 45f.Degrees();
        var a90 = 90f.Degrees();
        for (var i = 0; i < 4; ++i)
        {
            var ai = a90 * i;
            barricades[i] = new(ArenaCenter, 19.2f, 21.107f, a45 + ai, a22, 6); // each donut segment got 6 inner and 6 outer edges
        }
        return barricades;
    }

    public static readonly ArenaBoundsCustom StartingArena = new([new Polygon(ArenaCenter, 30f, 48)], [.. barricades, new Rectangle(new(default, 248f), 7.5f, 0.75f), new Rectangle(new(default, 307.85f), 7.5f, 0.75f)], AdjustForHitbox: true);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(ArenaCenter, 25.5f, 48)], barricades, AdjustForHitbox: true);
}

public abstract class Dahu(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(82f, 138f), 29.5f, 48)], [new Rectangle(new(82f, 108.233f), 20f, 1.25f), new Rectangle(new(82f, 167.738f), 20f, 1.25f)]);
}

public abstract class QueensGuard(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    private static readonly WPos center = new(244f, -162f);
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(center, 29.5f, 48)], [new Rectangle(new(244f, -132.145f), 20f, 1.25f), new Rectangle(new(244f, -192.063f), 20f, 1.25f)]);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(center, 25f, 48)]);
}

public abstract class Phantom(WorldState ws, Actor primary) : BossModule(ws, primary, startArenaCenter, new ArenaBoundsRect(23.5f, 29.5f))
{
    private static readonly WPos startArenaCenter = new(202f, -374f);
    public static readonly WPos DefaultCenter = new(202f, -370f);
    public static readonly ArenaBoundsRect DefaultArena = new(23.5f, 24f);
    public static readonly AOEShapeCustom ArenaChange = new([new Rectangle(startArenaCenter, 24f, 30f)], [new Rectangle(DefaultCenter, 24f, 24f)]);
}

public abstract class TrinityAvowed(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    public static readonly WPos ArenaCenter = new(-272f, -82f);
    public static readonly ArenaBoundsSquare StartingArena = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultArena = new(25f);
    public static readonly AOEShapeCustom ArenaChange1 = new([new Square(ArenaCenter, 30f)], [new Square(ArenaCenter, 25f)]);
    public static readonly AOEShapeRect ArenaChange2 = new(45f, 25f);
    public static readonly WPos WestRemovedCenter = new(-252f, -82f);
    public static readonly WPos EastRemovedCenter = new(-292f, -82f);
    public static readonly ArenaBoundsRect ShimmeringShotArena = new(5f, 25f);
}

public abstract class Queen(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(29.5f))
{
    public static readonly WPos ArenaCenter = new(-272f, -415f);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(ArenaCenter, 25f, 48)]);
    public static readonly AOEShapeDonut ArenaChange = new(25f, 43f);
}
