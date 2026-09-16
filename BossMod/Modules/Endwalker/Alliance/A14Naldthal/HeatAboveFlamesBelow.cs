namespace BossMod.Endwalker.Alliance.A14Naldthal;

sealed class HeatAboveFlamesBelow(BossModule module) : Components.GenericAOEs(module)
{
    public List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _shapeOut = new(8f);
    private readonly AOEShapeDonut _shapeIn = new(8f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.FlamesOfTheDeadReal => _shapeIn,
            (uint)AID.LivingHeatReal => _shapeOut,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FlamesOfTheDeadReal or (uint)AID.LivingHeatReal)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
