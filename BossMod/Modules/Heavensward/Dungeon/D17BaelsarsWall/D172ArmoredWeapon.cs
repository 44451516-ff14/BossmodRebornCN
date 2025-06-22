namespace BossMod.Heavensward.Dungeon.D17BaelsarsWall.D172ArmoredWeapon;

public enum OID : uint
{
    Boss = 0x193A, // R5.400, x1
    MagitekSlasher = 0x193C, // R1.05
    MagitekBit = 0x193B, // R0.9
    Helper2 = 0x19A,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 7351, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // MagitekSlasher->player, no cast, single-target
    Teleport = 7359, // MagitekBit->location, no cast, ???

    MagitekCannon = 7352, // Boss->player, no cast, single-target
    Launcher = 7356, // Boss->self, 3.0s cast, range 80+R circle, raidwide
    DynamicSensoryJammer = 7353, // Boss->self, 3.0s cast, range 80+R circle, applies extreme caution
    DynamicSensoryJammerFail = 7354, // Helper2->player, no cast, single-target, extreme caution fail

    DiffractiveLaserVisual = 31352, // Helper->self, 2.0s cast, range 5 circle
    DiffractiveLaser1 = 31469, // Boss->location, 4.0s cast, range 5 circle
    DiffractiveLaser2 = 7355, // Boss->location, 4.0s cast, range 5 circle
    DistressBeacon = 7358, // Boss->self, 3.0s cast, single-target

    MagitekBit = 7357, // Boss->self, 3.0s cast, single-target
    AssaultCannon = 7360 // MagitekBit->self, 4.0s cast, range 40+R width 2 rect
}

public enum SID : uint
{
    ExtremeCaution = 1132 // none->player, extra=0x0
}

class Launcher(BossModule module) : Components.RaidwideCast(module, (uint)AID.Launcher);
class AssaultCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultCannon, new AOEShapeRect(40.94f, 1f));

class DiffractiveLaser(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.DiffractiveLaser1, (uint)AID.DiffractiveLaser2], 5f);

class DynamicSensoryJammer(BossModule module) : Components.StayMove(module, 3f)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ExtremeCaution && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class D172ArmoredWeaponStates : StateMachineBuilder
{
    public D172ArmoredWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DynamicSensoryJammer>()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<Launcher>()
            .ActivateOnEnter<AssaultCannon>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 219, NameID = 5564)]
public class D172ArmoredWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, new(116, 0), new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MagitekSlasher));
    }
}
