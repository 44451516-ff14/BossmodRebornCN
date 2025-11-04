namespace BossMod.Endwalker.Ultimate.TOP;

sealed class P2Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);

sealed class P2PacketFilter(BossModule module) : Components.GenericInvincible(module)
{
    enum Firewall
    {
        None,
        PacketFilterF,
        PacketFilterM
    }

    private readonly List<Actor> _omegaM = [];
    private readonly List<Actor> _omegaF = [];

    private readonly Firewall[] _playerStates = Utils.MakeArray(PartyState.MaxPartySize, Firewall.None);

    protected override ReadOnlySpan<Actor> ForbiddenTargets(int slot, Actor actor) => _playerStates.BoundSafeAt(slot) switch
    {
        Firewall.PacketFilterF => CollectionsMarshal.AsSpan(_omegaF),
        Firewall.PacketFilterM => CollectionsMarshal.AsSpan(_omegaM),
        _ => []
    };

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.OmegaF:
                _omegaM.Remove(actor);
                _omegaF.Add(actor);
                break;
            case (uint)SID.OmegaM:
                _omegaF.Remove(actor);
                _omegaM.Add(actor);
                break;
            case (uint)SID.PacketFilterF:
            case (uint)SID.PacketFilterM:
                if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
                {
                    _playerStates[slot] = status.ID == (uint)SID.PacketFilterF ? Firewall.PacketFilterF : Firewall.PacketFilterM;
                }
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.PacketFilterF or (uint)SID.PacketFilterM && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerStates[slot] = Firewall.None;
        }
    }
}

sealed class P4HPThreshold(BossModule module) : Components.HPThreshold(module, (uint)OID.BossP3, 0.2f);
sealed class P5HPThreshold(BossModule module) : Components.HPThreshold(module, (uint)OID.BossP5, 0.2f);
