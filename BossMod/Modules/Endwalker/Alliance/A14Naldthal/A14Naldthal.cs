namespace BossMod.Endwalker.Alliance.A14Naldthal;

sealed class GoldenTenet(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.GoldenTenetAOE, 6f);
sealed class StygianTenet(BossModule module) : Components.BaitAwayCast(module, (uint)AID.StygianTenetAOE, 3f, true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class HellOfFire(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HellOfFireFrontAOE, (uint)AID.HellOfFireBackAOE], new AOEShapeCone(60f, 90f.Degrees()));

sealed class WaywardSoul(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaywardSoulAOE, 18f, 3);
sealed class SoulVessel(BossModule module) : Components.Adds(module, (uint)OID.SoulVesselReal);
sealed class Twingaze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Twingaze, new AOEShapeCone(60f, 15f.Degrees()));
sealed class MagmaticSpell(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MagmaticSpellAOE, 6f, 8, 24);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11286, SortOrder = 6, PlanLevel = 90)]
public sealed class A14Naldthal : BossModule
{
    public A14Naldthal(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A14Naldthal(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(750f, -750f), 29f, 180)]);
        return (arena.Center, arena);
    }
}
