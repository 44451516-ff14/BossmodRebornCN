namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

abstract class TemperatureAOE(BossModule module) : Components.GenericAOEs(module)
{
    private class PlayerState
    {
        public int BaseTemperature;
        public int Brand;

        public int Temperature => BaseTemperature + Brand;
    }

    private readonly Dictionary<ulong, PlayerState> _playerState = [];

    public int Temperature(Actor player) => _playerState.GetValueOrDefault(player.InstanceID)?.Temperature ?? 0;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.RunningHot1:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = +1;
                break;
            case (uint)SID.RunningHot2:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = +2;
                break;
            case (uint)SID.RunningCold1:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = -1;
                break;
            case (uint)SID.RunningCold2:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = -2;
                break;
            case (uint)SID.HotBrand1:
                _playerState.GetOrAdd(actor.InstanceID).Brand = +1;
                break;
            case (uint)SID.HotBrand2:
                _playerState.GetOrAdd(actor.InstanceID).Brand = +2;
                break;
            case (uint)SID.ColdBrand1:
                _playerState.GetOrAdd(actor.InstanceID).Brand = -1;
                break;
            case (uint)SID.ColdBrand2:
                _playerState.GetOrAdd(actor.InstanceID).Brand = -2;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.RunningHot1:
            case (uint)SID.RunningHot2:
            case (uint)SID.RunningCold1:
            case (uint)SID.RunningCold2:
                _playerState.GetOrAdd(actor.InstanceID).BaseTemperature = 0;
                break;
            case (uint)SID.HotBrand1:
            case (uint)SID.HotBrand2:
            case (uint)SID.ColdBrand1:
            case (uint)SID.ColdBrand2:
                _playerState.GetOrAdd(actor.InstanceID).Brand = 0;
                break;
        }
    }
}
