namespace BossMod.RealmReborn.Trial.T09WhorleaterH;

class BodySlamKB(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private Knockback? _knockback;
    private float LeviathanZ;
    private readonly Hydroshot _aoe1 = module.FindComponent<Hydroshot>()!;
    private readonly Dreadstorm _aoe2 = module.FindComponent<Dreadstorm>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _knockback);

    public override void Update()
    {
        var z = Module.PrimaryActor.Position.Z;
        if (LeviathanZ == default)
            LeviathanZ = z;
        if (z != LeviathanZ && z != default)
        {
            LeviathanZ = z;
            _knockback = new(Arena.Center, 25f, WorldState.FutureTime(4.8d), direction: z <= 0f ? 180f.Degrees() : default, kind: Kind.DirForward);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BodySlamNorth or (uint)AID.BodySlamSouth)
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

class BodySlamAOE(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private float LeviathanZ;
    private static readonly AOEShapeRect rect = new(34.5f, 5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void Update()
    {
        if (LeviathanZ == default)
            LeviathanZ = Module.PrimaryActor.Position.Z;
        if (Module.PrimaryActor.Position.Z != LeviathanZ && Module.PrimaryActor.Position.Z != 0)
        {
            LeviathanZ = Module.PrimaryActor.Position.Z;
            _aoe = new(rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, WorldState.FutureTime(2.6d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BodySlamRectAOE)
            _aoe = null;
    }
}
