namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class ThunderPlatform(BossModule module) : Components.GenericAOEs(module)
{
    public BitMask RequireLevitating;
    public BitMask RequireHint;
    private BitMask _levitating;

    private static readonly AOEShapeRect _shape = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!RequireHint[slot])
            return [];

        var highlightLevitate = RequireLevitating[slot];
        var aoes = new AOEInstance[12];
        var index = 0;

        for (var x = 0; x < 2; ++x)
        {
            for (var z = 0; z < 3; ++z)
            {
                var cellLevitating = ((x ^ z) & 1) != 0;
                if (cellLevitating != highlightLevitate)
                {
                    aoes[index++] = new(_shape, Arena.Center + new WDir(-5f - 10f * x, -10f + 10f * z));
                    aoes[index++] = new(_shape, Arena.Center + new WDir(+5f + 10f * x, -10f + 10f * z));
                }
            }
        }

        return aoes.AsSpan()[..index];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (RequireHint[slot])
            hints.Add(RequireLevitating[slot] ? "Levitate" : "Stay on ground", RequireLevitating[slot] != _levitating[slot]);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Levitate)
            _levitating[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Levitate)
            _levitating[Raid.FindSlot(actor.InstanceID)] = false;
    }
}
