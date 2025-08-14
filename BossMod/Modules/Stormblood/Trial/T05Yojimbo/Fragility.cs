namespace BossMod.Stormblood.Trial.T05Yojimbo;

class Fragility(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(8f);
    private const double _fragilityDelay = 4.85d;
    public readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Inoshikacho)
            _aoes.Add(new(_circle, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(_fragilityDelay)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fragility)
            _aoes.Clear();
    }
}
