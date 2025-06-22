namespace BossMod.Shadowbringers.Dungeon.D04MalikahsWell.D042AmphibiousTalos;

public enum OID : uint
{
    Boss = 0x267A, // R=3.15
    Geyser = 0x1EAAC9,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Efface = 15595, // Boss->player, 4.5s cast, single-target
    Wellbore = 15597, // Boss->self, 7.0s cast, range 15 circle
    GeyserEruption = 15598, // Helper->self, 3.5s cast, range 8 circle
    HighPressure = 15596, // Boss->self, 4.0s cast, range 40 circle, knockback 20, away from source
    SwiftSpillFirst = 15599, // Boss->self, 7.0s cast, range 50 60-degree cone
    SwiftSpillRest = 15600 // Boss->self, no cast, range 50 60-degree cone
}

public enum IconID : uint
{
    RotateCCW = 157, // Boss
    RotateCW = 156 // Boss
}

class SwiftSpillRotation(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly Angle a60 = 60f.Degrees();
    private Angle _increment;
    private Angle _rotation;
    private DateTime _activation;
    private static readonly AOEShapeCone _shape = new(50f, 30f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var increment = iconID switch
        {
            (uint)IconID.RotateCW => -a60,
            (uint)IconID.RotateCCW => a60,
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
        if (spell.Action.ID == (uint)AID.SwiftSpillFirst)
        {
            _rotation = spell.Rotation;
            _activation = Module.CastFinishAt(spell);
            InitIfReady(caster);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SwiftSpillFirst or (uint)AID.SwiftSpillRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }

    private void InitIfReady(Actor source)
    {
        if (_rotation != default && _increment != default)
        {
            Sequences.Add(new(_shape, source.Position, _rotation, _increment, _activation, 1.1f, 6));
            _rotation = default;
            _increment = default;
        }
    }
}

class Efface(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Efface);
class HighPressureRaidwide(BossModule module) : Components.RaidwideCast(module, (uint)AID.HighPressure);
class HighPressureKnockback(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.HighPressure, 20f, stopAtWall: true);
class GeyserEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GeyserEruption, 8f);
class Geysers(BossModule module) : Components.Voidzone(module, 4f, GetVoidzones)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.Geyser);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
class Wellbore(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wellbore, 15);

class D042AmphibiousTalosStates : StateMachineBuilder
{
    public D042AmphibiousTalosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwiftSpillRotation>()
            .ActivateOnEnter<Efface>()
            .ActivateOnEnter<HighPressureKnockback>()
            .ActivateOnEnter<HighPressureRaidwide>()
            .ActivateOnEnter<GeyserEruption>()
            .ActivateOnEnter<Geysers>()
            .ActivateOnEnter<Wellbore>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 656, NameID = 8250)]
public class D042AmphibiousTalos(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsComplex arena = new([new Polygon(new(208f, 275f), 19.5f, 48)], [new Rectangle(new(208f, 255.45f), 20f, 1f), new Rectangle(new(208.089f, 294.664f), 20f, 1.25f, -1.92f.Degrees())]);
}
