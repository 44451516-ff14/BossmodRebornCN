namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class BrutalRush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrutalGust, new AOEShapeRect(40f, 2f))
{
    private BitMask _pendingRushes;
    public bool HavePendingRushes => _pendingRushes.Any();

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BrutalRush)
            _pendingRushes[Raid.FindSlot(source.InstanceID)] = true;
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BrutalRush)
            _pendingRushes[Raid.FindSlot(source.InstanceID)] = false;
    }
}
