namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P7FlamesIceOfAscalon(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeCircle _shapeOut = new(8f);
    private static readonly AOEShapeDonut _shapeIn = new(8f, 50f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.GenericMechanic && actor.OID == (uint)OID.DragonKingThordan)
        {
            _aoe = [new(status.Extra == 0x12B ? _shapeIn : _shapeOut, actor.Position, default, WorldState.FutureTime(6.2d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FlamesOfAscalon or (uint)AID.IceOfAscalon)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}
