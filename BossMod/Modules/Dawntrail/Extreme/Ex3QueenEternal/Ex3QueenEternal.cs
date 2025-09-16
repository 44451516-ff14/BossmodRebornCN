namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class ProsecutionOfWar(BossModule module) : Components.TankSwap(module, (uint)AID.ProsecutionOfWar, (uint)AID.ProsecutionOfWar, (uint)AID.ProsecutionOfWarAOE, default, 3.1d);
sealed class DyingMemory(BossModule module) : Components.CastCounter(module, (uint)AID.DyingMemory);
sealed class DyingMemoryLast(BossModule module) : Components.CastCounter(module, (uint)AID.DyingMemoryLast);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1017u, NameID = 13029u, PlanLevel = 100)]
public sealed class Ex3QueenEternal(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, NormalBounds)
{
    public static readonly WPos ArenaCenter = Trial.T03QueenEternal.T03QueenEternal.ArenaCenter, HalfBoundsCenter = new(100, 110);
    public static readonly ArenaBoundsSquare NormalBounds = Trial.T03QueenEternal.T03QueenEternal.DefaultBounds;
    public static readonly ArenaBoundsRect HalfBounds = new(20f, 10f);
    public static readonly ArenaBoundsCustom WindBounds = Trial.T03QueenEternal.T03QueenEternal.XArena;
    public static readonly ArenaBoundsCustom EarthBounds = Trial.T03QueenEternal.T03QueenEternal.SplitArena;
    private static readonly Rectangle[] iceRects = [new(new(112f, 95f), 4f, 15f), new(new(88f, 95f), 4f, 15f), new(ArenaCenter, 2f, 10f)];
    public static readonly Rectangle[] IceRectsAll = [.. iceRects, new(new(100f, 96f), 8f, 2f), new(new(100f, 104f), 8f, 2f)];
    public static readonly ArenaBoundsCustom IceBounds = new(iceRects);

    private Actor? _bossP2;
    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        _bossP2 ??= GetActor((uint)OID.BossP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }
}
