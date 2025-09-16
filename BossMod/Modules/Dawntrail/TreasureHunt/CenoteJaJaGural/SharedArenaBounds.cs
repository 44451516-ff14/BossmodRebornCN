namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural;

public abstract class SharedBoundsBoss(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    public static readonly ArenaBoundsCustom ArenaBounds = new([new Polygon(new(0, -372), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(0, -352), 20, 1.65f)]);
}
