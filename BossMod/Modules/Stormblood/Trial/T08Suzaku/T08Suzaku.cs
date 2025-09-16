namespace BossMod.Stormblood.Trial.T08Suzaku;

class ScarletFever(BossModule module) : BossComponent(module)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScarletFever)
            Arena.Bounds = T08Suzaku.Phase2Arena;
    }
}

class ScreamsOfTheDamned(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScreamsOfTheDamned);
class SouthronStar(BossModule module) : Components.RaidwideCast(module, (uint)AID.SouthronStar);
class AshesToAshes(BossModule module) : Components.RaidwideCast(module, (uint)AID.AshesToAshes);
class ScarletFeverAOE(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScarletFever);

class RuthlessRefrain(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuthlessRefrain, 8);
class Cremate(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Cremate);
class PhantomFlurryTankbuster(BossModule module) : Components.SingleTargetCast(module, (uint)AID.PhantomFlurryTankbuster);
class PhantomFlurryAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhantomFlurryAOE, new AOEShapeCone(41, 90.Degrees()));
class FleetingSummer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FleetingSummer, new AOEShapeCone(40, 45.Degrees()));
class Hotspot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hotspot, new AOEShapeCone(21, 60.Degrees()));
class Swoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Swoop, new AOEShapeRect(55, 3));
class WellOfFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WellOfFlame, new AOEShapeRect(41, 10));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 596, NameID = 6221)]
public class T08Suzaku(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(20))
{
    private static readonly WPos ArenaCenter = new(100, 100);
    public static readonly ArenaBoundsCustom Phase2Arena = new([new Donut(ArenaCenter, 4, 20)]);
}
