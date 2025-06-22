namespace BossMod.Shadowbringers.Hunt.RankA.OPoorestPauldia;

public enum OID : uint
{
    Boss = 0x2820 // R=4.025
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    RustingClaw = 16830, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    TailDrive = 16831, // Boss->self, 5.0s cast, range 30+R 90-degree cone
    WordsOfWoe = 16832, // Boss->self, 3.0s cast, range 45+R width 6 rect
    TheSpin = 16833 // Boss->self, 3.0s cast, range 40 circle
}

class RustingClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RustingClaw, new AOEShapeCone(12.025f, 60f.Degrees()));
class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(34.025f, 60f.Degrees()));
class WordsOfWoe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WordsOfWoe, new AOEShapeRect(49.025f, 3f));
class TheSpin(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheSpin);

class OPoorestPauldiaStates : StateMachineBuilder
{
    public OPoorestPauldiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<WordsOfWoe>()
            .ActivateOnEnter<TheSpin>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 8655)]
public class OPoorestPauldia(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
