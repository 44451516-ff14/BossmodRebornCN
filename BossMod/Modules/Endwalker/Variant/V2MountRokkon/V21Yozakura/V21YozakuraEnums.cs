namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V21Yozakura;

public enum OID : uint
{
    Yozakura = 0x3EDB, // R3.45
    ShishuYamabiko = 0x3F00, // R0.8
    MirroredYozakura = 0x3EDC, // R3.45
    MudVoidzone = 0x1EB907, // R0.5
    MudBubble = 0x3EDD, // R4.0
    Kuromaru = 0x3EDF, // R0.4
    Shiromaru = 0x3EE0, // R0.4
    Shibamaru = 0x3EDE, // R0.4
    AccursedSeedling = 0x3EE1, // R0.75
    AutumnalTempest = 0x3EE3, // R0.8
    Wind = 0x1EB88C, // R0.5
    Thunder = 0x1EB88E, // R0.5
    Water = 0x1EB88D, // R0.5
    Fire = 0x1EB88B, // R0.5
    LivingGaol = 0x3EE2, // R3.7
    Helper2 = 0x3F53,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6497, // Yozakura->player, no cast, single-target
    Teleport = 34322, // Yozakura->location, no cast, single-target

    GloryNeverlasting = 33705, // Yozakura->player, 5.0s cast, single-target, tankbuster

    ArtOfTheFireblossom = 33640, // Yozakura->self, 3.0s cast, range 9 circle
    ArtOfTheWindblossom = 33641, // Yozakura->self, 5.0s cast, range 5-60 donut

    KugeRantsui = 33706, // Yozakura->self, 5.0s cast, range 60 circle, raidwide
    OkaRanman = 33646, // Yozakura->self, 5.0s cast, range 60 circle, raidwide

    SealOfRiotousBloom = 33652, // Yozakura->self, 5.0s cast, single-target
    SealOfTheWindblossomVisual = 33659, // Yozakura->location, no cast, single-target
    SealOfTheFireblossomVisual = 33658, // Yozakura->location, no cast, single-target
    SealOfTheRainblossomVisual = 33661, // Yozakura->location, no cast, single-target
    SealOfTheLevinblossomVisual = 33660, // Yozakura->location, no cast, single-target
    SealOfTheWindblossom = 33654, // Helper->self, 2.0s cast, range 5-60 donut
    SealOfTheFireblossom = 33653, // Helper->self, 2.0s cast, range 9 circle
    SealOfTheRainblossom = 33655, // Helper->self, 2.0s cast, range 70 45-degree cone, four cone AoEs from Yozakura's hitbox in the intercardinal directions
    SealOfTheLevinblossom = 33656, // Helper->self, 1.8s cast, range 70 45-degree cone, four cone AoEs from Yozakura's hitbox in the cardinal directions

    SealOfTheFleeting = 33657, // Yozakura->self, 3.0s cast, single-target, Yozakura tethers to the petal piles

    SeasonsOfTheFleetingVisual1 = 33665, // Yozakura->self, 10.0s cast, single-target, telegraph four sequential AoEs
    SeasonsOfTheFleetingVisual2 = 33666, // Yozakura->self, no cast, single-target
    FireAndWaterTelegraph = 33667, // Helper->self, 2.0s cast, range 46 width 5 rect
    EarthAndLightningTelegraph = 33668, // Helper->self, 2.0s cast, range 60 45-degree cone

    SeasonOfFire = 33669, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfWater = 33670, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfLightning = 33671, // Helper->self, 0.8s cast, range 70 45-degree cone
    SeasonOfEarth = 33672, // Helper->self, 0.8s cast, range 70 45-degree cone

    // Left Windy
    WindblossomWhirlVisual = 33679, // Yozakura->self, 3.0s cast, single-target
    WindblossomWhirl1 = 33680, // Helper->self, 5.0s cast, range 5-60 donut
    WindblossomWhirl2 = 34544, // Helper->self, 3.0s cast, range 5-60 donut
    LevinblossomStrikeVisual = 33681, // Yozakura->self, 2.3s cast, single-target
    LevinblossomStrike = 33682, // Helper->location, 3.0s cast, range 3 circle
    DriftingPetals = 33683, // Yozakura->self, 5.0s cast, range 60 circle, knockback

    // Left Rainy
    Bunshin = 33662, // Yozakura->self, 5.0s cast, single-target
    ShadowflightVisual = 33663, // Yozakura->self, 3.0s cast, single-target
    Shadowflight = 33664, // MirroredYozakura->self, 2.5s cast, range 10 width 6 rect
    MudrainVisual = 33673, // Yozakura->self, 3.0s cast, single-target
    Mudrain = 33674, // Helper->location, 3.8s cast, range 5 circle
    IcebloomVisual = 33675, // Yozakura->self, 3.0s cast, single-target
    Icebloom = 33676, // Helper->location, 3.0s cast, range 6 circle
    MudPieVisual = 33677, // Yozakura->self, 3.0s cast, single-target
    MudPie = 33678, // MudBubble->self, 4.0s cast, range 60 width 6 rect

    // Middle Rope Pulled
    ArtOfTheFluff1 = 33693, // Shibamaru/Kuromaru->self, 6.5s cast, range 60 circle, gaze
    ArtOfTheFluff2 = 33694, // Shiromaru->self, 6.5s cast, range 60 circle, gaze

    FireblossomFlareVisual = 33695, // Yozakura->self, 3.0s cast, single-target
    FireblossomFlare = 33696, // Helper->location, 3.0s cast, range 6 circle

    DondenGaeshi = 33692, // Yozakura->self, 3.0s cast, single-target, indicates platforms
    SilentWhistle = 33691, // Yozakura->self, 3.0s cast, single-target, dog summons

    // Middle Rope Unpulled
    LevinblossomLanceCW = 33687, // Yozakura->self, 5.0s cast, single-target
    LevinblossomLanceCCW = 33688, // Yozakura->self, 5.0s cast, single-target
    LevinblossomLanceFirst = 33689, // Helper->self, 5.8s cast, range 60 width 7 rect
    LevinblossomLanceRest = 33690, // Helper->self, no cast, range 60 width 7 rect

    TatamiTrap = 33684, // Yozakura->self, 3.0s cast, single-target
    TatamiGaeshiVisual = 33685, // Yozakura->self, 3.0s cast, single-target
    TatamiGaeshi = 33686, // Helper->self, 3.8s cast, range 40 width 10 rect

    // Right No Dogu
    Mebuki = 33697, // Yozakura->self, 3.0s cast, single-target
    RockRootArrangementVisual = 33700, // Yozakura->self, 5.0s cast, single-target
    RockRootArrangementFirst = 33701, // Helper->location, 3.0s cast, range 4 circle
    RockRootArrangementRest = 33702, // Helper->location, no cast, range 4 circle
    BehindBarbs = 33698, // AccursedSeedling->player, no cast, single-target
    Explosion = 33699, // LivingGaol->player, 15.0s cast, single-target, seedling turns into prison if touched, explodes after 15s if not killed

    // Right Dogu
    Witherwind = 33703, // Yozakura->self, 3.0s cast, single-target
    CuttingLeaves = 33704 // Helper->player, no cast, single-target, touch whirlwind
}

public enum IconID : uint
{
    RootArrangement = 197 // player
}
