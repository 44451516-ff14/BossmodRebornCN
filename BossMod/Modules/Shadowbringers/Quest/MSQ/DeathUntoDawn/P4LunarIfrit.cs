namespace BossMod.Shadowbringers.Quest.MSQ.DeathUntoDawn.P4;

public enum OID : uint
{
    Boss = 0x3202,
    Helper = 0x233C,
    InfernalNail = 0x3205,
}

public enum AID : uint
{
    RadiantPlume1 = 24057, // Helper->self, 7.0s cast, range 8 circle
    Hellfire1 = 24058, // Boss->self, 36.0s cast, range 40 circle
    Hellfire2 = 24059, // Boss->self, 28.0s cast, range 40 circle
    CrimsonCyclone = 24054, // 3203->self, 4.5s cast, range 49 width 18 rect
    Explosion = 24046, // 3204->self, 5.0s cast, range 80 width 10 cross
    AgonyOfTheDamned1 = 24062, // Helper->self, 0.7s cast, range 40 circle
}

class Hellfire1(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hellfire1);
class Hellfire2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hellfire2);
class AgonyOfTheDamned(BossModule module) : Components.RaidwideCast(module, (uint)AID.AgonyOfTheDamned1);
class RadiantPlume(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RadiantPlume1, 8f);
class CrimsonCyclone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrimsonCyclone, new AOEShapeRect(49f, 9f), 3);
class InfernalNail(BossModule module) : Components.Adds(module, (uint)OID.InfernalNail, 5);
class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeCross(80f, 5f), 2);

class LunarIfritStates : StateMachineBuilder
{
    public LunarIfritStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RadiantPlume>()
            .ActivateOnEnter<CrimsonCyclone>()
            .ActivateOnEnter<Hellfire1>()
            .ActivateOnEnter<Hellfire2>()
            .ActivateOnEnter<InfernalNail>()
            .ActivateOnEnter<AgonyOfTheDamned>()
            .ActivateOnEnter<Explosion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10041)]
public class LunarIfrit(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsCircle(20f));
