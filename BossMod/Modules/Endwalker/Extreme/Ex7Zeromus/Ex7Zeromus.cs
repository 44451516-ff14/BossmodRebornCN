namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class AbyssalEchoes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalEchoes, 12, 5);
class BigBangPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BigBangAOE, 5);
class BigBangSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BigBangSpread, 5);
class BigCrunchPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BigCrunchAOE, 5);
class BigCrunchSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BigCrunchSpread, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 965, NameID = 12586, PlanLevel = 90)]
public class Ex7Zeromus(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(20))
{
    public static readonly WPos ArenaCenter = new(100, 100);
}
