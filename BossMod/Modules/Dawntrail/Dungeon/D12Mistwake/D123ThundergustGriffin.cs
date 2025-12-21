namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D123ThundergustGriffin;

public enum OID : uint
{
    ThundergustGriffin = 0x4A65, // R6.9
    BallLightning = 0x4A66, // R2.0
    ShockStorm = 0x4A67, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 45307, // ThundergustGriffin->player, no cast, single-target

    Thunderspark = 45291, // ThundergustGriffin->self, 5.0s cast, range 60 circle, raidwide
    Teleport1 = 46852, // ThundergustGriffin->location, no cast, single-target
    Teleport2 = 45294, // ThundergustGriffin->location, no cast, single-target

    HighVolts = 45292, // ThundergustGriffin->self, 4.0s cast, single-target
    LightningBoltAOE = 45293, // Helper->self, 5.0s cast, range 5 circle
    LightningBoltSpread = 46856, // Helper->player, 5.0s cast, range 5 circle

    ThunderingRoar = 45295, // ThundergustGriffin->self, 5.0s cast, single-target
    ThunderboltVisual1 = 46944, // Helper->self, 4.3s cast, range 16 width 3 rect
    ThunderboltVisual2 = 46943, // Helper->self, 4.3s cast, range 20 width 3 rect
    ThunderboltVisual3 = 46945, // Helper->self, 4.3s cast, range 16 width 3 rect
    Thunderbolt1 = 45296, // Helper->self, 5.5s cast, range 92 width 6 rect
    Thunderbolt2 = 45297, // Helper->self, 5.5s cast, range 92 width 6 rect
    Thunderbolt3 = 45298, // Helper->self, 5.5s cast, range 92 width 6 rect
    GoldenTalons = 45305, // ThundergustGriffin->player, 5.0s cast, single-target, tankbuster
    FulgurousFall = 45301, // ThundergustGriffin->location, 6.0s cast, single-target
    Rush = 45302, // Helper->self, 6.0s cast, range 40 width 10 rect
    ElectrifyingFlight = 47022, // Helper->self, 6.2s cast, range 50 width 40 rect, knockback 12 left/right
    ElectrifyingFlightTeleport = 45300, // ShockStorm->location, 0.5s cast, single-target
    ElectrogeneticForce = 45304, // Helper->self, 9.6s cast, range 40 width 18 rect
    StormSurge = 45299 // ThundergustGriffin->self, 3.0s cast, range 50 width 10 rect
}

[SkipLocalsInit]
sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Thunderspark && Arena.Bounds.Radius > 20f)
        {
            _aoe = [new(new AOEShapeDonut(20f, 30f), D123ThundergustGriffin.ArenaCenter, default, Module.CastFinishAt(spell, 0.7d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x17 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsCustom([new Polygon(Arena.Center, 20f, 64)]);
            _aoe = [];
        }
    }
}

[SkipLocalsInit]
sealed class Thunderspark(BossModule module) : Components.RaidwideCast(module, (uint)AID.Thunderspark);

[SkipLocalsInit]
sealed class LightningBoltSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LightningBoltSpread, 5f);

[SkipLocalsInit]
sealed class LightningBoltAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBoltAOE, 5f);

[SkipLocalsInit]
sealed class Thunderbolt(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Thunderbolt1, (uint)AID.Thunderbolt2, (uint)AID.Thunderbolt3], new AOEShapeRect(92f, 3f));

[SkipLocalsInit]
sealed class GoldenTalons(BossModule module) : Components.SingleTargetCast(module, (uint)AID.GoldenTalons);

[SkipLocalsInit]
sealed class Rush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rush, new AOEShapeRect(40f, 5f));

[SkipLocalsInit]
sealed class ElectrogeneticForce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectrogeneticForce, new AOEShapeRect(40f, 9f))
{
    private readonly Rush aoe = module.FindComponent<Rush>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.Count != 0 && aoe.Casters.Count == 0)
        {
            return CollectionsMarshal.AsSpan(Casters);
        }
        return [];
    }
}

[SkipLocalsInit]
sealed class StormSurge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeRect rect = new(50f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ShockStorm)
        {
            var pos = actor.Position.Quantized();
            var rot = actor.Rotation;
            _aoe = [new(rect, pos, rot, WorldState.FutureTime(6d), shapeDistance: rect.Distance(pos, rot))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.StormSurge)
        {
            _aoe = [];
        }
    }
}

[SkipLocalsInit]
sealed class ElectrifyingFlight(BossModule module) : Components.GenericKnockback(module)
{
    private readonly List<Knockback> _kbs = new(2);

    private static readonly AOEShapeRect rect = new(30f, 30f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_kbs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrifyingFlight)
        {
            var act = Module.CastFinishAt(spell);
            var pos = Arena.Center;
            var rot = spell.Rotation;
            _kbs.Add(new(pos, 12f, act, rect, rot + 90f.Degrees(), Kind.DirForward));
            _kbs.Add(new(pos, 12f, act, rect, rot - 90f.Degrees(), Kind.DirForward));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectrifyingFlight)
        {
            _kbs.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs.Count != 0)
        {
            // circle intentionally slightly smaller to prevent sus knockback
            ref readonly var kb = ref _kbs.Ref(0);
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var dir = kb.Direction;
                var isAlongXAxis = dir.AlmostEqual(90f.Degrees(), Angle.DegToRad) || dir.AlmostEqual(-90f.Degrees(), Angle.DegToRad);
                hints.AddForbiddenZone(isAlongXAxis ? new SDKnockbackInCircleLeftRightAlongXAxis(Arena.Center, 12f, 19f) : new SDKnockbackInCircleLeftRightAlongZAxis(Arena.Center, 12f, 19f), act);
            }
        }
    }
}

[SkipLocalsInit]
sealed class D123ThundergustGriffinStates : StateMachineBuilder
{
    public D123ThundergustGriffinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Thunderspark>()
            .ActivateOnEnter<LightningBoltAOE>()
            .ActivateOnEnter<LightningBoltSpread>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<GoldenTalons>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<StormSurge>()
            .ActivateOnEnter<ElectrifyingFlight>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(D123ThundergustGriffinStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.ThundergustGriffin,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1064u,
NameID = 14288u,
SortOrder = 3,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D123ThundergustGriffin : BossModule
{
    public D123ThundergustGriffin(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D123ThundergustGriffin(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }
    public static readonly WPos ArenaCenter = new(281f, -620f);

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(281f, -620f), 29.5f, 64)], [new Rectangle(new(281f, -590f), 8f, 1.25f)]);
        return (arena.Center, arena);
    }
}
