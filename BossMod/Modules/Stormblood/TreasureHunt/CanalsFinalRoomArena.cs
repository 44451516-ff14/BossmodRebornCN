namespace BossMod.Stormblood.TreasureHunt;

public abstract class FinalRoomArena(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos center = new(0, -420);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(center, 19.51f * CosPI.Pi8th, 8, 22.5f.Degrees()), new Rectangle(center, 27.66f, 5.5f),
    new Rectangle(new(0, -440), 5.5f, 7.5f)], [new Rectangle(new(0, -400), 20, 3.35f)]);
}
