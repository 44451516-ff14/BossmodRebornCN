namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

class ImpurePurgationBait(BossModule module) : Components.BaitAwayEveryone(module, module.PrimaryActor, new AOEShapeCone(60f, 22.5f.Degrees()))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NImpurePurgationBait or (uint)AID.SImpurePurgationBait)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

abstract class ImpurePurgationAOE(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(60f, 22.5f.Degrees()));
class NImpurePurgationAOE(BossModule module) : ImpurePurgationAOE(module, (uint)AID.NImpurePurgationAOE);
class SImpurePurgationAOE(BossModule module) : ImpurePurgationAOE(module, (uint)AID.SImpurePurgationAOE);
