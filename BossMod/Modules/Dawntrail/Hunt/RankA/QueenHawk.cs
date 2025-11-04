namespace BossMod.Dawntrail.Hunt.RankA.QueenHawk;

public enum OID : uint
{
    Boss = 0x452B // R2.4
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    BeeBeGone = 39482, // Boss->self, 4.0s cast, range 12 circle
    BeeBeHere = 39483, // Boss->self, 4.0s cast, range 10-40 donut
    ResonantBuzz = 39486, // Boss->self, 5.0s cast, range 40 circle
    StingingVenom = 39488, // Boss->player, no cast, single-target
    FrenziedSting = 39489, // Boss->player, 5.0s cast, tankbuster
    StraightSpindle = 39490 // Boss->self, 4.0s cast, rect 50 range 5 width
}

public enum SID : uint
{
    BeeBeHere = 4148,
    BeeBeGone = 4147,
    RightFace = 2164,
    LeftFace = 2163,
    ForwardMarch = 2161,
    AboutFace = 2162
}

sealed class ResonantBuzz(BossModule module) : Components.RaidwideCast(module, (uint)AID.ResonantBuzz, "Apply forced march!");
sealed class ResonantBuzzMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly BeeBeAOE _aoe = module.FindComponent<BeeBeAOE>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOE.Length != 0)
        {
            ref var aoe = ref _aoe.AOE[0];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = CollectionsMarshal.AsSpan(ForcedMovements(actor));
        var len = movements.Length;
        if (len == 0)
        {
            return;
        }
        ref var last = ref movements[len - 1];
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
        {
            hints.Add("Aim outside of the AOE!");
        }
    }
}

sealed class StraightSpindle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StraightSpindle, new AOEShapeRect(50f, 2.5f));
sealed class FrenziedSting(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FrenziedSting);

sealed class BeeBeAOE(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    private static readonly AOEShapeCircle _shapeCircle = new(12f);
    private static readonly AOEShapeDonut _shapeDonut = new(10f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.BeeBeGone => _shapeCircle,
            (uint)AID.BeeBeHere => _shapeDonut,
            _ => null
        };
        if (shape != null)
        {
            AOE = [new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.BeeBeGone or (uint)SID.BeeBeHere)
        {
            AOE = [];
        }
    }
}

sealed class QueenHawkStates : StateMachineBuilder
{
    public QueenHawkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeeBeAOE>()
            .ActivateOnEnter<ResonantBuzz>()
            .ActivateOnEnter<ResonantBuzzMarch>()
            .ActivateOnEnter<StraightSpindle>()
            .ActivateOnEnter<FrenziedSting>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13361u)]
public sealed class QueenHawk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
