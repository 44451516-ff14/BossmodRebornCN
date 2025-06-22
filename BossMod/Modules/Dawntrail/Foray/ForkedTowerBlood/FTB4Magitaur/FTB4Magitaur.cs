namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB4Magitaur;

sealed class UnsealedAura(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnsealedAura);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018, NameID = 13947, SortOrder = 5)]
public sealed class FTB4Magitaur(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(700f, -674f);
    private static readonly Square[] baseArena = [new Square(ArenaCenter, 31.5f)];
    private static readonly ArenaBoundsComplex arena = new([new Polygon(ArenaCenter, 31.5f, 90)], [new Rectangle(new(700f, -705.916f), 7.5f, 1.25f), new Rectangle(new(700f, -641.5f), 7.5f, 1.25f)]);
    public static readonly WPos[] SquarePositions = [new(700f, -659.504f), new(712.554f, -681.248f), new(687.443f, -681.25f)];
    public static readonly Angle[] SquareAngles = [-45f.Degrees(), -15f.Degrees(), 105f.Degrees()];
    public static readonly WDir[] SquareDirs = [SquareAngles[0].ToDirection(), SquareAngles[1].ToDirection(), SquareAngles[2].ToDirection()];
    public static readonly AOEShapeCustom CircleMinusSquares = new(baseArena, [new Square(SquarePositions[0], 10f, SquareAngles[0]),
    new Square(SquarePositions[1], 10f, SquareAngles[1]), new Square(SquarePositions[2], 10f, SquareAngles[2])]);
    public static readonly AOEShapeCustom CircleMinusSquaresSpread = new(baseArena, [new Square(SquarePositions[0], 5f, SquareAngles[0]),
    new Square(SquarePositions[1], 5f, SquareAngles[1]), new Square(SquarePositions[2], 5f, SquareAngles[2])]);
}
