namespace BossMod.Stormblood.Ultimate.UCOB;

class P1Plummet(BossModule module) : Components.Cleave(module, (uint)AID.Plummet, new AOEShapeCone(12f, 60f.Degrees()), [(uint)OID.Twintania]);
class P1Fireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, (uint)AID.Fireball, 4f, 5.3f, 4, 4);
class P2BahamutsClaw(BossModule module) : Components.CastCounter(module, (uint)AID.BahamutsClaw);
class P3FlareBreath(BossModule module) : Components.Cleave(module, (uint)AID.FlareBreath, new AOEShapeCone(29.2f, 45f.Degrees()), [(uint)OID.BahamutPrime]); // TODO: verify angle
class P5MornAfah(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MornAfah, 4f, 8, 8); // TODO: verify radius

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Twintania, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 280, PlanLevel = 70)]
public class UCOB(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(21f))
{
    private Actor? _nael;
    private Actor? _bahamutPrime;

    public Actor? Twintania() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? Nael() => _nael;
    public Actor? BahamutPrime() => _bahamutPrime;

    public override bool ShouldPrioritizeAllEnemies => true;

    protected override void UpdateModule()
    {
        _nael ??= GetActor((uint)OID.NaelDeusDarnus);
        _bahamutPrime ??= GetActor((uint)OID.BahamutPrime);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(Twintania());
        Arena.Actor(Nael());
        Arena.Actor(BahamutPrime());
    }
}
