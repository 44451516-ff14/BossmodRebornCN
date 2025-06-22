namespace BossMod.Dawntrail.Savage.M03SBruteBomber;

sealed class KnuckleSandwich(BossModule module) : Components.GenericSharedTankbuster(module, default, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.KnuckleSandwich)
        {
            Source = caster;
            Target = WorldState.Actors.Find(spell.TargetID);
            Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.KnuckleSandwich or (uint)AID.KnuckleSandwichAOE)
        {
            ++NumCasts;
        }
    }
}
