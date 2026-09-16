namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

sealed class Rout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rout, new AOEShapeRect(55f, 3f));
sealed class FleetingSummer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FleetingSummer, new AOEShapeCone(40f, 45f.Degrees()));
sealed class WellOfFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WellOfFlame, new AOEShapeRect(41f, 10f));
sealed class ScathingNet(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, (uint)AID.ScathingNet, 6f, 5.1d, 8, 8);
sealed class PhantomFlurryTB(BossModule module) : Components.TankSwap(module, (uint)AID.PhantomFlurryVisual, (uint)AID.PhantomFlurryTB, (uint)AID.AutoAttack2, default, 3.5d);
sealed class PhantomFlurryAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhantomFlurryAOE, new AOEShapeCone(41f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), Kismet", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 597u, NameID = 7702u, PlanLevel = 70)]
public sealed class Ex7Suzaku : BossModule
{
    public Ex7Suzaku(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Ex7Suzaku(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 19.5f, 80)]);
        return (arena.Center, arena);
    }

    public static ArenaBoundsCustom GetPhase2Bounds() => new([new DonutV(new(100f, 100f), 3.5f, 20f, 80)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ScarletLady), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.ScarletPlume));
    }
}
