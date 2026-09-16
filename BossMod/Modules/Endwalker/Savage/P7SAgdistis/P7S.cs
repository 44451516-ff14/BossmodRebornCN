namespace BossMod.Endwalker.Savage.P7SAgdistis;

sealed class HemitheosHoly(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.HemitheosHolyAOE, 6f, 4, 4);
sealed class BoughOfAttisBack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoughOfAttisBackAOE, 25f);
sealed class BoughOfAttisFront(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoughOfAttisFrontAOE, 19f);
sealed class BoughOfAttisSide(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BoughOfAttisSideAOE, new AOEShapeRect(50f, 12.5f));
sealed class HemitheosAeroKnockback1(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.HemitheosAeroKnockback1, 16f); // TODO: verify distance...
sealed class HemitheosAeroKnockback2(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.HemitheosAeroKnockback2, 16f);
sealed class HemitheosHolySpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HemitheosHolySpread, 6f);
sealed class HemitheosTornado(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HemitheosTornado, 25f);
sealed class HemitheosGlareMine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HemitheosGlareMine, new AOEShapeDonut(5f, 30f)); // TODO: verify inner radius

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 877u, NameID = 11374u, PlanLevel = 90)]
public sealed class P7S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCustom([new Polygon(new(100f, 100f), 20f, 128)]));
