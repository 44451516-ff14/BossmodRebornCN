namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class NihilitysSong(BossModule module) : Components.RaidwideCast(module, (uint)AID.NihilitysSong);
sealed class PainStorm(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PainStorm, (uint)AID.PainStormShadow], new AOEShapeCone(35f, 65f.Degrees()));
sealed class FrigidPulse(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FrigidPulse, (uint)AID.FrigidPulseShadow], new AOEShapeDonut(8f, 25f));
sealed class PainfulGust(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PainfulGust, (uint)AID.PainfulGustShadow], 20f);
sealed class BroadsideBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BroadsideBarrage, new AOEShapeRect(40f, 20f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 32, SortOrder = 1)] // NameID 9834
public sealed class DAL1Gauntlet(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    public static readonly WPos ArenaCenter = new(222f, -689f);
    public static readonly ArenaBoundsSquare StartingArena = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultArena = new(24f);
    private Actor? _bossAugur;
    public Actor? BossAugur() => _bossAugur;
    private Actor? _bossAlkonost;
    public Actor? BossAlkonost() => _bossAlkonost;
    private Actor? _bossCrow;
    public Actor? BossCrow() => _bossCrow;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (_bossAugur == null)
        {
            var b = Enemies((uint)OID.ForthLegionAugur1);
            _bossAugur = b.Count != 0 ? b[0] : null;
        }
        if (_bossAlkonost == null)
        {
            var b = Enemies((uint)OID.TamedAlkonost);
            _bossAlkonost = b.Count != 0 ? b[0] : null;
        }
        if (_bossCrow == null)
        {
            var b = Enemies((uint)OID.TamedCrow);
            _bossCrow = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_bossAugur);
        Arena.Actor(_bossAlkonost);
        Arena.Actor(_bossCrow);
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ForthLegionInfantry));
        Arena.Actors(Enemies((uint)OID.FourthLegionHoplomachus));
    }
}
