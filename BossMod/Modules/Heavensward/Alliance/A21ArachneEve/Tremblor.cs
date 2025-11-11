namespace BossMod.Heavensward.Alliance.A21ArachneEve;

[SkipLocalsInit]
sealed class Tremblor(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10.5f), new AOEShapeDonut(10.5f, 20.5f), new AOEShapeDonut(20.5f, 30.5f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Tremblor1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.Tremblor1 => 0,
                (uint)AID.Tremblor2 => 1,
                (uint)AID.Tremblor3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}
