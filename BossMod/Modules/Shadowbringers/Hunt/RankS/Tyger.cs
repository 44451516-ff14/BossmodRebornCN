namespace BossMod.Shadowbringers.Hunt.RankS.Tyger;

public enum OID : uint
{
    Boss = 0x288E // R=5.92
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    TheLionsBreath = 16957, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheScorpionsSting = 16961, // Boss->self, no cast, range 18 90-degree cone, 2-4s after a voice attack, timing seems to vary, maybe depends if voice was interrupted and how fast?
    TheDragonsBreath = 16959, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheRamsBreath = 16958, // Boss->self, 4.0s cast, range 30 120-degree cone
    TheRamsEmbrace = 16960, // Boss->location, 3.0s cast, range 9 circle
    TheDragonsVoice = 16963, // Boss->self, 4.0s cast, range 8-30 donut, interruptible raidwide donut
    TheRamsVoice = 16962 // Boss->self, 4.0s cast, range 9 circle
}

sealed class Breaths(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TheDragonsBreath, (uint)AID.TheRamsBreath,
(uint)AID.TheLionsBreath], new AOEShapeCone(30f, 60f.Degrees()));

class TheScorpionsSting(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCone cone = new(18f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TheRamsVoice or (uint)AID.TheDragonsVoice) // timing varies, just used the lowest I could find, probably depends on interrupt status
        {
            _aoe = [new(cone, spell.LocXZ, spell.Rotation + 180f.Degrees(), Module.CastFinishAt(spell, 2.3d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TheScorpionsSting)
        {
            _aoe = [];
        }
    }
}

class TheRamsEmbrace(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsEmbrace, 9f);
class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9f);
class TheRamsVoiceHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheRamsVoice);
class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8f, 30f));
class TheDragonsVoiceHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.TheDragonsVoice, hintExtra: "Donut Raidwide");

class TygerStates : StateMachineBuilder
{
    public TygerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Breaths>()
            .ActivateOnEnter<TheScorpionsSting>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceHint>()
            .ActivateOnEnter<TheRamsEmbrace>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheRamsVoiceHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 8905)]
public class Tyger(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
