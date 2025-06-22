namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

sealed class FallingRock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FallingRock, 4f);
sealed class HotCharge(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HotCharge, 4f);
sealed class Firebreathe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firebreathe, new AOEShapeCone(60f, 45f.Degrees()));
sealed class HeadDown(BossModule module) : Components.ChargeAOEs(module, (uint)AID.HeadDown, 2f);
sealed class HuntersClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HuntersClaw, 8f);

sealed class Burn(BossModule module) : Components.BaitAwayIcon(module, 30f, (uint)IconID.Burn, (uint)AID.Burn, 8.2f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761, NameID = 9751, PlanLevel = 80)]
public class DRS3Dahu(WorldState ws, Actor primary) : Dahu(ws, primary)
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.CrownedMarchosias));
    }
}
