namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class WeaponTracker(BossModule module) : Components.GenericAOEs(module)
{
    public bool AOEImminent;
    private AOEInstance[] _aoe = [];
    public enum Stance { None, Sword, Staff, Chakram }
    public Stance CurStance;
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeCross cross = new(40f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.HydaelynsWeapon)
        {
            var activation = WorldState.FutureTime(6d);
            if (status.Extra == 0x1B4)
            {
                _aoe = [new(circle, actor.Position.Quantized(), default, activation)];
                CurStance = Stance.Staff;
                AOEImminent = true;
            }
            else if (status.Extra == 0x1B5)
            {
                _aoe = [new(donut, actor.Position.Quantized(), default, activation)];
                AOEImminent = true;
                CurStance = Stance.Chakram;
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.HydaelynsWeapon)
        {
            _aoe = [new(cross, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(6.9d))];
            AOEImminent = true;
            CurStance = Stance.Sword;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WeaponChangeAOEChakram or (uint)AID.WeaponChangeAOEStaff or (uint)AID.WeaponChangeAOESword)
        {
            AOEImminent = false;
            _aoe = [];
        }
    }
}
