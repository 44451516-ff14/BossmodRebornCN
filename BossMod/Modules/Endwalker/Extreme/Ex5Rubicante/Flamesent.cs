namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

sealed class Flamesent(BossModule module) : Components.AddsMulti(module, [(uint)OID.GreaterFlamesent, (uint)OID.FlamesentNS, (uint)OID.FlamesentSS, (uint)OID.FlamesentNC]);
sealed class GhastlyTorch(BossModule module) : Components.RaidwideCast(module, (uint)AID.GhastlyTorch);
sealed class ShatteringHeatAdd(BossModule module) : Components.TankbusterTether(module, (uint)AID.ShatteringHeatAdd, (uint)TetherID.ShatteringHeatAdd, 3f);
sealed class GhastlyWind(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(40f, 15f.Degrees()), (uint)TetherID.GhastlyWind, (uint)AID.GhastlyWind); // TODO: verify angle
sealed class GhastlyFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyFlameAOE, 5f);
