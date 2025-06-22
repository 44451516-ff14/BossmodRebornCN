namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

abstract class MadeMagic(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(50, 15));
class MadeMagic1(BossModule module) : MadeMagic(module, (uint)AID.MadeMagic1);
class MadeMagic2(BossModule module) : MadeMagic(module, (uint)AID.MadeMagic2);

class ScreamingScore(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScreamingScore);
class ScatteredMagic(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScatteredMagic, 4);
class DarkerNote2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkerNote2, 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948)]
public class A36FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, -700), new ArenaBoundsSquare(25));
