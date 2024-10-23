namespace BossMod.Dawntrail.Quest.RoleQuests.AHunterTrue;

public enum OID : uint
{
    Boss = 0x41FE, // R1.5-5.4
    Garula = 0x41FF, // R4.0
    Dzo = 0x4200, // R1.92
    SteppeEagle = 0x4201, // R2.52
    FilthyShackle = 0x4204, // R1.0
    BallOfFire = 0x4202, // R1.0
    BallOfNaught = 0x4203, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 28534, // Boss->player/Kuiyki, no cast, single-target
    AutoAttack2 = 6497, // Boss->Kuiyki, no cast, single-target
    AutoAttack3 = 6498, // SteppeEagle->player, no cast, single-target
    AutoAttack4 = 6499, // Dzo/Garula->player, no cast, single-target
    Teleport1 = 37048, // Boss->location, no cast, single-target
    Teleport2 = 39280, // Boss->location, no cast, single-target

    DreamweaveCocoon1 = 37067, // Boss->self, 4.0s cast, single-target
    DreamweaveCocoon2 = 37070, // Boss->self, 4.0s cast, single-target
    DreamweaveCocoon3 = 37072, // Boss->self, 4.0s cast, single-target
    DreamweaveCocoonVisual1 = 37073, // Helper->self, 2.5s cast, single-target
    DreamweaveCocoonVisual2 = 37074, // Helper->self, 2.5s cast, single-target

    ScathingSunshot = 37049, // Boss->self, 5.0s cast, range 70 120-degree cone

    DawnlitBoltVisual = 37050, // Boss->self, 3.3s cast, single-target
    DawnlitBoltTelegraph = 37051, // Helper->location, 4.5s cast, range 6 circle
    DawnlitBolt = 37052, // Helper->location, no cast, range 6 circle

    Thunderswarm = 37053, // Boss->self, 3.0s cast, single-target
    RushLineStackMarker = 37054, // Helper->Kuiyki, no cast, single-target
    RushLineStack = 37088, // Garula->location, 5.0s cast, width 8 rect charge
    RushBaitVisual = 37055, // Dzo->self, 8.0s cast, single-target
    RushBait = 37056, // Dzo->players/Kuiyki, no cast, width 8 rect charge
    RushBaitFail = 37057, // Dzo->player, no cast, width 8 rect charge, tether wasn't stretched enough

    SonicStormVisual = 37058, // SteppeEagle->player/Kuiyki, 6.0s cast, single-target
    SonicStorm = 37059, // Helper->player/Kuiyki, 6.5s cast, range 6 circle, spread

    TheOneRulerVisual = 37068, // Boss->self, 4.0s cast, single-target
    TheOneRuler = 37069, // Helper->player/Kuiyki, 4.5s cast, single-target, chains targets, quick time event follows

    FallingDusk = 37071, // Boss->location, 20.0s cast, range 20 circle, damage fall off aoe

    DancingWind = 37075, // Boss->self, 5.0s cast, range 40 circle, pull 8 between centers
    TrampleVisual = 37076, // Boss->self, 0.5s cast, single-target
    Trample = 37077, // Helper->self, 8.2s cast, range 15 circle
    FoxflareVisual = 37078, // Boss->self, 3.0s cast, single-target
    Foxflare = 37079, // BallOfFire->self, 4.5s cast, range 50 width 10 rect
    Roar = 38785, // Boss->self, 5.0s cast, range 30 circle, raidwide
    NinefoldCurseVisual = 37081, // Boss->self, 6.0s cast, single-target
    NinefoldCurse = 37082, // Helper->self, 2.0s cast, range 50 circle
    Yoki = 37083, // Boss->self, 3.0s cast, single-target
    BallOfNaughtDeath = 37087, // BallOfNaught->self, no cast, single-target
    NinetyNinefoldCurseVisual = 37084, // Boss->self, 35.0s cast, single-target, limit break phase
    NinetyNinefoldCurse = 37085, // Helper->self, 1.0s cast, range 50 circle
    NinetyNinefoldCurseEnrage = 37086, // Helper->self, 1.0s cast, range 50 circle
    Gnaw = 39244, // Boss->Kuiyki, no cast, single-target
}

public enum TetherID : uint
{
    TetherGood = 17, // Garula/Dzo->Kuiyki/player
    TetherBad = 57 // Dzo->player/Kuiyki
}

public enum IconID : uint
{
    Spreadmarker = 210, // player/Kuiyki
    Bait = 101 // player/Kuiyki
}

class ScathingSunshot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScathingSunshot), new AOEShapeCone(70, 60.Degrees()));
class Foxflare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Foxflare), new AOEShapeRect(50, 5));
class NinefoldCurse(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.NinefoldCurseVisual), ActionID.MakeSpell(AID.NinefoldCurse), 2);
class NinetyNinefoldCurse(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.NinetyNinefoldCurse));
class Roar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Roar));
class SonicStorm(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SonicStorm), 6);
class RushBait(BossModule module) : Components.BaitAwayChargeTether(module, 4, 8.3f, ActionID.MakeSpell(AID.RushBait), ActionID.MakeSpell(AID.RushBaitFail), (uint)TetherID.TetherBad, (uint)TetherID.TetherGood, (uint)OID.Dzo, 30);
class RushLineStack(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.RushLineStackMarker), ActionID.MakeSpell(AID.RushLineStack), 4.9f, markerIsFinalTarget: false);
class Trample(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Trample), new AOEShapeCircle(15));

class FallingDusk(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FallingDusk), 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.Enemies(OID.FilthyShackle).All(x => x.IsDead))
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class DancingWind(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.DancingWind), 8, kind: Kind.TowardsOrigin)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var source = Sources(slot, actor).FirstOrDefault();
        if (source != default)
            hints.AddForbiddenZone(ShapeDistance.Circle(source.Origin, 18.5f), source.Activation);
    }
}

class DawnlitBolt(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DawnlitBoltTelegraph)
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DawnlitBolt)
            _aoes.RemoveAll(x => x.Origin.AlmostEqual(spell.TargetXZ, 1));
    }
}

class Fetters(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.Enemies(OID.FilthyShackle).Any(x => !x.IsDead))
            hints.Add($"Destroy fetters on Kuiyki!");
    }
}

class AHunterTrueStates : StateMachineBuilder
{
    public AHunterTrueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScathingSunshot>()
            .ActivateOnEnter<Foxflare>()
            .ActivateOnEnter<NinefoldCurse>()
            .ActivateOnEnter<NinetyNinefoldCurse>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<SonicStorm>()
            .ActivateOnEnter<DawnlitBolt>()
            .ActivateOnEnter<RushBait>()
            .ActivateOnEnter<RushLineStack>()
            .ActivateOnEnter<Trample>()
            .ActivateOnEnter<FallingDusk>()
            .ActivateOnEnter<DancingWind>()
            .ActivateOnEnter<Fetters>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70371, NameID = 12846)]
public class AHunterTrue(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(4, 248), 19.5f, 20)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Garula).Concat([PrimaryActor]).Concat(Enemies(OID.SteppeEagle)).Concat(Enemies(OID.Dzo)).Concat(Enemies(OID.FilthyShackle)));
    }
}
