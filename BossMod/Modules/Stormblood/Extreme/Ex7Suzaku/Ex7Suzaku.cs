namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

class Rout(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Rout), new AOEShapeRect(55f, 3f));
class FleetingSummer(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.FleetingSummer), new AOEShapeCone(40f, 45f.Degrees()));
class WellOfFlame(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WellOfFlame), new AOEShapeRect(41f, 10f));
class ScathingNet(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.ScathingNet), 6f, 5.1f, 8);
class PhantomFlurryTB(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.PhantomFlurryVisual), ActionID.MakeSpell(AID.PhantomFlurryTB), ActionID.MakeSpell(AID.AutoAttack2), 3.5f);
class PhantomFlurryAOE(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.PhantomFlurryAOE), new AOEShapeCone(41f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus), Kismet", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 597, NameID = 7702, PlanLevel = 70)]
public class Ex7Suzaku(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, Phase1Bounds)
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsComplex Phase1Bounds = new([new Polygon(ArenaCenter, 19.5f, 80)]);
    public static readonly ArenaBoundsComplex Phase2Bounds = new([new DonutV(ArenaCenter, 3.5f, 20f, 80)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ScarletLady), Colors.Vulnerable);
        Arena.Actors(Enemies((uint)OID.ScarletPlume));
    }
}
