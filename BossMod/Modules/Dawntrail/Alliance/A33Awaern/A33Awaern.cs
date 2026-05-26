namespace BossMod.Dawntrail.Alliance.A33Awaern;

public enum OID : uint
{
    Awaern = 0x4DB6, // R4.500, x1 aern mob
    Awzdei = 0x4DB7, // R2.300, x4 : pot mobs
    Helper = 0x233C, // R0.500, x25, Helper type
}

public enum AID : uint
{
    BossAuto = 45307, // Boss->player, no cast, single-target
    AwzdeiAuto = 50477, // 4DB7->player, no cast, single-target
    GlacierSplitterVisual = 50104, // Boss->self, 2.9+0.6s cast, single-target
    GlacierSplitter = 50105, // Helper->self, 3.5s cast, range 60 30-degree cone
    OpticInduration = 50106, // Awzdei->self, 3.5s cast, range 60 30-degree cone
    StaticFilament = 50487, // Awzdei->location, 4.0s cast, range 8 circle
    AuroralWindCast = 50501, // Boss->self, 5.0s cast, single-target
    AuroralWind = 50502, // Helper->players, 5.0s cast, range 6 circle
    ImpactStreamCast = 50485, // Boss->self, 4.0s cast, single-target
    ImpactStream = 50486, // Helper->self, 4.0s cast, range 80 circle
}

class Awzdei(BossModule module) : Components.Adds(module, (uint)OID.Awzdei);
class GlacierSplitter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlacierSplitter, new AOEShapeCone(60, 15.Degrees()));
class OpticInduration(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OpticInduration, new AOEShapeCone(60, 15.Degrees()));
class StaticFilament(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StaticFilament, 8);
class AuroralWind(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AuroralWind, 6);
class ImpactStream(BossModule module) : Components.RaidwideCast(module, (uint)AID.ImpactStream);

class AwaernStates : StateMachineBuilder
{
    public AwaernStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Awzdei>()
            .ActivateOnEnter<GlacierSplitter>()
            .ActivateOnEnter<OpticInduration>()
            .ActivateOnEnter<StaticFilament>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<ImpactStream>();
    }
}


[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(AwaernStates),
    ConfigType = null,
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    PrimaryActorOID = (uint)OID.Awaern,
    Contributors = "Xan, ported by wen",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Alliance,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1117u,
    NameID = 14838u,
    SortOrder = 4,
    PlanLevel = 0)]


public sealed class A33Awaern(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(30f, 24f))
{
    public static readonly WPos ArenaCenter = new(-720f, 720f);

    public static readonly uint[] GardenofRuHmetMobs = [(uint)OID.Awaern, (uint)OID.Awzdei];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, GardenofRuHmetMobs);
    }
}

// Aw'aern = 14838u
// Aw'zdei = 14839u
