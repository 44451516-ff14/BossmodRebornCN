namespace BossMod.Endwalker.Ultimate.DSW2;

// TODO: assignments?
sealed class P7AkhMornsEdge(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornsEdgeAOEFirstNormal1 or (uint)AID.AkhMornsEdgeAOEFirstNormal2 or (uint)AID.AkhMornsEdgeAOEFirstTanks)
        {
            Towers.Add(new(spell.LocXZ, 4f, 1, 6));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornsEdgeAOEFirstTanks or (uint)AID.AkhMornsEdgeAOERestTanks)
        {
            ++NumCasts;
        }
    }
}
