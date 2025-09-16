namespace BossMod.Global.MaskedCarnivale;

public static class Layouts
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    private static readonly Angle a45 = 45f.Degrees();
    private static readonly Angle a90a = 89.98f.Degrees(), a90b = 90f.Degrees();
    private static readonly Polygon[] _circleBig = [new Polygon(ArenaCenter, 24.5f * CosPI.Pi32th, 32)];
    private static readonly Rectangle[] wallsInflated = [new(new(90f, 94.75f), 5.5f, 0.75f), new(new(94.75f, 92f), 3.51134109f, 0.75f, a90a),
    new(new(110f, 94.75f), 5.5f, 0.75f), new(new(105.25f, 92f), 3.511f, 0.75f, a90b)];
    private static readonly Rectangle[] walls = [new(new(90f, 94.75f), 5f, 0.25f), new(new(94.75f, 92f), 3.01134109f, 0.25f, a90a),
    new(new(110f, 94.75f), 5f, 0.25f), new(new(105.25f, 92f), 3.01134109f, 0.25f, a90b)];

    private const float sideLength = 2.4f;
    private static readonly Square[] squares = [new(new(110f, 110f), sideLength, a45), new(new(90f, 110f), sideLength, a45),
    new(new(110f, 90f), sideLength, a45), new(new(90f, 90f), sideLength, a45)];

    public static readonly ArenaBoundsCustom Layout4Quads = new(_circleBig, squares);
    public static readonly ArenaBoundsCustom Layout2Corners = new(_circleBig, wallsInflated);
    public static readonly RelSimplifiedComplexPolygon Layout2CornersBlockers = new AOEShapeCustom(_circleBig, walls).GetCombinedPolygon(ArenaCenter);
    public static readonly ArenaBoundsCustom LayoutBigQuad = new(_circleBig, [new Square(ArenaCenter, 5.4f, a45)]);
    public static readonly RelSimplifiedComplexPolygon LayoutBigQuadBlockers = new AOEShapeCustom(_circleBig, [new Square(ArenaCenter, 4.9f, a45)]).GetCombinedPolygon(ArenaCenter);
    public static readonly ArenaBoundsCustom CircleSmall = new([new Polygon(ArenaCenter, 16.01379f, 32)]) { IsCircle = true };
    public static readonly ArenaBoundsCustom CircleBig = new(_circleBig) { IsCircle = true };
}
