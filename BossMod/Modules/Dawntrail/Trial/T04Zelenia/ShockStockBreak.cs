namespace BossMod.Dawntrail.Trial.T04Zelenia;

sealed class ShockSpread(BossModule module) : Components.GenericStackSpread(module, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Shock)
        {
            Spreads.Add(new(actor, 4f, WorldState.FutureTime(8d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShockLock)
        {
            Spreads.Clear();
        }
    }
}

sealed class ShockAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.ShockLock)
        {
            _aoes.Add(new(circle, caster.Position.Quantized()));
        }
        else if (id == (uint)AID.Shock6 && ++NumCasts == 2 * _aoes.Count)
        {
            _aoes.Clear();
            NumCasts = 0;
        }
    }
}

sealed class StockBreak(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
        {
            Stacks.Add(new(actor, 6f, 8, 8, WorldState.FutureTime(7.1d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StockBreak4)
        {
            Stacks.Clear();
        }
    }
}
