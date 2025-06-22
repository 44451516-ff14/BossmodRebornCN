namespace BossMod.Dawntrail.Raid.M03NBruteBomber;

sealed class ExplosiveRainConcentric(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 16f), new AOEShapeDonut(16f, 24f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExplosiveRain1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.ExplosiveRain1 => 0,
                (uint)AID.ExplosiveRain2 => 1,
                (uint)AID.ExplosiveRain3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class ExplosiveRainCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosiveRain4, 6f);
