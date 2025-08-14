namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class MagicalInterference(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);
    private static readonly AOEShapeRect rect = new(50f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        ref var aoeL = ref aoes[^1];
        var act0 = aoe0.Activation;
        var deadline1 = act0.AddSeconds(6d);
        var deadline2 = act0.AddSeconds(2d);
        var actLast = aoeL.Activation.AddSeconds(-1d);

        var index = 0;
        var color = Colors.Danger;
        while (index < count)
        {
            ref var cur = ref aoes[index];
            var curAct = cur.Activation;
            if (curAct >= deadline1)
            {
                break;
            }
            var belowDeadline = curAct < deadline2;
            if (belowDeadline || _aoes.Count < 6)
            {
                cur.Risky = true;
            }
            if (belowDeadline)
            {
                cur.Color = curAct < actLast ? color : default;
            }
            ++index;
        }
        return aoes[..index];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.MagicalInterference)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(8.1d), risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.MagicalInterference)
        {
            _aoes.RemoveAt(0);
        }
    }
}
