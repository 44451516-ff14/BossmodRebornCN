namespace BossMod.Stormblood.Extreme.Ex6Byakko;

class StateOfShock(BossModule module) : Components.CastCounter(module, (uint)AID.StateOfShockSecond)
{
    public int NumStuns;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Stun)
            ++NumStuns;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Stun)
            --NumStuns;
    }
}

class HighestStakes(BossModule module) : Components.GenericTowers(module, (uint)AID.HighestStakesAOE)
{
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HighestStakes)
        {
            Towers.Add(new(actor.Position, 6f, 3, 3, _forbidden, WorldState.FutureTime(6.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            Towers.Clear();
            var count = spell.Targets.Count;
            for (var i = 0; i < count; ++i)
                _forbidden[Raid.FindSlot(spell.Targets[i].ID)] = true;
        }
    }
}
