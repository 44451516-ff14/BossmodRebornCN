namespace BossMod.Endwalker.VariantCriterion.C03AAI.C032Lala;

sealed class SymmetricSurge(BossModule module) : Components.UniformStackSpread(module, 6f, 0)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SurgeVector)
        {
            AddStack(actor, status.ExpireAt);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NSymmetricSurgeAOE or (uint)AID.SSymmetricSurgeAOE)
        {
            Stacks.Clear();
        }
    }
}
