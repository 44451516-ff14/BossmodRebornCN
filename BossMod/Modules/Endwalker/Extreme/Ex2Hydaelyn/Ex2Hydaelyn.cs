namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

sealed class HerosSundering(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HerosSundering, new AOEShapeCone(40f, 45f.Degrees()));
sealed class Aureole(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LateralAureole1AOE, (uint)AID.LateralAureole2AOE,
(uint)AID.Aureole1AOE, (uint)AID.Aureole2AOE], new AOEShapeCone(40f, 75f.Degrees()));
sealed class MousaScorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.MousaScorn, 4f);

// cast counter for pre-intermission AOE
sealed class PureCrystal(BossModule module) : Components.CastCounter(module, (uint)AID.PureCrystal);

// cast counter for post-intermission AOE
sealed class Exodus(BossModule module) : Components.CastCounter(module, (uint)AID.Exodus);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 791u, NameID = 10453u, PlanLevel = 90)]
public sealed class Ex2Hydaelyn(WorldState ws, Actor primary) : Trial.T02Hydaelyn.HydaelynTrial(ws, primary);
