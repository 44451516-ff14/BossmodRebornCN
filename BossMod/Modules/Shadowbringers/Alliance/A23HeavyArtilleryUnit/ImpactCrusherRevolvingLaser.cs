namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

sealed class ManeuverImpactCrusherRevolvingLaser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCircle circle = new(8f);
    private static readonly AOEShapeDonut donut = new(12f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ManeuverImpactCrusherVisual2)
        {
            AddAOE(circle);
            if (_aoes.Count == 3)
            {
                AddAOE(donut, 4.1d);
            }
            void AddAOE(AOEShape shape, double delay = 0.2d) => _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.ManeuverImpactCrusher or (uint)AID.ManeuverRevolvingLaser)
        {
            _aoes.RemoveAt(0);
        }
    }
}
