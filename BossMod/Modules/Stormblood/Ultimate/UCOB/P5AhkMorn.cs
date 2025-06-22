namespace BossMod.Stormblood.Ultimate.UCOB;

class P5AhkMorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.AkhMorn, 4f)
{
    // cast is only a first hit, don't deactivate
    public override void OnCastFinished(Actor caster, ActorCastInfo spell) { }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMorn or (uint)AID.AkhMornAOE)
            ++NumCasts;
    }
}
