namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

sealed class SurfaceLaserSpread(BossModule module) : Components.GenericStackSpread(module, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SurfaceLaser)
        {
            Spreads.Add(new(actor, 4f, WorldState.FutureTime(5.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SurfaceLaserLock)
        {
            Spreads.Clear();
        }
    }
}

sealed class SurfaceLaserAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.SurfaceLaserLock)
        {
            _aoes.Add(new(circle, caster.Position.Quantized()));
        }
        else if (id == (uint)AID.SurfaceLaser && ++NumCasts == 10 * _aoes.Count)
        {
            _aoes.Clear();
            NumCasts = 0;
        }
    }
}
