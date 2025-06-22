namespace BossMod.Dawntrail.Savage.M08SHowlingBlade;

sealed class ExtraplanarPursuit(BossModule module) : Components.CastCounter(module, (uint)AID.ExtraplanarPursuit);
sealed class TitanicPursuit(BossModule module) : Components.CastCounter(module, (uint)AID.TitanicPursuit);
sealed class HowlingHavoc(BossModule module) : Components.CastCounter(module, (uint)AID.HowlingHavoc);
sealed class GreatDivide(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.GreatDivide, new AOEShapeRect(60f, 3f));
sealed class RavenousSaber(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.RavenousSaber1,
(uint)AID.RavenousSaber2, (uint)AID.RavenousSaber3, (uint)AID.RavenousSaber4, (uint)AID.RavenousSaber5]);
sealed class Mooncleaver1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Mooncleaver1, 8f);
sealed class ProwlingGaleP2(BossModule module) : Components.CastTowers(module, (uint)AID.ProwlingGaleP2, 2f, 2, 2);
sealed class MoonbeamsBite : Components.SimpleAOEGroups
{
    public MoonbeamsBite(BossModule module) : base(module, [(uint)AID.MoonbeamsBite1, (uint)AID.MoonbeamsBite2], new AOEShapeRect(40f, 10f), 2, 6)
    {
        MaxDangerColor = 1;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1026, NameID = 13843, PlanLevel = 100)]
public sealed class M08SHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_bossP2 == null)
        {
            if (StateMachine.ActivePhaseIndex == 1)
            {
                var b = Enemies((uint)OID.BossP2);
                _bossP2 = b.Count != 0 ? b[0] : null;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
    }

    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly Polygon[] StartingArenaPolygon = [new(ArenaCenter, 12f, 40)];
    public static readonly ArenaBoundsComplex StartingArena = new(StartingArenaPolygon);
    public static readonly ArenaBoundsComplex DonutArena = new(StartingArenaPolygon, [new Polygon(ArenaCenter, 8f, 40)]);
}