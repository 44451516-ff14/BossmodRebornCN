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

class ResonantBuzz(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ResonantBuzz), "Apply forced march!");
class ResonantBuzzMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly BeeBeAOE _aoe = module.FindComponent<BeeBeAOE>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe.AOE is Components.GenericAOEs.AOEInstance aoe)
        {
            if (aoe.Check(pos))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var movements = ForcedMovements(actor);
        var count = movements.Count;
        if (count == 0)
            return;
        var last = movements[count - 1];
        if (last.from != last.to && DestinationUnsafe(slot, actor, last.to))
            hints.Add("Aim outside of the AOE!");
    }
}

class StraightSpindle(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.StraightSpindle), new AOEShapeRect(50f, 2.5f));
class FrenziedSting(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.FrenziedSting));

class BeeBeAOE(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance? AOE;
    private static readonly AOEShapeCircle _shapeCircle = new(12f);
    private static readonly AOEShapeDonut _shapeDonut = new(10f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.BeeBeGone => _shapeCircle,
            (uint)AID.BeeBeHere => _shapeDonut,
            _ => null
        };
        if (shape != null)
            AOE = new(shape, spell.LocXZ, default, Module.CastFinishAt(spell, 0.8f));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.BeeBeGone or (uint)SID.BeeBeHere)
            AOE = null;
    }
}

class QueenHawkStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Shinryin, Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13361)]
public class QueenHawk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
