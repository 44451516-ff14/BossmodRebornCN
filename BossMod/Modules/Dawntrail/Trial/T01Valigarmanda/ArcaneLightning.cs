namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class ArcaneLightning(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(50f, 2.5f);
    public readonly List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphere1)
        {
            AOEs.Add(new(rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(8.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArcaneLightning)
        {
            AOEs.Clear();
        }
    }
}
