namespace BossMod.RealmReborn.Dungeon.D08Qarn.D083Adjudicator;

public enum OID : uint
{
    // Boss
    Boss = 0x477E, // x1

    // Trash
    MythrilVerge1 = 0x477F, // Summoned during fight (VergeLine attacks)
    MythrilVerge2 = 0x4780 // Summoned during fight (VergePulse attacks)
}

public enum AID : uint
{
    // Boss
    AutoAttack = 872, // Boss->player, no cast
    LoomingJudgement = 42245, // Boss->player, 5.0s cast, tankbuster
    CreepingDarkness = 42247, // Boss->self, 5.0s cast, raidwide
    DarkII = 42248, // Boss->self, 6.0s cast, range 40 120-degree cone aoe
    Dark = 42246, // Boss->player, 3.0s cast, range 5 circle aoe

    // MythrilVerge
    SelfDestruct = 42242, // MythrilVerge->self, 3.0s cast, raidwide
    VergeLine = 42244, // MythrilVerge->self, 4.0s cast, range 60.6 width 4 rect aoe
    VergePulse = 42241 // MythrilVerge->self, 20.0s cast, range 60.6 width 4 rect aoe
}

class LoomingJudgement(BossModule module) : Components.SingleTargetCast(module, (uint)AID.LoomingJudgement);
class CreepingDarkness(BossModule module) : Components.RaidwideCast(module, (uint)AID.CreepingDarkness);
class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeCone(40f, 60.Degrees()));
class Dark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dark, new AOEShapeCircle(5f));
class SelfDestruct(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDestruct);
class VergeLine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VergeLine, new AOEShapeRect(60.6f, 2f));
class VergePulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VergePulse, new AOEShapeRect(60.6f, 2f));

class D083AdjudicatorStates : StateMachineBuilder
{
    public D083AdjudicatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LoomingJudgement>()
            .ActivateOnEnter<CreepingDarkness>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<VergeLine>()
            .ActivateOnEnter<VergePulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1570)]
public class D083Adjudicator(WorldState ws, Actor primary) : BossModule(ws, primary, new(236, 0), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.MythrilVerge1 or OID.MythrilVerge2 => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.MythrilVerge1));
        Arena.Actors(Enemies(OID.MythrilVerge2));
    }
}
