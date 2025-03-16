﻿namespace BossMod.Endwalker.Savage.P8S2;

class AshingBlaze(BossModule module) : Components.GenericAOEs(module)
{
    private WPos? _origin;
    private static readonly AOEShapeRect _shape = new(46, 10);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_origin != null)
            return new AOEInstance[1] { new(_shape, _origin.Value, default, Module.CastFinishAt(Module.PrimaryActor.CastInfo)) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AshingBlazeL:
                _origin = WPos.ClampToGrid(caster.Position - new WDir(_shape.HalfWidth, 0));
                break;
            case (uint)AID.AshingBlazeR:
                _origin = WPos.ClampToGrid(caster.Position + new WDir(_shape.HalfWidth, 0));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AshingBlazeL or (uint)AID.AshingBlazeR)
            _origin = null;
    }
}
