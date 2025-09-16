namespace BossMod.Shadowbringers.Foray.Skirmish.HeavyBootsOfLead;

public enum OID : uint
{
    Boss = 0x2EA5, // R3.15
    ForthLegionGunship = 0x2E3F, // R3.6
    BozjanDoblyn = 0x2E37, // R1.65
    ForthLegionAvenger = 0x2E3B, // R2.2
    BozjanSabotender = 0x2E38, // R1.0
    FourthLegionVanguard = 0x2E36, // R2.1
    Viy = 0x2E4B, // R4.8
    MagitekBit = 0x2FCE, // R0.66
    Helper = 0x115F
}

public enum AID : uint
{
    AutoAttackBoss = 21264, // Boss->player, no cast, single-target

    MagitekBit = 21518, // Boss->self, 3.0s cast, single-target
    Compress = 21520, // Boss->self, 3.0s cast, range 50 width 7 cross
    MagitekLaser = 21523, // MagitekBit->self, 3.0s cast, range 75 width 4 rect
    Pulverize = 21519, // Boss->self, 6.0s cast, range 15 circle
    Dispose = 21522, // Boss->self, 5.0s cast, range 40 60-degree cone
    Ventilate = 21538, // Boss->self, 5.0s cast, range 8 circle
    Accelerate = 21521, // Boss->players, 8.0s cast, range 6 circle, stack

    AutoAttackAdd1 = 21263, // BozjanDoblyn->player, no cast, single-target
    AutoAttackAdd2 = 21263, // ForthLegionGunship->player, no cast, single-target
    Shatter = 21188, // BozjanDoblyn->self, 4.0s cast, range 6 circle
    GarleanFire = 21246 // ForthLegionGunship->location, 3.0s cast, range 5 circle
}

sealed class MagitekLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekLaser, new AOEShapeRect(75f, 2f));
sealed class Pulverize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pulverize, 15f);
sealed class Ventilate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ventilate, 8f);
sealed class Compress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Compress, new AOEShapeCross(50f, 3.5f));
sealed class Accelerate(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Accelerate, 6f, 8);
sealed class Dispose(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Dispose, new AOEShapeCone(40f, 30f.Degrees()));

sealed class Shatter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shatter, 6f);
sealed class GarleanFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GarleanFire, 5f);

sealed class HeavyBootsOfLeadStates : StateMachineBuilder
{
    public HeavyBootsOfLeadStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitekLaser>()
            .ActivateOnEnter<Pulverize>()
            .ActivateOnEnter<Ventilate>()
            .ActivateOnEnter<Compress>()
            .ActivateOnEnter<Accelerate>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<Dispose>()
            .ActivateOnEnter<GarleanFire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 735, NameID = 1612)]
public sealed class HeavyBootsOfLead(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    private static readonly uint[] adds = [(uint)OID.BozjanDoblyn, (uint)OID.BozjanSabotender, (uint)OID.FourthLegionVanguard, (uint)OID.ForthLegionGunship,
    (uint)OID.Viy];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
