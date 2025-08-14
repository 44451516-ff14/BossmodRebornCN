namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

sealed class ColliderCannons(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeCone cone = new(40f, 15f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? offset = spell.Action.ID switch
        {
            (uint)AID.ManeuverColliderCannons1 => -120f.Degrees(),
            (uint)AID.ManeuverColliderCannons2 => (Angle)default,
            (uint)AID.ManeuverColliderCannons3 => 48f.Degrees(),
            _ => null
        };
        if (offset is Angle o)
        {
            var act = Module.CastFinishAt(spell, 0.5d);
            var pos = spell.LocXZ;
            var rot = spell.Rotation;
            var a72 = 72f.Degrees();
            for (var i = 0; i < 5; ++i)
            {
                _aoes.Add(new(cone, pos, rot + o + i * a72, act));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ColliderCannons)
        {
            _aoes.Clear();
        }
    }
}
