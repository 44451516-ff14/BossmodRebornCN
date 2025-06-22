namespace BossMod.Dawntrail.Raid.M02NHoneyBLovely;

sealed class Splinter(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(8f);
    private readonly List<AOEInstance> _aoes = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Splinter)
            _aoes.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SplinterVisual1)
            _aoes.Add(new(circle, spell.TargetXZ, default, WorldState.FutureTime(5d)));
    }
}
