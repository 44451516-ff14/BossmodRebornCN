﻿namespace BossMod.Dawntrail.Dungeon.D05Origenics.D052Deceiver;

public enum OID : uint
{
    Boss = 0x4170, // R5.0
    Cahciua = 0x418F, // R0.96
    OrigenicsSentryG91 = 0x4172, // R0.9
    OrigenicsSentryG92 = 0x4171, // R0.9
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 873, // OrigenicsSentryG92->player, no cast, single-target
    Teleport = 36362, // Boss->location, no cast, single-target

    Electrowave = 36371, // Boss->self, 5.0s cast, range 72 circle, raidwide

    BionicThrashVisual1 = 36369, // Boss->self, 7.0s cast, single-target
    BionicThrashVisual2 = 36368, // Boss->self, 7.0s cast, single-target
    BionicThrash = 36370, // Helper->self, 8.0s cast, range 30 90-degree cone

    InitializeAndroids = 36363, // Boss->self, 4.0s cast, single-target, spawns OrigenicsSentryG91 and OrigenicsSentryG92

    SynchroshotFake = 36373, // OrigenicsSentryG91->self, 5.0s cast, range 40 width 4 rect
    SynchroshotReal = 36372, // OrigenicsSentryG92->self, 5.0s cast, range 40 width 4 rect

    InitializeTurretsVisual = 36364, // Boss->self, 4.0s cast, single-target
    InitializeTurretsFake = 36426, // Helper->self, 4.7s cast, range 4 width 10 rect
    InitializeTurretsReal = 36365, // Helper->self, 4.7s cast, range 4 width 10 rect

    LaserLashReal = 36366, // Helper->self, 5.0s cast, range 40 width 10 rect
    LaserLashFake = 38807, // Helper->self, 5.0s cast, range 40 width 10 rect

    SurgeNPCs = 39736, // Helper->self, 8.5s cast, range 40 width 40 rect, knockback 15 dir left/right, only seems to apply to NPCs
    Surge = 36367, // Boss->location, 8.0s cast, range 40 width 40 rect, knockback 30 dir left/right

    Electray = 38320, // Helper->player, 8.0s cast, range 5 circle
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private const float HalfWidth = 5.5f; // adjusted for 0.5 player hitbox
    public static readonly WPos ArenaCenter = new(-172, -142);
    public static readonly ArenaBoundsSquare StartingBounds = new(24.5f);
    private static readonly ArenaBoundsSquare defaultBounds = new(20);
    private static readonly Square defaultSquare = new(ArenaCenter, 20);
    private static readonly AOEShapeCustom square = new([new Square(ArenaCenter, 25)], [new Square(ArenaCenter, 20)]);

    private static readonly RectangleSE[] westRows =
    [
        new RectangleSE(new(-192, -157), new(-187.5f, -157), HalfWidth),
        new RectangleSE(new(-192, -147), new(-187.5f, -147), HalfWidth),
        new RectangleSE(new(-192, -137), new(-187.5f, -137), HalfWidth),
        new RectangleSE(new(-192, -127), new(-187.5f, -127), HalfWidth),
    ];

    private static readonly RectangleSE[] eastRows =
    [
        new RectangleSE(new(-152, -157), new(-156.5f, -157), HalfWidth),
        new RectangleSE(new(-152, -147), new(-156.5f, -147), HalfWidth),
        new RectangleSE(new(-152, -137), new(-156.5f, -137), HalfWidth),
        new RectangleSE(new(-152, -127), new(-156.5f, -127), HalfWidth),
    ];

