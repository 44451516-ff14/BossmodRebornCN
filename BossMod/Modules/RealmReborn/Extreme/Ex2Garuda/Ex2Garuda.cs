namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

sealed class DownburstBoss(BossModule module) : Components.Cleave(module, (uint)AID.Downburst1, new AOEShapeCone(11.7f, 60f.Degrees())); // TODO: verify angle

abstract class Downburst(BossModule module, uint aid, uint oid) : Components.Cleave(module, aid, new AOEShapeCone(11.36f, 60f.Degrees()), [oid]); // TODO: verify angle
sealed class DownburstSuparna(BossModule module) : Downburst(module, (uint)AID.Downburst1, (uint)OID.Suparna); // TODO: verify angle
sealed class DownburstChirada(BossModule module) : Downburst(module, (uint)AID.Downburst2, (uint)OID.Chirada); // TODO: verify angle

sealed class Slipstream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Slipstream, new AOEShapeCone(11.7f, 45f.Degrees()));
sealed class FrictionAdds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrictionAdds, 5f);
sealed class FeatherRain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeatherRain, 3f);
sealed class AerialBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.AerialBlast);
sealed class MistralShriek(BossModule module) : Components.RaidwideCast(module, (uint)AID.MistralShriek);
sealed class Gigastorm(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Gigastorm, 6.5f);
sealed class GreatWhirlwind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWhirlwind, 8f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 65u, NameID = 1644u)]
public sealed class Ex2Garuda : BossModule
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
