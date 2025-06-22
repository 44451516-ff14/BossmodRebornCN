namespace BossMod.Shadowbringers.Hunt.RankS.ForgivenRebellion;

public enum OID : uint
{
    Boss = 0x28B6 // R=3.4
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    SanctifiedBlizzard = 17598, // Boss->self, 3.0s cast, range 40 45-degree cone
    RoyalDecree = 17597, // Boss->self, 4.0s cast, range 40 circle, raidwide
    SanctifiedBlizzardChain = 17628, // Boss->self, 5.0s cast, range 40 45-degree cone, seems to rotate 45° in a random direction, no AID or Icon to tell apart
    SanctifiedBlizzardChain2 = 17629, // Boss->self, 0.5s cast, range 40 45-degree cone
    SanctifiedBlizzardChain3 = 18080, // Boss->self, 0.5s cast, range 40 45-degree cone
    HeavenlyScythe = 17600, // Boss->self, 2.5s cast, range 10 circle
    Transference = 17611, // Boss->player, no cast, single-target, gap closer
    RotateCW = 18078, // Boss->self, 0.5s cast, single-target
    RotateCCW = 18079, // Boss->self, 0.5s cast, single-target
    HeavenlyCyclone = 18126, // Boss->self, 5.0s cast, range 28 180-degree cone
    HeavenlyCyclone1 = 18127, // Boss->self, 0.5s cast, range 28 180-degree cone
    HeavenlyCyclone2 = 18128, // Boss->self, 0.5s cast, range 28 180-degree cone
    Mindjack = 17599, // Boss->self, 4.0s cast, range 40 circle, applies forced march buffs
    RagingFire = 17601, // Boss->self, 5.0s cast, range 5-40 donut
    Interference = 17602 // Boss->self, 4.5s cast, range 28 180-degree cone
}

public enum SID : uint
{
    AboutFace = 1959, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    RightFace = 1961, // Boss->player, extra=0x0
    LeftFace = 1960 // Boss->player, extra=0x0
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167 // Boss
}

class SanctifiedBlizzardChain(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _rot1;
    public static readonly AOEShapeCone Cone = new(40f, 22.5f.Degrees());
    private readonly List<AOEInstance> _aoes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // direction seems to be server side until after first rotation
        if (Sequences.Count == 0)
        {
            var count = _aoes.Count;
            if (count == 0)
                return [];
            var act0 = _aoes[0].Activation;
            var aoes = new AOEInstance[count];
            var color = Colors.Danger;
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                aoes[i] = (aoe.Activation - act0).TotalSeconds < 1d ? aoe with { Color = color } : aoe;
            }
            return aoes;
        }
        else
            return base.ActiveAOEs(slot, actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(Angle offset, float delay = 1.3f) => _aoes.Add(new(Cone, spell.LocXZ, spell.Rotation + offset, Module.CastFinishAt(spell, delay)));
        if (spell.Action.ID == (uint)AID.SanctifiedBlizzardChain)
        {
            _rot1 = spell.Rotation;
            AddAOE(default, 0f);
            AddAOE(45f.Degrees());
            AddAOE(-45f.Degrees());
        }
        else if (Sequences.Count == 0 && spell.Action.ID is (uint)AID.SanctifiedBlizzardChain2 or (uint)AID.SanctifiedBlizzardChain3)
        {
            var rot2 = spell.Rotation;
            var inc = ((_rot1 - rot2).Normalized().Rad > 0 ? -1 : 1) * 45f.Degrees();
            if (Sequences.Count == 0)
            {
                Sequences.Add(new(Cone, spell.LocXZ, rot2, inc, Module.CastFinishAt(spell), 1.3f, 7));
                _aoes.Clear();
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.SanctifiedBlizzardChain)
            _aoes.RemoveAt(0);
        else if (spell.Action.ID is (uint)AID.SanctifiedBlizzardChain2 or (uint)AID.SanctifiedBlizzardChain3)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class SanctifiedBlizzardChainHint(BossModule module) : Components.RaidwideCast(module, (uint)AID.SanctifiedBlizzardChain, "Rotation direction undeterminable until start of the 2nd cast");

class HeavenlyCyclone(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(28f, 90f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -90f.Degrees(),
            (uint)IconID.RotateCCW => 90f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RotateCW or (uint)AID.RotateCCW)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell, 5.2f);
            InitIfReady(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HeavenlyCyclone or (uint)AID.HeavenlyCyclone1 or (uint)AID.HeavenlyCyclone2)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, WPos.ClampToGrid(source.Position), _rotation, _increment, _activation, 1.7f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class HeavenlyScythe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavenlyScythe, 10f);
class RagingFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RagingFire, new AOEShapeDonut(5f, 40f));
class Interference(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Interference, new AOEShapeCone(28f, 90f.Degrees()));
class SanctifiedBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SanctifiedBlizzard, SanctifiedBlizzardChain.Cone);
class RoyalDecree(BossModule module) : Components.RaidwideCast(module, (uint)AID.RoyalDecree);

class MindJack(BossModule module) : Components.StatusDrivenForcedMarch(module, 2f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly Interference _aoe1 = module.FindComponent<Interference>()!;
    private readonly RagingFire _aoe2 = module.FindComponent<RagingFire>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_aoe1.Casters.Count != 0 && _aoe1.Casters[0].Check(pos))
            return true;
        if (_aoe2.Casters.Count != 0 && _aoe2.Casters[0].Check(pos))
            return true;
        return false;
    }
}

class ForgivenRebellionStates : StateMachineBuilder
{
    public ForgivenRebellionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavenlyScythe>()
            .ActivateOnEnter<HeavenlyCyclone>()
            .ActivateOnEnter<RagingFire>()
            .ActivateOnEnter<Interference>()
            .ActivateOnEnter<SanctifiedBlizzard>()
            .ActivateOnEnter<SanctifiedBlizzardChain>()
            .ActivateOnEnter<SanctifiedBlizzardChainHint>()
            .ActivateOnEnter<RoyalDecree>()
            .ActivateOnEnter<MindJack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.SS, NameID = 8915)]
public class ForgivenRebellion(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
