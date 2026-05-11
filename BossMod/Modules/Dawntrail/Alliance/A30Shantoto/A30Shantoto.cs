namespace BossMod.Dawntrail.Alliance.A30Shantoto;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.ShantottoTheDemon, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14778u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class A30Shantoto(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(24f))
{
    public static readonly WPos ArenaCenter = new(0f, -720f);
}
