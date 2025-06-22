namespace BossMod.Dawntrail.Trial.T04Zelenia;

class ShockSpread(BossModule module) : Components.GenericStackSpread(module, true)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Shock)
            Spreads.Add(new(actor, 4f, WorldState.FutureTime(8d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShockLock)
            Spreads.Clear();
    }
}

class ShockAOE(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShockLock)
            _aoes.Add(new(circle, WPos.ClampToGrid(caster.Position)));
        else if (spell.Action.ID == (uint)AID.Shock6)
        {
            if (++NumCasts == 2 * _aoes.Count)
            {
                _aoes.Clear();
                NumCasts = 0;
            }
        }
    }
}

class StockBreak(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.StockBreak)
            Stacks.Add(new(actor, 6f, 8, 8, WorldState.FutureTime(7.1d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.StockBreak4)
            Stacks.Clear();
    }
}
