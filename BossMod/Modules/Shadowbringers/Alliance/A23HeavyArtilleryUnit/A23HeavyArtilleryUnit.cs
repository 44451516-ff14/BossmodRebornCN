namespace BossMod.Shadowbringers.Alliance.A23HeavyArtilleryUnit;

sealed class ManeuverVoltArray(BossModule module) : Components.RaidwideCast(module, (uint)AID.ManeuverVoltArray);
sealed class EnergyBombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EnergyBombardment, 4f);
sealed class R010Laser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R010Laser, new AOEShapeRect(60f, 6f));
sealed class R030Hammer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.R030Hammer, 18f);
sealed class ManeuverHighPoweredLaser(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 4f), (uint)IconID.ManeuverHighPoweredLaser, (uint)AID.ManeuverHighPoweredLaser, 5.4d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class UnconventionalVoltage(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 15f.Degrees()), (uint)IconID.UnconventionalVoltage, (uint)AID.UnconventionalVoltage, 6.8d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9650, SortOrder = 4)]
public sealed class A23HeavyArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new DonutV(new(200f, -100f), 6.5f, 29.5f, 192)]);
}
