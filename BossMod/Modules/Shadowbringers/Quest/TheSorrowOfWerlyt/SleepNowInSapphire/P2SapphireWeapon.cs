using BossMod.Shadowbringers.Quest.SorrowOfWerlyt.SleepNowInSapphire.P1GuidanceSystem;

namespace BossMod.Shadowbringers.Quest.SorrowOfWerlyt.SleepNowInSapphire.P2SapphireWeapon;

public enum OID : uint
{
    Boss = 0x2DFA, // R19.400, x1
    RegulasImage = 0x2DFB, // R0.75
    MagitekTurret = 0x2DFC, // R1.65
    CeruleumServant = 0x2DFD, // R6.25
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Teleport = 20333, // MagitekTurret->location, no cast, single-target, if turret sucessfully self destructs, it teleports to player

    Activate = 20335, // Boss->self, 3.0s cast, single-target, spawns Regula's Images
    SimultaneousActivation = 21395, // Boss->self, 3.0s cast, single-target
    TailSwing = 20326, // Boss->self, 4.0s cast, range 46 circle
    OptimizedJudgment = 20325, // Boss->self, 4.0s cast, range 22-60 donut
    MagitekSpread = 20336, // RegulasImage->self, 5.0s cast, range 43 240-degree cone
    RegulasVisual = 20337, // RegulasImage->self, no cast, single-target
    SideraysVisual = 20328, // Boss->self, 8.0s cast, single-target
    SideraysRight = 20329, // Helper->self, 8.0s cast, range 128 90-degree cone
    SideraysLeft = 21021, // Helper->self, 8.0s cast, range 128 90-degree cone
    SapphireRay = 20327, // Boss->self, 8.0s cast, range 120 width 40 rect
    Turret = 20331, // Boss->self, 3.0s cast, single-target
    TurretVisual = 21465, // MagitekTurret->self, no cast, single-target
    MagitekRay = 20332, // MagitekTurret->self, 3.0s cast, range 100 width 6 rect
    ServantRoar = 20339, // CeruleumServant->self, 2.5s cast, range 100 width 8 rect
    OptimizedUltima = 20342, // Boss->self, 6.0s cast, range 100 circle, raidwide
    SwiftbreachVisual = 20330, // Boss->self, 4.5s cast, single-target
    Swiftbreach = 21418, // Helper->self, 4.5s cast, range 120 width 120 rect, raidwide
    PlasmaShot = 20340, // Boss->player, 6.0s cast, single-target, tankbuster
    PlasmaCannon = 20341, // Boss->location, 5.5s cast, range 40 circle
    SelfDestructVisual = 21489, // MagitekTurret->self, 30.0s cast, single-target
    SelfDestruct = 20334, // MagitekTurret->self, no cast, range 50 circle, enrage, removes about 80% of total hp
    FloodRay = 20338 // Boss->self, 115.0s cast, range 120 width 120 rect, enrage, duty fail
}

public enum SID : uint
{
    Invincibility = 775 // none->Boss, extra=0x0
}

class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, new AOEShapeRect(100f, 3f));
class ServantRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ServantRoar, new AOEShapeRect(100f, 4f));
class MagitekSpread(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekSpread, new AOEShapeCone(43f, 120f.Degrees()));

class TailSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSwing, 46f)
{
    private readonly MagitekSpread _aoe = module.FindComponent<MagitekSpread>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Casters.Count != 0 && _aoe.Casters.Count == 0 ? CollectionsMarshal.AsSpan(Casters)[..1] : [];
    }
}

class OptimizedJudgment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OptimizedJudgment, new AOEShapeDonut(22f, 60f));
class SapphireRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SapphireRay, new AOEShapeRect(120f, 20f));

abstract class Siderays(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(128f, 45f.Degrees()));
class SideraysLeft(BossModule module) : Siderays(module, (uint)AID.SideraysLeft);
class SideraysRight(BossModule module) : Siderays(module, (uint)AID.SideraysRight);

class FloodRay(BossModule module) : Components.CastHint(module, (uint)AID.FloodRay, "Enrage", true);
class SelfDestruct(BossModule module) : Components.CastHint(module, (uint)AID.SelfDestructVisual, "Turrets are enraging!", true);
class OptimizedUltima(BossModule module) : Components.RaidwideCast(module, (uint)AID.OptimizedUltima);
class Swiftbreach(BossModule module) : Components.RaidwideCast(module, (uint)AID.Swiftbreach);
class PlasmaShot(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PlasmaShot);
class PlasmaCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PlasmaCannon, 40f);

class TheSapphireWeaponStates : StateMachineBuilder
{
    public TheSapphireWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekSpread>()
            .ActivateOnEnter<TailSwing>()
            .ActivateOnEnter<OptimizedJudgment>()
            .ActivateOnEnter<SideraysLeft>()
            .ActivateOnEnter<SideraysRight>()
            .ActivateOnEnter<SapphireRay>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<ServantRoar>()
            .ActivateOnEnter<GWarrior>()
            .ActivateOnEnter<FloodRay>()
            .ActivateOnEnter<SelfDestruct>()
            .ActivateOnEnter<OptimizedUltima>()
            .ActivateOnEnter<Swiftbreach>()
            .ActivateOnEnter<PlasmaCannon>()
            .ActivateOnEnter<PlasmaShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69431, NameID = 9458)]
public class TheSapphireWeapon(WorldState ws, Actor primary) : SleepNowInSapphireSharedBounds(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, [(uint)OID.MagitekTurret, (uint)OID.CeruleumServant]);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = hints.PotentialTargets[i];
            h.Priority = h.Actor.FindStatus((uint)SID.Invincibility) == null ? 1 : AIHints.Enemy.PriorityInvincible;
        }
    }
}
