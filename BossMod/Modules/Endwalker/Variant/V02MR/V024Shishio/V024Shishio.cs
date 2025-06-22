namespace BossMod.Endwalker.VariantCriterion.V02MR.V024Shishio;

class NoblePursuit(BossModule module) : Components.ChargeAOEs(module, (uint)AID.NoblePursuit, 6f);
class Enkyo(BossModule module) : Components.RaidwideCast(module, (uint)AID.Enkyo);

abstract class CloudToCloud : Components.SimpleAOEs
{
    protected CloudToCloud(BossModule module, uint aid, float halfWidth, int dangerCount) : base(module, aid, new AOEShapeRect(100f, halfWidth)) { MaxDangerColor = dangerCount; }
}
class CloudToCloud1(BossModule module) : CloudToCloud(module, (uint)AID.CloudToCloud1, 1f, 6);
class CloudToCloud2(BossModule module) : CloudToCloud(module, (uint)AID.CloudToCloud2, 3f, 4);
class CloudToCloud3(BossModule module) : CloudToCloud(module, (uint)AID.CloudToCloud3, 6f, 2);

abstract class Thunder(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
class ThunderOnefold(BossModule module) : Thunder(module, (uint)AID.ThunderOnefold);
class ThunderTwofold(BossModule module) : Thunder(module, (uint)AID.ThunderTwofold);
class ThunderThreefold(BossModule module) : Thunder(module, (uint)AID.ThunderThreefold);

class SplittingCry(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(60f, 7f), (uint)IconID.Tankbuster, (uint)AID.SplittingCry, 5f, tankbuster: true);

class ThunderVortex(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderVortex, new AOEShapeDonut(8f, 30f));

class Circles(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 6f);
class UnsagelySpin(BossModule module) : Circles(module, (uint)AID.UnsagelySpin);
class Yoki(BossModule module) : Circles(module, (uint)AID.Yoki);

class Rush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Rush, 4f);
class Vasoconstrictor(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Vasoconstrictor, 5f);

class Swipe(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(40f, 90f.Degrees()));
class RightSwipe(BossModule module) : Swipe(module, (uint)AID.RightSwipe);
class LeftSwipe(BossModule module) : Swipe(module, (uint)AID.LeftSwipe);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12428, SortOrder = 5)]
public class V024Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, NormalBounds)
{
    public static readonly WPos ArenaCenter = new(-40f, -300f);
    public static readonly ArenaBoundsSquare NormalBounds = new(19.5f);
    public static readonly ArenaBoundsComplex CircleBounds = new([new Circle(ArenaCenter, 20f)], [new Rectangle(ArenaCenter + new WDir(-20f, default), 0.5f, 20f),
    new Rectangle(ArenaCenter + new WDir(20f, default), 0.5f, 20f), new Rectangle(ArenaCenter + new WDir(default, 20f), 20f, 0.5f), new Rectangle(ArenaCenter + new WDir(default, -20f), 20f, 0.5f)]);
}
