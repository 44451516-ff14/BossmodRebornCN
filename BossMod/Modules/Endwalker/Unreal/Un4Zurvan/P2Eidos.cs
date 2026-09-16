namespace BossMod.Endwalker.Unreal.Un4Zurvan;

// this is used purely for tracking phase transitions
sealed class P2Eidos(BossModule module) : BossComponent(module)
{
    public int PhaseIndex;
    public override bool KeepOnPhaseChange => true;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var nextPhase = spell.Action.ID switch
        {
            (uint)AID.Eidos1 => 1,
            (uint)AID.Eidos2 => 2,
            (uint)AID.Eidos3 => 3,
            _ => 0
        };
        if (nextPhase > PhaseIndex)
            PhaseIndex = nextPhase;
    }
}
