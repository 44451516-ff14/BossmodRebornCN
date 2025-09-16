namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

class DownburstBoss(BossModule module) : Components.Cleave(module, (uint)AID.Downburst1, new AOEShapeCone(11.7f, 60.Degrees())); // TODO: verify angle

abstract class Downburst(BossModule module, uint aid, OID oid) : Components.Cleave(module, aid, new AOEShapeCone(11.36f, 60.Degrees()), [(uint)oid]); // TODO: verify angle
class DownburstSuparna(BossModule module) : Downburst(module, (uint)AID.Downburst1, OID.Suparna); // TODO: verify angle
class DownburstChirada(BossModule module) : Downburst(module, (uint)AID.Downburst2, OID.Chirada); // TODO: verify angle

class Slipstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Slipstream, new AOEShapeCone(11.7f, 45.Degrees()));
class FrictionAdds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrictionAdds, 5);
class FeatherRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeatherRain, 3);
class AerialBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.AerialBlast);
class MistralShriek(BossModule module) : Components.RaidwideCast(module, (uint)AID.MistralShriek);
class Gigastorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gigastorm, 6.5f);
class GreatWhirlwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWhirlwind, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 65, NameID = 1644)]
public class Ex2Garuda : BossModule
{
    public readonly List<Actor> Monoliths;
    public readonly List<Actor> RazorPlumes;
    public readonly List<Actor> SpinyPlumes;
    public readonly List<Actor> SatinPlumes;
    public readonly List<Actor> Chirada;
    public readonly List<Actor> Suparna;

    public Ex2Garuda(WorldState ws, Actor primary) : base(ws, primary, new(0, 0), new ArenaBoundsCircle(22))
    {
        Monoliths = Enemies((uint)OID.Monolith);
        RazorPlumes = Enemies((uint)OID.RazorPlume);
        SpinyPlumes = Enemies((uint)OID.SpinyPlume);
        SatinPlumes = Enemies((uint)OID.SatinPlume);
        Chirada = Enemies((uint)OID.Chirada);
        Suparna = Enemies((uint)OID.Suparna);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Monoliths.Where(a => !a.IsDead), Colors.Object, true);
        Arena.Actors(RazorPlumes);
        Arena.Actors(SpinyPlumes);
        Arena.Actors(SatinPlumes);
        Arena.Actors(Chirada);
        Arena.Actors(Suparna);
    }
}
