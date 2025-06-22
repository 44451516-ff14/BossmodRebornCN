namespace BossMod.Endwalker.Quest.Job.Sage.LifeEphemeralPathEternal.AncelAndMahaud;

public enum OID : uint
{
    Boss = 0x35C5, // R0.5
    MahaudFlamehand = 0x35C4, // R0.5
    ChiBomb = 0x35C7, // R1.0
    Helper = 0x233C, // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->Lalah, no cast, single-target
    Teleport1 = 26868, // Boss->location, no cast, single-target
    Teleport2 = 26869, // MahaudFlamehand->location, no cast, single-target

    ChiBlastVisual = 26838, // Boss->self, 5.0s cast, single-target
    ChiBlast = 26839, // Helper->self, 5.0s cast, range 100 circle
    ChiBomb = 26835, // Boss->self, 5.0s cast, single-target

    Explosion = 26837, // ChiBomb->self, 5.0s cast, range 6 circle
    ArmOfTheScholar = 26836, // Boss->self, 5.0s cast, range 5 circle
    RawRockbreaker = 26832, // Boss->self, 5.0s cast, single-target
    RawRockbreaker1 = 26833, // Helper->self, 4.0s cast, range 10 circle
    RawRockbreaker2 = 26834, // Helper->self, 4.0s cast, range 10-20 donut
    DemifireII = 26842, // MahaudFlamehand->Lalah, 8.0s cast, single-target
    Demiburst = 26843, // MahaudFlamehand->self, 7.0s cast, single-target
    ElectrogeneticForce = 26844, // Helper->self, 8.0s cast, range 6 circle
    DemifireIII = 26841, // MahaudFlamehand->Lalah, 3.0s cast, single-target
    FourElements = 26846, // MahaudFlamehand->self, 8.0s cast, single-target
    ClassicalFire = 26847, // Helper->Lalah, 8.0s cast, range 6 circle
    ClassicalThunder = 26848, // Helper->player/Loifa/Lalah, 5.0s cast, range 6 circle
    ClassicalBlizzard = 26849, // Helper->location, 5.0s cast, range 6 circle
    ClassicalStone = 26850, // Helper->self, 9.0s cast, range 50 circle
}

class DemifireII(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DemifireII);
class ElectrogeneticForce(BossModule module) : Components.CastTowers(module, (uint)AID.ElectrogeneticForce, 6);
class RawRockbreaker(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RawRockbreaker)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.RawRockbreaker1 => 0,
            AID.RawRockbreaker2 => 1,
            _ => -1
        };
        AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2));
    }
}

class ChiBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.ChiBlast);
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 6);
class ArmOfTheScholar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmOfTheScholar, 5);

class ClassicalFire(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ClassicalFire, 6);
class ClassicalThunder(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.ClassicalThunder, 6);
class ClassicalBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ClassicalBlizzard, 6);
class ClassicalStone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ClassicalStone, 15);

class AncelAndMahaudStates : StateMachineBuilder
{
    public AncelAndMahaudStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DemifireII>()
            .ActivateOnEnter<ChiBlast>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ArmOfTheScholar>()
            .ActivateOnEnter<RawRockbreaker>()
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<ClassicalFire>()
            .ActivateOnEnter<ClassicalThunder>()
            .ActivateOnEnter<ClassicalBlizzard>()
            .ActivateOnEnter<ClassicalStone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69608, NameID = 10732)]
public class AncelAndMahaud(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(new(224.8f, -855.8f), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies([(uint)OID.Boss, (uint)OID.MahaudFlamehand]));
    }
}
