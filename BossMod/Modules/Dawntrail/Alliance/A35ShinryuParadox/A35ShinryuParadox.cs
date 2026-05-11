namespace BossMod.Dawntrail.Alliance.A35ShinryuParadox;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.ShinryuParadox, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14729u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 6)]
public sealed class A35ShinryuParadox(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(30f, 20f))
{
    public static readonly WPos ArenaCenter = new(820f, -820f);
}
