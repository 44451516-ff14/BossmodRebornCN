namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

sealed class BrutalSmashTB(BossModule module) : Components.GenericSharedTankbuster(module, default, 6f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BrutalSmashTB)
        {
            Source = Module.PrimaryActor;
            Target = actor;
            Activation = WorldState.FutureTime(5.9d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BrutalSmashTB1 or (uint)AID.BrutalSmashTB2)
        {
            Source = null;
            Target = null;
        }
    }
}
