namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

sealed class RefractionCannons(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);
    private static readonly AOEShapeCone cone = new(40f, 18f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Angle? offset = spell.Action.ID switch
        {
            (uint)AID.ManeuverRefractionCannons1 => -18f.Degrees(),
            (uint)AID.ManeuverRefractionCannons2 => 18f.Degrees(),
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
        if (spell.Action.ID == (uint)AID.RefractionCannons)
        {
            _aoes.Clear();
        }
    }
}

