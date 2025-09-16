namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class MercifulMoon(BossModule module) : Components.GenericGaze(module, (uint)AID.MercifulMoon)
{
    private Eye[] _eye = [];

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor) => _eye;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AetherialOrb)
        {
            _eye = [new(actor.Position, WorldState.FutureTime(5.8d))];
        }
    }
}
