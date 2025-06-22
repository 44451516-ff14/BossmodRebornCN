namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

class Wail1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Wail1);
class Wail2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Wail2);

class CripplingBlow1(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow1);
class CripplingBlow2(BossModule module) : Components.SingleTargetCast(module, (uint)AID.CripplingBlow2);

abstract class BloodySweep(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(50, 12.5f));
class BloodySweep1(BossModule module) : BloodySweep(module, (uint)AID.BloodySweep1);
class BloodySweep2(BossModule module) : BloodySweep(module, (uint)AID.BloodySweep2);
class BloodySweep3(BossModule module) : BloodySweep(module, (uint)AID.BloodySweep3);
class BloodySweep4(BossModule module) : BloodySweep(module, (uint)AID.BloodySweep4);

class SeedOfMagicAlpha(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.SeedOfMagicAlpha, 5);
class RiotOfMagic(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.RiotOfMagic, 5, 8);

class PassingLance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PassingLance, new AOEShapeRect(50, 12));
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeRect(4, 25));
class UnevenFooting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnevenFooting, 17);

abstract class HungryLance(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40, 60.Degrees()));
class HungryLance1(BossModule module) : HungryLance(module, (uint)AID.HungryLance1);
class HungryLance2(BossModule module) : HungryLance(module, (uint)AID.HungryLance2);

class Breakthrough(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Breakthrough, new AOEShapeRect(53, 16));
class SeedOfMagicBeta(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedOfMagicBeta, 5);
class Lamentation(BossModule module) : Components.RaidwideCast(module, (uint)AID.Lamentation);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", PrimaryActorOID = (uint)OID.Gretel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9990)]
public class A32HanselGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -951), new ArenaBoundsCircle(25))
{
    private Actor? _hansel;

    public Actor? Gretel() => PrimaryActor;
    public Actor? Hansel() => _hansel;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _hansel ??= StateMachine.ActivePhaseIndex == 0 ? Enemies(OID.Hansel).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_hansel);
    }
}
