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
                ++Car;
                WPos centerCar2 = new(100f, 150f);
                Arena.Center = centerCar2;
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(centerCar2, 10.5f, 15f)],
                    [new Square(new(107.575f, 147.575f), 2.5f), new Square(new(92.575f, 157.425f), 2.5f),
                    new Square(new(97.5f, 147.5f), 2.01f), new Square(new(102.5f, 157.5f), 2.01f)], AdjustForHitboxInwards: true);
                break;
            case 0x05 when state == 0x00020001u:
                ++Car;
                Arena.Center = new(100f, 200f);
                Arena.Bounds = new ArenaBoundsRect(10f, 14.5f);
                break;
        }
    }
}
