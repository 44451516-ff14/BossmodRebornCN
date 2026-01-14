namespace BossMod.Dawntrail.Savage.M12SLindwurm;

public enum OID : uint
{
    Boss = 0x4B02, // R5.0, intargetable
    Lindwurm0 = 0x233C, // R0.5, Helper, intargetable
    Lindwurm1 = 0x4AFB, // R13.8
    Lindwurm2 = 0x4AFC, // R4.0, Helper, intargetable
    Lindwurm3 = 0x4AFD, // R1.0, Helper, intargetable
    Lindwurm4 = 0x4AFE, // R0.0, Part, intargetable
    BloodVessel = 0x4AFF, // R0.8, intargetable
    Lindwurm5 = 0x4B00, // R1.8, intargetable
    Lindwurm6 = 0x4B01, // R1.8, intargetable
    Luzzelwurm = 0x4B03, // R2.1, intargetable
    Lindschrat0 = 0x4B04, // R5.0, intargetable
    Understudy = 0x4B0A, // R1.0, intargetable
    Lindwurm7 = 0x4B11, // R4.0, intargetable
    Lindwurm8 = 0x4B27, // R1.0, Helper, intargetable
    Lindwurm9 = 0x4B28, // R1.0, Helper, intargetable
    Lindschrat1 = 0x4B29, // R7.5, intargetable
    Actor1ea1a1 = 0x1EA1A1, // R0.5, EventObj, intargetable
    Actor1ebf29 = 0x1EBF29, // R0.5, EventObj, intargetable
}

