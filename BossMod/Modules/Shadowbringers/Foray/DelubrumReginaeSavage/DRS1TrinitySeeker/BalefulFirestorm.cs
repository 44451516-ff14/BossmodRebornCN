namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

// TODO: consider showing something before clones jump?
sealed class BalefulFirestorm(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(50f, 10f);
    public readonly List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BalefulComet:
                var delay = 7.6d + AOEs.Count;
                AOEs.Add(new(_shape, caster.Position.Quantized(), caster.Rotation, WorldState.FutureTime(delay), actorID: caster.InstanceID));
                break;
            case (uint)AID.BalefulFirestorm:
                var count = AOEs.Count;
                var id = caster.InstanceID;
                for (var i = 0; i < count; ++i)
                {
                    if (AOEs[i].ActorID == id)
                    {
                        AOEs.RemoveAt(i);
                        return;
                    }
                }
                ++NumCasts;
                break;
        }
    }
}
