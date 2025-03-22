namespace BossMod.Dawntrail.Hunt.RankA.Starcrier;

public enum OID : uint
{
    Boss = 0x41FC // R5.0
}
public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    WingsbreadthWinds = 37038, // Boss->self, 5.0s cast, range 8 circle
    StormwallWinds = 37039, // Boss->self, 5.0s cast, range 8-25 donut
    DirgeOfTheLost = 37040, // Boss->self, 3.0s cast, range 40 circle, applies Temporary Misdirection
    AeroIV = 37163, // Boss->self, 4.0s cast, range 20 circle
    SwiftwindSerenade = 37305 // Boss->self, 4.0s cast, range 40 width 8 rect
}

class WingsbreadthWinds(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.WingsbreadthWinds), 8f);
class StormwallWinds(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StormwallWinds), new AOEShapeDonut(8f, 25f));
class DirgeOfTheLost(BossModule module) : Components.TemporaryMisdirection(module, ActionID.MakeSpell(AID.DirgeOfTheLost));
class AeroIV(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AeroIV));
class SwiftwindSerenade(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.SwiftwindSerenade), new AOEShapeRect(40f, 4f));

class StarcrierStates : StateMachineBuilder
{
    public StarcrierStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WingsbreadthWinds>()
            .ActivateOnEnter<StormwallWinds>()
            .ActivateOnEnter<DirgeOfTheLost>()
            .ActivateOnEnter<AeroIV>()
            .ActivateOnEnter<SwiftwindSerenade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 12692)]
public class Starcrier(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
