namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class ChillingCataclysm(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCross _shape = new(40f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ArcaneSphere2)
        {
            var activation = WorldState.FutureTime(7.1d);
            var pos = actor.Position.Quantized();
            AOEs.Add(new(_shape, pos, Angle.AnglesCardinals[1], activation));
            AOEs.Add(new(_shape, pos, Angle.AnglesIntercardinals[1], activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChillingCataclysm)
        {
            AOEs.Clear();
        }
    }
}
