namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

sealed class AbyssalEchoes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalEchoes, 12f, 5);
sealed class BigBangPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BigBangAOE, 5f);
sealed class BigBangSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BigBangSpread, 5f);
sealed class BigCrunchPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BigCrunchAOE, 5f);
sealed class BigCrunchSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BigCrunchSpread, 5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 965u, NameID = 12586u, PlanLevel = 90)]
public sealed class Ex7Zeromus(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20u));
