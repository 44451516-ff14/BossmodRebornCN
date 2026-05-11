using BossMod;

namespace BossMod.Dawntrail.Alliance.A34Promathia;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Promathia, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14779u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 5)]
public sealed class A34Promathia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(25f))
{
    public static readonly WPos ArenaCenter = new(-820f, -820f);
}
