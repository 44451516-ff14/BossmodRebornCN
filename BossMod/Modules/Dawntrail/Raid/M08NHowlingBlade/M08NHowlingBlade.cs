namespace BossMod.Dawntrail.Raid.M08NHowlingBlade;

sealed class ExtraplanarTitanicPursuit(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ExtraplanarPursuit, (uint)AID.TitanicPursuit]);
sealed class RavenousSaber(BossModule module) : Components.RaidwideCast(module, (uint)AID.RavenousSaber5, "Raidwide x5");
sealed class GreatDivide(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.GreatDivide, new AOEShapeRect(60f, 3f));
sealed class Heavensearth1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Heavensearth1, 6, 8, 8);
sealed class Heavensearth2(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Heavensearth2, 6, 8, 8);
sealed class Gust(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Gust, 5f);
sealed class TargetedQuake(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TargetedQuake, 4f);
sealed class GrowlingWindWealofStone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GrowlingWind, (uint)AID.WealOfStone1, (uint)AID.WealOfStone2, (uint)AID.WealOfStone3],
new AOEShapeRect(40f, 3f));

sealed class FangedCharge : Components.SimpleAOEs
{
    public FangedCharge(BossModule module) : base(module, (uint)AID.FangedCharge, new AOEShapeRect(46f, 3f))
    {
        MaxDangerColor = 2;
        MaxRisky = 2;
    }
}

sealed class MoonbeamsBite : Components.SimpleAOEGroups
{
    public MoonbeamsBite(BossModule module) : base(module, [(uint)AID.MoonbeamsBite1, (uint)AID.MoonbeamsBite2, (uint)AID.MoonbeamsBite3,
    (uint)AID.MoonbeamsBite4], new AOEShapeRect(40f, 10f), 2, 6)
    {
        MaxDangerColor = 1;
    }
}

sealed class RoaringWindShadowchase(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RoaringWind, (uint)AID.Shadowchase1, (uint)AID.Shadowchase2], new AOEShapeRect(40f, 4f));
sealed class TerrestrialTitans(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TerrestrialTitans, 3f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1025, NameID = 13843)]
public sealed class M08NHowlingBlade(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, startingArena)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    private static readonly ArenaBoundsComplex startingArena = new([new Polygon(ArenaCenter, 17f, 40)]);
    public static readonly Polygon[] EndArenaPolygon = [new Polygon(ArenaCenter, 12f, 40)]; // 11.2s after 0x200010 then 0x00 20001
    public static readonly ArenaBoundsComplex EndArena = new(EndArenaPolygon);
}
