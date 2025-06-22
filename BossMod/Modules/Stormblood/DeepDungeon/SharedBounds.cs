namespace BossMod.Stormblood.DeepDungeon.HeavenOnHigh;

public static class HoHArenas
{
    public static readonly WPos ArenaCenter = new(-300f, -300f);
    public static readonly Rectangle[] Entrance = [new Rectangle(new(-299.839f, -325.43f), 20f, 1.25f)];
}

public abstract class HoHArena1(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(HoHArenas.ArenaCenter, 24.5f * CosPI.Pi48th, 48, 3.75f.Degrees())], [new Rectangle(new(-300f, -325f), 20f, 1.25f)]);
}

public abstract class HoHArena2(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(HoHArenas.ArenaCenter, 24.5f, 48)], HoHArenas.Entrance);
}
