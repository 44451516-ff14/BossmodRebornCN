namespace BossMod.Dawntrail.Raid.BruteAmbombinatorSharedBounds;

public static class BruteAmbombinatorSharedBounds
{
    public static readonly WPos FirstCenter = new(100f, 100f);
    public static readonly WPos FinalCenter = new(100f, 5f);
    public static readonly ArenaBoundsSquare DefaultArena = new(20f);
    public static readonly ArenaBoundsRect RectArena = new(12.5f, 25f);
    public static readonly ArenaBoundsComplex KnockbackArena = new([new Square(FirstCenter, 20f), new Rectangle(FinalCenter, 12.5f, 25f)]);
}
