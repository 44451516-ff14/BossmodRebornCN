namespace BossMod.Stormblood.Extreme.Ex6Byakko;

class StormPulseRepeat(BossModule module) : Components.CastCounter(module, (uint)AID.StormPulseRepeat);
class HeavenlyStrike(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HeavenlyStrike, 3f);
class FireAndLightningBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireAndLightningBoss, new AOEShapeRect(54.3f, 10f));
class FireAndLightningAdd(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireAndLightningAdd, new AOEShapeRect(54.75f, 10f));
class SteelClaw(BossModule module) : Components.Cleave(module, (uint)AID.SteelClaw, new AOEShapeCone(17.75f, 60f.Degrees()), [(uint)OID.Hakutei]);
class WhiteHerald(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.WhiteHerald, (uint)AID.WhiteHerald, 15f, 5.1f); // TODO: verify falloff
class DistantClap(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DistantClap, new AOEShapeDonut(4f, 25f));
class SweepTheLegBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SweepTheLegBoss, new AOEShapeCone(28.3f, 135f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 291, NameID = 7092, PlanLevel = 70)]
public class Ex6Byakko(WorldState ws, Actor primary) : BossModule(ws, primary, default, NormalBounds)
{
    public static readonly ArenaBoundsComplex NormalBounds = new([new Polygon(default, 19.5f, 48)]);
    public static readonly ArenaBoundsComplex IntermissionBounds = new([new Polygon(default, 15, 48)]);

    private Actor? _hakutei;
    public Actor? Boss() => PrimaryActor;
    public Actor? Hakutei() => _hakutei;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _hakutei ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.Hakutei).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_hakutei);
    }
}
