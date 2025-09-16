namespace BossMod.Endwalker.Alliance.A34Eulogia;

sealed class ThousandfoldThrust(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThousandfoldThrustAOEFirst)
        {
            _aoe = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ThousandfoldThrustAOEFirst or (uint)AID.ThousandfoldThrustAOERest)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}
