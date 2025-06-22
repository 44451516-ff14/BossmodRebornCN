namespace BossMod.Components;

// generic temporary misdirection component
public class TemporaryMisdirection(BossModule module, uint aid, string hint = "Applies temporary misdirection") : CastHint(module, aid, hint)
{
    public BitMask Mask;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
            Mask.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is 1422u or 2936u or 3694u or 3909u)
            Mask[Raid.FindSlot(actor.InstanceID)] = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Mask[slot] != default)
            hints.AddSpecialMode(AIHints.SpecialMode.Misdirection, default);
    }
}
