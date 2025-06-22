namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class WickedBolt(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, (uint)AID.WickedBoltAOE, 5, 5.1f, 8, 8)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction)
            ++NumFinishedStacks;
    }
}

sealed class WickedBlaze(BossModule module) : Components.StackWithIcon(module, (uint)IconID.WickedBolt, (uint)AID.WickedBlazeAOE, 10, 5.1f, 4, 4)
{
    private DateTime _nextNonDuplicate; // we only want to count individual ticks, so if only 1 person is alive, it doesn't fuck up the states

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction && WorldState.CurrentTime > _nextNonDuplicate)
        {
            ++NumFinishedStacks;
            _nextNonDuplicate = WorldState.FutureTime(0.5d);
        }
    }
}
