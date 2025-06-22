namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class FieryIcyPortent(BossModule module) : Components.StayMove(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var req = spell.Action.ID switch
        {
            (uint)AID.FieryPortent => Requirement.Stay,
            (uint)AID.IcyPortent => Requirement.Move,
            _ => Requirement.None
        };
        if (req != Requirement.None)
        {
            Array.Fill(PlayerStates, new(req, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FieryPortent or (uint)AID.IcyPortent)
        {
            Array.Clear(PlayerStates);
        }
    }
}
