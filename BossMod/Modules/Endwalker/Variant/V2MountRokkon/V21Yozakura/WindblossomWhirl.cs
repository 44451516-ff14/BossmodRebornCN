namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class WindblossomWhirl(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(5f, 60f);
    private AOEInstance? _aoe;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WindblossomWhirlVisual)
        {
            _aoe = new(donut, Arena.Center, default, Module.CastFinishAt(spell, 6.3d));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WindblossomWhirl1 or (uint)AID.WindblossomWhirl2)
        {
            if (++NumCasts == 5 && _aoe != null)
            {
                _aoe = null;
                NumCasts = 0;
            }
        }
    }
}
