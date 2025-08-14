namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P2PlasmaShield(BossModule module) : Components.DirectionalParry(module, [(uint)OID.PlasmaShield])
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.PlasmaShield)
            PredictParrySide(actor.InstanceID, Side.Left | Side.Right | Side.Back);
    }
}
