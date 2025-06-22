namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

sealed class WrathOfBozja(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.WrathOfBozja, new AOEShapeCone(60f, 45f.Degrees()));
sealed class WrathOfBozjaBow(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.WrathOfBozjaBow, new AOEShapeCone(60f, 45f.Degrees()));

// note: it is combined with different AOEs (bow1, bow2, staff1)
class QuickMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);

sealed class GleamingArrow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GleamingArrow, new AOEShapeRect(60f, 5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9853, PlanLevel = 80)]
public sealed class DRS6TrinityAvowed(WorldState ws, Actor primary) : TrinityAvowed(ws, primary);
