namespace BossMod.Dawntrail.Hunt.RankA.CatsEye;

public enum OID : uint
{
    Boss = 0x4396 // R6.02
}

public enum AID : uint
{
    AutoAttack = 38517, // Boss->player, no cast, single-target
    CatsEye1 = 38510, // Boss->location, 7.0s cast, range 40 circle, non-inverted.
    KillerCuriosity = 38514, // Boss->self, 4.0s cast, single-target
    CatsEye2 = 38511, // Boss->location, 7.0s cast, range 40 circle, inverted gaze on end.
    Unknown = 38516, // Boss->self, no cast, single-target (lose Wandering Eyes)
    GravitationalWave = 39887, // Boss->self, 5.0s cast, range 40 circle
    BloodshotGaze = 38515, // Boss->players, 5.0s cast, range 8 circle, non-inverted.
    BloodshotGazeInverted = 39668, // Boss->players, 5.0s cast, range 8 circle, inverted.
}

class CatsEyeGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.CatsEye1));
class CatsEyeInvertedGaze(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.CatsEye2), true);
class GravitationalWave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GravitationalWave));
class BloodshotGaze(BossModule module) : Components.GenericGaze(module)
{
    private readonly BloodshotStack _stack = module.FindComponent<BloodshotStack>()!;
    private readonly BloodshotStackInverted _stackInv = module.FindComponent<BloodshotStackInverted>()!;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        Eye[] TryGetEye(Components.GenericStackSpread comp)
        {
            var count = comp.Stacks.Count;
            if (count == 0)
                return [];
            var stack = CollectionsMarshal.AsSpan(comp.Stacks)[0];
            if (stack.Target == actor)
                return [];
            return [new(stack.Target.Position, stack.Activation)];
        }
        var stack = TryGetEye(_stack);
        return stack.Length != 0 ? stack : TryGetEye(_stackInv);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BloodshotGaze)
            Inverted = false;
        else if (spell.Action.ID == (uint)AID.BloodshotGazeInverted)
            Inverted = true;
    }
}

class BloodshotStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BloodshotGaze), 8f, 8);
class BloodshotStackInverted(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BloodshotGazeInverted), 8f, 8);

class CatsEyeStates : StateMachineBuilder
{
    public CatsEyeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CatsEyeGaze>()
            .ActivateOnEnter<CatsEyeInvertedGaze>()
            .ActivateOnEnter<GravitationalWave>()
            .ActivateOnEnter<BloodshotStack>()
            .ActivateOnEnter<BloodshotStackInverted>()
            .ActivateOnEnter<BloodshotGaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13436)]
public class CatsEye(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
