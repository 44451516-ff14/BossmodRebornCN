namespace BossMod.Shadowbringers.Ultimate.TEA;

// note: sets are 2s apart, 8-9 casts per set
sealed class P4AlmightyJudgment(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos pos, DateTime activation)> _casters = [];

    private static readonly AOEShapeCircle _shape = new(6f);

    public bool Active => _casters.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countC = _casters.Count;
        if (countC == 0)
            return [];

        var act = _casters[0].activation;
        var deadlineImminent = act.AddSeconds(1d);
        var deadlineFuture = act.AddSeconds(3d);

        var count = 0;

        for (var i = countC - 1; i >= 0; --i)
        {
            if (_casters[i].activation <= deadlineFuture)
                ++count;
        }

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;

        for (var i = countC - 1; i >= 0; --i)
        {
            var c = _casters[i];
            if (c.activation <= deadlineFuture)
            {
                var color = c.activation < deadlineImminent ? Colors.Danger : 0;
                aoes[index++] = new(_shape, c.pos, default, c.activation, color);
            }
        }

        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AlmightyJudgmentVisual)
            _casters.Add((spell.LocXZ, WorldState.FutureTime(8d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AlmightyJudgmentAOE)
            _casters.RemoveAll(c => c.pos.AlmostEqual(caster.Position, 1f));
    }
}
