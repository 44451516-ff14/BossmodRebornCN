namespace BossMod.Stormblood.Trial.T09Seiryu;

class SerpentAscending(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Towers)
            Towers.Add(new(actor.Position, 3f, activation: WorldState.FutureTime(7.8d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SerpentsFang or (uint)AID.SerpentsJaws)
            Towers.Clear();
    }
}
