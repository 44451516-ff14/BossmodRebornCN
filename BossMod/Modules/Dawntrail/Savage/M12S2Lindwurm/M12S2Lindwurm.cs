using BossMod.Dawntrail.Savage.M12SLindwurm;

namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

class ArcadiaAflame(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadiaAflame);
class IdyllicDreamRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.IdyllicDream);
class LindwurmsMeteor(BossModule module) : Components.RaidwideCast(module, (uint)AID.LindwurmsMeteor);
class ArcadianHell5x(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadianHell4x);
class ArcadianHell9x(BossModule module) : Components.RaidwideCast(module, (uint)AID.ArcadianHell8x);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
GroupType = BossModuleInfo.GroupType.CFC,
StatesType = typeof(M12S2LindwurmStates),
ConfigType = typeof(M12S2LindwurmConfig),
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = typeof(TetherID),
PrimaryActorOID = (uint)OID.Boss,
Contributors = "BossMod Team, ported by Topas",
GroupID = 1075,
NameID = 14379,
SortOrder = 1,
PlanLevel = 100)]
public class M12S2TheLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));