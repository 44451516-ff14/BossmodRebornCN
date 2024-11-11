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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002 && (OID)actor.OID == OID.ArenaVoidzone)
        {
            Arena.Bounds = D152DotoliCiloc.DefaultBounds;
            _aoe = null;
            begin = true;
        }
    }

    public override void Update()
    {
        if (!begin && _aoe == null)
            _aoe = new(donut, Arena.Center, default, WorldState.FutureTime(4));
    }
}

class DarkWings(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.DarkWings), 6, 5.1f);
class Whirlwind(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.Whirlwind));
class Stormcoming(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Stormcoming), 6);
class OnLow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OnLow), new AOEShapeCone(10.98f, 60.Degrees()));

class OnLowHaste(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.Swiftfeather), new AOEShapeCone(10.98f, 60.Degrees()))
{
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Swiftfeather)
            active = true;
        else if ((AID)spell.Action.ID == AID.OnLow)
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

class OnHigh(BossModule module) : Components.Knockback(module)
{
    private Source? _source;
    private static readonly SafeWall[] safeWallsW = [new(new(227.487f, 16.825f), new(226.567f, 13.39f)), new(new(226.567f, 13.39f), new(227.392f, 10.301f))];
    private static readonly SafeWall[] safeWallsN = GenerateRotatedSafeWalls(safeWallsW, 90);
    private static readonly SafeWall[] safeWallsE = GenerateRotatedSafeWalls(safeWallsW, 180);
    private static readonly SafeWall[] safeWallsS = GenerateRotatedSafeWalls(safeWallsW, 270);
    private static readonly SafeWall[] allSafeWalls = [.. safeWallsW, .. safeWallsN, .. safeWallsE, .. safeWallsS];

    private static SafeWall[] GenerateRotatedSafeWalls(SafeWall[] baseWalls, float angle)
    => baseWalls.Select(wall => new SafeWall(GenerateRotatedVertice(wall.Vertex1, angle), GenerateRotatedVertice(wall.Vertex2, angle))).ToArray();

    private static WPos GenerateRotatedVertice(WPos vertex, float rotationAngle) => WPos.RotateAroundOrigin(rotationAngle, D152DotoliCiloc.ArenaCenter, vertex);

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OnHigh)
            _source = new(caster.Position, 30, Module.CastFinishAt(spell), SafeWalls: allSafeWalls);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OnHigh)
            _source = null;
    }
}

class OnHighHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<ConeHA> cones = [];
    private AOEInstance? _aoe;
    private const string RiskHint = "Use safewalls for knockback!";
    private static readonly Angle angle = 11.25f.Degrees();
    private DateTime activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OnHigh)
        {
            activation = Module.CastFinishAt(spell);
            GenerateHints();
        }
    }

    private void GenerateHints()
    {
        for (var i = 0; i < 4; ++i)
        {
            var deg = (i * 90).Degrees();
            if (!Module.Enemies(OID.Whirlwind).Any(x => x.Position.InCone(D152DotoliCiloc.ArenaCenter, deg, angle)))
                cones.Add(new(D152DotoliCiloc.ArenaCenter, 20, deg, angle));
        }
        _aoe = new(new AOEShapeCustom(cones, InvertForbiddenZone: true), D152DotoliCiloc.ArenaCenter, default, activation, Colors.SafeFromAOE);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (cones.Count > 0 && (OID)actor.OID == OID.Whirlwind) // sometimes the creation of whirlwinds is delayed
        {
            cones.Clear();
            GenerateHints();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OnHigh)
        {
            cones.Clear();
            _aoe = null;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeAOEs = ActiveAOEs(slot, actor).ToList();
        if (activeAOEs.Any(c => !c.Check(actor.Position)))
            hints.Add(RiskHint);
        else if (activeAOEs.Any(c => c.Check(actor.Position)))
            hints.Add(RiskHint, false);
    }
}

class D152DotoliCilocStates : StateMachineBuilder
{
    public D152DotoliCilocStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Components.StayInBounds>()
            .ActivateOnEnter<DarkWings>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Stormcoming>()
            .ActivateOnEnter<OnLow>()
            .ActivateOnEnter<OnLowHaste>()
            .ActivateOnEnter<OnHigh>()
            .ActivateOnEnter<OnHighHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182, NameID = 5269)]
public class D152DotoliCiloc(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingBounds)
{
    public static readonly WPos ArenaCenter = new(245.289f, 13.626f);
    private static readonly float multi = 1 / MathF.Cos(MathF.PI / 16);
    private const float offset = 0.42f;
    public static readonly Polygon[] StartingBoundsP = [new Polygon(ArenaCenter, 29.45f * multi, 16, 11.25f.Degrees())];
    public static readonly Polygon[] DefaultBoundsP = [new Polygon(ArenaCenter, 20 * multi, 16, 11.25f.Degrees())];
    private static readonly WPos[] verticesW = [new(227.1f, 17.333f), new(226.122f, 13.411f), new(227, 10.126f), new(225.087f, 9.583f), new(224.016f, 13.541f), new(225.124f, 17.756f)];
    private static readonly WPos[] verticesN = GenerateRotatedVertices(verticesW, 90);
    private static readonly WPos[] verticesE = GenerateRotatedVertices(verticesW, 180);
    private static readonly WPos[] verticesS = GenerateRotatedVertices(verticesW, 270);
    private static readonly PolygonCustomO[] difference = [new PolygonCustomO(verticesW, offset), new PolygonCustomO(verticesN, offset),
    new PolygonCustomO(verticesE, offset), new PolygonCustomO(verticesS, offset)];
    public static readonly ArenaBoundsComplex StartingBounds = new(StartingBoundsP, difference);
    public static readonly ArenaBoundsComplex DefaultBounds = new(DefaultBoundsP, difference);

    public static WPos[] GenerateRotatedVertices(WPos[] vertices, float rotationAngle)
    => vertices.Select(vertex => WPos.RotateAroundOrigin(rotationAngle, ArenaCenter, vertex)).ToArray();
}
