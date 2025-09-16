namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P6HotWingTail(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeWing = new(50f, 10.5f);
    private static readonly AOEShapeRect _shapeTail = new(50f, 8f);

    public int NumAOEs => _aoes.Count; // 0 if not started, 1 if tail, 2 if wings

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.HotWingAOE => _shapeWing,
            (uint)AID.HotTailAOE => _shapeTail,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    // note: we don't remove aoe's, since that is used e.g. by spreads component
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HotWingAOE or (uint)AID.HotTailAOE)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
