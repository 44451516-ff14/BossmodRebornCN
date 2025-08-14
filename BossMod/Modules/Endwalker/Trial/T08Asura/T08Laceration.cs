namespace BossMod.Endwalker.Trial.T08Asura;

class Laceration(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(9f);
    private readonly List<AOEInstance> _aoes = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D6)
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.CurrentTime.AddSeconds(7.1d - 0.5d * _aoes.Count)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Laceration)
            _aoes.Clear();
    }
}
