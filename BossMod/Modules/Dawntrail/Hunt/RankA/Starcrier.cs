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

sealed class WingsbreadthWinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingsbreadthWinds, 8f);
sealed class StormwallWinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StormwallWinds, new AOEShapeDonut(8f, 25f));
sealed class DirgeOfTheLost(BossModule module) : Components.TemporaryMisdirection(module, (uint)AID.DirgeOfTheLost);
sealed class AeroIV(BossModule module) : Components.RaidwideCast(module, (uint)AID.AeroIV);
sealed class SwiftwindSerenade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SwiftwindSerenade, new AOEShapeRect(40f, 4f));

sealed class StarcrierStates : StateMachineBuilder
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
public sealed class Starcrier(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
