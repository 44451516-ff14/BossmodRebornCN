namespace BossMod.Dawntrail.Trial.T03QueenEternal;

// mechanic spawns target markers that result in 5 rectangles of 7 front and 7 back length
// forming almost a circle that can be approximated with a circle radius of sqrt(212)/2 for easy coding
// since i have no clue how to find out the final rotation of the rectangles
// it doesn't seem to be the final rotation of the target and the helpers only spawn as soon as the 1s cast time starts

sealed class WaltzOfTheRegaliaBait(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(MathF.Sqrt(212f) * 0.5f);
    private readonly List<(Actor, DateTime)> _targets = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _targets.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var t = _targets[i];
            aoes[i] = new(circle, t.Item1.Position, default, t.Item2);
        }
        return aoes;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D7 && actor.OID == (uint)OID.QueenEternal3)
        {
            _targets.Add((actor, WorldState.FutureTime(7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WaltzOfTheRegaliaVisual)
        {
            var count = _targets.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (_targets[i].Item1.Position.AlmostEqual(pos, 0.5f))
                {
                    _targets.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        // not sure if needed, just a safeguard incase the removal by OnEventCast failed for whatever reason
        if (actor.OID == (uint)OID.QueenEternal3)
        {
            var count = _targets.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_targets[i].Item1 == actor)
                {
                    _targets.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class WaltzOfTheRegalia(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaltzOfTheRegalia, new AOEShapeRect(14f, 2f));
