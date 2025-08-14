namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V15ThorneKnight;

sealed class BlisteringBlow(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.BlisteringBlow);
sealed class BlazingBeacon(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BlazingBeacon1, (uint)AID.BlazingBeacon2], new AOEShapeRect(50f, 8f));
sealed class SacredFlay(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SacredFlay1, (uint)AID.SacredFlay2], new AOEShapeCone(50f, 45f.Degrees()));
sealed class SignalFlare : Components.SimpleAOEs
{
    public SignalFlare(BossModule module) : base(module, (uint)AID.SignalFlare, 10f, 6)
    {
        MaxDangerColor = 3;
    }
}
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeCross(50f, 3f));
sealed class ForeHonor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ForeHonor, new AOEShapeCone(50f, 90f.Degrees()));
sealed class Cogwheel(BossModule module) : Components.RaidwideCast(module, (uint)AID.Cogwheel);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.ThorneKnight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11419, SortOrder = 6, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V15ThorneKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new(289f, -230f), new ArenaBoundsSquare(17.5f, 45f.Degrees()));