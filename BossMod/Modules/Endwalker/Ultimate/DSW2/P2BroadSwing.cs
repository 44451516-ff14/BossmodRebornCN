namespace BossMod.Endwalker.Ultimate.DSW2;

class P2BroadSwing(BossModule module) : Components.GenericAOEs(module, (uint)AID.BroadSwingAOE)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _aoe = new(40f, 60f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var rot = spell.Action.ID switch
        {
            (uint)AID.BroadSwingRL => -60f.Degrees(),
            (uint)AID.BroadSwingLR => 60f.Degrees(),
            _ => default
        };
        if (rot != default)
        {
            _aoes.Add(new(_aoe, spell.LocXZ, spell.Rotation + rot, Module.CastFinishAt(spell, 0.8f), Colors.Danger));
            _aoes.Add(new(_aoe, spell.LocXZ, spell.Rotation - rot, Module.CastFinishAt(spell, 1.8f)));
            _aoes.Add(new(_aoe, spell.LocXZ, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 2.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            if (_aoes.Count > 0)
                _aoes.AsSpan()[0].Color = Colors.Danger;
        }
    }
}
