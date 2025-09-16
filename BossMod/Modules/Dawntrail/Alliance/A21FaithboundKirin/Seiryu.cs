namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

sealed class EastwindWheel(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(60f, 9f);
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = spell.Action.ID switch
        {
            (uint)AID.EastwindWheelCCWVisual => 45f.Degrees(),
            (uint)AID.EastwindWheelCWVisual => -45f.Degrees(),
            _ => default
        };
        if (offset != default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation.Round(45f);
            AddAOE(rect, rot, 0.8d);
            AddAOE(cone, rot + offset, 2.8d);
            void AddAOE(AOEShape shape, Angle rotation, double delay = default) => _aoes.Add(new(shape, loc, rotation, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EastwindWheelRect or (uint)AID.EastwindWheelCone)
        {
            if (++NumCasts > 3 && _aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
            if (_aoes.Count == 0)
            {
                NumCasts = 0;
            }
        }
    }
}
