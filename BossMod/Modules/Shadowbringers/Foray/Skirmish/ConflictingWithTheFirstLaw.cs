namespace BossMod.Shadowbringers.Foray.Skirmish.ConflictingWithTheFirstLaw;

public enum OID : uint
{
    Boss = 0x2EA3, // R3.15
    FourthLegionNimrod = 0x2E19, // R1.5
    FourthLegionDeathClaw = 0x2E1C, // R1.5
    InkClaw = 0x2E24, // R5.0
    NimrodEscort = 0x2F71, // R1.5
    BozjanMatamata = 0x2E1B, // R4.05
    FourthLegionSlasher = 0x2E17, // R1.05
    FourthLegionRoader = 0x2E1D, // R1.92
    BozjanGeshunpest = 0x2E28, // R1.5
    BozjanWraith = 0x2E29, // R2.5
    WaterSprite = 0x2E27, // R0.8
    LightningSprite = 0x2E32, // R0.8
    Helper = 0x115F
}

public enum AID : uint
{
    AutoAttack = 21264, // Boss->player, no cast, single-target
    AutoAttackAdd1 = 21265, // FourthLegionNimrod/NimrodEscort->player no cast, single-target
    AutoAttackAdd2 = 21260, // FourthLegionSlasher/InkClaw->player, no cast, single-target
    AutoAttackAdd3 = 21264, // BozjanMatamata/BozjanWraith/->player, no cast, single-target
    AutoAttackAdd4 = 21261, // FourthLegionRoader->player, no cast, single-target
    AutoAttackAdd5 = 21263, // FourthLegionDeathClaw/BozjanGeshunpest/2E37->player, no cast, single-target
    Wheel = 21242, // FourthLegionRoader->player, no cast, single-target
    UnbreakableCermetScythe = 21240, // FourthLegionSlasher->player, no cast, single-target
    Water = 21165, // WaterSprite->player, no cast, single-target

    Compress = 21315, // Boss->self, 2.5s cast, range 100 width 7 rect
    Pulverize = 21314, // Boss->self, 6.0s cast, range 15 circle
    Accelerate = 21316, // Boss->player, 8.0s cast, range 6 circle, stack

    TheHand = 21241, // FourthLegionDeathClaw->self, 3.0s cast, range 8 120-degree cone
    CannonShot = 21243, // FourthLegionNimrod/NimrodEscort->location, 4.5s cast, range 12 circle
    Darkness = 21167, // BozjanGeshunpest->location, 3.0s cast, range 5 circle
    AccursedPox = 21180, // BozjanWraith->location, 3.0s cast, range 8 circle
    EmbalmingEarth = 21166 // BozjanMatamata->self, 4.0s cast, range 10 circle
}

sealed class Compress(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Compress, new AOEShapeCross(50f, 3.5f));
sealed class Accelerate(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Accelerate, 6f, 8);
sealed class Pulverize(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pulverize, 15f);

sealed class CannonShot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CannonShot, 12f);
sealed class EmbalmingEarth(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EmbalmingEarth, 10f);
sealed class AccursedPox(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AccursedPox, 8f);
sealed class Darkness(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Darkness, 5f);
sealed class TheHand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheHand, new AOEShapeCone(8f, 60f.Degrees()));

sealed class ConflictingWithTheFirstLawStates : StateMachineBuilder
{
    public ConflictingWithTheFirstLawStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Compress>()
            .ActivateOnEnter<Accelerate>()
            .ActivateOnEnter<Pulverize>()
            .ActivateOnEnter<EmbalmingEarth>()
            .ActivateOnEnter<AccursedPox>()
            .ActivateOnEnter<Darkness>()
            .ActivateOnEnter<TheHand>()
            .ActivateOnEnter<CannonShot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 735, NameID = 1605)]
public sealed class ConflictingWithTheFirstLaw(WorldState ws, Actor primary) : SimpleBossModule(ws, primary)
{
    private static readonly uint[] adds = [(uint)OID.FourthLegionRoader, (uint)OID.FourthLegionNimrod, (uint)OID.FourthLegionSlasher, (uint)OID.FourthLegionDeathClaw,
    (uint)OID.InkClaw, (uint)OID.BozjanMatamata, (uint)OID.LightningSprite, (uint)OID.NimrodEscort, (uint)OID.WaterSprite, (uint)OID.BozjanGeshunpest, (uint)OID.BozjanWraith];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(adds));
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
