namespace BossMod.Stormblood.Ultimate.UWU;

class P4CeruleumVent(BossModule module) : Components.GenericAOEs(module, (uint)AID.CeruleumVent)
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(14f);

    public bool Active => _source != null;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            return new AOEInstance[1] { new(_shape, _source.Position, _source.Rotation, _activation) };
        return [];
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.UltimaWeapon && id == 0x1E43)
        {
            _source = actor;
            _activation = WorldState.FutureTime(10.1d);
        }
    }
}
