namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class ChaosCondensedParticleBeam(BossModule module) : Components.GenericWildCharge(module, 3f, default, 50f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChaosCondensedParticleBeam)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell, 0.7f);
            foreach (var (i, p) in Raid.WithSlot(true, false, true))
                PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Target : PlayerRole.ShareNotFirst;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ChaosCondensedParticleBeamAOE1 or (uint)AID.ChaosCondensedParticleBeamAOE2)
        {
            ++NumCasts;
            Source = null;
        }
    }
}
