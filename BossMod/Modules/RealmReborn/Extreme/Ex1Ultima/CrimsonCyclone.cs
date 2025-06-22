namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

class CrimsonCyclone(BossModule module) : Components.GenericAOEs(module, (uint)AID.CrimsonCyclone)
{
    private Actor? _ifrit; // non-null while mechanic is active
    private DateTime _resolve;

    public bool Active => _ifrit != null;

    private static readonly AOEShapeRect _shape = new(43f, 6f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_ifrit != null)
            return new AOEInstance[1] { new(_shape, _ifrit.Position, _ifrit.Rotation, _resolve) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _ifrit = caster;
            _resolve = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _ifrit = null;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.UltimaIfrit && id == 0x008D)
        {
            _ifrit = actor;
            _resolve = WorldState.FutureTime(5d);
        }
    }
}
