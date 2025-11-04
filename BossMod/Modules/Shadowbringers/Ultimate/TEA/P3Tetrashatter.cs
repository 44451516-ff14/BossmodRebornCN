namespace BossMod.Shadowbringers.Ultimate.TEA;

[SkipLocalsInit]
sealed class P3Tetrashatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);

    private readonly AOEShapeCircle circle = new(21f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.JudgmentCrystal)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(5.3d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Tetrashatter)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (pos.AlmostEqual(aoes[i].Origin, 1f))
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
