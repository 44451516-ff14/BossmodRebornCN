namespace BossMod.Endwalker.Alliance.A14Naldthal;

class GoldenTenet(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.GoldenTenetAOE, 6f);
class StygianTenet(BossModule module) : Components.BaitAwayCast(module, (uint)AID.StygianTenetAOE, 3f, true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class HellOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HellOfFireFrontAOE, (uint)AID.HellOfFireBackAOE], new AOEShapeCone(60f, 90f.Degrees()));

class WaywardSoul(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaywardSoulAOE, 18f, 3);
class SoulVessel(BossModule module) : Components.Adds(module, (uint)OID.SoulVesselReal);
class Twingaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Twingaze, new AOEShapeCone(60f, 15f.Degrees()));
class MagmaticSpell(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MagmaticSpellAOE, 6f, 8, 24);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11286, SortOrder = 6, PlanLevel = 90)]
public class A14Naldthal(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(750f, -750f), 29f, 180)]);
}