    public static readonly Dictionary<byte, ArenaBoundsComplex> ArenaBoundsMap = new()
    {
        { 0x2A, new ArenaBoundsComplex([defaultSquare], [westRows[1], westRows[3]]) },
        { 0x1B, new ArenaBoundsComplex([defaultSquare], [westRows[1], westRows[3], eastRows[0], eastRows[2]]) },
        { 0x2C, new ArenaBoundsComplex([defaultSquare], [westRows[1], westRows[2]]) },
        { 0x1E, new ArenaBoundsComplex([defaultSquare], [westRows[1], westRows[2], eastRows[0], eastRows[3]]) },
        { 0x2D, new ArenaBoundsComplex([defaultSquare], [westRows[0], westRows[3]]) },
        { 0x1D, new ArenaBoundsComplex([defaultSquare], [westRows[0], westRows[3], eastRows[1], eastRows[2]]) },
        { 0x2B, new ArenaBoundsComplex([defaultSquare], [westRows[0], westRows[2]]) },
        { 0x1C, new ArenaBoundsComplex([defaultSquare], [westRows[0], westRows[2], eastRows[1], eastRows[3]]) },
    };

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Electrowave && Module.Arena.Bounds == StartingBounds)
            _aoe = new AOEInstance(square, Module.Center, default, spell.NPCFinishAt.AddSeconds(0.7f));
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (ArenaBoundsMap.TryGetValue(index, out var value))
                Module.Arena.Bounds = value;
            else if (index == 0x12)
            {
                Module.Arena.Bounds = defaultBounds;
                _aoe = null;
            }
        }
        else if (state == 0x00080004)
            Module.Arena.Bounds = defaultBounds;
    }
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class BionicThrash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BionicThrash), new AOEShapeCone(30, 45.Degrees()));
class Synchroshot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SynchroshotReal), new AOEShapeRect(40, 2));
class InitializeTurrets(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InitializeTurretsReal), new AOEShapeRect(4, 5));
class LaserLash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LaserLashReal), new AOEShapeRect(40, 5));
class Electray(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Electray), 5);

class Surge(BossModule module) : Components.Knockback(module)
{
    private DateTime activation;
    private readonly List<Source> _sources = [];
    private static readonly List<SafeWall> walls2A1B = [new(new(-187.5f, -142), new(-187.5f, -152)), new(new(-187.5f, -122), new(-187.5f, -132)),
    new(new(-156.5f, -152), new(-156.5f, -162)), new(new(-156.5f, -132), new(-156.5f, -142))];
    private static readonly List<SafeWall> walls2C1E = [new(new(-187.5f, -142), new(-187.5f, -152)), new(new(-187.5f, -132), new(-187.5f, -142)),
    new(new(-156.5f, -152), new(-156.5f, -162)), new(new(-156.5f, -122), new(-156.5f, -132))];
    private static readonly List<SafeWall> walls2D1D = [new(new(-187.5f, -152), new(-187.5f, -162)), new(new(-187.5f, -122), new(-187.5f, -132)),
    new(new(-156.5f, -142), new(-156.5f, -152)), new(new(-156.5f, -132), new(-156.5f, -142))];
    private static readonly List<SafeWall> walls2B1C = [new(new(-187.5f, -152), new(-187.5f, -162)), new(new(-187.5f, -122), new(-187.5f, -132)),
    new(new(-156.5f, -142), new(-156.5f, -152)), new(new(-156.5f, -132), new(-156.5f, -142))];
    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Surge)
        {
            var safewalls = GetActiveSafeWalls();
            activation = spell.NPCFinishAt.AddSeconds(0.8f);
            _sources.Add(new(caster.Position, 30, activation, _shape, spell.Rotation + 90.Degrees(), Kind.DirForward, default, safewalls));
            _sources.Add(new(caster.Position, 30, activation, _shape, spell.Rotation - 90.Degrees(), Kind.DirForward, default, safewalls));
        }
    }

    private List<SafeWall> GetActiveSafeWalls()
    {
        foreach (var kvp in ArenaChanges.ArenaBoundsMap)
        {
            if (Module.Arena.Bounds == kvp.Value)
            {
                return kvp.Key switch
                {
                    0x1B => walls2A1B,
                    0x1E => walls2C1E,
                    0x1D => walls2D1D,
                    0x1C => walls2B1C,
                    _ => []
                };
            }
        }
        return [];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Surge)
        {
            _sources.Clear();
            ++NumCasts;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Sources(slot, actor).Any() || activation > Module.WorldState.CurrentTime) // 0.8s delay to wait for action effect
        {
            var forbidden = new List<Func<WPos, float>>();
            var safewalls = GetActiveSafeWalls();
            foreach (var w in safewalls)
                forbidden.Add(ShapeDistance.InvertedRect(new(Module.Center.X, w.Vertex1.Z - 5), w.Vertex1.X == -187.5f ? new WDir(-4, 0) : new(4, 0), 8, 0, 20));
            hints.AddForbiddenZone(p => forbidden.Select(f => f(p)).Max(), activation);
        }
    }
}

class D052DeceiverStates : StateMachineBuilder
{
    public D052DeceiverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BionicThrash>()
            .ActivateOnEnter<Synchroshot>()
            .ActivateOnEnter<InitializeTurrets>()
            .ActivateOnEnter<LaserLash>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Surge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12693)]
public class D052Deceiver(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.StartingBounds)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.OrigenicsSentryG92))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.OrigenicsSentryG91))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
