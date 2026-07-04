namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Lightwave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> waves = [with(4)];
    private static readonly AOEShapeRect rect = new(16f, 8f, 12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = waves.Count;
        if (waves.Count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var w = waves[i];
            aoes[i] = new(rect, w.Position, w.Rotation, WorldState.FutureTime(1.1d));
        }
        return aoes;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RayOfLight && !waves.Contains(caster))
            waves.Add(caster);
    }
}
