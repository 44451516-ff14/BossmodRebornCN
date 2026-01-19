namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public uint Car = 1u;

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x04 when state == 0x00020001u:
                ++Car; // Car 2
                WPos centerCar2 = new(100f, 150f);
                Arena.Center = centerCar2;
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(centerCar2, 10.5f, 15f)],
                    [new Square(new(107.575f, 147.575f), 2.5f), new Square(new(92.575f, 157.425f), 2.5f),
                    new Square(new(97.5f, 147.5f), 2.01f), new Square(new(102.5f, 157.5f), 2.01f)], AdjustForHitboxInwards: true);
                break;
            case 0x05 when state == 0x00020001u:
                ++Car; // Car 3
                Arena.Center = new(100f, 200f);
                Arena.Bounds = new ArenaBoundsRect(10f, 14.5f);
                break;
        }
    }

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.Aether)
        {
            // Intermission
            var center = new WPos(-400f, -400f);
            Arena.Center = center;
            Arena.Bounds = new ArenaBoundsCustom([new Polygon(center, 14.495f, 64)]);
        }
        else if (actor.OID == (uint)OID.Doomtrain && Car == 3)
        {
            // Return from intermission, back to car 3
            Arena.Center = new(100f, 200f);
            Arena.Bounds = new ArenaBoundsRect(10f, 14.5f);
        }
    }

    // car 4
    // private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    // {
    //     var arena = new ArenaBoundsCustom([new Rectangle(new(100f, 250f), 10.5f, 15f)],
    //     [new Rectangle(new(107.5f, 250f), 2.297f, 4.797f), new Rectangle(new(100f, 252.5f), 5f, 1.95f),
    //     new Rectangle(new(92.5f, 257.5f), 2.297f, 7.297f)], [new Square(new(107.5f, 247.5f), 2.5f), new Rectangle(new(107.75f, 252.5f), 2.25f, 2.5f),
    //     new Rectangle(new(92.5f, 259.75f), 2.5f, 4.75f), new Rectangle(new(92.25f, 252.5f), 2.25f, 2.5f)], AdjustForHitboxInwards: true);
    //     return (arena.Center, arena);
    // }

    // car 5: 92.5, 292.5, 97.5, 292.5, 97.5, 297.5
    // 0x10         0x11                0x15
}
