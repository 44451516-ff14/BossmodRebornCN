namespace BossMod.Endwalker.Dungeon.D01TheTowerOifZot.D012Sanduruva;

public enum OID : uint
{
    Boss = 0x33EF, // R=2.5
    BerserkerSphere = 0x33F0 // R=1.5-2.5
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Teleport = 25254,  // Boss->location, no cast, single-target

    ExplosiveForce = 25250, //Boss->self, 3.0s cast, single-target
    IsitvaSiddhi = 25257, // Boss->player, 4.0s cast, single-target
    ManusyaBerserk = 25249, // Boss->self, 3.0s cast, single-target
    ManusyaConfuse = 25253, // Boss->self, 3.0s cast, range 40 circle
    ManusyaStop = 25255, // Boss->self, 3.0s cast, range 40 circle
    PrakamyaSiddhi = 25251, // Boss->self, 4.0s cast, range 5 circle
    PraptiSiddhi = 25256, //Boss->self, 2.0s cast, range 40 width 4 rect
    SphereShatter = 25252 // BerserkerSphere->self, 2.0s cast, range 15 circle
}

public enum SID : uint
{
    ManusyaBerserk = 2651, // Boss->player, extra=0x0
    ManusyaStop = 2653, // none->player, extra=0x0
    TemporalDisplacement = 900, // none->player, extra=0x0
    ManusyaConfuse = 2652, // Boss->player, extra=0x1C6
    WhoIsShe = 2655, // none->Boss, extra=0x0
    WhoIsShe2 = 2654 // none->BerserkerSphere, extra=0x1A8
}

class IsitvaSiddhi(BossModule module) : Components.SingleTargetCast(module, (uint)AID.IsitvaSiddhi);

class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        var time = _aoes[0].Activation.AddSeconds(-7d) > WorldState.CurrentTime;
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            aoes[i] = time ? aoe with { Risky = false } : aoe;
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BerserkerSphere)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, NumCasts == 0 ? WorldState.FutureTime(10.8d) : WorldState.FutureTime(20d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

class PraptiSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PraptiSiddhi, new AOEShapeRect(40f, 2f));
class PrakamyaSiddhi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrakamyaSiddhi, 5f);
class ManusyaConfuse(BossModule module) : Components.CastHint(module, (uint)AID.ManusyaConfuse, "Applies Manyusa Confusion");
class ManusyaStop(BossModule module) : Components.CastHint(module, (uint)AID.ManusyaStop, "Applies Manyusa Stop");

class D012SanduruvaStates : StateMachineBuilder
{
    public D012SanduruvaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ManusyaConfuse>()
            .ActivateOnEnter<IsitvaSiddhi>()
            .ActivateOnEnter<ManusyaStop>()
            .ActivateOnEnter<PrakamyaSiddhi>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<PraptiSiddhi>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "dhoggpt, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 783, NameID = 10257)]
public class D012Sanduruva(WorldState ws, Actor primary) : BossModule(ws, primary, new(-258f, -26f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BerserkerSphere => AIHints.Enemy.PriorityPointless,
                _ => 0
            };
        }
    }
}
