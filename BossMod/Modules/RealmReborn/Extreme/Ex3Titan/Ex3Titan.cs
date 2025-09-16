namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class WeightOfTheLand(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WeightOfTheLandAOE, 6);
class GaolerVoidzone(BossModule module) : Components.Voidzone(module, 5, m => m.Enemies((uint)OID.GaolerVoidzone).Where(e => e.EventState != 7));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 64, NameID = 1801, PlanLevel = 50)]
public class Ex3Titan : BossModule
{
    private readonly List<Actor> _heart;
    public Actor? Heart() => _heart.Count != 0 ? _heart[0] : null;

    public readonly List<Actor> Gaolers;
    public readonly List<Actor> Gaols;
    public readonly List<Actor> Bombs;

    public Ex3Titan(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(25))
    {
        _heart = Enemies((uint)OID.TitansHeart);
        Gaolers = Enemies((uint)OID.GraniteGaoler);
        Gaols = Enemies((uint)OID.GraniteGaol);
        Bombs = Enemies((uint)OID.BombBoulder);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
        Arena.Actors(Gaolers);
        Arena.Actors(Gaols, Colors.Object);
        Arena.Actors(Bombs, Colors.Object);
    }
}
