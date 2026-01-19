namespace BossMod.DawnTrail.Raid.M10NDaringDevils;

// Towers: 4 intercardinal, require at least 1 soaker.
sealed class HotAerialTowers(BossModule module) : Components.CastTowers(
    module,
    (uint)AID.HotAerial2,
    radius: 6f,
    minSoakers: 1,
    maxSoakers: 1,
    damageType: AIHints.PredictedDamageType.Raidwide);

// Persistent fire puddles left behind after Red Hot jumps to each tower location.
sealed class HotAerialFirePuddles(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _puddles = [];
    private readonly List<AOEInstance> _visible = []; // persistent buffer to return spans over

    private static readonly AOEShapeCircle Shape = new(6f);
    private static readonly float PuddleDelay = 1.5f;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _visible.Clear();
        var now = WorldState.CurrentTime;

        foreach (var p in _puddles)
            if (now >= p.Activation)
                _visible.Add(p);

        return CollectionsMarshal.AsSpan(_visible);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.HotAerial1:
                _puddles.Add(new AOEInstance(
                    Shape,
                    spell.TargetXZ,
                    default,
                    WorldState.CurrentTime.AddSeconds(PuddleDelay),
                    Colors.AOE,
                    true));
                break;

            case (uint)AID.DiversDare:
            case (uint)AID.DiversDare1:
                _puddles.Clear();
                _visible.Clear();
                break;
        }
    }
}