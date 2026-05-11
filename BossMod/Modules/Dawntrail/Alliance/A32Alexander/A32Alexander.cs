using BossMod;

namespace BossMod.Dawntrail.Alliance.A32Alexander;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.AlexanderResurrected, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14529u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class A32Alexander(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(25f))
{
    public static readonly WPos ArenaCenter = new(0f, 360f);
}
