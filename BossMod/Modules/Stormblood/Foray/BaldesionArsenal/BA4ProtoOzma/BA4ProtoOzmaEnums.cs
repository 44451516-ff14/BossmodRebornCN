namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

public enum OID : uint
{
    Boss = 0x25E8, // R13.5
    BlackHoleBuffer = 0x1EA1A1, // R2.0
    Shadow = 0x25E9, // R13.5
    Ozmasphere = 0x25EA, // R1.0
    ArsenalUrolith = 0x25EB, // R3.0
    Helper = 0x2629
}

public enum AID : uint
{
    AutoAttackSphere = 14251, // Helper->player, no cast, single-target, targets highest enmity on platform
    AutoAttackCube = 14252, // Helper->self, no cast, range 40+R width 4 rect, targets highest enmity on platform
    AutoAttackStar = 14262, // Helper->players, no cast, range 6 circle, stack AOE on a random player on each platform
    AutoAttackPyramid = 14253, // Helper->players, no cast, range 4 circle, targets furthest player on each platform
    AutoAttackAdd = 872, // ArsenalUrolith->player, no cast, single-target

    TransfigurationStar = 14258, // Boss/Shadow->self, no cast, single-target
    TransfigurationSphere1 = 14259, // Boss->self, no cast, single-target, star to sphere
    TransfigurationSphere2 = 14245, // Boss->self, no cast, single-target, pyramid to sphere
    TransfigurationSphere3 = 14239, // Boss->self, no cast, single-target, cube to sphere
    TransfigurationCube = 14238, // Shadow/Boss->self, no cast, single-target
    TransfigurationPyramid = 14244, // Boss/Shadow->self, no cast, single-target

    MourningStarVisual = 14260, // Boss/Shadow->self, no cast, single-target, transition attack star
    MourningStar = 14261, // Helper->self, no cast, range 27 circle
    ExecrationVisual = 14246, // Boss/Shadow->self, no cast, single-target, transition attack pyramid
    Execration = 14247, // Helper->self, no cast, range 40+R width 11 rect
    FlareStarVisual = 14240, // Shadow/Boss->self, no cast, single-target, transition attack cube
    FlareStar = 14241, // Helper->self, no cast, range 17+R-38+R donut

    ShootingStarVisual = 14263, // Boss->self, 5.0s cast, single-target
    ShootingStar = 14264, // Helper->self, 5.0s cast, range 26 circle

    BlackHole = 14237, // Boss->self, no cast, range 40 circle

    Explosion = 14242, // Ozmasphere->self, no cast, range 6 circle
    Meteor = 14248, // Helper->location, no cast, range 10 circle, stack
    Holy = 14249, // Boss->self, 4.0s cast, range 50 circle, knockback 3, away from source

    Tornado = 14255, // ArsenalUrolith->players, 5.0s cast, range 6 circle, enrage cast on random player if not killed within 30s, instant kills everyone in circle
    MeteorImpact = 14256, // ArsenalUrolith->self, 4.0s cast, range 20 circle
    DebrisBurst = 14257, // ArsenalUrolith->self, no cast, range 40 circle

    AccelerationBomb = 14250, // Boss->self, no cast, ???

    ShootingStarEnrageVisualFirst = 14701, // Boss->self, 10.0s cast, single-target
    ShootingStarEnrageVisualRepeat = 14715, // Boss->self, no cast, single-target
    ShootingStarEnrageFirst = 14702, // Helper->self, 10.0s cast, range 26 circle
    ShootingStarEnrageRepeat = 14716 // Helper->self, no cast, range 26 circle
}

public enum SID : uint
{
    AccelerationBomb = 1072 // none->player, extra=0x0
}

public enum IconID : uint
{
    MeteorStack = 62, // player->self
    MeteorBaitaway = 57 // player->self
}
