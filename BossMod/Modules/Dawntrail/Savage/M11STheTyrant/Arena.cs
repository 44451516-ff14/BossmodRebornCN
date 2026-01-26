namespace BossMod.Dawntrail.Savage.M11STheTyrant;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    // Full arena: 40x40 square centered at (100, 100)
    // Split arena: 50x40 rectangle centered at (100, 100) with 10x40 rectangle removed from middle
    //   split arena also slides the east/west halves outward by 5 units each

    public static readonly WPos ArenaCenter = new(100, 100);
    public static readonly WPos WestArenaCenter = new(85, 100);
    public static readonly WPos EastArenaCenter = new(115, 100);
    private static readonly Shape[] BigArenaShapes =
    [
        new PolygonCustom([
            new(75, 80),
            new(75, 120),
            new(125, 120),
            new(125, 80),
        ])
    ];

    // Removed middle strip (north–south), splitting east/west
    private static readonly Shape[] RemovedMiddle =
    [
        new PolygonCustom([
            new(95, 80),
            new(95, 120),
            new(105, 120),
            new(105, 80),
        ])
    ];
    private static readonly Shape[] LeftOnly =
    [
        new PolygonCustom([
            new(75, 80),
            new(75, 120),
            new(95, 120),
            new(95, 80),
        ])
    ];
    private static readonly Shape[] RightOnly =
    [
        new PolygonCustom([
            new(105, 80),
            new(105, 120),
            new(125, 120),
            new(125, 80),
        ])
    ];
    public static readonly ArenaBoundsCustom WestOnly = new(LeftOnly);
    public static readonly ArenaBoundsCustom EastOnly = new(RightOnly);
    // Initial arena (no split)
    public static readonly ArenaBounds InitialBounds = new ArenaBoundsSquare(20);

    // Arena after Flatliner
    public static readonly ArenaBoundsCustom SplitArena =
        new(BigArenaShapes, RemovedMiddle);

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        // Map changes in 0x80000000D param 1
        // 0x01u = base arena
        // 0x02u = split arenas
        // 0x08u = destroying west platform
        // 0x10u = destroying east platform
        if (updateID != 0x8000000D || param1 > 0x10u)
        {
            return;
        }
        switch (param1)
        {
            case 0x01u:
                Arena.Bounds = InitialBounds;
                Arena.Center = ArenaCenter;
                break;
            case 0x02u:
                Arena.Bounds = SplitArena;
                Arena.Center = ArenaCenter;
                break;
            case 0x08u:
                Arena.Bounds = EastOnly;
                Arena.Center = EastArenaCenter;
                break;
            case 0x10u:
                Arena.Bounds = WestOnly;
                Arena.Center = WestArenaCenter;
                break;
            default: break;
        }
    }
}