namespace BossMod.Stormblood.Trial.T05Yojimbo;

class Kasha2(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut _donut = new(2.9f, 10f);
    private const float _Kasha2Delay = 4.164f;
    public readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Kasha2)
            _aoes.Add(new(_donut, spell.LocXZ, spell.Rotation, WorldState.FutureTime(_Kasha2Delay)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Kasha2)
            _aoes.Clear();
    }
}
