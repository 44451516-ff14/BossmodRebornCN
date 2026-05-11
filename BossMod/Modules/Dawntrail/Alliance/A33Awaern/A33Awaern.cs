namespace BossMod.Dawntrail.Alliance.A33Awaern;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Awaern, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14838u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 4)]
public sealed class A33Awaern(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(30f, 24f))
{
    public static readonly WPos ArenaCenter = new(-720f, 720f);

    public static readonly uint[] GardenofRuHmetMobs = [(uint)OID.Awaern, (uint)OID.Awzdei];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, GardenofRuHmetMobs);
    }
}

// Aw'aern = 14838u
// Aw'zdei = 14839u
