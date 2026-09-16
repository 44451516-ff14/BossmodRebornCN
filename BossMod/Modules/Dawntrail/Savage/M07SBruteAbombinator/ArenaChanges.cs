namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
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
            var arena = Dawntrail.Raid.M07NBruteAbombinator.ArenaChanges.GetKnockbackArena();
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
        }
    }
}
