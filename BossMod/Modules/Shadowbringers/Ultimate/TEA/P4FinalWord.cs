namespace BossMod.Shadowbringers.Ultimate.TEA;

sealed class P4FinalWordDebuffs(BossModule module) : P4ForcedMarchDebuffs(module)
{
    protected override WDir SafeSpotDirection(int slot) => Debuffs[slot] switch
    {
        Debuff.LightBeacon => new(default, -15f), // N
        Debuff.DarkBeacon => new(default, 13f), // S
        _ => new(default, 10f), // slightly N of dark beacon
    };

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.FinalWordContactProhibition:
                AssignDebuff(actor, Debuff.LightFollow);
                break;
            case (uint)SID.FinalWordContactRegulation:
                AssignDebuff(actor, Debuff.LightBeacon);
                LightBeacon = actor;
                break;
            case (uint)SID.FinalWordEscapeProhibition:
                AssignDebuff(actor, Debuff.DarkFollow);
                break;
            case (uint)SID.FinalWordEscapeDetection:
                AssignDebuff(actor, Debuff.DarkBeacon);
                DarkBeacon = actor;
                break;
            case (uint)SID.ContactProhibitionOrdained:
            case (uint)SID.ContactRegulationOrdained:
            case (uint)SID.EscapeProhibitionOrdained:
            case (uint)SID.EscapeDetectionOrdained:
                Done = true;
                break;
        }
    }

    private void AssignDebuff(Actor actor, Debuff debuff)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot >= 0)
            Debuffs[slot] = debuff;
    }
}

sealed class P4FinalWordStillnessMotion(BossModule module) : Components.StayMove(module)
{
    private Requirement _first;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_first != Requirement.None)
            return; // we've already seen first cast, so we no longer care - we assume stillness is always followed by motion and vice versa

        var req = spell.Action.ID switch
        {
            (uint)AID.OrdainedMotion => Requirement.Move,
            (uint)AID.OrdainedStillness => Requirement.Stay,
            _ => Requirement.None
        };
        if (req != Requirement.None)
        {
            _first = req;
            Array.Fill(PlayerStates, new(req, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.OrdainedMotionSuccess or (uint)AID.OrdainedMotionFail or (uint)AID.OrdainedStillnessSuccess or (uint)AID.OrdainedStillnessFail)
        {
            var slot = Raid.FindSlot(spell.MainTargetID);
            if (slot >= 0)
            {
                PlayerStates[slot] = PlayerStates[slot].Requirement != _first ? default : new(_first == Requirement.Move ? Requirement.Stay : Requirement.Move, WorldState.FutureTime(11d));
            }
        }
    }
}
