namespace BossMod.RealmReborn.Extreme.Ex3Titan;

// burst (bomb explosion) needs to be shown in particular moment (different for different patterns) so that ai can avoid them nicely
sealed class LandslideBurst(BossModule module) : Components.GenericAOEs(module)
{
    public int MaxBombs = 9;
    private readonly List<Actor> _landslides = [];
    private readonly List<Actor> _bursts = []; // TODO: reconsider: we can start showing bombs even before cast starts...
    public int NumActiveBursts => _bursts.Count;

    private readonly AOEShapeRect _shapeLandslide = new(40.25f, 3f);
    private readonly AOEShapeCircle _shapeBurst = new(6.3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var landslideCount = _landslides.Count;
        var burstCount = Math.Min(_bursts.Count, MaxBombs);

        var totalCount = landslideCount + burstCount;
        if (totalCount == 0)
            return [];

        var aoes = new AOEInstance[totalCount];
        var index = 0;

        for (var i = 0; i < landslideCount; ++i)
        {
            var l = _landslides[i];
            aoes[index++] = new(_shapeLandslide, l.CastInfo!.LocXZ, l.CastInfo.Rotation, Module.CastFinishAt(l.CastInfo));
        }
        for (var i = 0; i < burstCount; ++i)
        {
            var b = _bursts[i];
            aoes[index++] = new(_shapeBurst, b.CastInfo!.LocXZ, b.CastInfo.Rotation, Module.CastFinishAt(b.CastInfo));
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LandslideBoss:
            case (uint)AID.LandslideHelper:
            case (uint)AID.LandslideGaoler:
                _landslides.Add(caster);
                break;
            case (uint)AID.Burst:
                _bursts.Add(caster);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LandslideBoss:
            case (uint)AID.LandslideHelper:
            case (uint)AID.LandslideGaoler:
                _landslides.Remove(caster);
                break;
            case (uint)AID.Burst:
                _bursts.Remove(caster);
                break;
        }
    }
}
