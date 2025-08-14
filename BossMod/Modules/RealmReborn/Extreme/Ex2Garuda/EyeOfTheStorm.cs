namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class EyeOfTheStorm(BossModule module) : Components.GenericAOEs(module, (uint)AID.EyeOfTheStorm)
{
    private Actor? _caster;
    private DateTime _nextCastAt;
    private static readonly AOEShapeDonut _shape = new(12f, 25f); // TODO: verify inner radius

    public bool Active() => _caster?.CastInfo != null || _nextCastAt > WorldState.CurrentTime;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster != null)
            return new AOEInstance[1] { new(_shape, _caster.Position.Quantized(), default, _nextCastAt) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _caster = caster;
            _nextCastAt = Module.CastFinishAt(caster.CastInfo!);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _nextCastAt = WorldState.FutureTime(4.2d);
        }
    }
}
