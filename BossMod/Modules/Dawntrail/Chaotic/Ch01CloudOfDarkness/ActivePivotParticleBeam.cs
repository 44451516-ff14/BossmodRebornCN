namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class ActivePivotParticleBeam(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect _shape = new(40f, 9f, 40f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var rotation = spell.Action.ID switch
        {
            (uint)AID.ActivePivotParticleBeamCW => -22.5f.Degrees(),
            (uint)AID.ActivePivotParticleBeamCCW => 22.5f.Degrees(),
            _ => default
        };
        if (rotation != default)
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, rotation, Module.CastFinishAt(spell, 0.6f), 1.6f, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ActivePivotParticleBeamAOE)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}
