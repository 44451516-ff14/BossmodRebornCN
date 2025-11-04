namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

[SkipLocalsInit]
sealed class BoundsOfSinSmallAOE : Components.SimpleAOEs
{
    public BoundsOfSinSmallAOE(BossModule module) : base(module, (uint)AID.BoundsOfSin, 3f)
    {
        MaxDangerColor = 6;
    }
}

[SkipLocalsInit]
sealed class BoundsOfSinPull(BossModule module) : Components.CastCounter(module, (uint)AID.BoundsOfSinPull);

[SkipLocalsInit]
sealed class BoundsOfSinEnd(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeCircle circle = new(8f);
    private readonly AOEShapeDonut donut = new(8f, 30f);
    public readonly List<Polygon> Pillars = new(12);
    private const float radius = 2.57745f; // adjusted for hitbox radius

    private readonly Polygon[] pillarPolygons =
    [
        new(new(-600f, -306.99991f), radius, 64), // ENVC 0x00
        new(new(-596.50012f, -306.06201f), radius, 64, -30f.Degrees()), // ENVC 0x01
        new(new(-593.93793f, -303.49991f), radius, 64, -60f.Degrees()), // ENVC 0x02
        new(new(-592.99988f, -299.99991f), radius, 64, -89.98f.Degrees()), // ENVC 0x03
        new(new(-593.93781f, -296.49991f), radius, 64, -120f.Degrees()), // ENVC 0x04
        new(new(-596.50012f, -293.93771f), radius, 64, -150f.Degrees()), // ENVC 0x05
        new(new(-600f, -293f), radius, 64), // ENVC 0x06
        new(new(-603.5f, -293.93781f), radius, 64, 150f.Degrees()), // ENVC 0x07
        new(new(-606.06219f, -296.5f), radius, 64, 120f.Degrees()), //  ENVC 0x08
        new(new(-607.00012f, -300f), radius, 64, 89.98f.Degrees()), //  ENVC 0x09
        new(new(-606.06219f, -303.5f), radius, 64, 60f.Degrees()), // ENVC 0x0A
        new(new(-603.5f, -306.06219f), radius, 64, 30f.Degrees()), // ENVC 0x0B

        new(new(-600.00018f, -307f), radius, 64), // ENVC 0x0C
        new(new(-596.50012f, -306.06201f), radius, 64, 150f.Degrees()), // ENVC 0x0D
        new(new(-593.93799f, -303.49991f), radius, 64, 120f.Degrees()), // ENVC 0x0E
        new(new(-592.99988f, -299.99991f), radius, 64, 89.98f.Degrees()), // ENVC 0x0F
        new(new(-593.93793f, -296.49991f), radius, 64, 60f.Degrees()), // ENVC 0x10
        new(new(-596.50018f, -293.93771f), radius, 64, 30f.Degrees()), // ENVC 0x11
        new(new(-600.00018f, -293f), radius, 64), // ENVC 0x12
        new(new(-603.50018f, -293.9379f), radius, 64, -30f.Degrees()), // ENVC 0x13
        new(new(-606.06219f, -296.5f), radius, 64, -60f.Degrees()), //  ENVC 0x14
        new(new(-607.00012f, -300f), radius, 64, -89.98f.Degrees()), //  ENVC 0x15
        new(new(-606.06219f, -303.5f), radius, 64, -120f.Degrees()), // ENVC 0x16
        new(new(-603.5f, -306.06219f), radius, 64, -150f.Degrees()) // ENVC 0x17
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoe.Length == 0 && spell.Action.ID == (uint)AID.BoundsOfSin)
        {
            var loc = Arena.Center.Quantized();
            var isCircle = (loc - caster.Position).Dot(caster.Rotation.ToDirection()) > 0f;
            AOEShape shape = isCircle ? circle : donut;
            _aoe = [new(shape, loc, default, WorldState.FutureTime(6d), shapeDistance: shape.Distance(loc, default))]; // true activation is 10.8, but slightly lower values seem to improve the pathfinding here
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BoundsOfSinEndCircle or (uint)AID.BoundsOfSinEndDonut)
        {
            _aoe = [];
            ++NumCasts;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x00 and <= 0x17)
        {
            if (state == 0x00020001u)
            {
                Pillars.Add(pillarPolygons[index]);
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(Q1FinalVerse.ArenaCenter, 20f, 15f)], [.. Pillars]);
            }
            else if (index is 0x00 or 0x0C && state == 0x00080004u)
            {
                Arena.Bounds = new ArenaBoundsCustom([new Rectangle(Q1FinalVerse.ArenaCenter, 20f, 15f)], AdjustForHitboxOutwards: true);
                Pillars.Clear();
            }
        }
    }
}
