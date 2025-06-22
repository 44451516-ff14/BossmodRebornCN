namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class Border(BossModule module) : Components.GenericAOEs(module)
{
    private bool Active;
    private static readonly WPos ArenaCenter = new(-416f, -184f);
    private static readonly Polygon[] DefaultPolygon = [new(ArenaCenter, 34.5f, 48)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(DefaultPolygon, [new Rectangle(new(-416f, -219f), 20f, 1.4f), new Rectangle(new(-416f, -149.014f), 20f, 1.25f)]);
    private static readonly Shape[] labyrinthDifference = [new DonutV(ArenaCenter, 30f, 34.5f, 48), new DonutV(ArenaCenter, 17f, 25f, 48), new Polygon(ArenaCenter, 12f, 48)];
    private static readonly Rectangle[] labyrinthUnion = [.. GenerateAlcoves(new(-416f, -211.5f)), .. GenerateAlcoves(WPos.RotateAroundOrigin(22.5f, ArenaCenter, new(-416f, -198.5f)), -22.5f.Degrees())];
    private static readonly ArenaBoundsComplex labPhase = new(DefaultPolygon, labyrinthDifference, labyrinthUnion);
    private static readonly AOEShapeCustom customShape = new(labyrinthDifference, labyrinthUnion);
    private DateTime activation;

    private static Rectangle[] GenerateAlcoves(WPos basePosition, Angle start = default)
    {
        var a45 = -45f.Degrees();

        var rects = new Rectangle[8];
        rects[0] = new(basePosition, 2f, 4f, start);

        for (var i = 1; i < 8; ++i)
            rects[i] = new(WPos.RotateAroundOrigin(i * 45f, ArenaCenter, basePosition), 2f, 4f, start + a45 * i);
        return rects;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            return new AOEInstance[1] { new(customShape, Arena.Center, default, activation) };
        return [];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MemoryOfTheLabyrinth)
        {
            Active = true;
            activation = Module.CastFinishAt(spell, 0.7f);
            Arena.Bounds = labPhase;
            Arena.Center = labPhase.Center;
        }
    }
}
