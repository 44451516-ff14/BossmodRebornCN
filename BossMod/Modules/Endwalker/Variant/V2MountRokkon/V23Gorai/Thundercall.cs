namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V23Gorai;

sealed class Thundercall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private int counter;
    private static readonly AOEShapeCircle circleSmall = new(8f), circleBig = new(18f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (counter < 2 && actor.OID == (uint)OID.BallOfLevin)
        {
            _aoes.Add(new(circleBig, actor.Position.Quantized(), default, WorldState.FutureTime(10.8d)));
            ++counter;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {

        if (actor.OID == (uint)OID.BallOfLevin && status.ID == (uint)SID.SmallOrb)
        {
            var activation = WorldState.FutureTime(10d);
            AddAOE(circleSmall, actor.Position);

            var orbs = Module.Enemies((uint)OID.BallOfLevin);
            var count = orbs.Count;
            for (var i = 0; i < count; ++i)
            {
                var orb = orbs[i];
                if (orb != actor)
                {
                    AddAOE(circleBig, orb.Position);
                }
            }
            void AddAOE(AOEShape shape, WPos origin) => _aoes.Add(new(shape, origin.Quantized(), default, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShockSmall or (uint)AID.ShockLarge)
        {
            _aoes.Clear();
        }
    }
}
