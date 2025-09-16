namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

sealed class FloodConcentric(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 16f), new AOEShapeDonut(16f, 24f), new AOEShapeDonut(24f, 32f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Flood1)
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
                (uint)AID.Flood1 => 0,
                (uint)AID.Flood2 => 1,
                (uint)AID.Flood3 => 2,
                (uint)AID.Flood4 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class FloodProximity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flood, 20f);
