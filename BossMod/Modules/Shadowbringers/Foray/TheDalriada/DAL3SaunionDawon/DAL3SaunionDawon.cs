namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL3SaunionDawon;

sealed class HighPoweredMagitekRay(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HighPoweredMagitekRay, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class ToothAndTalon(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ToothAndTalon);
sealed class Pentagust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pentagust, new AOEShapeCone(50f, 10f.Degrees()));
sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class SwoopingFrenzy(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwoopingFrenzy, 12f);
sealed class MissileSalvo(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MissileSalvo, 6f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class MagitekCrossray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekCrossray, MobileHaloCrossray.Cross);
sealed class MagitekHalo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekHalo, MobileHaloCrossray.Donut);
sealed class FrigidPulse(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrigidPulse, Obey.Donut);
sealed class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AntiPersonnelMissile, 6f)
{
    public override bool KeepOnPhaseChange => true;
}
sealed class Touchdown(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Touchdown1, 30f, stopAtWall: true);
sealed class WildfireWinds(BossModule module) : Components.RaidwideCast(module, (uint)AID.WildfireWinds1);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778, NameID = 10192, SortOrder = 5)]
public sealed class DAL3SaunionDawon(WorldState ws, Actor primary) : BossModule(ws, primary, new(650f, -659f), new ArenaBoundsSquare(26.5f))
{
    public Actor? BossDawon;

    protected override void UpdateModule()
    {
        BossDawon ??= GetActor((uint)OID.Dawon);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(BossDawon);
        Arena.Actor(PrimaryActor);
    }
}
