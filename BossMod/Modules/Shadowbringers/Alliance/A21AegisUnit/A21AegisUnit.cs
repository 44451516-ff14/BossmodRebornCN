namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

sealed class AntiPersonnelLaser(BossModule module) : Components.BaitAwayIcon(module, 3f, (uint)IconID.AntiPersonnelLaser, (uint)AID.AntiPersonnelLaser, 4d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class FlightPath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlightPath, new AOEShapeRect(60f, 5f));
sealed class HighPoweredLaser(BossModule module) : Components.StackWithIcon(module, (uint)IconID.HighPoweredLaser, (uint)AID.HighPoweredLaser, 6f, 5.1d, 8, 8);
sealed class LifesLastSong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LifesLastSong, new AOEShapeCone(30f, 50f.Degrees()), 3);
sealed class ManeuverDiffusionCannon(BossModule module) : Components.RaidwideCast(module, (uint)AID.ManeuverDiffusionCannon);
sealed class ManeuverSaturationBombing(BossModule module) : Components.CastHint(module, (uint)AID.ManeuverSaturationBombing, "Enrage!", true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9642, SortOrder = 1)]
public sealed class A21AegisUnit(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos arenaCenter = new(-230f, 192f);
    private static readonly Polygon circle = new(new(-230f, 209.5f), 12.144f, 64);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(arenaCenter, 25f, 90), circle, circle with { Center = new(-214.845f, 183.25f) },
    circle with { Center = new(-245.155f, 183.25f) }], [new Polygon(arenaCenter, 10.5f, 90)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.FlightUnit));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.FlightUnit => 1,
                _ => 0
            };
        }
    }
}
