namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

sealed class FireSpin(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly AOEShapeCone cone = new(40f, 30f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FireSpinCCW:
            case (uint)AID.InfernalSpinCCW:
                AddSequence(45f.Degrees());
                break;
            case (uint)AID.FireSpinCW:
            case (uint)AID.InfernalSpinCW:
                AddSequence(-45f.Degrees());
                break;
        }
        void AddSequence(Angle increment) => Sequences.Add(new(cone, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell, 0.5d), 1f, 8));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FireSpinFirst or (uint)AID.FireSpinRest or (uint)AID.InfernalSpinFirst or (uint)AID.InfernalSpinRest)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}
