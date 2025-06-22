namespace BossMod.Endwalker.Quest.MSQ.Endwalker;

sealed class Candlewick(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CandlewickPointBlank)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell, 2f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.CandlewickPointBlank => 0,
                (uint)AID.CandlewickDonut => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}
