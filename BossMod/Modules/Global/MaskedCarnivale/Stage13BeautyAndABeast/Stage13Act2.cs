namespace BossMod.Global.MaskedCarnivale.Stage13.Act2;

public enum OID : uint
{
    Boss = 0x26F8, // R=2.0
    Succubus = 0x26F7, //R=1.0
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 6497, // Boss/Succubus->player, no cast, single-target
    VoidFireII = 14880, // Boss->location, 3.0s cast, range 5 circle
    VoidAero = 14881, // Boss->self, 3.0s cast, range 40+R width 8 rect
    DarkSabbath = 14951, // Boss->self, 3.0s cast, range 60 circle, gaze
    DarkMist = 14884, // Boss->self, 3.0s cast, range 8+R circle
    CircleOfBlood = 15043, // Boss->self, 3.0s cast, single-target
    CircleOfBlood2 = 14887, // Helper->self, 3.0s cast, range 10-20 donut
    VoidFireIV = 14888, // Boss->location, 4.0s cast, range 10 circle
    VoidFireIV2 = 14886, // Boss->self, no cast, single-target
    VoidFireIV3 = 14889, // Helper->location, 3.0s cast, range 6 circle
    SummonDarkness = 14885, // Boss->self, no cast, single-target, summons succubus add
    BeguilingMist = 15045, // Succubus->self, 7.0s cast, range 50+R circle, interruptable, applies hysteria
    FatalAllure = 14952, // Boss->self, no cast, range 50+R circle, attract, applies terror
    BloodRain = 14882 // Boss->location, 3.0s cast, range 50 circle
}

class VoidFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireII, 5f);
class VoidFireIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireIV, 10f);
class VoidFireIV3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidFireIV3, 6f);
class VoidAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidAero, new AOEShapeRect(42f, 4f));
class DarkSabbath(BossModule module) : Components.CastGaze(module, (uint)AID.DarkSabbath);
class DarkMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkMist, 10f);
class CircleOfBlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CircleOfBlood2, new AOEShapeDonut(10f, 20f));
class BeguilingMist(BossModule module) : Components.CastInterruptHint(module, (uint)AID.BeguilingMist);
class BloodRain(BossModule module) : Components.RaidwideCast(module, (uint)AID.BloodRain, "Harmless raidwide unless you failed to kill succubus in time");

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will cast various AOEs and summons adds.\nInterrupt the adds with Flying Sardine and kill them fast.\nIf the add is still alive during the next Black Sabbath, you will be wiped.");
    }
}

class Stage13Act2States : StateMachineBuilder
{
    public Stage13Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidFireII>()
            .ActivateOnEnter<VoidFireIV>()
            .ActivateOnEnter<VoidFireIV3>()
            .ActivateOnEnter<VoidAero>()
            .ActivateOnEnter<DarkSabbath>()
            .ActivateOnEnter<DarkMist>()
            .ActivateOnEnter<CircleOfBlood>()
            .ActivateOnEnter<BeguilingMist>()
            .ActivateOnEnter<BloodRain>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 623, NameID = 8107, SortOrder = 2)]
public class Stage13Act2 : BossModule
{
    public Stage13Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Succubus), Colors.Object);
    }
}
