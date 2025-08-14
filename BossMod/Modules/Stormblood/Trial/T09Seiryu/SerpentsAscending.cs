namespace BossMod.Stormblood.Trial.T09Seiryu;

sealed class SerpentAscending(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower)
        {
            Towers.Add(new(actor.Position.Quantized(), 3f, activation: WorldState.FutureTime(7.8d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SerpentsFang or (uint)AID.SerpentsJaws)
        {
            Towers.Clear();
        }
    }
}
