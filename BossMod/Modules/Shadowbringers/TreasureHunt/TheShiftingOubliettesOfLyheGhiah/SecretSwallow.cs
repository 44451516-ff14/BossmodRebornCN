namespace BossMod.Shadowbringers.TreasureHunt.ShiftingOubliettesOfLyheGhiah.SecretSwallow;

public enum OID : uint
{
    Boss = 0x302B, //R=4.0
    SwallowHatchling = 0x302C, //R=2.0
    KeeperOfKeys = 0x3034, // R3.23
    FuathTrickster = 0x3033, // R0.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/SwallowHatchling->player, no cast, single-target
    AutoAttack2 = 872, // KeeperOfKeys->player, no cast, single-target

    ElectricWhorl = 21720, // Boss->self, 4.5s cast, range 8-60 donut
    Hydrocannon = 21712, // Boss->self, no cast, single-target
    Hydrocannon2 = 21766, // Helper->location, 3.0s cast, range 8 circle
    Ceras = 21716, // Boss->player, 4.0s cast, single-target, applies poison
    SeventhWave = 21719, // Boss->self, 4.5s cast, range 11 circle
    BodySlam = 21718, // Boss->location, 4.0s cast, range 10 circle, knockback 20, away from source
    PrevailingCurrent = 21717, // SwallowHatchling->self, 3.0s cast, range 22+R width 6 rect

    Telega = 9630, // KeeperOfKeys/FuathTrickster->self, no cast, single-target, bonus adds disappear
    Mash = 21767, // KeeperOfKeys->self, 3.0s cast, range 13 width 4 rect
    Inhale = 21770, // KeeperOfKeys->self, no cast, range 20 120-degree cone, attract 25 between hitboxes, shortly before Spin
    Spin = 21769, // KeeperOfKeys->self, 4.0s cast, range 11 circle
    Scoop = 21768 // KeeperOfKeys->self, 4.0s cast, range 15 120-degree cone
}

class ElectricWhorl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeventhWave, 11f);
class PrevailingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrevailingCurrent, new AOEShapeRect(24f, 3f));
class SeventhWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricWhorl, new AOEShapeDonut(8f, 60f));
class Hydrocannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrocannon2, 8f);
class Ceras(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Ceras);
class BodySlam(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BodySlam, 10f);
class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
class Mash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mash, new AOEShapeRect(13f, 2f));
class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));

class SecretSwallowStates : StateMachineBuilder
{
    public SecretSwallowStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricWhorl>()
            .ActivateOnEnter<PrevailingCurrent>()
            .ActivateOnEnter<SeventhWave>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Ceras>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Mash>()
            .ActivateOnEnter<Scoop>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(SecretSwallow.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 745, NameID = 9782)]
public class SecretSwallow(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.FuathTrickster, (uint)OID.KeeperOfKeys];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.SwallowHatchling, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.SwallowHatchling));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FuathTrickster => 3,
                (uint)OID.KeeperOfKeys => 2,
                (uint)OID.SwallowHatchling => 1,
                _ => 0
            };
        }
    }
}
