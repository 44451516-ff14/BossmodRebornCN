namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

sealed class Pyroclysm(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Pyroclysm or (uint)AID.Pyroplexy)
        {
            var count = Towers.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (Towers[i].Position.AlmostEqual(pos, 1f))
                {
                    Towers.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.TowerVisual1)
            Towers.Add(new(WPos.ClampToGrid(actor.Position), 4f, 1, 1, activation: WorldState.FutureTime(8.7d)));
    }
}