public enum AID : uint
{
    Slaughtershed0 = 44489, // no cast, range 60 circle
    RavenousReach0 = 46185, // 0.7s cast, single-target
    unk_46190 = 46190, // no cast, single-target
    unk_46194 = 46194, // no cast, range 60 circle
    unk_46201 = 46201, // no cast, single-target
    MortalSlayer0 = 46229, // 11.7s cast, single-target
    MortalSlayer1 = 46230, // no cast, range 6 circle
    MortalSlayer2 = 46232, // no cast, range 6 circle
    unk_46234 = 46234, // no cast, single-target
    unk_46235 = 46235, // no cast, single-target
    RavenousReach1 = 46237, // 10.3s cast, range 35 120-degree cone
    PhagocyteSpotlight0 = 46238, // 2.7s cast, range 5 circle
    Burst = 46239, // 2.2s cast, range 12 circle
    GrandEntrance0 = 46240, // 2.7s cast, range 2 circle
    GrandEntrance1 = 46241, // 2.7s cast, range 2 circle
    GrandEntrance2 = 46242, // 2.7s cast, range 2 circle
    GrandEntrance3 = 46243, // 3.2s cast, range 2 circle
    BringDownTheHouse0 = 46244, // 0.7s cast, range 10 width 20 rect
    BringDownTheHouse1 = 46245, // 0.7s cast, range 10 width 15 rect
    BringDownTheHouse2 = 46246, // 0.7s cast, range 10 width 10 rect
    SplitScourge0 = 46247, // no cast, single-target
    VenomousScourge = 46248, // no cast, range 5 circle
    DramaticLysis0 = 46250, // no cast, range 6 circle
    SplitScourge1 = 46251, // no cast, range 60 width 10 rect
    CellShedding = 46252, // no cast, range 6 circle
    FourthWallFusion0 = 46254, // no cast, range 6 circle
    HemorrhagicProjection0 = 46255, // no cast, range 60 ?-degree cone
    DramaticLysis1 = 46256, // no cast, range 9 circle
    Metamitosis0 = 46257, // 1.2s cast, single-target
    UnmitigatedExplosion = 46258, // no cast, range 60 circle
    RoilingMass0 = 46259, // 2.7s cast, range 3 circle
    DramaticLysis2 = 46260, // no cast, range 4 circle
    DramaticLysis3 = 46261, // no cast, ???
    PhagocyteSpotlight1 = 46262, // 2.7s cast, range 5 circle
    RoilingMass1 = 46263, // 2.7s cast, range 3 circle
    CruelCoil0 = 46265, // 2.7s cast, single-target
    CruelCoil1 = 46266, // 2.7s cast, single-target
    CruelCoil2 = 46267, // 2.7s cast, single-target
    Skinsplitter0 = 46268, // no cast, single-target
    Constrictor0 = 46270, // no cast, single-target
    Constrictor1 = 46271, // no cast, single-target
    Constrictor2 = 46272, // no cast, single-target
    unk_46273 = 46273, // 2.7s cast, range 9 circle
    Constrictor3 = 46274, // 0.7s cast, range 13 circle
    Slaughtershed1 = 46275, // 2.7s cast, single-target
    Slaughtershed2 = 46278, // 2.7s cast, single-target
    unk_46283 = 46283, // no cast, single-target
    unk_46284 = 46284, // no cast, single-target
    unk_46286 = 46286, // no cast, single-target
    RaptorKnuckles0 = 46287, // no cast, single-target
    RaptorKnuckles1 = 46288, // no cast, single-target
    SerpentineScourge0 = 46289, // no cast, single-target
    SerpentineScourge1 = 46290, // no cast, single-target
    unk_46291 = 46291, // no cast, single-target
    DramaticLysis4 = 46292, // no cast, range 6 circle
    FourthWallFusion1 = 46293, // no cast, range 6 circle
    VisceralBurst = 46294, // no cast, range 6 circle
    TheFixer = 46295, // 4.7s cast, range 60 circle
    Replication = 46296, // 2.7s cast, single-target
    unk_46297 = 46297, // no cast, single-target
    WingedScourge0 = 46298, // 2.7s cast, single-target
    WingedScourge1 = 46299, // 2.7s cast, single-target
    WingedScourge2 = 46300, // 3.7s cast, range 50 30-degree cone
    TopTierSlam0 = 46301, // 2.7s cast, single-target
    TopTierSlam1 = 46302, // no cast, range 5 circle
    MightyMagic0 = 46303, // 2.7s cast, single-target
    MightyMagic1 = 46304, // no cast, range 5 circle
    Staging = 46305, // 2.7s cast, single-target
    unk_46306 = 46306, // no cast, single-target
    FirefallSplash0 = 46307, // 5.4s cast, single-target
    FirefallSplash1 = 46308, // no cast, range 5 circle
    ScaldingWaves0 = 46309, // no cast, range 50 ?-degree cone
    ManaBurst0 = 46310, // no cast, single-target
    ManaBurst1 = 46311, // no cast, range 20 circle
    HeavySlam0 = 46312, // no cast, range 5 circle
    Grotesquerie0 = 46313, // no cast, single-target
    Grotesquerie1 = 46314, // no cast, single-target
    HemorrhagicProjection1 = 46315, // no cast, range 50 ?-degree cone
    Reenactment = 46316, // 2.7s cast, single-target
    FirefallSplash2 = 46317, // no cast, range 5 circle
    ManaBurst2 = 46318, // no cast, single-target
    HeavySlam1 = 46319, // no cast, single-target
    UnmitigatedImpact = 46320, // no cast, range 60 circle
    Grotesquerie2 = 46321, // no cast, single-target
    unk_46367 = 46367, // no cast, single-target
    DoubleSobat0 = 46368, // 4.7s cast, single-target
    DoubleSobat1 = 46369, // no cast, ???
    DoubleSobat2 = 46370, // no cast, ???
    DoubleSobat3 = 46371, // no cast, ???
    DoubleSobat4 = 46372, // no cast, ???
    DoubleSobat5 = 46373, // 4.3s cast, range 40 ?-degree cone
    EsotericFinisher = 46374, // no cast, range 10 circle
    SnakingKick = 46375, // 4.7s cast, range 40 ?-degree cone
    ArcadiaAflame = 46376, // 4.7s cast, range 60 circle
    NetherwrathNear = 46382, // 4.7s cast, single-target
    NetherwrathFar = 46383, // 4.7s cast, single-target
    TimelessSpite = 46384, // no cast, range 6 circle
    RefreshingOverkill0 = 46392, // 9.7s cast, single-target
    RefreshingOverkill1 = 46393, // no cast, range 60 circle
    RefreshingOverkill2 = 46394, // no cast, range 60 circle
    unk_46395 = 46395, // 4.7s cast, range 20-30 donut
    Skinsplitter1 = 46398, // no cast, range ?-13 donut
    unk_46447 = 46447, // no cast, single-target
    unk_46448 = 46448, // no cast, single-target
    unk_46449 = 46449, // no cast, single-target
    RavenousReach2 = 46953, // no cast, single-target
    RavenousReach3 = 46954, // no cast, single-target
    unk_47044 = 47044, // no cast, single-target
    unk_47045 = 47045, // no cast, single-target
    ScaldingWaves1 = 47329, // no cast, range 50 ?-degree cone
    HemorrhagicProjection2 = 47394, // no cast, range 50 ?-degree cone
    Metamitosis1 = 47395, // 1.5s cast, range 3 circle
    FourthWallFusion2 = 47545, // no cast, range 6 circle
    SerpentineScourge2 = 47548, // 0.7s cast, range 30 width 20 rect
    Splattershed0 = 47555, // 2.7s cast, single-target
    Splattershed1 = 47556, // 2.7s cast, single-target
    Splattershed2 = 47558, // no cast, range 60 circle
    RaptorKnuckles2 = 47559, // 0.5s cast, range 60 circle
    unk_47579 = 47579, // no cast, single-target
    unk_48028 = 48028, // no cast, range 60 circle
    ManaBurst3 = 48099, // no cast, range 20 circle
    FeralFission = 48649, // 1.7s cast, single-target
    BringDownTheHouse3 = 48650, // no cast, single-target
    HeavySlam2 = 48733, // no cast, range 5 circle
    GrotesquerieAct1 = 48829, // 2.7s cast, single-target
    GrotesquerieAct2 = 48830, // 2.7s cast, single-target
    GrotesquerieAct3 = 48831, // 2.7s cast, single-target
    GrotesquerieCurtainCall = 48832, // 2.7s cast, single-target
}

