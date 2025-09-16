namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultWindSpirit;

public enum OID : uint
{
    VaultAgavoides = 0x489B, // R1.5
    VaultTree = 0x489C, // R1.6
    VaultWindSpirit = 0x489D, // R2.4

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GoldyCat = 0x48B7, // R1.87
    Vaultkeeper = 0x48B8 // R2.0
}

public enum AID : uint
{
    AutoAttack1 = 872, // VaultAgavoides/VaultTree/VaultWindSpirit->player, no cast, single-target

    Leafcutter = 43620, // VaultAgavoides->self, 3.6s cast, range 14 width 4 rect
    Entangle = 43621, // VaultTree->location, 3.0s cast, range 4 circle
    Stump = 43622, // VaultTree->self, 3.0s cast, range 6 circle
    Gust = 43624, // VaultWindSpirit->location, 3.0s cast, range 6 circle
    WhirlingGaol = 43626, // VaultWindSpirit->self, 5.0s cast, range 40 circle, raidwide
    Whipwind = 43625, // VaultWindSpirit->self, 6.0s cast, range 60 width 40 rect
    ChangelessWinds = 43623, // VaultWindSpirit->self/player, 5.0s cast, range 40 width 8 rect, tankbuster

    AutoAttack2 = 871, // Vaultkeeper->player, no cast, single-target
    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class Leafcutter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Leafcutter, new AOEShapeRect(14f, 2f));
sealed class Entangle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Entangle, 4f);
sealed class StumpGust(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Stump, (uint)AID.Gust], 6f);
sealed class Whipwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Whipwind, new AOEShapeRect(60f, 20f));
sealed class ChangelessWinds(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ChangelessWinds, new AOEShapeRect(40f, 4f), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster, endsOnCastEvent: true);
sealed class WhirlingGaol(BossModule module) : Components.RaidwideCast(module, (uint)AID.WhirlingGaol);

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);
sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class VaultWindSpiritStates : StateMachineBuilder
{
    public VaultWindSpiritStates(VaultWindSpirit module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Leafcutter>()
            .ActivateOnEnter<Entangle>()
            .ActivateOnEnter<StumpGust>()
            .ActivateOnEnter<Whipwind>()
            .ActivateOnEnter<ChangelessWinds>()
            .ActivateOnEnter<WhirlingGaol>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .Raw.Update = () => AllDestroyed(VaultWindSpirit.All) && (module.BossWindSpirit?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultAgavoides, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13989u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 7)]
public sealed class VaultWindSpirit(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    private static readonly uint[] trash = [(uint)OID.VaultAgavoides, (uint)OID.VaultTree];
    public static readonly uint[] All = [.. trash, .. bonusAdds];

    public Actor? BossWindSpirit;

    protected override void UpdateModule()
    {
        BossWindSpirit ??= GetActor((uint)OID.VaultWindSpirit);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossWindSpirit);
        var m = this;
        Arena.Actors(m, trash);
        Arena.Actors(m, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VaultOnion => 6,
                (uint)OID.VaultEggplant => 5,
                (uint)OID.VaultGarlic => 4,
                (uint)OID.VaultTomato => 3,
                (uint)OID.VaultQueen => 2,
                (uint)OID.Vaultkeeper => 1,
                _ => 0
            };
        }
    }
}

