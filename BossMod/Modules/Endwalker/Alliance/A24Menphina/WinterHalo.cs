namespace BossMod.Endwalker.Alliance.A24Menphina;

sealed class WinterHalo(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeDonut _shape = new(10f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WinterHaloShortAOE or (uint)AID.WinterHaloLongMountedAOE or (uint)AID.WinterHaloLongDismountedAOE)
        {
            _aoe = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WinterHaloShortAOE or (uint)AID.WinterHaloLongMountedAOE or (uint)AID.WinterHaloLongDismountedAOE)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}