public enum SID : uint
{
    Bind = 2518,
    FireResistanceDownII = 2937,
    MagicVulnerabilityUp = 2941,
    FirstInLine = 3004,
    SecondInLine = 3005,
    ThirdInLine = 3006,
    DarkResistanceDownII = 3323,
    FourthInLine = 3451,
    _Gen_Direction = 3558,
    unk_3792 = 3792,
    unk_3913 = 3913,
    PoisonResistanceDownII = 3935,
    SustainedDamage = 4149,
    BondsOfFleshAlpha = 4752,
    UnbreakableFlesh0 = 4753,
    BondsOfFleshBeta = 4754,
    UnbreakableFlesh1 = 4755,
    BurstingGrotesquerie = 4761,
    SharedGrotesquerie = 4762,
    RottingFlesh = 4763,
    MitoticPhase = 4764,
    FateOfTheWurm = 4772,
    DirectedGrotesquerie = 4976,
}

public enum IconID : uint
{
    SharedGrotesquerie = 93, // stack marker
    SpreadBurstingGrotesquerie = 139, // spread marker
    Icon161 = 161, // com_share3t
    Icon317 = 317, // com_share3_7s0p
    Icon344 = 344, // tank_lockonae_6m_5s_01t
    Countdown = 354, // countdown timer for DirectedGrotesquerie
    Icon375 = 375, // target_ae_s7k1
    Icon598 = 598, // sharelaser2tank5sec_c0k1
    Icon657 = 657, // x6rc_cellchain_01x
}

public enum TetherID : uint
{
    Tether366 = 366, // chn_x6rc_cell_01x
    Tether367 = 367, // chn_x6rc_fr_fan01x
    Tether368 = 368, // chn_x6rc_fr_tgae01x
    Tether369 = 369, // chn_x6rc_fr_share01x
    Tether373 = 373, // chn_tergetfix1f
    Tether374 = 374, // chn_teke01h
}

public enum NameID : uint
{
    Lindwurm0 = 14378,
    Lindwurm1 = 14379,
    Lindschrat = 14380,
    BloodVessel = 14381,
    Understudy = 14383,
    Luzzelwurm = 14469,
}

public enum GroupID : uint
{
    Lindwurm = 1075,
}
