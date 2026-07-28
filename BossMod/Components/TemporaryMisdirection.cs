namespace BossMod.Components;

// generic temporary misdirection component
[SkipLocalsInit]
public abstract class TemporaryMisdirection(BossModule module, uint aid, string hint = "Applies temporary misdirection") : CastHint(module, aid, hint)
{
    private BitMask mask;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
        {
            mask.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
        {
            mask.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (mask[slot])
        {
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
        }
    }
}

// component for Spinning mechanic
[SkipLocalsInit]
public abstract class Spinning(BossModule module, uint aid, bool createforbiddenzones = true, uint statusID = 2973u, string hint = "Applies spinning") : CastHint(module, aid, hint)
{
    internal BitMask mask;
    private readonly uint StatusID = statusID;
    const float SpinningLookahead = 5.5f;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == StatusID)
        {
            mask.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == StatusID)
        {
            mask.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (createforbiddenzones && mask[slot])
        {
            var pos = actor.Position;
            var rot = actor.Rotation;
            // simulate forward forced movement; this is kind of a hack
            // rect is offset by -1 unit player-relative. we know very well that player-centered shapes make the pathfinder freak the fuck out
            hints.AddForbiddenZone(new SDRect(pos, rot, SpinningLookahead, SpinningLookahead + 2, SpinningLookahead + 2), WorldState.FutureTime(2d));
            hints.AddForbiddenZone(new SDCone(pos, 100f, rot + 180f.Degrees(), 45f.Degrees()), DateTime.MaxValue);
        }
    }
}
