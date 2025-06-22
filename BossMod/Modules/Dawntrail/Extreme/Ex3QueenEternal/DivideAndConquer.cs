namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class DivideAndConquerBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DivideAndConquerBait)
{
    private static readonly AOEShapeRect _shape = new(60f, 2.5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DivideAndConquer && WorldState.Actors.Find(targetID) is var target && target != null)
            CurrentBaits.Add(new(actor, target, _shape, WorldState.FutureTime(3.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count != 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}

sealed class DivideAndConquerAOE(BossModule module) : Components.GenericAOEs(module, (uint)AID.DivideAndConquerBait)
{
    private static readonly AOEShapeRect rect = new(60f, 2.5f);
    public readonly List<AOEInstance> AOEs = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
            AOEs.Add(new(rect, caster.Position, caster.Rotation, WorldState.FutureTime(11d - AOEs.Count)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DivideAndConquerAOE)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}
