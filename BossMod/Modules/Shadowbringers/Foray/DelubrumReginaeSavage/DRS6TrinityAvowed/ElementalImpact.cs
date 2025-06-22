namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

sealed class ElementalImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpact1 or (uint)AID.ElementalImpact2)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ElementalImpactAOE1 or (uint)AID.ElementalImpactAOE2 or (uint)AID.ElementalImpactAOE3 or (uint)AID.ElementalImpactAOE4)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }
}
