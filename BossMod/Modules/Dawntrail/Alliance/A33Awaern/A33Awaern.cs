namespace BossMod.Dawntrail.Alliance.A33Awaern;

public enum OID : uint
{
    Awaern = 0x4DB6, // R4.500, x1 aern mob
    Awzdei = 0x4DB7, // R2.300, x4 : pot mobs
    Helper = 0x233C
}

public enum AID : uint
{
    BossAuto = 45307, // Boss->player, no cast, single-target
    AwzdeiAuto = 50477, // Awzdei->player, no cast, single-target

    GlacierSplitterVisual = 50104, // Boss->self, 2.9+0.6s cast, single-target
    GlacierSplitter = 50105, // Helper->self, 3.5s cast, range 60 30-degree cone
    OpticInduration = 50106, // Awzdei->self, 3.5s cast, range 60 30-degree cone
    StaticFilament = 50487, // Awzdei->location, 4.0s cast, range 8 circle
    AuroralWindCast = 50501, // Boss->self, 5.0s cast, single-target
    AuroralWind = 50502, // Helper->players, 5.0s cast, range 6 circle
    ImpactStreamCast = 50485, // Boss->self, 4.0s cast, single-target
    ImpactStream = 50486 // Helper->self, 4.0s cast, range 80 circle
}

sealed class Awzdei(BossModule module) : Components.Adds(module, (uint)OID.Awzdei);
sealed class GlacierSplitterOpticInduration(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GlacierSplitter, (uint)AID.OpticInduration], new AOEShapeCone(60f, 15f.Degrees()));
sealed class StaticFilament(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StaticFilament, 8f);
sealed class AuroralWind(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AuroralWind, 6f);
sealed class ImpactStream(BossModule module) : Components.RaidwideCast(module, (uint)AID.ImpactStream);

sealed class A33AwaernStates : StateMachineBuilder
{
    public A33AwaernStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Awzdei>()
            .ActivateOnEnter<GlacierSplitterOpticInduration>()
            .ActivateOnEnter<StaticFilament>()
            .ActivateOnEnter<AuroralWind>()
            .ActivateOnEnter<ImpactStream>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.Awaern, Contributors = "Xan, ported by wen", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14838u)]
public sealed class A33Awaern(WorldState ws, Actor primary) : BossModule(ws, primary, new(-720f, 720f), new ArenaBoundsRect(30f, 24f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Awzdei));
    }

    public override bool ShouldPrioritizeAllEnemies => true;
}
