namespace BossMod.Heavensward.Alliance.A21ArachneEve;

class DarkSpike(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DarkSpike);
class SilkenSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilkenSpray, new AOEShapeCone(24.5f, 30f.Degrees()));
class ShadowBurst(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ShadowBurst, 6, 8);
class SpiderThread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SpiderThread, 6);
sealed class Pitfall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pitfall1, 20f);
class FrondAffeared(BossModule module) : Components.CastGaze(module, (uint)AID.FrondAffeared);
class TheWidowsEmbrace(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TheWidowsEmbrace, 18f, kind: Kind.TowardsOrigin, stopAtWall: true);
class TheWidowsKiss(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.TheWidowsKiss, 4, kind: Kind.TowardsOrigin, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4871)]
[SkipLocalsInit]
public sealed class A21ArachneEve : BossModule
{
    public A21ArachneEve(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private A21ArachneEve(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(16f, -55f), 29.1f, 96)],
        [new Rectangle(new(16f, -84.5f), 22.9f, 1.25f), new Rectangle(new(16f, -25.6f), 22.9f, 1.25f)]);
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Keyknot));
        Arena.Actors(Enemies((uint)OID.Webmaiden));
        Arena.Actors(Enemies((uint)OID.SpittingSpider));
        Arena.Actors(Enemies((uint)OID.SkitteringSpider));
        Arena.Actors(Enemies((uint)OID.EarthAether));
        Arena.Actors(Enemies((uint)OID.DeepEarthAether));
    }
}
