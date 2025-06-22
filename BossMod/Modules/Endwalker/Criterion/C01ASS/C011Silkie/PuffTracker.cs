namespace BossMod.Endwalker.VariantCriterion.C01ASS.C011Silkie;

sealed class PuffTracker(BossModule module) : BossComponent(module)
{
    public List<Actor> BracingPuffs = [];
    public List<Actor> ChillingPuffs = [];
    public List<Actor> FizzlingPuffs = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(BracingPuffs, Colors.Other3, true);
        Arena.Actors(ChillingPuffs, Colors.Other4, true);
        Arena.Actors(FizzlingPuffs, Colors.Other5, true);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.BracingSudsPuff:
                BracingPuffs.Add(actor);
                ChillingPuffs.Remove(actor);
                FizzlingPuffs.Remove(actor);
                break;
            case (uint)SID.ChillingSudsPuff:
                BracingPuffs.Remove(actor);
                ChillingPuffs.Add(actor);
                FizzlingPuffs.Remove(actor);
                break;
            case (uint)SID.FizzlingSudsPuff:
                BracingPuffs.Remove(actor);
                ChillingPuffs.Remove(actor);
                FizzlingPuffs.Add(actor);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.BracingSudsPuff:
                BracingPuffs.Remove(actor);
                break;
            case (uint)SID.ChillingSudsPuff:
                ChillingPuffs.Remove(actor);
                break;
            case (uint)SID.FizzlingSudsPuff:
                FizzlingPuffs.Remove(actor);
                break;
        }
    }
}
