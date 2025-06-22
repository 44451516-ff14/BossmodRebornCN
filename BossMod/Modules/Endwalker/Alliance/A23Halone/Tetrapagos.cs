namespace BossMod.Endwalker.Alliance.A23Halone;

class Tetrapagos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeDonut donut = new(10f, 30f);
    private static readonly AOEShapeCone cone = new(30f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = new AOEInstance[max];
        for (var i = 0; i < max; ++i)
        {
            var aoe = _aoes[i];
            if (i == 0)
                aoes[i] = count > 1 ? aoe with { Color = Colors.Danger } : aoe;
            else
                aoes[i] = aoe with { Risky = false };
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.TetrapagosHailstormPrepare => circle,
            (uint)AID.TetrapagosSwirlPrepare => donut,
            (uint)AID.TetrapagosRightrimePrepare or (uint)AID.TetrapagosLeftrimePrepare => cone,
            _ => null
        };
        if (shape != null)
            _aoes.Add(new(shape, spell.LocXZ, caster.Rotation, Module.CastFinishAt(spell, 7.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TetrapagosHailstormAOE or (uint)AID.TetrapagosSwirlAOE or (uint)AID.TetrapagosRightrimeAOE or (uint)AID.TetrapagosLeftrimeAOE)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
