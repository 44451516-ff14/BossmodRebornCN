namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

sealed class Meltdown(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Meltdown)
        {
            ++NumCasts;
            var count = _aoes.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i) // this assumes player actually moves between the 3x meltdown casts, otherwise 2 or more meltdowns might have the same origin
            {
                if (_aoes[i].Origin == pos)
                {
                    goto skip;
                }
            }
            _aoes.Add(new(circle, caster.Position));
        skip:
            count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin == pos)
                {
                    if (++aoe.ActorID == 8u)
                    {
                        _aoes.RemoveAt(i);
                    }
                    break;
                }
            }
        }
    }
}
