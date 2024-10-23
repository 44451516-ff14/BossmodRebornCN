namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

class ArcaneLightning(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(50, 2.5f);
    public readonly List<AOEInstance> AOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ArcaneSphere1)
            AOEs.Add(new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(8.6f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArcaneLightning)
            AOEs.Clear();
    }
}
