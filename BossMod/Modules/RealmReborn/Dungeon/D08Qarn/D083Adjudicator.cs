namespace BossMod.RealmReborn.Dungeon.D08Qarn.D083Adjudicator;

public enum OID : uint
{
    Boss = 0x477E, // R1.5
    MythrilVerge1 = 0x477F, // R0.6
    MythrilVerge2 = 0x4780 // R0.6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    LoomingJudgement = 42245, // Boss->player, 5.0s cast, single-target
    Summon1 = 42243, // Boss->self, 3.0s cast, single-target
    Summon2 = 42239, // Boss->self, 3.0s cast, single-target
    VergeLine = 42244, // MythrilVerge1->self, 2.2s cast, range 60+R width 4 rect
    CreepingDarkness = 42247, // Boss->self, 5.0s cast, range 50 circle
    Stun = 30506, // MythrilVerge2->player, no cast, single-target, stun before pull
    MythrilChains = 42240, // MythrilVerge2->player, no cast, single-target, pulls player to verge
    SelfDestruct = 42242, // MythrilVerge1/MythrilVerge2->self, 3.0s cast, range 60 circle
    VergePulse = 42241, // MythrilVerge2->self, 20.0s cast, range 60+R width 4 rect
    DarkII = 42248, // Boss->self, 6.0s cast, range 40 120-degree cone
    Dark = 42246 // Boss->location, 3.0s cast, range 5 circle
}

sealed class LoomingJudgement(BossModule module) : Components.SingleTargetCast(module, (uint)AID.LoomingJudgement);
sealed class CreepingDarknessSelfDestruct(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.CreepingDarkness, (uint)AID.SelfDestruct]);
sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeCone(40f, 60f.Degrees()));
sealed class Dark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dark, 5f);
sealed class VergeLineVergePulse(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VergeLine, (uint)AID.VergePulse], new AOEShapeRect(60.6f, 2f));

sealed class D083AdjudicatorStates : StateMachineBuilder
{
    public D083AdjudicatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LoomingJudgement>()
            .ActivateOnEnter<CreepingDarknessSelfDestruct>()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<Dark>()
            .ActivateOnEnter<VergeLineVergePulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 9, NameID = 1570)]
public sealed class D083Adjudicator(WorldState ws, Actor primary) : BossModule(ws, primary, new(236, 0), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MythrilVerge1));
        Arena.Actors(Enemies((uint)OID.MythrilVerge2));
    }
}
