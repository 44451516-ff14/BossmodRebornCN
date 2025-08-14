namespace BossMod.Endwalker.Ultimate.DSW2;

sealed class P7MornAfahsEdge(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MornAfahsEdgeFirst1 or (uint)AID.MornAfahsEdgeFirst2 or (uint)AID.MornAfahsEdgeFirst3)
        {
            Towers.Add(new(spell.LocXZ, 4));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MornAfahsEdgeFirst1 or (uint)AID.MornAfahsEdgeFirst2 or (uint)AID.MornAfahsEdgeFirst3 or (uint)AID.MornAfahsEdgeRest)
        {
            ++NumCasts;
        }
    }
}
