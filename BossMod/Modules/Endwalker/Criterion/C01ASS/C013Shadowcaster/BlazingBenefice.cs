namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

abstract class BlazingBenifice(BossModule module, uint aid, uint oid) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rect = new(100f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];

        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var startTime = WorldState.CurrentTime.AddSeconds(-5d);
        var deadline = aoes[0].Activation.AddSeconds(1d);
        var index = 0;

        while (index < count && aoes[index].Activation >= startTime && aoes[index].Activation < deadline)
            ++index;

        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == oid)
            _aoes.Add(new(rect, WPos.ClampToGrid(actor.Position), actor.Rotation, WorldState.FutureTime(21.7d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == aid)
        {
            _aoes.RemoveAt(0);
            ++NumCasts;
        }
    }
}

sealed class NBlazingBenifice(BossModule module) : BlazingBenifice(module, (uint)AID.NBlazingBenifice, (uint)OID.NArcaneFont);
sealed class SBlazingBenifice(BossModule module) : BlazingBenifice(module, (uint)AID.SBlazingBenifice, (uint)OID.SArcaneFont);
