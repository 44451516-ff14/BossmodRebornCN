namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

class FlightPath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlightPath), new AOEShapeRect(60, 5, 60));
class AntiPersonnelLaser(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AntiPersonnelLaser), 6);
class HighPoweredLaser(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HighPoweredLaser), 6, 8);
class LifesLastSong(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LifesLastSong), new AOEShapeCone(30, 50.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9642)]
public class A21AegisUnit(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly Circle[] union = [new(new(-230, 192), 25), new(new(-245.1f, 183.2f), 12), new(new(-230, 209.5f), 12), new(new(-214.9f, 183.2f), 12)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(union, [new Circle(new(-230, 192), 10)]);
}
