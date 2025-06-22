namespace BossMod.Shadowbringers.Alliance.A24TheCompound;

class MechanicalLaceration1(BossModule module) : Components.RaidwideCast(module, (uint)AID.MechanicalLaceration1);
class MechanicalDissection(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalDissection, new AOEShapeRect(85, 5.5f));
class MechanicalDecapitation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalDecapitation, new AOEShapeDonut(8, 43));
class MechanicalContusionGround(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MechanicalContusionGround, 6);
class MechanicalContusionSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MechanicalContusionSpread, 6);
class IncongruousSpinAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IncongruousSpinAOE, new AOEShapeRect(80, 75));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646)]
public class A24TheCompound(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsSquare(29.5f));
