namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class BladeOfDarkness(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeDonutSector _shapeIn = new(12f, 60f, 75f.Degrees());
    private static readonly AOEShapeCone _shapeOut = new(30f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.BladeOfDarknessLAOE or (uint)AID.BladeOfDarknessRAOE => _shapeIn,
            (uint)AID.BladeOfDarknessCAOE => _shapeOut,
            _ => null
        };
        if (shape != null)
            _aoe = new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BladeOfDarknessLAOE or (uint)AID.BladeOfDarknessRAOE or (uint)AID.BladeOfDarknessCAOE)
        {
            _aoe = null;
            ++NumCasts;
        }
    }
}
