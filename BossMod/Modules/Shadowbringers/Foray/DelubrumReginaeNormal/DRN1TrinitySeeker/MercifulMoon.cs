namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN1TrinitySeeker;

sealed class MercifulMoon(BossModule module) : Components.GenericGaze(module)
{
    private Eye? _eye;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => Utils.ZeroOrOne(ref _eye);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AetherialOrb)
            _eye = new(actor.Position, WorldState.FutureTime(5.8d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MercifulMoon)
        {
            _eye = null;
        }
    }
}
