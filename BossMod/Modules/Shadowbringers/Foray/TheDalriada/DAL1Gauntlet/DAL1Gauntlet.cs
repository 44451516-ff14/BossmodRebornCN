namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class NihilitysSong(BossModule module) : Components.RaidwideCast(module, (uint)AID.NihilitysSong);
sealed class SanctifiedQuakeIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.SanctifiedQuakeIII);
sealed class BroadsideBarrage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BroadsideBarrage, new AOEShapeRect(40f, 20f));
sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f);
sealed class CeruleumExplosion(BossModule module) : Components.CastHint(module, (uint)AID.CeruleumExplosion, "Enrage!", true);
sealed class FlamingCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlamingCyclone, 10f);
sealed class SeventyFourDegrees(BossModule module) : Components.DonutStack(module, (uint)AID.SeventyFourDegrees, (uint)IconID.SeventyFourDegrees, 4f, 8f, 9f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778, NameID = 10212, SortOrder = 1)]
public sealed class DAL1Gauntlet(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    public static readonly WPos ArenaCenter = new(222f, -689f);
    public static readonly ArenaBoundsSquare StartingArena = new(29.5f);
    public static readonly ArenaBoundsSquare DefaultArena = new(23f);
    public Actor? BossAugur;
    public Actor? BossAlkonost;
    public Actor? BossCrow;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (BossAugur == null)
        {
            var b = Enemies((uint)OID.ForthLegionAugur1);
            BossAugur = b.Count != 0 ? b[0] : null;
        }
        if (StateMachine.ActivePhaseIndex >= 1)
        {
            if (BossAlkonost == null)
            {
                var b = Enemies((uint)OID.TamedAlkonost);
                BossAlkonost = b.Count != 0 ? b[0] : null;
            }
            if (BossCrow == null)
            {
                var b = Enemies((uint)OID.TamedCrow);
                BossCrow = b.Count != 0 ? b[0] : null;
            }
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                Arena.Actors(Enemies((uint)OID.ForthLegionInfantry));
                break;
            case 1:
                Arena.Actor(BossAugur);
                Arena.Actors(Enemies((uint)OID.WaveborneZirnitra));
                Arena.Actors(Enemies((uint)OID.FlameborneZirnitra));
                break;
            case 2:
                Arena.Actor(BossAlkonost);
                Arena.Actor(BossCrow);
                break;
        }
    }
}
