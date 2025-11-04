namespace BossMod.Stormblood.Ultimate.UWU;

sealed class P3Geocrush1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Geocrush1, 18f);

// TODO: add prediction after PATE xxx - need non-interpolated actor rotation for that...
sealed class P3Geocrush2(BossModule module) : Components.GenericAOEs(module, (uint)AID.Geocrush2)
{
    private Actor? _caster;

    //private static WDir[] _possibleOffsets = { new(14, 0), new(0, 14), new(-14, 0), new(0, -14) };
    private static readonly AOEShapeCircle _shapeCrush = new(24f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster != null)
            return new AOEInstance[1] { new(_shapeCrush, _caster.Position, _caster.CastInfo!.Rotation, Module.CastFinishAt(_caster.CastInfo)) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _caster = caster;
            Arena.Bounds = new ArenaBoundsCircle(NumCasts == 0 ? 16f : 12f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _caster = null;
        }
    }

    //public override void Update()
    //{
    //    if (_caster == null || _caster.CastInfo != null)
    //        return;

    //    // TODO: find a way to get non-interpolated actor rotation...
    //    if (_prevPredictedAngle == _caster.Rotation && ++_numFramesStill > 2)
    //    {
    //        var dir = _caster.Rotation.ToDirection();
    //        _predictedPos = Arena.Center + _possibleOffsets.MinBy(o =>
    //        {
    //            var off = Arena.Center + o - _caster.Position;
    //            var proj = off.Dot(dir);
    //            return proj > 0 ? (off - proj * dir).LengthSq() : float.MaxValue;
    //        });
    //    }
    //    else
    //    {
    //        _prevPredictedAngle = _caster.Rotation; // retry on next frame
    //        _numFramesStill = 0;
    //    }
    //}
}
