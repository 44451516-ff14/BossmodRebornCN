namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D151MarkIIIBMagitekColossus;

public enum OID : uint
{
    Boss = 0x25DA, // R3.5
    FireVoidzone = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    JarringBlow = 14190, // Boss->player, 4.0s cast, single-target
    WildFireBeamVisual = 14193, // Boss->self, no cast, single-target
    WildFireBeam = 14194, // Helper->player, 5.0s cast, range 6 circle, spread
    MagitekRay = 14191, // Boss->player, 5.0s cast, range 6 circle, stack

    MagitekSlashVisual1 = 14197, // Boss->self, no cast, range 20+R 60-degree cone
    MagitekSlashVisual2 = 14670, // Boss->self, no cast, range 20+R 60-degree cone
    MagitekSlashFirst = 14196, // Boss->self, 5.0s cast, range 20+R 60-degree cone
    MagitekSlashRest = 14671, // Helper->self, no cast, range 20+R 60-degree cone

    Exhaust = 14192, // Boss->self, 3.0s cast, range 40+R width 10 rect
    CeruleumVent = 14195 // Boss->self, 4.0s cast, range 40 circle
}

public enum IconID : uint
{
    RotateCCW = 168, // Boss
    RotateCW = 167 // Boss
}

class MagitektSlashRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    public static readonly AOEShapeCone Cone = new(23.5f, 30f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -60f.Degrees(),
            (uint)IconID.RotateCCW => 60f.Degrees(),
            _ => default
        };
        if (increment != default)
        {
            _increment = increment;
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MagitekSlashFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell, 2.2d);
        }
        if (_rotation != default)
            InitIfReady();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MagitekSlashFirst or (uint)AID.MagitekSlashRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady()
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(Cone, D151MarkIIIBMagitekColossus.ArenaCenter.Quantized(), _rotation, _increment, _activation, 1.1d, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class MagitektSlashVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID != (uint)OID.FireVoidzone)
            return;
        if (state == 0x001)
        {
            _aoes.Add(new(MagitektSlashRotation.Cone, D151MarkIIIBMagitekColossus.ArenaCenter, actor.Rotation));
        }
        else if (state == 0x004)
        {
            var count = _aoes.Count;
            var rot = actor.Rotation;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].Rotation == rot)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class JarringBlow(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.JarringBlow);
class Exhaust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Exhaust, new AOEShapeRect(43.5f, 5f));
class WildFireBeam(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WildFireBeam, 6f);
class MagitekRay(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.MagitekRay, 6f, 4, 4);
class CeruleumVent(BossModule module) : Components.RaidwideCast(module, (uint)AID.CeruleumVent);

class D151MarkIIIBMagitekColossusStates : StateMachineBuilder
{
    public D151MarkIIIBMagitekColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MagitektSlashRotation>()
            .ActivateOnEnter<MagitektSlashVoidzone>()
            .ActivateOnEnter<JarringBlow>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<WildFireBeam>()
            .ActivateOnEnter<CeruleumVent>()
            .ActivateOnEnter<MagitekRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7855, SortOrder = 1)]
public class D151MarkIIIBMagitekColossus(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly WPos ArenaCenter = new(-180.569f, 68.523f);
    private static readonly ArenaBoundsCustom arena = new([new Polygon(ArenaCenter, 19.55f, 24)], [new Rectangle(new(-180f, 88.3f), 20, 1), new Rectangle(new(-160f, 68f), 20, 1, 102.5f.Degrees())]);
}
