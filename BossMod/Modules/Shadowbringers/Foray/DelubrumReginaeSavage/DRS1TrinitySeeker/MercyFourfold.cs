namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class MercyFourfold(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(4);
    private static readonly AOEShapeCone cone = new(50f, 90f.Degrees());
    private static readonly Angle a180 = 180f.Degrees();
    private BalefulFirestorm? _aoe1 = module.FindComponent<BalefulFirestorm>();
    private IronSplitter? _aoe2 = module.FindComponent<IronSplitter>();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoe1 ??= Module.FindComponent<BalefulFirestorm>();
        if (_aoe1 != null && _aoe1.AOEs.Count != 0)
            return [];
        _aoe2 ??= Module.FindComponent<IronSplitter>();
        if (_aoe2 != null && _aoe2.AOEs.Count != 0)
            return CollectionsMarshal.AsSpan(AOEs)[..1];
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                if (aoes[0].Rotation.AlmostEqual(aoe.Rotation + a180, Angle.DegToRad))
                    aoe.Risky = false;
            }
        }
        return aoes[..max];
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID != (uint)SID.Mercy)
            return;

        var dirOffset = status.Extra switch
        {
            0xF7 => -45f.Degrees(),
            0xF8 => -135f.Degrees(),
            0xF9 => 45f.Degrees(),
            0xFA => 135f.Degrees(),
            _ => default
        };
        if (dirOffset == default)
            return;
        AOEs.Add(new(cone, WPos.ClampToGrid(actor.Position), (Module.PrimaryActor.CastInfo?.Rotation ?? actor.Rotation) + dirOffset, WorldState.FutureTime(13.4d + 1.7d * AOEs.Count)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MercyFourfoldAOE)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}
