namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

sealed class LevinblossomLance(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect rect = new(30f, 3.5f, 30f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = spell.Action.ID switch
        {
            (uint)AID.LevinblossomLanceCW => -28f.Degrees(),
            (uint)AID.LevinblossomLanceCCW => 28f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(rect, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell, 0.8d), 1d, 5, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.LevinblossomLanceFirst or (uint)AID.LevinblossomLanceRest)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}
