﻿namespace BossMod.Shadowbringers.Alliance.A31KnaveofHearts;

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9955)]
public class A31KnaveofHearts(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsCircle(new(-800, -750), 30));