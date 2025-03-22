namespace BossMod.Endwalker.VariantCriterion.V02MR.V023Gorai;

class ImpurePurgation(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());
    public readonly List<AOEInstance> AOEs = new(8);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ImpurePurgationFirst or (uint)AID.ImpurePurgationSecond)
        {
            AOEs.Add(new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            if (AOEs.Count == 8)
                AOEs.SortBy(x => x.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (AOEs.Count != 0 && spell.Action.ID is (uint)AID.ImpurePurgationFirst or (uint)AID.ImpurePurgationSecond)
            AOEs.RemoveAt(0);
    }
}
