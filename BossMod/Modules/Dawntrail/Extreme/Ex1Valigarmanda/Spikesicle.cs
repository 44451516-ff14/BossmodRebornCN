namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class Spikesicle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(5);

    private readonly AOEShape[] _shapes = [new AOEShapeDonut(20f, 25f), new AOEShapeDonut(25f, 30f), new AOEShapeDonut(30f, 35f), new AOEShapeDonut(35f, 40f), new AOEShapeRect(40f, 2.5f)]; // TODO: verify inner radius

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020004u && index is >= 4 and <= 13)
        {
            // 4  - 53 +20
            // 5  - 53 -20
            // 6  - 54 +20
            // 7  - 54 -20
            // 8  - 55 +20
            // 9  - 55 -20
            // 10 - 56 +20
            // 11 - 56 -20
            // 12 - 57 -17
            // 13 - 57 +17
            var shape = _shapes[(index - 4) >> 1];
            var odd = (index & 1) != 0;
            var x = index < 12 ? (odd ? -20f : +20f) : (odd ? +17f : -17f);
            var activationDelay = 11.3d + 0.2d * _aoes.Count;
            var pos = (Module.PrimaryActor.Position + new WDir(x, default)).Quantized();
            _aoes.Add(new(shape, pos, default, WorldState.FutureTime(activationDelay), _aoes.Count == 0 ? Colors.Danger : default, shapeDistance: shape.Distance(pos, default)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SpikesicleAOE1 or (uint)AID.SpikesicleAOE2 or (uint)AID.SpikesicleAOE3 or (uint)AID.SpikesicleAOE4 or (uint)AID.SpikesicleAOE5)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count > 0)
            {
                _aoes.RemoveAt(0);
                if (count > 2)
                {
                    _aoes.Ref(0).Color = Colors.Danger;
                }
            }
        }
    }
}

sealed class SphereShatter(BossModule module) : Components.GenericAOEs(module, (uint)AID.SphereShatter)
{
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeCircle circle = new(13f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.IceBoulder)
        {
            var pos = actor.Position.Quantized();
            _aoes.Add(new(circle, pos, default, WorldState.FutureTime(6.5d), _aoes.Count == 0 ? Colors.Danger : default, shapeDistance: circle.Distance(pos, default)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count > 0)
            {
                _aoes.RemoveAt(0);
                if (count > 2)
                {
                    _aoes.Ref(0).Color = Colors.Danger;
                }
            }
        }
    }
}
