namespace BossMod.Shadowbringers.Dungeon.D08AkadaemiaAnyder.D082MorbolMarquis;

public enum OID : uint
{
    Boss = 0x249E, // R5.5
    Voidzone = 0x1EA1A1, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    SapShowerVisual = 15892, // Boss->self, no cast, single-target, spread
    SapShower = 15893, // Helper->player, 5.0s cast, range 8 circle

    Lash = 15894, // Boss->players, no cast, single-target, double hit tankbuster, after every putrid breath and every sap shower

    ArborStorm = 15895, // Boss->self, 3.0s cast, range 50 circle, raidwide

    ExtensibleTendrilsFirst = 15888, // Boss->self, 5.0s cast, range 25 width 6 cross, 5 hits, sort of a rotation, except sometimes the boss seems to hit the same spot multiple times (random?)
    ExtensibleTendrilsRest = 15889, // Boss->self, no cast, range 25 width 6 cross

    PutridBreath = 15890, // Boss->self, no cast, range 25 90-degree cone, after 5 hits of Extensible Tendrils
    Blossom = 15891 // Boss->self, 4.0s cast, single-target
}

class ArborStorm(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ArborStorm));

abstract class Leash(BossModule module, AID aid) : Components.SingleTargetEventDelay(module, ActionID.MakeSpell(aid), ActionID.MakeSpell(AID.Lash), 3.4f)  // actual delay can be higher since boss needs to run into melee range for it
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Targets.Count > 0 && spell.Action != ActionVisual && spell.Action != ActionVisual)  // it seems like sometimes the tankbuster gets skipped and it does it twice next time
            Targets.Clear();
    }
}

class LeashSapShower(BossModule module) : Leash(module, AID.SapShowerVisual);
class LeashPutridBreath(BossModule module) : Leash(module, AID.PutridBreath);

class SapShower(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SapShower), 8);

class ExtensibleTendrilsPutridBreath(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCross cross = new(25, 3);
    private static readonly AOEShapeCone cone = new(25, D082MorbolMarquis.A45);
    private AOEInstance? _aoe;
    private DateTime activation;
    private int remainingCasts;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe != null)
        {
            yield return _aoe.Value;
            yield break;
        }
        if (remainingCasts > 0)
        {
            var delay1 = activation.AddSeconds((5 - remainingCasts) * 6.1f);
            if ((delay1 - WorldState.CurrentTime).TotalSeconds <= 2.5f)
                yield return new(cross, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + D082MorbolMarquis.A45, delay1);
        }
        var delay2 = activation.AddSeconds(27.1f);
        if (activation != default && (delay2 - WorldState.CurrentTime).TotalSeconds <= 4.9f)
            yield return new(cone, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, delay2);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExtensibleTendrilsFirst)
        {
            remainingCasts = 5;
            activation = Module.CastFinishAt(spell);
            _aoe = new(cross, Module.PrimaryActor.Position, spell.Rotation, activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ExtensibleTendrilsFirst:
            case AID.ExtensibleTendrilsRest:
                _aoe = null;
                --remainingCasts;
                break;

            case AID.PutridBreath:
                activation = default;
                break;
        }
    }
}

class BlossomArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.Voidzone)
        {
            Arena.Bounds = state switch
            {
                0x00100020 => D082MorbolMarquis.YellowBlossomBounds,
                0x00010002 => D082MorbolMarquis.BlueBlossomBounds,
                _ => D082MorbolMarquis.DefaultBounds
            };
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Arena.InBounds(actor.Position))
            hints.AddForbiddenZone(ShapeDistance.InvertedDonut(Arena.Center, 11, 14));
    }
}

class D082MorbolMarquisStates : StateMachineBuilder
{
    public D082MorbolMarquisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlossomArenaChanges>()
            .ActivateOnEnter<ArborStorm>()
            .ActivateOnEnter<LeashSapShower>()
            .ActivateOnEnter<LeashPutridBreath>()
            .ActivateOnEnter<SapShower>()
            .ActivateOnEnter<ExtensibleTendrilsPutridBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 661, NameID = 8272)]
public class D082MorbolMarquis(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private const int X = -224, InnerRadius = 10, OuterRadius = 15, Radius = 25, Edges = 12;
    private static readonly WPos ArenaCenter = new(X, -38);
    public static readonly Angle A45 = 45.Degrees(), a135 = 135.Degrees();
    private static readonly Polygon[] defaultCircle = [new(ArenaCenter, 24.5f / MathF.Cos(MathF.PI / 48), 48)];
    private static readonly Rectangle[] defaultDifference = [new(new(X, -13), Radius, 1.1f), new(new(X, -63), Radius, 1.1f)];
    private static readonly Shape[] blueBlossom = [new ConeV(ArenaCenter, InnerRadius, A45, A45, Edges), new ConeV(ArenaCenter, InnerRadius, -a135, A45, Edges),
    new DonutSegmentV(ArenaCenter, OuterRadius, Radius, A45, A45, Edges), new DonutSegmentV(ArenaCenter, OuterRadius, Radius, -a135, A45, Edges)];
    private static readonly Shape[] yellowBlossom = [new ConeV(ArenaCenter, InnerRadius, -A45, A45, Edges), new ConeV(ArenaCenter, InnerRadius, a135, A45, Edges),
    new DonutSegmentV(ArenaCenter, OuterRadius, Radius, -A45, A45, Edges), new DonutSegmentV(ArenaCenter, OuterRadius, Radius, a135, A45, Edges)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(defaultCircle, defaultDifference);
    public static readonly ArenaBoundsComplex BlueBlossomBounds = new(defaultCircle, [.. defaultDifference, .. blueBlossom]);
    public static readonly ArenaBoundsComplex YellowBlossomBounds = new(defaultCircle, [.. defaultDifference, .. yellowBlossom]);
}
