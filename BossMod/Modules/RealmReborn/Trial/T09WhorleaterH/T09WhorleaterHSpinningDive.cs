namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class SpinningDive(BossModule module) : Components.GenericAOEs(module) //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
{
    private AOEInstance? _aoe;
    public static readonly AOEShapeRect Rect = new(50.5f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SpinningDiveHelper)
            _aoe = new(Rect, actor.Position, actor.Rotation, WorldState.FutureTime(0.6d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SpinningDiveSnapshot)
            _aoe = null;
    }
}

class SpinningDiveKB(BossModule module) : Components.GenericKnockback(module, stopAtWall: true) //TODO: Find out how to detect spinning dives earlier eg. the water column telegraph
{
    private Knockback? _knockback;
    private readonly Hydroshot _aoe1 = module.FindComponent<Hydroshot>()!;
    private readonly Dreadstorm _aoe2 = module.FindComponent<Dreadstorm>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SpinningDiveHelper)
            _knockback = new(actor.Position, 10, WorldState.FutureTime(1.4d), SpinningDive.Rect, actor.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SpinningDiveEffect)
            _knockback = null;
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes1 = _aoe1.ActiveAOEs(slot, actor);
        var len1 = aoes1.Length;
        for (var i = 0; i < len1; ++i)
        {
            ref readonly var aoe = ref aoes1[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        var aoes2 = _aoe2.ActiveAOEs(slot, actor);
        var len2 = aoes1.Length;
        for (var i = 0; i < len2; ++i)
        {
            ref readonly var aoe = ref aoes2[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }
}
