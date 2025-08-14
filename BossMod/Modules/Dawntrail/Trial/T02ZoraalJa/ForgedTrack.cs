namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

sealed class ForgedTrack(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeRect _shape = new(10f, 2.5f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 4 ? 4 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        // there are is only one possible pattern for each direction in normal mode. NE and SW are always XX pattern, NW and SE are always crossed resulting in AOEs in the same lane
        var casterOffset = source.Position - Arena.Center;
        var patternX = source.Rotation.AlmostEqual(Angle.AnglesIntercardinals[0], Angle.DegToRad) || source.Rotation.AlmostEqual(Angle.AnglesIntercardinals[2], Angle.DegToRad);
        var rightDir = source.Rotation.ToDirection().OrthoR();
        var laneOffset = casterOffset.Dot(rightDir);
        var lane1 = laneOffset is < -7 and > -8;
        var lane3 = laneOffset is > 2 and < 3;
        var adjustedLaneOffset = laneOffset + (patternX ? 5 * (lane1 || lane3 ? 1 : -1) : 0);
        _aoes.Add(new(_shape, (Arena.Center + rightDir * adjustedLaneOffset).Quantized(), source.Rotation, WorldState.FutureTime(13.3d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.ForgedTrack)
            _aoes.RemoveAt(0);
    }
}
