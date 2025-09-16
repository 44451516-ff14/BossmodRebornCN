namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class SublimeEstoc(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(14);
    private static readonly AOEShapeRect rect = new(40f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x2488 && actor.OID == (uint)OID.SublimeEstoc)
        {
            _aoes.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(5.1d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SublimeEstoc)
        {
            _aoes.Clear();
        }
    }
}
