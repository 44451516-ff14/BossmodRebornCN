namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// simple component tracking raidwide cast at the end of intermission
public sealed class Apomnemoneumata(BossModule module) : Components.CastCounter(module, (uint)AID.ApomnemoneumataNormal);

public sealed class Phlegethon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhlegetonAOE, 5f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 803u, NameID = 10456u, PlanLevel = 90)]
public sealed class Ex1Zodiark(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
