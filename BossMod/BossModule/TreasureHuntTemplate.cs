namespace BossMod;

// for treasure hunt roulettes
public abstract class THTemplate(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(100f, 100f), 19f, 48)]);
}
