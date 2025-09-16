namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultFluidSoul;

public enum OID : uint
{
    VaultTorbalan = 0x4895, // R1.5
    VaultWaterSpirit = 0x4896, // R2.16
    VaultFluidSoul = 0x4897, // R2.5

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
    AutoAttack1 = 872, // VaultTorbalan/VaultFluidSoul->player, no cast, single-target
    AutoAttack2 = 43604, // VaultWaterSpirit->player, no cast, single-target

    Batter = 43602, // VaultTorbalan->self, 3.0s cast, range 6 circle
    TwistedPool = 43603, // VaultTorbalan->self, 5.0s cast, range 6-40 donut
    Flood = 43605, // VaultWaterSpirit->self, 3.0s cast, range 8 circle
    WaterIII = 43606, // VaultWaterSpirit->location, 4.0s cast, range 8 circle
    ProteanWave = 43609, // VaultFluidSoul->self, 4.0s cast, range 39 30-degree cone
    Splash = 43608, // VaultFluidSoul->self, 5.0s cast, range 80 circle, raidwide
    FluidSwing = 43607, // VaultFluidSoul->self/players, 5.0s cast, range 13 90-degree cone, tankbuster
    BrineBomb = 43610, // VaultFluidSoul->location, 3.0s cast, range 5 circle

    AutoAttack3 = 871, // Vaultkeeper->player, no cast, single-target
    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class FluidSwing(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FluidSwing, new AOEShapeCone(13f, 45f.Degrees()), tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster, endsOnCastEvent: true);
sealed class Batter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Batter, 6f);
sealed class TwistedPool(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwistedPool, new AOEShapeDonut(6f, 40f));
sealed class BrineBomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BrineBomb, 5f);
sealed class WaterIIIFlood(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WaterIII, (uint)AID.Flood], 8f);
sealed class ProteanWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProteanWave, new AOEShapeCone(39f, 15f.Degrees()));
sealed class Splash(BossModule module) : Components.RaidwideCast(module, (uint)AID.Splash);

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);
sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class VaultFluidSoulStates : StateMachineBuilder
{
    public VaultFluidSoulStates(VaultFluidSoul module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<Batter>()
            .ActivateOnEnter<TwistedPool>()
            .ActivateOnEnter<BrineBomb>()
            .ActivateOnEnter<WaterIIIFlood>()
            .ActivateOnEnter<ProteanWave>()
            .ActivateOnEnter<Splash>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .Raw.Update = () => AllDestroyed(VaultFluidSoul.All) && (module.BossFluidSoul?.IsDeadOrDestroyed ?? true);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.VaultTorbalan, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 13983u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 13)]
public sealed class VaultFluidSoul(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    private static readonly uint[] trash = [(uint)OID.VaultTorbalan, (uint)OID.VaultWaterSpirit];
    public static readonly uint[] All = [.. trash, .. bonusAdds];

    public Actor? BossFluidSoul;

    protected override void UpdateModule()
    {
        BossFluidSoul ??= GetActor((uint)OID.VaultFluidSoul);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossFluidSoul);
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
                (uint)OID.VaultQueen or (uint)OID.GoldyCat => 2,
                (uint)OID.Vaultkeeper => 1,
                _ => 0
            };
        }
    }
}
