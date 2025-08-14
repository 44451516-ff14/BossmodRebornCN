namespace BossMod.Shadowbringers.Quest.MSQ.FadedMemories.Ardbert;

public enum OID : uint
{
    Boss = 0x2F2E, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    Teleport = 21128, // Boss->location, no cast, single-target
    HeavySwing = 21123, // Boss->player, no cast, single-target
    Maim = 21124, // Boss->player, no cast, single-target
    Stormwind = 21125, // Boss->player, no cast, single-target

    Overcome = 21126, // Boss->self, 2.5s cast, range 8 120-degree cone
    Skydrive = 21127, // Boss->self, 2.5s cast, range 5 circle

    SkyHighDriveCW = 21138, // Boss->self, 4.5s cast, single-target
    SkyHighDriveCCW = 21139, // Boss->self, 4.5s cast, single-target
    SkyHighDriveFirst = 21140, // Helper->self, 5.0s cast, range 40 width 8 rect
    SkyHighDriveRest = 21141, // Helper->self, no cast, range 40 width 8 rect
    SkyHighDriveVisual = 21564, // Boss->self, no cast, single-target

    AvalancheAxeVisual = 21142, // Boss->self, 9.0s cast, single-target
    AvalanceAxe1 = 21145, // Helper->self, 4.0s cast, range 10 circle
    AvalanceAxe2 = 21144, // Helper->self, 7.0s cast, range 10 circle
    AvalanceAxe3 = 21143, // Helper->self, 10.0s cast, range 10 circle

    OvercomeAllOddsVisual = 21129, // Boss->self, 3.0s cast, single-target
    OvercomeAllOdds = 21130, // Helper->self, 2.5s cast, range 60 30-degree cone
    SoulflashVisual = 21135, // Boss->self, 4.0s cast, single-target
    Soulflash1 = 21136, // Helper->self, 4.0s cast, range 4 circle
    EtesianAxe = 21147, // Helper->self, 6.5s cast, range 80 circle
    Soulflash2 = 21137, // Helper->self, 4.0s cast, range 8 circle

    GroundbreakerVisualExa = 21150, // Boss->self, 5.0s cast, single-target
    GroundbreakerVisualCone = 21152, // Boss->self, 3.0s cast, single-target
    GroundbreakerVisualDonut = 21156, // Boss->self, 3.0s cast, single-target
    GroundbreakerVisualCircle = 21154, // Boss->self, 3.0s cast, single-target
    GroundbreakerExaFirst = 21563, // Helper->self, 5.0s cast, range 6 circle
    GroundbreakerExaRest = 21151, // Helper->self, no cast, range 6 circle
    GroundbreakerCone = 21153, // Helper->self, 6.0s cast, range 40 90-degree cone
    GroundbreakerDonut = 21157, // Helper->self, 6.0s cast, range 5-20 donut
    GroundbreakerCircle = 21155, // Helper->self, 6.0s cast, range 15 circle

    SoulsRelease = 21132, // Boss->self, 18.0s cast, single-target
    Shockwave1 = 21133, // Helper->self, no cast, range 40 circle
    Shockwave2 = 21134, // Helper->self, no cast, range 40 circle

    LandsOfOldVisual = 21148, // Boss->self, 5.0s cast, single-target
    LandsOfOld = 21149 // Helper->self, 5.0s cast, range 60 circle
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167 // Boss
}

class Overcome(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overcome, new AOEShapeCone(8f, 60f.Degrees()), 2);
class Skydrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Skydrive, 5f);
class LandsOfOld(BossModule module) : Components.RaidwideCast(module, (uint)AID.LandsOfOld);

class SkyHighDrive(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private readonly List<Angle> _rotation = new(2);

    private static readonly AOEShapeRect rect = new(40f, 4f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -20f.Degrees(),
            (uint)IconID.RotateCCW => 20f.Degrees(),
            _ => default
        };
        InitIfReady(actor);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation.Count == 2 && _increment != default)
        {
            for (var i = 0; i < 2; ++i)
                Sequences.Add(new(rect, source.Position.Quantized(), _rotation[i], _increment, _activation, 0.6f, 10, 4));
            _rotation.Clear();
            _increment = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SkyHighDriveFirst)
        {
            _rotation.Add(spell.Rotation);
            _activation = Module.CastFinishAt(spell);
            InitIfReady(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SkyHighDriveFirst or (uint)AID.SkyHighDriveRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class AvalancheAxe1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AvalanceAxe1, 10f);
class AvalancheAxe2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AvalanceAxe2, 10f);
class AvalancheAxe3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AvalanceAxe3, 10f);
class OvercomeAllOdds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OvercomeAllOdds, new AOEShapeCone(60f, 15f.Degrees()), 1)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (NumCasts > 0)
            MaxCasts = 2;
    }
}
class Soulflash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Soulflash1, 4f);
class EtesianAxe(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.EtesianAxe, 15f, kind: Kind.DirForward);
class Soulflash2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Soulflash2, 8f);

class GroundbreakerExaflares(BossModule module) : Components.Exaflare(module, 6f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GroundbreakerExaFirst)
        {
            Lines.Add(new(caster.Position, 6f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1d, 8, 3));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.GroundbreakerExaFirst or (uint)AID.GroundbreakerExaRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class GroundbreakerCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundbreakerCone, new AOEShapeCone(40f, 45f.Degrees()));
class GroundbreakerDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundbreakerDonut, new AOEShapeDonut(5f, 20f));
class GroundbreakerCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GroundbreakerCircle, 15f);

class ArdbertStates : StateMachineBuilder
{
    public ArdbertStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LandsOfOld>()
            .ActivateOnEnter<SkyHighDrive>()
            .ActivateOnEnter<Skydrive>()
            .ActivateOnEnter<Overcome>()
            .ActivateOnEnter<AvalancheAxe1>()
            .ActivateOnEnter<AvalancheAxe2>()
            .ActivateOnEnter<AvalancheAxe3>()
            .ActivateOnEnter<OvercomeAllOdds>()
            .ActivateOnEnter<Soulflash>()
            .ActivateOnEnter<EtesianAxe>()
            .ActivateOnEnter<Soulflash2>()
            .ActivateOnEnter<GroundbreakerExaflares>()
            .ActivateOnEnter<GroundbreakerCone>()
            .ActivateOnEnter<GroundbreakerDonut>()
            .ActivateOnEnter<GroundbreakerCircle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 8258)]
public class Ardbert(WorldState ws, Actor primary) : BossModule(ws, primary, new(-392f, 780f), new ArenaBoundsCircle(20f));
