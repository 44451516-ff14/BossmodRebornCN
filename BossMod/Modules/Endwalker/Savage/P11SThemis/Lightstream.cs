﻿namespace BossMod.Endwalker.Savage.P11SThemis;

class Lightstream(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(50, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LightstreamAOEFirst or (uint)AID.LightstreamAOERest)
        {
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rotation = iconID switch
        {
            (uint)IconID.RotateCW => -10f.Degrees(),
            (uint)IconID.RotateCCW => 10f.Degrees(),
            _ => default
        };
        if (rotation != default)
        {
            for (var i = 0; i < 7; ++i)
                _aoes.Add(new(_shape, WPos.ClampToGrid(actor.Position), actor.Rotation + i * rotation, WorldState.FutureTime(8 + i * 1.1f)));
            _aoes.SortBy(x => x.Activation);
        }
    }
}
