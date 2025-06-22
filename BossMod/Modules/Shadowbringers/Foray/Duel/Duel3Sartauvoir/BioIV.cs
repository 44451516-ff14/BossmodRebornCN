namespace BossMod.Shadowbringers.Foray.Duel.Duel3Sartauvoir;

sealed class BioIV(BossModule module) : BossComponent(module)
{
    private bool poisoned;
    private static readonly ActionID esuna = ActionID.MakeSpell(ClassShared.AID.Esuna);
    private static readonly ActionID wardensPaean = ActionID.MakeSpell(BRD.AID.WardensPaean);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Poison)
        {
            poisoned = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Poison)
        {
            poisoned = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (poisoned)
            hints.Add($"Cleanse yourself fast (poison).");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (poisoned)
        {
            ActionID action = default;
            if (actor.Role == Role.Healer)
                action = esuna;
            else if (actor.Class == Class.BRD)
                action = wardensPaean;
            if (action != default)
                hints.ActionsToExecute.Push(action, actor, ActionQueue.Priority.High, castTime: action == esuna ? 1f : default);
        }
    }
}
