namespace BossMod.Stormblood.Trial.T05Yojimbo;

class ElectrogeneticForce2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrogeneticForce2, _circle)
{
    private static readonly AOEShapeCircle _circle = new(8f);
    private const float _electrogeneticForce2Delay = 6.7f;
    public readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ElectrogeneticForce)
            _aoes.Add(new(_circle, actor.Position, default, WorldState.FutureTime(_electrogeneticForce2Delay)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrogeneticForce2)
            _aoes.Clear();
    }
}
