namespace BossMod.Stormblood.Ultimate.UCOB;

// TODO: generalize to tankswap
sealed class P2Ravensbeak(BossModule module) : BossComponent(module)
{
    private Actor? _caster;
    private ulong _targetId;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_caster == null || _caster.TargetID != _targetId)
        {
            return;
        }

        if (actor.InstanceID == _targetId)
        {
            hints.Add("Pass aggro!");
        }
        else if (actor.Role == Role.Tank)
        {
            hints.Add("Taunt!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Ravensbeak)
        {
            _caster = caster;
            _targetId = spell.TargetID;
        }
    }
}
