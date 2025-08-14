namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

public enum OID : uint
{
    Boss = 0x318D, // R26.0
    BossP2 = 0x3190, // R5.999
    Energy = 0x3192, // R1.0
    RedGirl = 0x3191, // R3.45
    LighterNoteNS = 0x318F, // R1.0
    LighterNoteWS = 0x318E, // R1.0
    MagicalInterference = 0x1EB169, // R0.5
    UnevenFooting = 0x1EB16C, // R0.5
    WhiteDissonance = 0x1EB16A, // R0.5
    BlackDissonance = 0x1EB16B, // R0.5
    Tower = 0x1EB16D, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttackP1 = 24572, // Boss->player, no cast, single-target
    AutoAttackP2 = 24575, // Boss->player, no cast, single-target

    Eminence = 24021, // Boss->location, 5.0s cast, range 60 circle

    RhythmRingsP1 = 23508, // Boss->self, 3.0s cast, single-target
    RhythmRingsP2 = 23563, // Boss->self, 3.0s cast, single-target
    MagicalInterference = 23509, // Helper->self, no cast, range 50 width 10 rect

    MadeMagic1 = 23510, // Boss->self, 7.0s cast, range 50 width 30 rect
    MadeMagic2 = 23511, // Boss->self, 7.0s cast, range 50 width 30 rect

    LighterNoteVisualP1 = 23512, // Boss->self, 3.0s cast, single-target, baited exaflares
    LighterNoteVisualP2 = 23564, // Boss->self, 3.0s cast, single-target
    LighterNoteFirst = 23513, // Helper->location, no cast, range 6 circle
    LighterNoteRest = 23514, // Helper->location, no cast, range 6 circle

    DarkerNoteVisualP1 = 23515, // Boss->self, 5.0s cast, single-target, tankbuster
    DarkerNoteVisualP2 = 23562, // Boss->self, 5.0s cast, single-target
    DarkerNote = 23516, // Helper->players, 5.0s cast, range 6 circle

    ScreamingScoreP1 = 23517, // Boss->self, 5.0s cast, range 60 circle, raidwide
    ScreamingScoreP2 = 23541, // Boss->self, 5.0s cast, range 71 circle

    SeedOfMagic = 23518, // Boss->self, 3.0s cast, single-target
    ScatteredMagic = 23519, // Helper->location, 3.0s cast, range 4 circle

    Pervasion = 23520, // Boss->self, 3.0s cast, single-target
    RecreateStructure = 23521, // Boss->self, 3.0s cast, single-target
    UnevenFooting = 23522, // Helper->self, 1.9s cast, range 80 width 30 rect

    RecreateSignal = 23523, // Boss->self, 3.0s cast, single-target
    MixedSignals = 23524, // Boss->self, 3.0s cast, single-target
    Crash = 23525, // Helper->self, 0.8s cast, range 50 width 10 rect

    HeavyArmsVisual = 23534, // Boss->self, 7.0s cast, single-target
    HeavyArms1 = 23535, // Helper->self, 7.0s cast, range 44 width 100 rect
    HeavyArms2 = 23533, // Boss->self, 7.0s cast, range 100 width 12 rect

    PlaceOfPower = 23565, // Helper->location, 3.0s cast, range 6 circle
    Distortion1 = 23529, // Boss->self, 3.0s cast, range 60 circle
    Distortion2 = 24664, // Boss->self, 3.0s cast, range 60 circle
    TheFinalSong = 23530, // Boss->self, 3.0s cast, single-target
    WhiteDissonance = 23531, // Helper->self, no cast, range 60 circle, look away from mid
    BlackDissonance = 23532, // Helper->self, no cast, range 60 circle, look at mid

    PillarImpactVisual1 = 23566, // Boss->self, no cast, single-target
    PillarImpactVisual2 = 23536, // Boss->self, 10.0s cast, single-target
    ShockwaveKB = 23538, // Helper->self, 6.5s cast, range 71 circle, knockback 35, away from source
    ShockwaveAOE = 23537, // Helper->self, 6.5s cast, range 7 circle
    TowerfallVisual = 23539, // Boss->self, 3.0s cast, single-target
    Towerfall = 23540, // Helper->self, 3.0s cast, range 70 width 14 rect

    ModelStateVisual1 = 23526, // RedGirl->self, no cast, single-target
    ModelStateVisual2 = 23527, // RedGirl->self, no cast, single-target
    ScatteredMagicEnergy = 23528 // Energy->player, no cast, single-target
}

public enum IconID : uint
{
    LighterNote = 1 // player
}
