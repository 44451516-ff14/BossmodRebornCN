namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class StarvingStampede(BossModule module) : Components.GenericAOEs(module, (uint)AID.StarvingStampede)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShape _shape = new AOEShapeCircle(12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.JawsTeleport)
        {
            if (_aoes.Count == 0)
                _aoes.Add(new(_shape, caster.Position));
            _aoes.Add(new(_shape, spell.TargetXZ));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == WatchedAction)
            _aoes.RemoveAt(0);
    }
}
