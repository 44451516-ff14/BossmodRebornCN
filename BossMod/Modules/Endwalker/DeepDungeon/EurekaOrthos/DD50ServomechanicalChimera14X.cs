namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD50ServomechanicalChimera14X;

public enum OID : uint
{
    Boss = 0x3D9C, // R6.0
    Cacophony = 0x3D9D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    Attack = 6499, // Boss->player, no cast, single-target
    SongsOfIceAndThunder = 31851, // Boss->self, 5.0s cast, range 9 circle
    SongsOfThunderAndIce = 31852, // Boss->self, 5.0s cast, range 8-40 donut
    TheRamsVoice1 = 31854, // Boss->self, no cast, range 9 circle
    TheRamsVoice2 = 32807, // Boss->self, no cast, range 9 circle
    TheDragonsVoice1 = 31853, // Boss->self, no cast, range 8-40 donut
    TheDragonsVoice2 = 32806, // Boss->self, no cast, range 8-40 donut
    RightbreathedCold = 31863, // Boss->self, 5.0s cast, range 40 180-degree cone
    LeftbreathedThunder = 31861, // Boss->self, 5.0s cast, range 40 180-degree cone
    ColdThunder = 31855, // Boss->player, 5.0s cast, width 8 rect charge
    ThunderousCold = 31856, // Boss->player, 5.0s cast, width 8 rect charge
    Cacophony = 31864, // Boss->self, 3.0s cast, single-target
    ChaoticChorus = 31865 // Cacophony->self, no cast, range 6 circle
}

public enum TetherID : uint
{
    OrbTether = 17 // Cacophony->player
}

class RamsDragonVoice(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(8, 40);
    private static readonly AOEShapeCircle circle = new(9);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly HashSet<AID> castEnd = [AID.TheRamsVoice1, AID.TheRamsVoice2, AID.TheDragonsVoice1,
    AID.TheDragonsVoice2, AID.SongsOfIceAndThunder, AID.SongsOfThunderAndIce];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (_aoes.Count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.SongsOfIceAndThunder:
                AddAOEs(circle, donut, spell);
                break;
            case AID.SongsOfThunderAndIce:
                AddAOEs(donut, circle, spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ColdThunder:
                AddAOEs(circle, donut, spell);
                break;
            case AID.ThunderousCold:
                AddAOEs(donut, circle, spell);
                break;
        }
    }

    public override void Update()
    {
        var count = _aoes.Count;
        if (count > 0)
            for (var i = 0; i < count; ++i)
                _aoes[i] = new(_aoes[i].Shape, Module.PrimaryActor.Position, default, _aoes[i].Activation);
    }

    private void AddAOEs(AOEShape first, AOEShape second, ActorCastInfo spell)
    {
        var position = Module.PrimaryActor.Position;
        _aoes.Add(new(first, position, default, Module.CastFinishAt(spell)));
        _aoes.Add(new(second, position, default, Module.CastFinishAt(spell, 3)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class TC(BossModule module, AID aid) : Components.BaitAwayChargeCast(module, ActionID.MakeSpell(aid), 4);
class ThunderousCold(BossModule module) : TC(module, AID.ThunderousCold);
class ColdThunder(BossModule module) : TC(module, AID.ColdThunder);

class Breath(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(40, 90.Degrees()));
class RightbreathedCold(BossModule module) : Breath(module, AID.RightbreathedCold);
class LeftbreathedThunder(BossModule module) : Breath(module, AID.LeftbreathedThunder);

class ChargeTether(BossModule module) : Components.StretchTetherDuo(module, 15, 5.1f);

class Cacophony(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<Actor> _orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < _orbs.Count; ++i)
            yield return new(circle, _orbs[i].Position);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Cacophony)
            _orbs.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ChaoticChorus)
            _orbs.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        for (var i = 0; i < _orbs.Count; ++i)
        {
            var w = _orbs[i];
            hints.AddForbiddenZone(circle, w.Position + 2 * w.Rotation.ToDirection());
        }
    }
}

class CacophonyTether(BossModule module) : Components.StretchTetherSingle(module, (uint)TetherID.OrbTether, 15, needToKite: true);

class DD50ServomechanicalChimera14XStates : StateMachineBuilder
{
    public DD50ServomechanicalChimera14XStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<CacophonyTether>()
            .ActivateOnEnter<RamsDragonVoice>()
            .ActivateOnEnter<ThunderousCold>()
            .ActivateOnEnter<ColdThunder>()
            .ActivateOnEnter<RightbreathedCold>()
            .ActivateOnEnter<LeftbreathedThunder>()
            .ActivateOnEnter<ChargeTether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 901, NameID = 12265)]
public class DD50ServomechanicalChimera14X(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(19.5f));