namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class Hydrorythmos(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _dir;
    private DateTime _activation;

    private static readonly AOEShapeRect _shapeFirst = new(25f, 5f, 25f);
    private static readonly AOEShapeRect _shapeRest = new(25f, 2.5f, 25f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = new List<AOEInstance>();
        if (NumCasts > 0)
        {
            var offset = (2.5f + ((NumCasts + 1) >> 1) * 5f) * _dir.ToDirection().OrthoL();
            aoes.Add(new(_shapeRest, Arena.Center + offset, _dir, _activation));
            aoes.Add(new(_shapeRest, Arena.Center - offset, _dir, _activation));
        }
        else if (_activation != default)
        {
            aoes.Add(new(_shapeFirst, Arena.Center, _dir, _activation));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydrorythmosFirst)
        {
            _dir = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HydrorythmosFirst or (uint)AID.HydrorythmosRest)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(2.1d);
        }
    }
}
