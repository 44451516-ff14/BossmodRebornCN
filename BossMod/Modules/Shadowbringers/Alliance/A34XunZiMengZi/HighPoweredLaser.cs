namespace BossMod.Shadowbringers.Alliance.A34XunZiMengZi;

sealed class HighPoweredLaser(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> casters = new(4);
    private static readonly AOEShapeRect rect = new(70f, 2f);
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = casters.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        var isRisky = WorldState.CurrentTime > activation.AddSeconds(-1d); // lasers stop tracking about 1s before activation
        for (var i = 0; i < count; ++i)
        {
            var c = casters[i];
            aoes[i] = new(rect, c.Position.Quantized(), c.Rotation, activation, risky: isRisky);
        }
        return aoes;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Tracking)
        {
            casters.Add(actor);
            activation = WorldState.FutureTime(6.6d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HighPoweredLaser)
        {
            casters.Clear();
            activation = default;
        }
    }
}
