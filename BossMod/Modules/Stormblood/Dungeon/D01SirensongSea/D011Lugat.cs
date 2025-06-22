namespace BossMod.Stormblood.Dungeon.D01SirensongSea.D011Lugat;

public enum OID : uint
{
    Boss = 0x1AFB, // R4.5
    Helper = 0x18D6 // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AmorphousApplause = 8022, // Boss->location, 5.0s cast, range 20+R 180-degree cone
    Hydroball = 8023, // Boss->players, 5.0s cast, range 5 circle
    SeaSwallowsAll = 8024, // Boss->location, 3.0s cast, range 60 circle, pull 30 between centers
    ConcussiveOscillation = 8027, // Helper->location, 4.0s cast, range 8 circle
    ConcussiveOscillationBoss = 8026, // Boss->location, 4.0s cast, range 7 circle
    Overtow = 8025 // Boss->location, 3.0s cast, range 60 circle, knockback 30, away from source
}

public enum IconID : uint
{
    Stackmarker = 62
}

class ConcussiveOscillationBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcussiveOscillationBoss, 7);
class ConcussiveOscillation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ConcussiveOscillation, 8);
class AmorphousApplause(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AmorphousApplause, new AOEShapeCone(24.5f, 90.Degrees()));
class Hydroball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.Hydroball, 5, 5, 4, 4);
class SeaSwallowsAll(BossModule module) : Components.RaidwideCast(module, (uint)AID.SeaSwallowsAll);
class Overtow(BossModule module) : Components.RaidwideCast(module, (uint)AID.Overtow);

class D011LugatStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 238, NameID = 6071)]
public class D011Lugat(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-2.7f, -217), 21.5f), new Rectangle(new(-0.9f, -237.7f), 5, 1, -5.Degrees())], [new Rectangle(new(-3, -195), 20, 1.25f)]);
}
