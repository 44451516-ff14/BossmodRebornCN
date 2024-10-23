namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class ThunderPlatform(BossModule module) : Components.GenericAOEs(module)
{
    public BitMask RequireLevitating;
    public BitMask RequireHint;
    private BitMask _levitating;

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (RequireHint[slot])
        {
            var highlightLevitate = RequireLevitating[slot];
            for (var x = 0; x < 2; ++x)
            {
                for (var z = 0; z < 3; ++z)
                {
                    var cellLevitating = ((x ^ z) & 1) != 0;
                    if (cellLevitating != highlightLevitate)
                    {
                        yield return new(_shape, Arena.Center + new WDir(-5 - 10 * x, -10 + 10 * z));
                        yield return new(_shape, Arena.Center + new WDir(+5 + 10 * x, -10 + 10 * z));
                    }
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (RequireHint[slot])
            hints.Add(RequireLevitating[slot] ? "Levitate" : "Stay on ground", RequireLevitating[slot] != _levitating[slot]);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Levitate)
            _levitating.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Levitate)
            _levitating.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
