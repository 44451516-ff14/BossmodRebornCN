namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB3MarbleDragon;

sealed class ImitationStar(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.ImitationStarVisual, (uint)AID.ImitationStar, 1.9f);
sealed class ImitationRain(BossModule module) : Components.RaidwideInstant(module, (uint)AID.ImitationRain);
sealed class WitheringEternity(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.WitheringEternity, (uint)AID.ImitationRain, 2.6f);
sealed class WickedWater(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.WickedWater, (uint)AID.ImitationRain, 2.7f);
sealed class ImitationIcicle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ImitationIcicle, 8f);
sealed class DreadDeluge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DreadDeluge);

sealed class FrigidTwister(BossModule module) : Components.Voidzone(module, 5.5f, GetIcewinds, 3f)
{
    private static List<Actor> GetIcewinds(BossModule module) => module.Enemies((uint)OID.Icewind);
}
sealed class FrigidDive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidDive, new AOEShapeRect(60f, 10f));
sealed class GelidGaol(BossModule module) : Components.Adds(module, (uint)OID.GelidGaol, 1);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.MarbleDragon, GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018u, NameID = 13838u, PlanLevel = 100, SortOrder = 4, Category = BossModuleInfo.Category.Foray, Expansion = BossModuleInfo.Expansion.Dawntrail)]
public sealed class FTB3MarbleDragon(WorldState ws, Actor primary) : BossModule(ws, primary, startingArena.Center, startingArena)
{
    public static readonly WPos ArenaCenter = new(-337f, 157f);
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(ArenaCenter, 39.5f, 48)], [new Rectangle(new(-337f, 116.853f), 11f, 1.25f),
    new Rectangle(new(-337f, 197.413f), 11f, 1.25f)]);
    public static readonly ArenaBoundsCircle DefaultArena = new(30f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.FindStatus((uint)SID.Invincibility) == null)
            Arena.Actor(PrimaryActor);
    }
}
