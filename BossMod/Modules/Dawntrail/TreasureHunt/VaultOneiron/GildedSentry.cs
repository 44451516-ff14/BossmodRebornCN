namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.GildedSentry;

public enum OID : uint
{
    GildedSentry = 0x48B0, // R3.0
    Vaultkeeper = 0x48B8, // R2.0
    VaultLaserTurret = 0x48B1, // R1.2
    GoldyCat = 0x48B7, // R1.87
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // GildedSentry/Vaultkeeper->player, no cast, single-target

    CrossLightningVisual = 43718, // GildedSentry->self, 3.0s cast, single-target
    CrossLightning = 43719, // Helper->self, 5.0s cast, range 50 width 10 cross
    OrderToCharge = 43724, // GildedSentry->self, 3.0s cast, single-target
    Electray = 43728, // VaultLaserTurret->self, 2.5s cast, range 75 width 4 rect
    AlexandrianThunderVisual = 43720, // GildedSentry->self, 2.5+0,5s cast, single-target
    AlexandrianThunder = 43721, // Helper->location, 3.0s cast, range 6 circle
    SkeweringLance = 43725, // GildedSentry->player, 5.0s cast, single-target, tankbuster
    AlexandrianThunderIIIVisual = 43722, // GildedSentry->self, 4.5+0,5s cast, single-target
    AlexandrianThunderIII = 43723, // Helper->location, 5.0s cast, range 35 circle, raidwide

    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class AlexandrianThunder(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AlexandrianThunder, 6f);
sealed class CrossLightning(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossLightning, new AOEShapeCross(50f, 5f));
sealed class Electray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray, new AOEShapeRect(75f, 2f));
sealed class SkeweringLance(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SkeweringLance);
sealed class AlexandrianThunderIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.AlexandrianThunderIII);

sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class GildedSentryStates : StateMachineBuilder
{
    public GildedSentryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AlexandrianThunder>()
            .ActivateOnEnter<CrossLightning>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<SkeweringLance>()
            .ActivateOnEnter<AlexandrianThunderIII>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .Raw.Update = () => AllDeadOrDestroyed(GildedSentry.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.GildedSentry, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14007u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 10)]
public sealed class GildedSentry(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GoldyCat, (uint)OID.Vaultkeeper];
    public static readonly uint[] All = [(uint)OID.GildedSentry, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.GoldyCat => 2,
                (uint)OID.Vaultkeeper => 1,
                _ => 0
            };
        }
    }
}
