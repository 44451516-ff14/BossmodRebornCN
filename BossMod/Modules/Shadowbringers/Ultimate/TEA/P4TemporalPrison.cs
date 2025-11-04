namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P4TemporalPrison(BossModule module) : Components.GenericTowers(module)
{
    public int NumPrisons;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.ArenaFeatures)
        {
            switch (state)
            {
                case 0x04000840u:
                    var position = actor.LayoutID switch
                    {
                        0x7C86C6u => 1,
                        0x7C86C8u => 2,
                        0x7C86C9u => 3,
                        0x7C86CAu => 4,
                        0x7C86CBu => 5,
                        0x7C86CCu => 6,
                        0x7C86CDu => 7,
                        _ => -1
                    };
                    if (position != -1)
                    {
                        Towers.Add(new(Arena.Center + 12f * (position * -45f.Degrees() - 180f.Degrees()).ToDirection(), 4f, 1, 1, default, WorldState.FutureTime(8d)));
                    }
                    break;
                case 0x10002000u:
                    if (Towers.Count != 0) // layoutID 0x7C86CE is the final prison that covers the whole arena, we don't want an index out of range error because of it
                    {
                        Towers.RemoveAt(0);
                    }
                    ++NumPrisons;
                    break;
            }
        }
    }
}
