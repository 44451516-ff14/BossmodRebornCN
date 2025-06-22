namespace BossMod.Stormblood.Quest.Job.BloodOnTheDeck;

public enum OID : uint
{
    Boss = 0x1BED,
    ShamShinobi = 0x1BE8, // R0.5
    AdjunctOstyrgreinHelper = 0x1BEB, // R0.5
    AdjunctOstyrgrein = 0x1BEA, // R0.5
    Vanara = 0x1BE9, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    ScytheTail = 8407, // Vanara->self, 5.0s cast, range 4+R circle
    Butcher = 8405, // Vanara->self, 5.0s cast, range 6+R ?-degree cone
    TenkaGoken = 8408, // AdjunctOstyrgrein->self, 5.0s cast, range 8+R 120-degree cone
    Bombslinger1 = 8411, // AdjunctOstyrgreinHelper->location, 3.0s cast, range 6 circle
}

class ScytheTail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScytheTail, 7);
class Butcher(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Butcher, new AOEShapeCone(9, 45.Degrees()));
class TenkaGoken(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenkaGoken, new AOEShapeCone(8.5f, 60.Degrees()));
class Bombslinger(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombslinger1, 6);

class GurumiBorlumiStates : StateMachineBuilder
{
    public GurumiBorlumiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScytheTail>()
            .ActivateOnEnter<Butcher>()
            .ActivateOnEnter<TenkaGoken>()
            .ActivateOnEnter<Bombslinger>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68098, NameID = 6289)]
public class GurumiBorlumi(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 15.8f), new ArenaBoundsRect(8, 7.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly));
}

