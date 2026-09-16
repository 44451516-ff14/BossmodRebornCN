namespace BossMod.Endwalker.Savage.P7SAgdistis;

sealed class WindsHoly(BossModule module) : Components.UniformStackSpread(module, 6, 7, 4)
{
    public int NumCasts;
    private readonly List<Actor>[] _futureStacks = [[], [], [], []];
    private readonly List<Actor>[] _futureSpreads = [[], [], [], []];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.InviolateWinds1:
            case (uint)SID.PurgatoryWinds1:
                AddSpread(actor);
                break;
            case (uint)SID.InviolateWinds2:
            case (uint)SID.PurgatoryWinds2:
                _futureSpreads[0].Add(actor);
                break;
            case (uint)SID.PurgatoryWinds3:
                _futureSpreads[1].Add(actor);
                break;
            case (uint)SID.PurgatoryWinds4:
                _futureSpreads[2].Add(actor);
                break;
            case (uint)SID.HolyBonds1:
            case (uint)SID.HolyPurgation1:
                AddStack(actor);
                break;
            case (uint)SID.HolyBonds2:
            case (uint)SID.HolyPurgation2:
                _futureStacks[0].Add(actor);
                break;
            case (uint)SID.HolyPurgation3:
                _futureStacks[1].Add(actor);
                break;
            case (uint)SID.HolyPurgation4:
                _futureStacks[2].Add(actor);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HemitheosHolyExpire)
        {
            Stacks.Clear();
            Spreads.Clear();
            AddStacks(_futureStacks[NumCasts]);
            AddSpreads(_futureSpreads[NumCasts]);
            ++NumCasts;
        }
    }
}
