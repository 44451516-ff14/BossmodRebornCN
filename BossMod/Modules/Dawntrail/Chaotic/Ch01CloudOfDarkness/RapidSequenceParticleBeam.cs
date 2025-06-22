namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class RapidSequenceParticleBeam(BossModule module) : Components.GenericWildCharge(module, 3f, (uint)AID.RapidSequenceParticleBeamAOE, 50f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Source = null; // just in case, if mechanic was not finished properly, reset on next cast start
        if (spell.Action.ID == (uint)AID.RapidSequenceParticleBeam)
        {
            NumCasts = 0;
            Source = caster;
            Activation = Module.CastFinishAt(spell, 0.8f);
            // TODO: not sure how targets are selected, assume it's first healer of each alliance
            BitMask selectedTargetsInAlliance = default;
            foreach (var (i, p) in Raid.WithSlot(false, false, true))
            {
                if (p.Role == Role.Healer && !selectedTargetsInAlliance[i >> 3])
                {
                    PlayerRoles[i] = PlayerRole.TargetNotFirst;
                    selectedTargetsInAlliance.Set(i >> 3);
                }
                else
                {
                    PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RapidSequenceParticleBeamAOE && ++NumCasts >= 12)
            Source = null;
    }
}
