namespace BossMod.Global.MaskedCarnivale.Stage16.Act2;

public enum OID : uint
{
    Boss = 0x26F4, // R=4.0
    Cyclops = 0x26F3, //R=3.2
    Helper = 0x233C //R=0.5
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target

    TenTonzeSlash = 14871, // Boss->self, 4.0s cast, range 40+R 60-degree cone
    VoiceOfAuthority = 14874, // Boss->self, 1.5s cast, single-target, spawns cyclops add
    OneOneOneTonzeSwing = 14872, // Boss->self, 4.5s cast, range 8+R circle, knockback dist 20
    CryOfRage = 14875, // Boss->self, 3.0s cast, range 50+R circle, gaze
    TheBullsVoice = 14779, // Boss->self, 1.5s cast, single-target, damage buff
    PredatorialInstinct = 14685, // Boss->self, no cast, range 50+R circle, raidwide attract with dist 50
    OneOneOneOneTonzeSwing = 14686, // Boss->self, 9.0s cast, range 20+R circle, raidwide, needs diamondback to survive
    ZoomIn = 14873, // Boss->player, 4.0s cast, width 8 rect unavoidable charge, knockback dist 20
    TenTonzeWaveCone = 14876, // Boss->self, 4.0s cast, range 40+R 60-degree cone
    TenTonzeWaveDonut = 15268 // Helper->self, 4.6s cast, range 10-20 donut
}

class OneOneOneOneTonzeSwing(BossModule module) : Components.RaidwideCast(module, (uint)AID.OneOneOneOneTonzeSwing, "Use Diamondback!");
class OneOneOneTonzeSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OneOneOneTonzeSwing, 12f);
class CryOfRage(BossModule module) : Components.CastGaze(module, (uint)AID.CryOfRage);

abstract class TenTonzeCone(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(44f, 30f.Degrees()));
class TenTonzeSlash(BossModule module) : TenTonzeCone(module, (uint)AID.TenTonzeSlash);
class TenTonzeWaveCone(BossModule module) : TenTonzeCone(module, (uint)AID.TenTonzeWaveCone);

class TenTonzeWaveDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TenTonzeWaveDonut, new AOEShapeDonut(10f, 20f));
class ZoomIn(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.ZoomIn, 4f);

class ZoomInKB(BossModule module) : Components.GenericKnockback(module) // actual knockback happens ~0.7s after snapshot
{
    private DateTime _activation;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_activation != default)
            return new Knockback[1] { new(Module.PrimaryActor.Position, 20f, _activation) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ZoomIn)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ZoomIn)
            _activation = default;
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"{Module.PrimaryActor.Name} will spawn a cyclops a few seconds into the fight. Make sure\nto kill it before it reaches you. After that you can just slowly take down the\nboss. Use Diamondback to survive the 1111 Tonze Swing. Alternatively\nyou can try the Final Sting combo when he drops to about 75% health.\n(Off-guard->Bristle->Moonflute->Final Sting)");
    }
}

class Stage16Act2States : StateMachineBuilder
{
    public Stage16Act2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OneOneOneOneTonzeSwing>()
            .ActivateOnEnter<OneOneOneTonzeSwing>()
            .ActivateOnEnter<TenTonzeSlash>()
            .ActivateOnEnter<CryOfRage>()
            .ActivateOnEnter<ZoomIn>()
            .ActivateOnEnter<ZoomInKB>()
            .ActivateOnEnter<TenTonzeWaveCone>()
            .ActivateOnEnter<TenTonzeWaveDonut>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 626, NameID = 8113, SortOrder = 2)]
public class Stage16Act2 : BossModule
{
    public Stage16Act2(WorldState ws, Actor primary) : base(ws, primary, Layouts.ArenaCenter, Layouts.CircleSmall)
    {
        ActivateComponent<Hints>();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Cyclops), Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Cyclops => 1,
                _ => 0
            };
        }
    }
}
