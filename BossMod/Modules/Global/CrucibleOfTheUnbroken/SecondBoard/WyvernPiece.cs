namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.WyvernPiece;

public enum OID : uint
{
    WyvernPiece = 0x4C58,
    WindSprite = 0x4C5B, // R1.600, x0 (spawn during fight)
    WhirlwindSmall = 0x4C59, // R2.000, x0 (spawn during fight), mixed types
    WhirlwindBig = 0x4C5A, // R3.000, x0 (spawn during fight)
    LiquidHellPuddle = 0x1EA66D, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49680, // WyvernPiece->player, no cast, single-target
    Teleport = 48173, // WyvernPiece->location, no cast, single-target
    TheStormsGrip = 48166, // WyvernPiece->self, 4.0s cast, range 60 circle
    Buffet = 48167, // WindSprite->self, 6.0s cast, range 40 width 10 rect
    Typhoon = 48168, // WyvernPiece->self, 3.0s cast, range 40 circle
    LiquidHellBoss = 48171, // WyvernPiece->self, 3.0s cast, single-target
    LiquidHell = 48172, // Helper->self, 6.0s cast, range 6 circle
    BlazingTrailBoss = 48174, // WyvernPiece->self, 7.0+1.0s cast, single-target
    BlazingTrail = 48175, // Helper->self, 8.0s cast, range 60 180.000-degree cone
    StormTrailBoss = 48176, // WyvernPiece->self, 5.0+1.0s cast, single-target
    StormTrailBoss1 = 48177, // WyvernPiece->self, 5.0+1.0s cast, single-target
    StormTrail = 48178, // Helper->self, 6.0s cast, range 25 60.000-degree cone
}

public enum SID : uint
{
    Burns = 3065, // none->player, extra=0x0
    Burns1 = 3066, // none->player, extra=0x0
}

//sealed class TheStormsGrip(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheStormsGrip); // TODO confirm this is not a raidwide
sealed class Buffet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Buffet, new AOEShapeRect(40.0f, 5.0f));
sealed class Typhoon(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Typhoon, 10.0f); // TODO add AI
sealed class LiquidHell(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LiquidHell, 6.0f);
sealed class BlazingTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingTrail, new AOEShapeCone(60.0f, 90.0f.Degrees()));
sealed class StormTrail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormTrail, new AOEShapeCone(25.0f, 30.0f.Degrees()));
sealed class LiquidHellPuddle(BossModule module) : Components.Voidzone(module, 5.0f,
    module => module.Enemies((uint)OID.LiquidHellPuddle).Where(z => z.EventState != 7));

sealed class Whirlwind(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] aoes = [];
    private readonly List<Actor> puddles = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.WhirlwindSmall or (uint)OID.WhirlwindBig)
        {
            puddles.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.WhirlwindSmall or (uint)OID.WhirlwindBig)
        {
            puddles.Remove(actor);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return aoes;
    }

    public override void Update()
    {
        var count = puddles.Count;
        aoes = new AOEInstance[count];
        for (var i = 0; i < count; i++)
        {
            var puddle = puddles[i];
            AOEShapeCapsule shape = puddle.OID == (uint)OID.WhirlwindSmall ? new AOEShapeCapsule(2.0f, 2.5f) : new AOEShapeCapsule(3.0f, 3.5f);
            aoes[i] = new(shape, puddle.Position, puddle.Rotation, color: Colors.Danger);
        }
    }
}

sealed class WyvernPieceStates : StateMachineBuilder
{
    public WyvernPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //.ActivateOnEnter<TheStormsGrip>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<Typhoon>()
            .ActivateOnEnter<LiquidHell>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<LiquidHellPuddle>()
            .ActivateOnEnter<BlazingTrail>()
            .ActivateOnEnter<StormTrail>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.WyvernPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14549u, SortOrder = 2)]
public sealed class WyvernPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, 0f), new ArenaBoundsRect(20f, 14.8f));
