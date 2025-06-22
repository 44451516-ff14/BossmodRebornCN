namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN1TrinitySeeker;

sealed class MercifulBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MercifulBreeze, new AOEShapeRect(50f, 2.5f));
sealed class BalefulSwathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BalefulSwathe, new AOEShapeCone(50f, 90f.Degrees()));
sealed class ActOfMercy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ActOfMercy, new AOEShapeCross(50f, 4f));
sealed class MercifulBlooms(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MercifulBlooms, 20f);
sealed class MercifulArc(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(12f, 45f.Degrees()), (uint)IconID.MercifulArc, (uint)AID.MercifulArc, tankbuster: true);

sealed class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.ScorchingShackle);
sealed class IronImpact(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.IronImpactMarker, (uint)AID.IronImpact, 5.9f);

sealed class DeadIron(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50f, 15f.Degrees()), (uint)IconID.DeadIron, (uint)AID.DeadIronAOE, 4.6d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9834)]
public sealed class DRN1TrinitySeeker(WorldState ws, Actor primary) : TrinitySeeker(ws, primary);
