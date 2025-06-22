namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

sealed class ArroganceIncarnate(BossModule module) : Components.StackWithIcon(module, (uint)IconID.ArroganceIncarnate, (uint)AID.ArroganceIncarnateAOE, 6f, 5.8f, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArroganceIncarnate)
            NumFinishedStacks = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
