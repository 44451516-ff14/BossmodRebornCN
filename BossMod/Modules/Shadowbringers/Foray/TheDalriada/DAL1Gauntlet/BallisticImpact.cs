namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class BallisticImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private static readonly AOEShapeRect rect = new(12f, 12f, 12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BallisticImpact)
            _aoes.Add(new(rect, actor.Position, default, WorldState.FutureTime(12d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.BallisticImpact)
            _aoes.RemoveAt(0);
    }
}
