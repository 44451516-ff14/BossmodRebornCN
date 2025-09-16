namespace BossMod.Heavensward.Quest.MSQ.FlyFreeMyPretty;

public enum OID : uint
{
    Boss = 0x195E,
    GrynewahtP2 = 0x195F, // R0.5
    ImperialColossus = 0x1966, // R3.0
    ImperialHoplomachus = 0x1960, // R0.5
    ImperialSecutor = 0x1963, // R0.5
    ImperialEques = 0x1962, // R0.5
    ImperialSagittarius = 0x1965, // R0.5
    ImperialSignifer = 0x1964, // R0.5
    ImperialLaquearius = 0x1961, // R0.5
    FireVoidzone = 0x1E86DF // R0.5
}

public enum AID : uint
{
    AutoAttack1 = 872, // Boss/ImperialSecutor/ImperialSignifer->player/allies, no cast, single-target
    AutoAttack2 = 871, // ImperialEques->allies, no cast, single-target
    AutoAttack3 = 870, // ImperialHoplomachus/ImperialLaquearius/ImperialColossus->player/allies, no cast, single-target
    AutoAttack4 = 873, // ImperialSagittarius->player/allies, no cast, single-target

    AugmentedUprising = 7608, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    AugmentedSuffering = 7607, // Boss->self, 3.5s cast, range 6+R circle
    Heartstopper = 866, // ImperialEques->self, 2.5s cast, range 3+R width 3 rect
    Overpower = 720, // ImperialLaquearius->self, 2.1s cast, range 6+R 90-degree cone
    GrandSword = 7615, // ImperialColossus->self, 3.0s cast, range 18+R 120-degree cone
    MagitekRay = 7617, // ImperialColossus->location, 3.0s cast, range 6 circle
    GrandStrike = 7616, // ImperialColossus->self, 2.5s cast, range 45+R width 4 rect
    ShrapnelShellVisual = 7613, // Boss->self, no cast, single-target
    ShrapnelShell = 7614, // GrynewahtP2->location, 2.5s cast, range 6 circle
    MagitekMissilesVisual = 7611, // Boss->self, no cast, single-target
    MagitekMissiles = 7612, // GrynewahtP2->location, 5.0s cast, range 15 circle

    AugmentedShatter = 7609, // Boss->allies, no cast, single-target
    Rampart = 10, // ImperialHoplomachus->self, no cast, single-target
    FastBlade = 717, // ImperialHoplomachus->player/allies, no cast, single-target
    FightOrFlight = 20, // ImperialHoplomachus->self, no cast, single-target
    Acris = 4541, // ImperialEques/ImperialLaquearius/ImperialSignifer->self, 4.0s cast, single-target
    Featherfoot = 55, // ImperialSecutor->self, no cast, single-target
    Vitalis = 4542, // ImperialSecutor/ImperialHoplomachus->self, 4.0s cast, single-target
    Feint = 76, // ImperialEques->allies, no cast, single-target
    SecondWind = 57, // ImperialSecutor->self, no cast, single-target
    TripleThreat = 475, // ImperialSecutor->player/allies, no cast, single-target
    TrueThrust = 722, // ImperialEques->allies, no cast, single-target
    Thunder = 968, // ImperialSignifer->player/allies, 0.9s cast, single-target
    VenomousBite = 100, // ImperialSagittarius->player/allies, no cast, single-target
    Windbite = 113, // ImperialSagittarius->player/allies, no cast, single-target
    Firebomb = 7610, // Boss->player/allies, no cast, range 4 circle
    SelfDetonate = 7618 // ImperialColossus->self, 10.0s cast, range 40+R circle
}

class MagitekMissiles(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekMissiles, 15f);
class ShrapnelShell(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShrapnelShell, 6f);
class Firebomb(BossModule module) : Components.Voidzone(module, 4, m => m.Enemies((uint)OID.FireVoidzone).Where(e => e.EventState != 7));

class AugmentedUprising(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedUprising, new AOEShapeCone(8.5f, 60f.Degrees()));
class AugmentedSuffering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AugmentedSuffering, 6.5f);
class Heartstopper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Heartstopper, new AOEShapeRect(3.5f, 1.5f));
class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(6f, 45f.Degrees()));
class GrandSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandSword, new AOEShapeCone(21f, 60f.Degrees()));
class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, 6f);
class GrandStrike(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandStrike, new AOEShapeRect(48f, 2f));

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x10000002u)
        {
            Arena.Bounds = Grynewaht.CircleBounds;
            Arena.Center = Grynewaht.CircleBounds.Center;
        }
    }
}

class AutoReaperAI(WorldState ws) : QuestBattle.UnmanagedRotation(ws, 10f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (Player.MountId != 103u || primaryTarget == null)
            return;
        var pos = primaryTarget.PosRot.XYZ();
        UseAction(Roleplay.AID.DiffractiveMagitekCannon, primaryTarget, targetPos: pos);
        UseAction(Roleplay.AID.HighPoweredMagitekCannon, primaryTarget, -5f);
        UseAction(Roleplay.AID.MagitekCannon, primaryTarget, -10f, pos);
    }
}

class ReaperAI(BossModule module) : QuestBattle.RotationModule<AutoReaperAI>(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID == (uint)OID.ImperialColossus ? 5 : e.Actor.TargetID == actor.InstanceID ? 1 : 0;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

class GrynewahtStates : StateMachineBuilder
{
    public GrynewahtStates(Grynewaht module) : base(module)
    {
        State build(uint id) => SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<AugmentedUprising>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<Overpower>()
            .ActivateOnEnter<Heartstopper>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandStrike>()
            .ActivateOnEnter<ShrapnelShell>()
            .ActivateOnEnter<Firebomb>()
            .ActivateOnEnter<MagitekMissiles>();
        SimplePhase(1u, id => build(id).ActivateOnEnter<Bounds>(), "P1")
            .Raw.Update = () => Module.Enemies((uint)OID.GrynewahtP2).Count != 0;
        DeathPhase(2u, id => build(id).ActivateOnEnter<ReaperAI>())
            .Raw.Update = () => module.BossP2()?.IsDeadOrDestroyed ?? false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 67894, NameID = 5576)]
public class Grynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, default, hexBounds)
{
    private static readonly ArenaBoundsCustom hexBounds = new([new Polygon(default, 10.675f, 6, 30f.Degrees())]);
    public static readonly ArenaBoundsCustom CircleBounds = new([new Polygon(new(-0.092f, 0.101f), 19.5f, 20)]);

    private Actor? _bossP2;
    public Actor? BossP2() => _bossP2;
    private static readonly uint[] adds = [(uint)OID.ImperialHoplomachus, (uint)OID.ImperialLaquearius, (uint)OID.ImperialEques, (uint)OID.ImperialSecutor,
    (uint)OID.ImperialSignifer, (uint)OID.ImperialSagittarius, (uint)OID.ImperialColossus];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }

    protected override void UpdateModule()
    {
        _bossP2 ??= GetActor((uint)OID.GrynewahtP2);
    }
}
