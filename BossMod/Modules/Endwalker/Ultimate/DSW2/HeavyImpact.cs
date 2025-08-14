namespace BossMod.Endwalker.Ultimate.DSW2;

// used by two trio mechanics, in p2 and in p5
abstract class HeavyImpact(BossModule module, double activationDelay) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private readonly double _activationDelay = activationDelay;

    private const float _impactRadiusIncrement = 6f;

    public bool Active => _aoe != null;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID == (uint)OID.SerGuerrique)
        {
            _aoe = new(new AOEShapeCircle(_impactRadiusIncrement), actor.Position, default, WorldState.FutureTime(_activationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HeavyImpactHit1 or (uint)AID.HeavyImpactHit2 or (uint)AID.HeavyImpactHit3 or (uint)AID.HeavyImpactHit4 or (uint)AID.HeavyImpactHit5)
        {
            if (++NumCasts < 5)
            {
                var inner = _impactRadiusIncrement * NumCasts;
                _aoe = new(new AOEShapeDonut(inner, inner + _impactRadiusIncrement), caster.Position, default, WorldState.FutureTime(1.9d));
            }
            else
            {
                _aoe = null;
            }
        }
    }
}
