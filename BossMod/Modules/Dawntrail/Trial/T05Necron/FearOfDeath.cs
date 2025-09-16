namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class FearOfDeathAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeCircle circle = new(3);
    private bool fearOfDeath;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (fearOfDeath && actor.OID == (uint)OID.IcyHands1)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(8d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FearOfDeath)
        {
            fearOfDeath = true;
            var enemies = Module.Enemies((uint)OID.IcyHands1);
            var count = enemies.Count;
            var act = WorldState.FutureTime(8d);
            for (var i = 0; i < count; ++i)
            {
                var e = enemies[i];
                _aoes.Add(new(circle, e.Position.Quantized(), default, act));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FearOfDeathAOE1)
        {
            _aoes.Clear();
            fearOfDeath = false;
        }
    }
}
