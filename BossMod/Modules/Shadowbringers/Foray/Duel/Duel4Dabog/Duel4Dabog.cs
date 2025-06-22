namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

abstract class RightArmBlaster(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(100, 3));
sealed class RightArmBlasterFragment(BossModule module) : RightArmBlaster(module, (uint)AID.RightArmBlasterFragment);
sealed class RightArmBlasterBoss(BossModule module) : RightArmBlaster(module, (uint)AID.RightArmBlasterBoss);

sealed class LeftArmSlash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LeftArmSlash, new AOEShapeCone(10, 90.Degrees())); // TODO: verify angle
sealed class LeftArmWave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LeftArmWaveAOE, 24);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaDuel, GroupID = 778, NameID = 19)] // bnpcname=9958
public sealed class Duel4Dabog(WorldState ws, Actor primary) : BossModule(ws, primary, new(250f, 710f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 20f);
}
