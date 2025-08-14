namespace BossMod.Dawntrail.Raid.M04NWickedThunder;

sealed class WitchHunt(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(25);
    private static readonly AOEShapeCircle circle = new(6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 12 ? 12 : count;
        var color = Colors.Danger;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (count > 12 && i < 12)
                aoe.Color = color;
            aoe.Risky = true;
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WitchHuntTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 6.3d), risky: false));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.WitchHunt)
            _aoes.RemoveAt(0);
    }
}
