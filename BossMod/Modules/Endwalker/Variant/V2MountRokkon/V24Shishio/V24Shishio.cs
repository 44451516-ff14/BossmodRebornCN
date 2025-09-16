namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V24Shishio;

sealed class NoblePursuit(BossModule module) : Components.ChargeAOEs(module, (uint)AID.NoblePursuit, 6f);
sealed class Enkyo(BossModule module) : Components.RaidwideCast(module, (uint)AID.Enkyo);

abstract class CloudToCloud : Components.SimpleAOEs
{
    protected CloudToCloud(BossModule module, uint aid, float halfWidth, int dangerCount) : base(module, aid, new AOEShapeRect(100f, halfWidth))
    {
        MaxDangerColor = dangerCount;
    }
}
sealed class CloudToCloud1(BossModule module) : CloudToCloud(module, (uint)AID.CloudToCloud1, 1f, 6);
sealed class CloudToCloud2(BossModule module) : CloudToCloud(module, (uint)AID.CloudToCloud2, 3f, 4);
sealed class CloudToCloud3(BossModule module) : CloudToCloud(module, (uint)AID.CloudToCloud3, 6f, 2);

sealed class SplittingCry(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 7f), (uint)IconID.Tankbuster, (uint)AID.SplittingCry, 5d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class ThunderVortex(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderVortex, new AOEShapeDonut(8f, 30f));
sealed class UnsagelySpinYokiThunderOneTwoThreefold(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.UnsagelySpin, (uint)AID.Yoki,
(uint)AID.ThunderOnefold, (uint)AID.ThunderTwofold, (uint)AID.ThunderThreefold], 6f);
sealed class Rush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Rush, 4f);
sealed class Vasoconstrictor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Vasoconstrictor, 5f);

sealed class RightLeftSwipe(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSwipe, (uint)AID.LeftSwipe], new AOEShapeCone(40f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Shishio, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12428, SortOrder = 6, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V24Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(20f))
{
    public static readonly WPos ArenaCenter = new(-40f, -300f);
    public static readonly ArenaBoundsCustom CircleBounds = new([new Circle(ArenaCenter, 20f)], [new Rectangle(ArenaCenter + new WDir(-20f, default), 0.5f, 20f),
    new Rectangle(ArenaCenter + new WDir(20f, default), 0.5f, 20f), new Rectangle(ArenaCenter + new WDir(default, 20f), 20f, 0.5f), new Rectangle(ArenaCenter + new WDir(default, -20f), 20f, 0.5f)]);
}
