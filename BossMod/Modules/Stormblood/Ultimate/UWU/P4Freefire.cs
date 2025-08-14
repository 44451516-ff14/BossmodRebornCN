namespace BossMod.Stormblood.Ultimate.UWU;

class P4Freefire(BossModule module) : Components.GenericAOEs(module, (uint)AID.FreefireIntermission)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShape _shape = new AOEShapeCircle(15); // TODO: verify falloff

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Helper && id == 0x0449)
        {
            _aoes.Add(new(_shape, actor.Position.Quantized(), default, WorldState.FutureTime(5.9d)));
        }
    }
}
