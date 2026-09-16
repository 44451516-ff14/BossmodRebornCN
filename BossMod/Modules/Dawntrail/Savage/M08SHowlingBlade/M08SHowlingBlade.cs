namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class ExtraplanarPursuit(BossModule module) : Components.CastCounter(module, (uint)AID.ExtraplanarPursuit);
sealed class TitanicPursuit(BossModule module) : Components.CastCounter(module, (uint)AID.TitanicPursuit);
sealed class HowlingHavoc(BossModule module) : Components.CastCounter(module, (uint)AID.HowlingHavoc);
sealed class GreatDivide(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.GreatDivide, new AOEShapeRect(60f, 3f));
sealed class RavenousSaber(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.RavenousSaber1,
(uint)AID.RavenousSaber2, (uint)AID.RavenousSaber3, (uint)AID.RavenousSaber4, (uint)AID.RavenousSaber5]);
sealed class Mooncleaver1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mooncleaver1, 8f);
sealed class ProwlingGaleP2(BossModule module) : Components.CastTowers(module, (uint)AID.ProwlingGaleP2, 2f, 2, 2);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1026u, NameID = 13843u, PlanLevel = 100)]
public sealed class M08SHowlingBlade : BossModule
{
    public M08SHowlingBlade(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private M08SHowlingBlade(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private Actor? _bossP2;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        if (StateMachine.ActivePhaseIndex == 1)
        {
            _bossP2 ??= GetActor((uint)OID.BossP2);
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }

    public static Polygon[] GetStartingArenaPolygon() => [new(new(100f, 100f), 12f, 40)];

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom(GetStartingArenaPolygon(), MapResolution: 0.25f) { Y = 0f, BorderY = 0f };
        return (arena.Center, arena);
    }
}
