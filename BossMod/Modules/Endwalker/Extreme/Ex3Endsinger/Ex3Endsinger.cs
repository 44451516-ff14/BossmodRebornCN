namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

// raidwide is slightly delayed
sealed class Elegeia(BossModule module) : Components.CastCounter(module, (uint)AID.Elegeia);

sealed class Telomania(BossModule module) : Components.CastCounter(module, (uint)AID.TelomaniaLast);

sealed class UltimateFate(BossModule module) : Components.CastCounter(module, (uint)AID.EnrageAOE);

// TODO: proper tankbuster component...
sealed class Hubris(BossModule module) : Components.CastCounter(module, (uint)AID.HubrisAOE);

// TODO: proper stacks component
sealed class Eironeia(BossModule module) : Components.CastCounter(module, (uint)AID.EironeiaAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 846u, NameID = 10448u, PlanLevel = 90)]
public sealed class Ex3Endsinger(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
