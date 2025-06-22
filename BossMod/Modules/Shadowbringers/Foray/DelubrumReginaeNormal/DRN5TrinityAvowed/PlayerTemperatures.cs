namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class PlayerTemperatures(BossModule module) : BossComponent(module)
{
    public readonly uint[] Temperatures = new uint[PartyState.MaxAllianceSize];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot is < 0 or > 23)
            return;
        switch (status.ID)
        {
            case (uint)SID.RunningHot1:
                Temperatures[slot] = 1u;
                break;
            case (uint)SID.RunningHot2:
                Temperatures[slot] = 2u;
                break;
            case (uint)SID.RunningCold1:
                Temperatures[slot] = 3u;
                break;
            case (uint)SID.RunningCold2:
                Temperatures[slot] = 4u;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (Raid.FindSlot(actor.InstanceID) is var slot && slot is < 0 or > 23)
            return;
        switch (status.ID)
        {
            case (uint)SID.RunningHot1:
            case (uint)SID.RunningHot2:
            case (uint)SID.RunningCold1:
            case (uint)SID.RunningCold2:
                Temperatures[slot] = default;
                break;
        }
    }
}
