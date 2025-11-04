namespace BossMod.Endwalker.Trial.T02Hydaelyn;

sealed class WeaponTracker(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeCross cross = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HydaelynsWeapon)
        {
            AOEShape? shape = status.Extra switch
            {
                0x1B4 => circle,
                0x1B5 => donut,
                _ => null
            };
            if (shape != null)
            {
                _aoe = [new(shape, Arena.Center.Quantized(), default, WorldState.FutureTime(6d))];
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.HydaelynsWeapon)
        {
            _aoe = [new(cross, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(6.9d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Equinox2 or (uint)AID.HighestHoly or (uint)AID.Anthelion)
        {
            _aoe = [];
        }
    }
}
