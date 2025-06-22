namespace BossMod.Endwalker.Quest.Job.Sage.SagesFocus;

public enum OID : uint
{
    Boss = 0x3587, // R0.5
    Loifa = 0x3588, // R0.5
    Mahaud = 0x3586, // R0.5
    StrengthenedNoulith = 0x358E, // R1.0
    ChiBomb = 0x358D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/Loifa->Lalah, no cast, single-target
    Teleport1 = 26544, // Boss->location, no cast, single-target
    Teleport2 = 26556, // Loifa->location, no cast, single-target
    Teleport3 = 26557, // Mahaud->location, no cast, single-target
    Teleport4 = 26540, // StrengthenedNoulith->location, no cast, single-target

    Demifire = 26558, // Mahaud->Lalah, no cast, single-target
    TripleThreat = 26535, // Boss->Lalah, 8.0s cast, single-target
    ChiBomb = 26536, // Boss->self, 5.0s cast, single-target
    Explosion = 26537, // ChiBomb->self, 5.0s cast, range 6 circle
    ArmOfTheScholar = 26543, // Boss->self, 5.0s cast, range 5 circle
    Nouliths = 26538, // 3588->self, 5.0s cast, single-target
    Noubelea = 26541, // 3588->self, 5.0s cast, single-target
    Noubelea1 = 26542, // 358E->self, 5.0s cast, range 50 width 4 rect
    DemiblizzardIII = 26545, // Mahaud->self, 5.0s cast, single-target
    DemiblizzardIII1 = 26546, // Helper->self, 5.0s cast, range 10-40 donut
    Demigravity1 = 26539, // Mahaud->location, 5.0s cast, range 6 circle
    Demigravity2 = 26550, // Helper->location, 5.0s cast, range 6 circle
    DemifireIII = 26547, // Mahaud->self, 5.0s cast, single-target
    DemifireIII1 = 26548, // Helper->self, 5.6s cast, range 40 circle
    DemifireIIVisual = 26552, // Mahaud->self, 7.0s cast, single-target
    DemifireIISpread = 26553, // Helper->player/Lalah, 5.0s cast, range 5 circle
    DemifireIIAOE = 26554, // Helper->location, 5.0s cast, range 14 circle
    Diagnosis = 26555, // Loifa->Mahaud, 3.0s cast, single-target
    DosisIII = 26551, // Loifa->Lalah, 8.0s cast, single-target
}

class DosisIII(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DosisIII);
class DemifireSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DemifireIISpread, 5);
class DemifireIIAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DemifireIIAOE, 14);
class DemifireIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.DemifireIII1);
class Noubelea(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Noubelea1, new AOEShapeRect(50, 2));
class Demigravity1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Demigravity1, 6);
class Demigravity2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Demigravity2, 6);
class Demiblizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DemiblizzardIII1, new AOEShapeDonut(10, 40));
class TripleThreat(BossModule module) : Components.SingleTargetCast(module, (uint)AID.TripleThreat);
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 6);
class ArmOfTheScholar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ArmOfTheScholar, 5);

class AncelRockfistStates : StateMachineBuilder
{
    public AncelRockfistStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TripleThreat>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ArmOfTheScholar>()
            .ActivateOnEnter<Noubelea>()
            .ActivateOnEnter<Demiblizzard>()
            .ActivateOnEnter<Demigravity1>()
            .ActivateOnEnter<Demigravity2>()
            .ActivateOnEnter<DemifireIII>()
            .ActivateOnEnter<DemifireIIAOE>()
            .ActivateOnEnter<DemifireSpread>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69604, NameID = 10732)]
public class AncelRockfist(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(0, -82.146f), 18.5f, 20)]);
    private static readonly uint[] bosses = [(uint)OID.Boss, (uint)OID.Mahaud, (uint)OID.Loifa];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(bosses));
    }
}

