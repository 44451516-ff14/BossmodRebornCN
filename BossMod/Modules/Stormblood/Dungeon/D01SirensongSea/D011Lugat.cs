namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D011Lugat;

public enum OID : uint
{
    Lugat = 0x1AFB, // R4.5
    Helper = 0x18D6 // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // Lugat->player, no cast, single-target

    AmorphousApplause = 8022, // Lugat->location, 5.0s cast, range 20+R 180-degree cone
    Hydroball = 8023, // Lugat->players, 5.0s cast, range 5 circle
    SeaSwallowsAll = 8024, // Lugat->location, 3.0s cast, range 60 circle, pull 30 between centers
    ConcussiveOscillation = 8027, // Helper->location, 4.0s cast, range 8 circle
    ConcussiveOscillationBoss = 8026, // Lugat->location, 4.0s cast, range 7 circle
    Overtow = 8025 // Lugat->location, 3.0s cast, range 60 circle, knockback 30, away from source
}

public enum IconID : uint
{
    Stackmarker = 62
}

[SkipLocalsInit]
sealed class ConcussiveOscillationBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcussiveOscillationBoss, 7f);
[SkipLocalsInit]
sealed class ConcussiveOscillation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcussiveOscillation, 8f);
[SkipLocalsInit]
sealed class AmorphousApplause(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AmorphousApplause, new AOEShapeCone(24.5f, 90f.Degrees()));
[SkipLocalsInit]
sealed class Hydroball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.Hydroball, 5f, 5f, 4, 4);
[SkipLocalsInit]
sealed class SeaSwallowsAll(BossModule module) : Components.RaidwideCast(module, (uint)AID.SeaSwallowsAll);
[SkipLocalsInit]
sealed class Overtow(BossModule module) : Components.RaidwideCast(module, (uint)AID.Overtow);

[SkipLocalsInit]
sealed class D011LugatStates : StateMachineBuilder
{
    public D011LugatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Overtow>()
            .ActivateOnEnter<SeaSwallowsAll>()
            .ActivateOnEnter<Hydroball>()
            .ActivateOnEnter<AmorphousApplause>()
            .ActivateOnEnter<ConcussiveOscillation>()
            .ActivateOnEnter<ConcussiveOscillationBoss>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
StatesType = typeof(D011LugatStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Lugat,
Contributors = "The Combat Reborn Team (Malediktus), erdelf",
Expansion = BossModuleInfo.Expansion.Stormblood,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 238u,
NameID = 6071u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D011Lugat : BossModule
{
    public D011Lugat(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D011Lugat(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(-2.7f, -217f), 21.55f, 32), new Rectangle(new(-0.9f, -237.7f), 5, 1, -5f.Degrees())],
        [new Rectangle(new(-3f, -195f), 20, 1.25f)]);
        return (arena.Center, arena);
    }
}
