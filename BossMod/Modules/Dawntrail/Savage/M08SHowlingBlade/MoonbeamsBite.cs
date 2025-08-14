namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class MoonbeamsBite(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 10f);
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly List<Actor> casters = new(4);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var max = count > 2 ? 2 : count;
        if (count > 1)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
        }
        return aoes[..max];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.MoonlitShadow && modelState is 6 or 7)
        {
            casters.Add(actor);
        }
    }

    public override void Update()
    {
        var count = casters.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var c = casters[i];
            if (c.LastFrameMovementVec4 == default) // depending on server ticks or latency the actor can still be moving and/or rotate when the model state changes
            {
                var rot = c.Rotation;
                _aoes.Add(new(rect, (c.Position + (c.ModelState.ModelState == 7 ? 1f : -1f) * 10f * (rot + 90f.Degrees()).ToDirection()).Quantized(), rot, WorldState.FutureTime(11.1d + 1d * _aoes.Count), actorID: c.InstanceID));
                casters.RemoveAt(i);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // ensure pixel perfectness (even though the prediction should have less than 0.001y error)
        if (spell.Action.ID is (uint)AID.MoonbeamsBite1 or (uint)AID.MoonbeamsBite2)
        {
            var count = _aoes.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    aoe.Origin = spell.LocXZ;
                    aoe.Rotation = spell.Rotation;
                    aoe.Activation = Module.CastFinishAt(spell);
                    return;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MoonbeamsBite1 or (uint)AID.MoonbeamsBite2)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
