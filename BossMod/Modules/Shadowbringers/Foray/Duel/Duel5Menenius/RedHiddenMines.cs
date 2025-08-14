namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

sealed class RedHiddenMines(BossModule module) : Components.GenericAOEs(module)
{
    private List<AOEInstance> _mines = [];
    private static readonly AOEShapeCircle _shapeTrigger = new(3.6f);
    private static readonly AOEShapeCircle _shapeExplosion = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_mines);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ActivateRedMine)
            _mines.Add(new(_shapeTrigger, caster.Position.Quantized(), color: Colors.Trap));
        else if (spell.Action.ID is (uint)AID.DetonateRedMine or (uint)AID.Explosion)
        {
            var count = _mines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var m = _mines[i];
                if (m.Origin.AlmostEqual(pos, 1f))
                {
                    _mines.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.IndiscriminateDetonation)
        {
            List<AOEInstance> _detonatingMines = [];
            var count = _mines.Count;
            for (var i = 0; i < count; ++i)
                _detonatingMines.Add(new(_shapeExplosion, _mines[i].Origin));
            _mines = _detonatingMines;
        }
    }
}
