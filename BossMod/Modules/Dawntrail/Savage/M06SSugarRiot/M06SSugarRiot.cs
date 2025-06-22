using static BossMod.Dawntrail.Raid.SugarRiotSharedBounds.SugarRiotSharedBounds;

namespace BossMod.Dawntrail.Savage.M06SSugarRiot;

sealed class SprayPain1 : Components.SimpleAOEs
{
    public SprayPain1(BossModule module) : base(module, (uint)AID.SprayPain1, 10f, 10)
    {
        MaxDangerColor = 5;
    }
}
sealed class SprayPain2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SprayPain2, 10f);
sealed class LightningBolt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightningBolt, 4f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1022, NameID = 13822, PlanLevel = 100)]
public sealed class M06SSugarRiot(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena);