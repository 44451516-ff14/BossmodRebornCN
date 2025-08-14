namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class FatefulWords(BossModule module) : Components.GenericKnockback(module, (uint)AID.FatefulWordsAOE)
{
    private readonly Kind[] _mechanics = new Kind[PartyState.MaxPartySize];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var kind = _mechanics[slot];
        if (kind != Kind.None)
            return new Knockback[1] { new(Arena.Center, 6f, Module.CastFinishAt(Module.PrimaryActor.CastInfo), kind: kind, ignoreImmunes: true) };
        return [];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var kind = status.ID switch
        {
            (uint)SID.WanderersFate => Kind.AwayFromOrigin,
            (uint)SID.SacrificesFate => Kind.TowardsOrigin,
            _ => Kind.None
        };
        if (kind != Kind.None)
            AssignMechanic(actor, kind);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.WanderersFate or (uint)SID.SacrificesFate)
            AssignMechanic(actor, Kind.None);
    }

    private void AssignMechanic(Actor actor, Kind mechanic)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0 && slot < _mechanics.Length)
            _mechanics[slot] = mechanic;
    }
}
