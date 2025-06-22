using BossMod.Endwalker.Trial.T02Hydaelyn;

namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class HerosSundering(BossModule module) : Components.BaitAwayCast(module, (uint)AID.HerosSundering, new AOEShapeCone(40f, 45f.Degrees()));
class Aureole(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LateralAureole1AOE, (uint)AID.LateralAureole2AOE,
(uint)AID.Aureole1AOE, (uint)AID.Aureole2AOE], new AOEShapeCone(40f, 75f.Degrees()));
class MousaScorn(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.MousaScorn, 4f);

// cast counter for pre-intermission AOE
class PureCrystal(BossModule module) : Components.CastCounter(module, (uint)AID.PureCrystal);

// cast counter for post-intermission AOE
class Exodus(BossModule module) : Components.CastCounter(module, (uint)AID.Exodus);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 791, NameID = 10453, PlanLevel = 90)]
public class Ex2Hydaelyn(WorldState ws, Actor primary) : BossModule(ws, primary, T02Hydaelyn.ArenaCenter, T02Hydaelyn.ArenaBounds);
