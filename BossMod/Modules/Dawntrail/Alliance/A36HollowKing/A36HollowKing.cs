using BossMod;

namespace BossMod.Dawntrail.Alliance.A36HollowKing;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.HollowKing, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14730u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 7)]
public sealed class A36HollowKing(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(30f, 20f))
{
    public static readonly WPos ArenaCenter = new(820f, -820f);
}
