namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class AlexandrianThunderIV(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private static readonly AOEShapeDonut donut = new(8f, 24f);
    private readonly List<AOEInstance> _aoes = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.AlexandrianThunderIVCircle => circle,
            (uint)AID.AlexandrianThunderIVDonut => donut,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlexandrianThunderIVCircle or (uint)AID.AlexandrianThunderIVDonut)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
