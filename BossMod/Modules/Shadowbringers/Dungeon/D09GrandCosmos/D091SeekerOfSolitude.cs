namespace BossMod.Shadowbringers.Dungeon.D09GrandCosmos.D091SeekerOfSolitude;

public enum OID : uint
{
    Boss = 0x2C1A, // R2.0
    MagickedBroom = 0x2C1B, // R3.125
    SweepVoidzone = 0x1EAEAE, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 18280, // Boss->player, no cast, single-target

    Shadowbolt = 18281, // Boss->player, 4.0s cast, single-target
    ImmortalAnathema = 18851, // Boss->self, 4.0s cast, range 60 circle

    TribulationVisual = 18283, // Boss->self, 3.0s cast, single-target
    Tribulation = 18852, // Helper->location, 3.0s cast, range 3 circle

    Sweep = 18288, // Helper->player, no cast, single-target

    DarkShockVisual = 18286, // Boss->self, 3.0s cast, single-target
    DarkShock = 18287, // Helper->location, 3.0s cast, range 6 circle
    DeepClean = 18289, // Helper->player, no cast, single-target
    DarkPulse = 18282, // Boss->players, 5.0s cast, range 6 circle, stack
    DarkWellVisual = 18284, // Boss->self, no cast, single-target
    DarkWell = 18285, // Helper->player, 5.0s cast, range 5 circle, spread
    MovementMagick = 18713 // Boss->self, 3.0s cast, single-target
}

class ImmortalAnathema(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ImmortalAnathema));
class Shadowbolt(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Shadowbolt));
class Tribulation(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.Tribulation), m => m.Enemies(OID.SweepVoidzone).Where(z => z.EventState != 7), 0.1f);
class DarkPulse(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DarkPulse), 6, 4, 4);
class DarkWell(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.DarkWell), 5);
class DarkShock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DarkShock), 6);
class MagickedBroom(BossModule module) : Components.PersistentVoidzone(module, 3.125f, m => m.Enemies(OID.MagickedBroom).Where(x => x.ModelState.AnimState1 == 1), 10);

class D091SeekerOfSolitudeStates : StateMachineBuilder
{
    public D091SeekerOfSolitudeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ImmortalAnathema>()
            .ActivateOnEnter<Shadowbolt>()
            .ActivateOnEnter<Tribulation>()
            .ActivateOnEnter<DarkPulse>()
            .ActivateOnEnter<DarkWell>()
            .ActivateOnEnter<DarkShock>()
            .ActivateOnEnter<MagickedBroom>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 692, NameID = 9041)]
public class D091SeekerOfSolitude(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 187), new ArenaBoundsRect(20.5f, 14.5f));
