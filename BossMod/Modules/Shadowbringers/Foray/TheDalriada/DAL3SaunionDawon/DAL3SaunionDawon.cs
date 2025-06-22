namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

class HighPoweredMagitekRay(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HighPoweredMagitekRay);
class ToothAndTalon(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ToothAndTalon);
class PentagustAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PentagustAOE, new AOEShapeCone(50, 10.Degrees()));
class RawHeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RawHeat, 10);
class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6);
class SwoopingFrenzyAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwoopingFrenzyAOE, 12);
class Touchdown3(BossModule module) : Components.RaidwideCast(module, (uint)AID.Touchdown3);
class MissileSalvo(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MissileSalvo, 6);
class MagitekCrossray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekCrossray, new AOEShapeCross(60, 8));

class Donut(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(7, 60));
class MagitekHalo(BossModule module) : Donut(module, (uint)AID.MagitekHalo);
class FrigidPulseAOE(BossModule module) : Donut(module, (uint)AID.FrigidPulseAOE);

class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 32, SortOrder = 4)] //BossNameID = 10192 & 10006
public class DAL3SaunionDawon : BossModule
{
    public readonly List<Actor> Boss;
    public readonly List<Actor> Dawon;

    public DAL3SaunionDawon(WorldState ws, Actor primary) : base(ws, primary, new(650f, -659f), new ArenaBoundsSquare(26.5f))
    {
        Boss = Enemies(OID.Boss);
        Dawon = Enemies(OID.Dawon);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Boss);
        Arena.Actors(Dawon);
        Arena.Actor(PrimaryActor);
    }
}
