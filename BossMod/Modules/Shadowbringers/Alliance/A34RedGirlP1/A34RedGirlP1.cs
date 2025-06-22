namespace BossMod.Shadowbringers.Alliance.A34RedGirlP1;

class Cruelty1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Cruelty1);
class ShockWhite2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockWhite2, 5);
class ShockBlack2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockBlack2, 5);
class GenerateBarrier2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrier2, new AOEShapeRect(18, 1.5f));
class GenerateBarrier3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrier3, new AOEShapeRect(24, 1.5f));
class DiffuseEnergy1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffuseEnergy1, new AOEShapeCone(12, 60.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9920)]
public class A34RedGirlP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(845, -851), new ArenaBoundsRect(19, 20));
