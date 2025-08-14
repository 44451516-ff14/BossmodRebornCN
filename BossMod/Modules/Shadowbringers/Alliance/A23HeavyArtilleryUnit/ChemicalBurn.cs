namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

sealed class ChemicalBurn(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.Tower)
        {
            Towers.Add(new(actor.Position.Quantized(), 3, 3, activation: WorldState.FutureTime(20d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ChemicalBurn)
        {
            Towers.Clear();
        }
    }
}
