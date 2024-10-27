namespace BossMod.Dawntrail.Trial.T03QueenEternal;

// mechanic spawns target markers that result in 5 rectangles of 7 front and 7 back length
// forming almost a circle that can be approximated with a circle radius of sqrt(212)/2 for easy coding
// since i have no clue how to find out the final rotation of the rectangles
// it doesn't seem to be the final rotation of the target and the helpers only spawn as soon as the 1s cast time starts

class WaltzOfTheRegaliaBait(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(MathF.Sqrt(212) * 0.5f);
    private readonly List<(Actor, DateTime)> _targets = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var t in _targets)
            yield return new(circle, t.Item1.Position, default, t.Item2);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.QueenEternal3 && id == 0x11D7)
            _targets.Add((actor, WorldState.FutureTime(7)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WaltzOfTheRegaliaVisual)
            _targets.RemoveAll(x => x.Item1.Position.AlmostEqual(caster.Position, 0.5f));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        // not sure if needed, just a safeguard incase the removal by OnEventCast failed for whatever reason
        if (_targets.Count > 0 && (OID)actor.OID == OID.QueenEternal3)
            _targets.RemoveAll(x => x.Item1 == actor);
    }
}

class WaltzOfTheRegalia(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WaltzOfTheRegalia), new AOEShapeRect(7, 2, 7));