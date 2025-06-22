namespace BossMod.Heavensward.Dungeon.D11Antitower.D152DotoliCiloc;

public enum OID : uint
{
    Boss = 0x179F, // R1.98
    ArenaVoidzone = 0x1EA187, // R2.0
    Whirlwind = 0x17A0 // R1.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    OnLow = 6606, // Boss->self, 4.0s cast, range 9+R 120-degree cone
    OnHigh = 6607, // Boss->self, 3.0s cast, range 50+R circle, knockback 30, away from source
    DarkWings = 32556, // Boss->player, no cast, range 6 circle, spread
    Swiftfeather = 6609, // Boss->self, 3.0s cast, single-target, applies Haste to boss
    Stormcoming = 32557, // Boss->location, 4.0s cast, range 6 circle
    TerribleFlurry = 6610 // Whirlwind->self, no cast, range 6 circle
}

public enum IconID : uint
{
    Spreadmarker = 139 // player
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCustom donut = new(D152DotoliCiloc.StartingBoundsP, D152DotoliCiloc.DefaultBoundsP);
    private AOEInstance? _aoe;
    private bool begin;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002 && actor.OID == (uint)OID.ArenaVoidzone)
        {
            Arena.Bounds = D152DotoliCiloc.DefaultBounds;
            _aoe = null;
            begin = true;
        }
    }

    public override void Update()
    {
        if (!begin && _aoe == null)
            _aoe = new(donut, Arena.Center, default, WorldState.FutureTime(4d));
    }
}

class DarkWings(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.DarkWings, 6f, 5.1f);
class Whirlwind(BossModule module) : Components.Voidzone(module, 6f, GetWhirlwinds)
{
    private static List<Actor> GetWhirlwinds(BossModule module) => module.Enemies((uint)OID.Whirlwind);
}
class Stormcoming(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Stormcoming, 6f);
class OnLow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OnLow, new AOEShapeCone(10.98f, 60f.Degrees()));

class OnLowHaste(BossModule module) : Components.Cleave(module, (uint)AID.Swiftfeather, new AOEShapeCone(10.98f, 60f.Degrees()))
{
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Swiftfeather)
            active = true;
        else if (spell.Action.ID == (uint)AID.OnLow)
            active = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (active)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class OnHigh(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback? _source;
    private static readonly SafeWall[] safeWallsW = [new(new(227.487f, 16.825f), new(226.567f, 13.39f)), new(new(226.567f, 13.39f), new(227.392f, 10.301f))];
    private static readonly SafeWall[] safeWallsN = GenerateRotatedSafeWalls(ref safeWallsW, 90f);
    private static readonly SafeWall[] safeWallsE = GenerateRotatedSafeWalls(ref safeWallsW, 180f);
    private static readonly SafeWall[] safeWallsS = GenerateRotatedSafeWalls(ref safeWallsW, 270f);
    private static readonly SafeWall[] allSafeWalls = [.. safeWallsW, .. safeWallsN, .. safeWallsE, .. safeWallsS];

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var whirlwinds = Module.Enemies((uint)OID.Whirlwind);
        var count = whirlwinds.Count;
        for (var i = 0; i < count; ++i)
        {
            if (pos.InCircle(whirlwinds[i].Position, 6f))
                return true;
        }
        return !Arena.InBounds(pos);
    }

    private static SafeWall[] GenerateRotatedSafeWalls(ref SafeWall[] baseWalls, float angle)
    {
        var len = baseWalls.Length;
        var rotatedWalls = new SafeWall[len];
        for (var i = 0; i < len; ++i)
        {
            ref var bw = ref baseWalls[i];
            var rotatedVertex1 = GenerateRotatedVertice(bw.Vertex1, angle);
            var rotatedVertex2 = GenerateRotatedVertice(bw.Vertex2, angle);
            rotatedWalls[i] = new(rotatedVertex1, rotatedVertex2);
        }
        return rotatedWalls;
    }

    private static WPos GenerateRotatedVertice(WPos vertex, float rotationAngle) => WPos.RotateAroundOrigin(rotationAngle, D152DotoliCiloc.ArenaCenter, vertex);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => Utils.ZeroOrOne(ref _source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
            _source = new(spell.LocXZ, 30f, Module.CastFinishAt(spell), SafeWalls: allSafeWalls);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
            _source = null;
    }
}

class OnHighHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<ConeHA> cones = new(4);
    private AOEInstance? _aoe;
    private const string RiskHint = "Use safewalls for knockback!";
    private static readonly Angle angle = 11.25f.Degrees();
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
        {
            activation = Module.CastFinishAt(spell);
            GenerateHints();
        }
    }

    private void GenerateHints()
    {
        for (var i = 0; i < 4; ++i)
        {
            var deg = (i * 90f).Degrees();
            var enemyInCone = false;
            var whirlwinds = Module.Enemies((uint)OID.Whirlwind);
            var count = whirlwinds.Count;
            for (var j = 0; j < count; ++j)
            {
                if (whirlwinds[j].Position.InCone(D152DotoliCiloc.ArenaCenter, deg, angle))
                {
                    enemyInCone = true;
                    break;
                }
            }
            if (!enemyInCone)
                cones.Add(new(D152DotoliCiloc.ArenaCenter, 20f, deg, angle));
        }
        _aoe = new(new AOEShapeCustom([.. cones], InvertForbiddenZone: true), D152DotoliCiloc.ArenaCenter, default, activation, Colors.SafeFromAOE);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (cones.Count != 0 && actor.OID == (uint)OID.Whirlwind) // sometimes the creation of whirlwinds is delayed
        {
            cones.Clear();
            GenerateHints();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
        {
            cones.Clear();
            _aoe = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe is AOEInstance aoe)
        {
            var check = true;
            if (aoe.Check(actor.Position))
                check = false;
            hints.Add(RiskHint, check);
        }
    }
}

class D152DotoliCilocStates : StateMachineBuilder
{
    public D152DotoliCilocStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DarkWings>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Stormcoming>()
            .ActivateOnEnter<OnLow>()
            .ActivateOnEnter<OnLowHaste>()
            .ActivateOnEnter<OnHigh>()
            .ActivateOnEnter<OnHighHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5269, SortOrder = 6)]
public class D152DotoliCiloc(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(245.289f, 13.626f);
    private const float offset = 0.42f;
    public static readonly Polygon[] StartingBoundsP = [new Polygon(ArenaCenter, 29.45f * CosPI.Pi16th, 16, 11.25f.Degrees())];
    public static readonly Polygon[] DefaultBoundsP = [new Polygon(ArenaCenter, 20f * CosPI.Pi16th, 16, 11.25f.Degrees())];
    private static readonly WPos[] verticesW = [new(227.1f, 17.333f), new(226.122f, 13.411f), new(227f, 10.126f), new(225.087f, 9.583f), new(224.016f, 13.541f), new(225.124f, 17.756f)];
    private static readonly WPos[] verticesN = WPos.GenerateRotatedVertices(ArenaCenter, verticesW, 90f);
    private static readonly WPos[] verticesE = WPos.GenerateRotatedVertices(ArenaCenter, verticesW, 180f);
    private static readonly WPos[] verticesS = WPos.GenerateRotatedVertices(ArenaCenter, verticesW, 270f);
    private static readonly PolygonCustomO[] difference = [new PolygonCustomO(verticesW, offset), new PolygonCustomO(verticesN, offset),
    new PolygonCustomO(verticesE, offset), new PolygonCustomO(verticesS, offset)];
    public static readonly ArenaBoundsComplex StartingBounds = new(StartingBoundsP, difference);
    public static readonly ArenaBoundsComplex DefaultBounds = new(DefaultBoundsP, difference);
}
