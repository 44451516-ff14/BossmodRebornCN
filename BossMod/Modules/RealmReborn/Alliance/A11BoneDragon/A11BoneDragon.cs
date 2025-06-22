namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

class Apocalypse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Apocalypse, 6);
class EvilEye(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EvilEye, new AOEShapeCone(105, 60.Degrees()));
class Stone(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Stone);
class Level5Petrify(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Level5Petrify, new AOEShapeCone(7.8f, 45.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 706)]
public class A11BoneDragon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Circle(new(-450, 30), 15), new Rectangle(new(-450, 0), 10, 20)]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.Platinal));
        Arena.Actors(Enemies(OID.RottingEye));
    }
}
