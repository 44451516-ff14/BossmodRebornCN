namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Katarraktes(BossModule module) : Components.CastCounter(module, (uint)AID.KatarraktesAOE);
class Thlipsis(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThlipsisAOE, 6f, 8);
class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HydroptosisAOE, 6f);
class Rhyton(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.Rhyton, (uint)AID.RhytonAOE, 6f);
class Bank(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftBank, (uint)AID.RightBank, (uint)AID.HieroglyphikaLeftBank,
(uint)AID.HieroglyphikaRightBank], new AOEShapeCone(60f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11298, SortOrder = 2, PlanLevel = 90)]
public class A31Thaliak(WorldState ws, Actor primary) : BossModule(ws, primary, TetraktysBorder.NormalCenter, new ArenaBoundsSquare(24f));
