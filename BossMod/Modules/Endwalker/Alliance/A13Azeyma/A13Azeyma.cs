namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WardensWarmth(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.WardensWarmthAOE), 6);
class FleetingSpark(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FleetingSpark), new AOEShapeCone(60, 135.Degrees()));
class SolarFold(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SolarFoldAOE), new AOEShapeCross(30, 5));
class Sunbeam(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Sunbeam), 9, 14);
class SublimeSunset(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SublimeSunsetAOE), 40); // TODO: check falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11277, SortOrder = 5, PlanLevel = 90)]
public class A13Azeyma(WorldState ws, Actor primary) : BossModule(ws, primary, NormalCenter, NormalBounds)
{
    public static readonly WPos NormalCenter = new(-750, -750);
    public static readonly ArenaBoundsCircle NormalBounds = new(29.5f);
}
