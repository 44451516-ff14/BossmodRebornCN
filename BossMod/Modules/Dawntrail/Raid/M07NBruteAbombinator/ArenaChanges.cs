namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public static ArenaBoundsCustom GetKnockbackArena() => new([new Square(new(100f, 100f), 20f), new Rectangle(new(100f, 5f), 12.5f, 25f)]);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (index == 0x00)
            {
                Arena.Bounds = new ArenaBoundsRect(12.5f, 25f);
                Arena.Center = new(100f, 5f);
            }
            else if (index == 0x01)
            {
                Arena.Bounds = new ArenaBoundsSquare(20f) { Y = -200f, BorderY = -200f };
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NeoBombarianSpecial)
        {
            var arena = GetKnockbackArena();
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
        }
    }
}
