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

sealed class CatsEyeGaze(BossModule module) : Components.CastGaze(module, (uint)AID.CatsEye1);
sealed class CatsEyeInvertedGaze(BossModule module) : Components.CastGaze(module, (uint)AID.CatsEye2, true);
sealed class GravitationalWave(BossModule module) : Components.RaidwideCast(module, (uint)AID.GravitationalWave);
sealed class BloodshotGaze(BossModule module) : Components.GenericGaze(module)
{
    private readonly BloodshotStack _stack = module.FindComponent<BloodshotStack>()!;
    private readonly BloodshotStackInverted _stackInv = module.FindComponent<BloodshotStackInverted>()!;

    public override ReadOnlySpan<Eye> ActiveEyes(int slot, Actor actor)
    {
        Eye[] TryGetEye(Components.GenericStackSpread comp, bool inverted)
        {
            var count = comp.Stacks.Count;
            if (count == 0)
                return [];
            ref var stack = ref comp.Stacks.Ref(0);
            if (stack.Target == actor)
                return [];
            return [new(stack.Target.Position, stack.Activation, inverted: inverted)];
        }
        var stack = TryGetEye(_stack, false);
        return stack.Length != 0 ? stack : TryGetEye(_stackInv, true);
    }
}

sealed class BloodshotStack(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BloodshotGaze, 8f, 8);
sealed class BloodshotStackInverted(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.BloodshotGazeInverted, 8f, 8);

sealed class CatsEyeStates : StateMachineBuilder
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
public sealed class CatsEye(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
