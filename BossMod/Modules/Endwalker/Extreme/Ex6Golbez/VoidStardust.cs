namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class VoidStardust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(6);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 12 ? 12 : count;
        var aoes = new AOEInstance[max];

        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                aoes[i] = count > 2 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(float delay = 0f) => _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, delay)));
        switch (spell.Action.ID)
        {
            case (uint)AID.VoidStardustFirst:
                AddAOE();
                break;
            case (uint)AID.VoidStardustRestVisual:
                AddAOE(2.9f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.VoidStardustFirst or (uint)AID.VoidStardustRestAOE)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}

class AbyssalQuasar(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.AbyssalQuasar, 3f, 2, 2);
