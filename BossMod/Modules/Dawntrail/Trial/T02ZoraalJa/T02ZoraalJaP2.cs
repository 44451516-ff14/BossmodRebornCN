namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

abstract class Donuts(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeDonut(10, 30));
class SmitingCircuitDonut(BossModule module) : Donuts(module, AID.SmitingCircuitDonut);
class HalfCircuitDonut(BossModule module) : Donuts(module, AID.HalfCircuitDonut);

abstract class Circles(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCircle(10));
class SmitingCircuitCircle(BossModule module) : Circles(module, AID.SmitingCircuitCircle);
class HalfCircuitCircle(BossModule module) : Circles(module, AID.HalfCircuitCircle);

class DawnOfAnAge(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DawnOfAnAge));
class BitterReaping(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BitterReaping));
class Actualize(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Actualize));
class HalfFull(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfFull), new AOEShapeRect(60, 30))
{
    private readonly ChasmOfVollok _aoe = module.FindComponent<ChasmOfVollok>()!;
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _aoe.AOEs.Count == 0
            ? ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Color == 0 ? Colors.AOE : Color, Risky)) : ([]);
    }
}

class HalfCircuitRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HalfCircuitRect), new AOEShapeRect(60, 30));
class FireIII(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.FireIII), 6, 5.1f);
class DutysEdge(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.DutysEdgeMarker), ActionID.MakeSpell(AID.DutysEdge), 5.4f, 100, minStackSize: 8, maxStackSize: 8, maxCasts: 4, markerIsFinalTarget: false);

// P2 is a checkpoint so we can't make it one module since it would prevent reloading the module incase of wipes
class T02ZoraalJaP2States : StateMachineBuilder
{
    public T02ZoraalJaP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Components.StayInBounds>()
            .ActivateOnEnter<DawnOfAnAgeArenaChange>()
            .ActivateOnEnter<SmitingCircuitDonut>()
            .ActivateOnEnter<SmitingCircuitCircle>()
            .ActivateOnEnter<DawnOfAnAge>()
            .ActivateOnEnter<BitterReaping>()
            .ActivateOnEnter<ChasmOfVollok>()
            .ActivateOnEnter<ForgedTrack>()
            .ActivateOnEnter<Actualize>()
            .ActivateOnEnter<HalfFull>()
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<HalfCircuitDonut>()
            .ActivateOnEnter<HalfCircuitCircle>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<DutysEdge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12882, SortOrder = 2)]
public class T02ZoraalJaP2(WorldState ws, Actor primary) : T02ZoraalJa.ZoraalJa(ws, primary);
