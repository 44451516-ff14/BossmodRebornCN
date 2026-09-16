namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

sealed class Hellfire(BossModule module) : BossComponent(module)
{
    private DateTime _expectedRaidwide = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.AddPredictedDamage(Raid.WithSlot(false, true, true).Mask(), _expectedRaidwide);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hellfire)
            _expectedRaidwide = Module.CastFinishAt(spell);
    }
}
