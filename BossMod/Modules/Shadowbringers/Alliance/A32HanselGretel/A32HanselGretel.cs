namespace BossMod.Shadowbringers.Alliance.A32HanselGretel;

sealed class WailLamentation(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Wail1, (uint)AID.Wail2, (uint)AID.Lamentation1, (uint)AID.Lamentation2]);

sealed class CripplingBlow1(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.CripplingBlow1);
sealed class CripplingBlow2(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.CripplingBlow2);

sealed class BloodySweep(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BloodySweep1, (uint)AID.BloodySweep2,
(uint)AID.BloodySweep3, (uint)AID.BloodySweep4], new AOEShapeRect(50, 12.5f));

sealed class PassingLance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PassingLance, new AOEShapeRect(50f, 12f));
sealed class UnevenFooting(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnevenFooting, 23f);

sealed class HungryLance(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HungryLance1, (uint)AID.HungryLance2], new AOEShapeCone(40f, 60.Degrees()));

sealed class Breakthrough(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Breakthrough, new AOEShapeRect(53f, 16f));
sealed class SeedOfMagicBeta(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SeedOfMagicBeta, 5f);

sealed class UpgradedShield(BossModule module) : Components.DirectionalParry(module, [(uint)OID.Gretel, (uint)OID.Hansel])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.UpgradedShield1 or (uint)AID.UpgradedShield2)
        {
            PredictParrySide(caster.InstanceID, Side.All ^ Side.Front);
        }
    }
}

sealed class MagicalConfluence(BossModule module) : Components.Voidzone(module, 4f, GetVoidzones, 8f)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.MagicalConfluence);
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Gretel, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9990, SortOrder = 2)]
public sealed class A32HanselGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800f, -951.03119f), new ArenaBoundsCircle(24.5f))
{
    public Actor? BossHansel;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        if (BossHansel == null)
        {
            var b = Enemies((uint)OID.Hansel);
            BossHansel = b.Count != 0 ? b[0] : null;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossHansel);
    }
}
