namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL3Adrammelech;

sealed class StoneIV(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.StoneIV1 or (uint)AID.StoneIV4)
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
                (uint)AID.StoneIV1 or (uint)AID.StoneIV4 => 0,
                (uint)AID.StoneIV2 or (uint)AID.StoneIV5 => 1,
                (uint)AID.StoneIV3 or (uint)AID.StoneIV6 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}
