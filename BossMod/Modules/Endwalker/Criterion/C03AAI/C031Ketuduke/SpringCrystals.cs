namespace BossMod.Endwalker.VariantCriterion.C03AAI.C031Ketuduke;

abstract class SpringCrystalsRect(BossModule module, bool moveCasters, bool risky, double delay) : Components.GenericAOEs(module)
{
    public List<WPos> SafeZoneCenters = InitialSafeZoneCenters(module.Center);
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(38f, 5f, 38f);

    private static List<WPos> InitialSafeZoneCenters(WPos origin)
    {
        List<WPos> res = [];
        for (var z = -15; z <= 15; z += 10)
            for (var x = -15; x <= 15; x += 10)
                res.Add(origin + new WDir(x, z));
        return res;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NSpringCrystalRect or (uint)OID.SSpringCrystalRect)
        {
            var pos = actor.Position;
            if (moveCasters)
            {
                // crystals are moved once or twice, but never outside arena bounds
                // orthogonal movement always happens, movement along direction happens only for half of them - but it doesn't actually affect aoe, so we can ignore it
                var posX = pos.X;
                var posZ = pos.Z;
                var center = Arena.Center;
                posX += posX < center.X ? 20f : -20f;
                posZ += posZ < center.Z ? 20f : -20f;
                pos = new(posX, posZ);
            }
            _aoes.Add(new(_shape, pos, actor.Rotation, WorldState.FutureTime(delay), risky: risky));
            SafeZoneCenters.RemoveAll(c => _shape.Check(c, pos, actor.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NSaturateRect or (uint)AID.SSaturateRect)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
class SpringCrystalsRectMove(BossModule module) : SpringCrystalsRect(module, true, false, 40.3d);
class SpringCrystalsRectStay(BossModule module) : SpringCrystalsRect(module, false, true, 24.2d);

class SpringCrystalsSphere(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private bool _active;

    private static readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? CollectionsMarshal.AsSpan(_aoes) : [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NSpringCrystalSphere or (uint)OID.SSpringCrystalSphere)
        {
            _aoes.Add(new(_shape, actor.Position.Quantized(), default, WorldState.FutureTime(23.9d)));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (actor.OID is (uint)OID.NSpringCrystalSphere or (uint)OID.SSpringCrystalSphere && status.ID == (uint)SID.Bubble)
        {
            _active = true;
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(actor.Position, 1f));
            if (index >= 0)
            {
                ref var aoe = ref _aoes.Ref(index);
                aoe.Origin += new WDir(aoe.Origin.X < Arena.Center.X ? 20f : -20f, default);
            }
            else
            {
                ReportError($"Failed to find aoe for {actor}");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NSaturateSphere or (uint)AID.SSaturateSphere)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}
