namespace BossMod.RealmReborn.Alliance.A32FiveheadedDragon;

class WhiteBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WhiteBreath, new AOEShapeCone(30, 60.Degrees()));
class BreathOfFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfFire, 6);
class BreathOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfLight, 6);
class BreathOfPoison(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfPoison, 6);
class BreathOfIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BreathOfIce, 6);

class Radiance(BossModule module) : Components.RaidwideCast(module, (uint)AID.Radiance);
class HeatWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatWave);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3227)]
public class A32FiveheadedDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, 180), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(OID.HeadOfFire));
        Arena.Actors(Enemies(OID.HeadOfPoison));
        Arena.Actors(Enemies(OID.HeadOfThunder));
        Arena.Actors(Enemies(OID.HeadOfIce));
        Arena.Actors(Enemies(OID.Prominence));
        Arena.Actors(Enemies(OID.PoisonSlime));
        Arena.Actors(Enemies(OID.ToxicSlime));
        Arena.Actors(Enemies(OID.DragonfireFly));
    }
}
