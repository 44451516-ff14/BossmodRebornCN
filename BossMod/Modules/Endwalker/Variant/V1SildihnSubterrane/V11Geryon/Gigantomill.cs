namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V11Geryon;

sealed class Gigantomill(BossModule module) : Components.GenericRotatingAOE(module)
{
    private readonly AOEShapeCross cross = new(72f, 5f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var increment = spell.Action.ID switch
        {
            (uint)AID.GigantomillFirstCW => -22.5f.Degrees(),
            (uint)AID.GigantomillFirstCCW => 22.5f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            Sequences.Add(new(cross, spell.LocXZ, spell.Rotation, increment, Module.CastFinishAt(spell), 1.7d, 5, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GigantomillFirstCW or (uint)AID.GigantomillFirstCCW or (uint)AID.GigantomillRest)
        {
            AdvanceSequence(0, WorldState.CurrentTime);
        }
    }
}
