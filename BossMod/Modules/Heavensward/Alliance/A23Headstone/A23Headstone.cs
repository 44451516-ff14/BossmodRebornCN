namespace BossMod.Heavensward.Alliance.A23Headstone;

class TremblingEpigraph(BossModule module) : Components.RaidwideCast(module, (uint)AID.TremblingEpigraph);
class FlaringEpigraph(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlaringEpigraph);
class BigBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.BigBurst);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4868)]
[SkipLocalsInit]
public sealed class A23Headstone : BossModule
{
    public A23Headstone(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A23Headstone(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var am60 = -60f.Degrees();
        var arena = new ArenaBoundsCustom([new Polygon(new(-184.52202f, 197.28719f), 16f, 32, am60),
        new Polygon(new(-168.522f, 225f), 20f, 32, am60), new Polygon(new(-152.522f, 252.7128f), 16f, 32, am60)],
        [new Rectangle(new(-185.9566f, 235.09061f), 9f, 0.75f, am60), new Rectangle(new(-150.99809f, 214.88989f), 9f, 0.75f, am60)], AdjustForHitboxInwards: true);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Parthenope));
        Arena.Actors(Enemies((uint)OID.VoidFire));
    }
}
