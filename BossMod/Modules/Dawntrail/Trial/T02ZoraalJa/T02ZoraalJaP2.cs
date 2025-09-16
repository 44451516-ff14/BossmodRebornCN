namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

sealed class SmitingCircuitHalfCircuitDonut(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SmitingCircuitDonut, (uint)AID.HalfCircuitDonut], new AOEShapeDonut(10f, 30f));
sealed class SmitingCircuitHalfCircuitCircle(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SmitingCircuitCircle, (uint)AID.HalfCircuitCircle], 10f);
sealed class DawnOfAnAgeActualize(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.DawnOfAnAge, (uint)AID.Actualize]);
sealed class BitterReaping(BossModule module) : Components.SingleTargetCast(module, (uint)AID.BitterReaping);

abstract class HalfRect(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeRect(60f, 30f));
sealed class HalfFull(BossModule module) : HalfRect(module, (uint)AID.HalfFull)
{
    private readonly ChasmOfVollok _aoe = module.FindComponent<ChasmOfVollok>()!;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Casters.Count != 0 && _aoe.AOEs.Count == 0 ? CollectionsMarshal.AsSpan(Casters)[..1] : [];
    }
}

sealed class HalfCircuitRect(BossModule module) : HalfRect(module, (uint)AID.HalfCircuitRect);

sealed class FireIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, (uint)AID.FireIII, 6f, 5.1d);
sealed class DutysEdge(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.DutysEdgeMarker, (uint)AID.DutysEdge, 5.4d, 100f, minStackSize: 8, maxStackSize: 8, maxCasts: 4, markerIsFinalTarget: false);

// P2 is a checkpoint so we can't make it one module since it would prevent reloading the module incase of wipes
sealed class T02ZoraalJaP2States : StateMachineBuilder
{
    public T02ZoraalJaP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<SmitingCircuitHalfCircuitDonut>()
            .ActivateOnEnter<SmitingCircuitHalfCircuitCircle>()
            .ActivateOnEnter<DawnOfAnAgeActualize>()
            .ActivateOnEnter<BitterReaping>()
            .ActivateOnEnter<ChasmOfVollok>()
            .ActivateOnEnter<ForgedTrack>()
            .ActivateOnEnter<HalfFull>()
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<DutysEdge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12882, SortOrder = 2)]
public sealed class T02ZoraalJaP2(WorldState ws, Actor primary) : T02ZoraalJa.ZoraalJa(ws, primary);
