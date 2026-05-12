namespace BossMod.Dawntrail.Alliance.A32Alexander;

sealed class BanishgaIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.BanishgaIV);
sealed class DivineArrowCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineArrowCone, new AOEShapeCone(45f, 45f.Degrees()));
sealed class DivineArrowClose(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineArrowClose, new AOEShapeCircle(10f));
sealed class DivineArrowMid(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineArrowMid, new AOEShapeDonut(10f, 23f));
sealed class DivineArrowFar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DivineArrowFar, new AOEShapeDonut(23f, 36f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.AlexanderResurrected, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1117u, NameID = 14529u, Category = BossModuleInfo.Category.Alliance, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class A32Alexander(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(25f))
{
    public static readonly WPos ArenaCenter = new(0f, 360f);
}
