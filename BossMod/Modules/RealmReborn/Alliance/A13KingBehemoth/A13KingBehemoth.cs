namespace BossMod.RealmReborn.Alliance.A13KingBehemoth;

class EclipticMeteor(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.EclipticMeteor, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors()
    {
        var boulders = Module.Enemies((uint)OID.Comet);
        var count = boulders.Count;
        if (count == 0)
            return [];
        var actors = new List<Actor>();
        for (var i = 0; i < count; ++i)
        {
            var b = boulders[i];
            if (!b.IsDead)
                actors.Add(b);
        }
        return CollectionsMarshal.AsSpan(actors);
    }
}
class SelfDestruct(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SelfDestruct, 8.4f);
class CharybdisAOE(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.CharybdisAOE, GetVoidzones, 0.1f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Charybdis);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 727)]
public class A13KingBehemoth(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110, -380), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.IronGiant));
        Arena.Actors(Enemies((uint)OID.Puroboros));
    }
}
