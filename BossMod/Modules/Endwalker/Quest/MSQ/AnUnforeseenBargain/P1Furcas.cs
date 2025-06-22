using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.MSQ.AnUnforeseenBargain.P1Furcas;

public enum OID : uint
{
    Boss = 0x3D71, // R6.0
    VisitantBlackguard = 0x3EA2, // R1.7
    VisitantTaurus = 0x3EA7, // R1.68
    VisitantPersona = 0x3D74, // R1.6
    VisitantArchDemon = 0x3EA3, // R1.0
    VisitantDahak = 0x3D75, // R2.75
    VisitantVoidskipper = 0x3D72, // R1.08
    Hellsfire = 0x3ED4, // R0.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // VisitantTaurus/VisitantBlackguard->player, no cast, single-target
    AutoAttack2 = 33023, // Boss->player, no cast, single-target
    AutoAttack3 = 872, // VisitantVoidskipper->player, no cast, single-target

    SinisterSphere = 33003, // Boss->self, 4.0s cast, single-target

    VoidSlash = 33027, // VisitantBlackguard->self, 4.0s cast, range 8+R 90-degree cone
    Explosion = 33004, // Helper->self, 10.0s cast, range 5 circle
    UnmitigatedExplosion = 33039, // Helper->self, no cast, range 60 circle, tower fail
    JongleursX = 31802, // Boss->player, 4.0s cast, single-target
    StraightSpindle = 31796, // VisitantVoidskipper->self, 4.0s cast, range 50+R width 5 rect
    VoidTorchVisual = 33006, // Boss->self, 3.0s cast, single-target
    VoidTorch = 33007, // Helper->location, 3.0s cast, range 6 circle
    HellishScythe = 31800, // Boss->self, 5.0s cast, range 10 circle
    FlameBlast = 33008, // Hellsfire->self, 4.0s cast, range 80+R width 4 rect
    Blackout1 = 31801, // Boss->self, 4.0s cast, range 60 circle
    Blackout2 = 31798, // VisitantVoidskipper->self, 13.0s cast, range 60 circle
    JestersReward = 33031 // Boss->self, 6.0s cast, range 28 180-degree cone
}

class AutoZero(BossModule module) : QuestBattle.RotationModule<ZeroAI>(module);
class Explosion(BossModule module) : Components.CastTowers(module, (uint)AID.Explosion, 5f);
class VoidSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidSlash, new AOEShapeCone(9.7f, 45f.Degrees()));
class JongleursX(BossModule module) : Components.SingleTargetCast(module, (uint)AID.JongleursX);
class StraightSpindle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StraightSpindle, new AOEShapeRect(51.08f, 2.5f));
class VoidTorch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidTorch, 6f);
class HellishScythe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HellishScythe, 10f);
class FlameBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlameBlast, new AOEShapeRect(80.6f, 2f));
class JestersReward(BossModule module) : Components.SimpleAOEs(module, (uint)AID.JestersReward, new AOEShapeCone(28f, 90f.Degrees()));

class Blackout1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Blackout1);
class Blackout2(BossModule module) : Components.CastHint(module, (uint)AID.Blackout2, "Kill the Voidskipper!", true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.CastInfo?.Action.ID == WatchedAction)
                e.Priority = 5;
        }
    }
}

class FurcasStates : StateMachineBuilder
{
    public FurcasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoZero>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<VoidSlash>()
            .ActivateOnEnter<JongleursX>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<VoidTorch>()
            .ActivateOnEnter<HellishScythe>()
            .ActivateOnEnter<FlameBlast>()
            .ActivateOnEnter<Blackout1>()
            .ActivateOnEnter<Blackout2>()
            .ActivateOnEnter<JestersReward>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70209, NameID = 12066)]
public class Furcas(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(new(97.85f, 286), 19.5f, 20)]);
    private static readonly uint[] trash = [(uint)OID.VisitantTaurus, (uint)OID.VisitantDahak, (uint)OID.VisitantVoidskipper,
    (uint)OID.VisitantPersona, (uint)OID.VisitantBlackguard, (uint)OID.VisitantArchDemon];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(trash));
    }
}
