namespace BossMod.Endwalker.Trial.T02Hydaelyn;

sealed class Echoes(BossModule module) : Components.UniformStackSpread(module, 6f, 0f, 8, 8)
{
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Echoes)
        {
            ++NumCasts;
            if (NumCasts == 5)
            {
                Stacks.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Echoes)
        {
            AddStack(actor, WorldState.FutureTime(5d));
        }
    }
}
