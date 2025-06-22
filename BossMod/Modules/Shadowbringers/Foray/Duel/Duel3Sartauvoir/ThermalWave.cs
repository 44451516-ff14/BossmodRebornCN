namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

sealed class ThermalWave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private static readonly AOEShapeCone cone = new(60f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThermalWaveVisual3)
        {
            _aoes.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ThermalWave)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
                _aoes.RemoveAt(0);
        }
    }
}
