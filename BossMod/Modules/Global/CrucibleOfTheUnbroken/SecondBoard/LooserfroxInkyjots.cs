namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.LooserfroxInkyjots;

public enum OID : uint
{
    LoosefroxInkyjots = 0x4C65,
    ChewchumPopoto = 0x4C66, // R6.105, x1
    Quicksand = 0x1EC026, // R0.500, x6, EventObj type
    GobbieBombBig = 0x4C67, // R1.200, x0 (spawn during fight)
    GobbieBombSmall = 0x4C69, // R0.600, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50784, // LoosefroxInkyjots/4C66->player, no cast, single-target

    Burrow = 48224, // 4C66->self, no cast, single-target
    SandPillar = 48225, // Helper->location, no cast, range 4 circle
    Teleport = 50451, // Helper->location, no cast, single-target
    SandBreathVisual = 48226, // 4C66->self, 7.8+1.2s cast, single-target
    SandBreath = 48227, // Helper->self, 9.0s cast, range 60 90.000-degree cone
    GobspinHeadlopsVisual = 48228, // LoosefroxInkyjots->self, 5.3+0.7s cast, single-target
    GobspinHeadlops = 48229, // Helper->self, 6.0s cast, range 8 circle
    Kinborrow = 48242, // LoosefroxInkyjots->self, 4.0s cast, single-target
    GobbieboomBarrage1 = 50446, // LoosefroxInkyjots->self, 3.0s cast, single-target
    GobbieboomBarrage2 = 48234, // LoosefroxInkyjots->self, 3.0s cast, single-target
    _Weaponskill_1 = 50452, // Helper->location, no cast, range 6 circle
    Explosion = 48235, // 4C69->self, 2.0s cast, range 12 circle
    BombTossVisual = 48518, // LoosefroxInkyjots->location, 3.0+1.0s cast, single-target
    BombToss = 48519, // Helper->location, 4.0s cast, range 6 circle

    GobspinHeadlopsDonutVisual = 48230, // LoosefroxInkyjots->self, 5.3+0.7s cast, single-target
    GobspinHeadlopsDonut = 48231, // Helper->self, 6.0s cast, range 4-40 donut
    ExplosionCross = 48236, // 4C67->self, 2.0s cast, range 40 width 8 cross
    GoblinHammerVisual = 48232, // LoosefroxInkyjots->self, 3.0s cast, single-target
    GoblinHammer = 48233, // Helper->self, 5.0s cast, range 8 circle
    Earthquake = 48239, // ChewchumPopoto->self, 5.0s cast, range 60 circle
    EarthquakeRest = 48240, // ChewchumPopoto->self, no cast, range 60 circle
}

public enum SID : uint
{
    Blind = 5381, // Helper->player, extra=0x0
    PopotoSkin = 5423, // LoosefroxInkyjots->LoosefroxInkyjots, extra=0x0
}

public enum IconID : uint
{
    Exclamation_8s_x = 569, // Helper->self
}

sealed class BurrowSandPillar(BossModule module) : Components.CastCounter(module, (uint)AID.Burrow)
{
    // Burrow at current position
    // 1st SandPillar happens near but not exactly at sandpit, 4-5f away
    // Kinsburrow = 4f dist, Regular/Burrow = 5f?
    // can bounce between sandpits before stopping for breath, anywhere from 6-14 SandPillar so far
    private readonly WPos[] _sandpits = [
        new(533f, -434f),
        new(538f, -421f),
        new(529.65f, -407f),
        new(510.35f, -407f),
        new(504f, -423f),
        new(516f, -439f)
        ];
    private readonly float _radius = 6f;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = _sandpits.Length;
        for (var i = 0; i < count; i++)
        {
            Arena.ZoneCircle(_sandpits[i], _radius, default);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _sandpits.Length;
        for (var i = 0; i < count; i++)
        {
            var sd = new SDCircle(_sandpits[i], _radius);
            hints.AddForbiddenZone(sd);
        }
    }
}
sealed class SandBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SandBreath, new AOEShapeCone(60f, 45f.Degrees()));
sealed class GobspinHeadlops(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GobspinHeadlops, 8f);
sealed class BombToss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BombToss, 6f);
sealed class GobspinHeadlopsDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GobspinHeadlopsDonut, new AOEShapeDonut(4f, 40f));
sealed class GoblinHammer(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GoblinHammer, 8f);
sealed class GobbieBombs(BossModule module) : Components.AddsMulti(module, [(uint)OID.GobbieBombBig, (uint)OID.GobbieBombSmall], AIHints.Enemy.PriorityUndesirable);
sealed class Earthquake(BossModule module) : Components.RaidwideCast(module, (uint)AID.Earthquake);
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 12f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.GobbieBombSmall && animState1 == 1)
        {
            var origin = actor.Position;
            var rotation = actor.Rotation;
            var layer = ResolveArenaProjectionLayer(actor.PosRot.Y);
            var activation = WorldState.CurrentTime.AddSeconds(7d);
            Casters.Add(new(Shape, origin, rotation, activation, actorID: actor.InstanceID, shapeDistance: Shape.Distance(origin, rotation),
                arenaProjectionLayer: layer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }
}
sealed class ExplosionCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ExplosionCross, new AOEShapeCross(40f, 4f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) { }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.GobbieBombBig && animState1 == 1)
        {
            var origin = actor.Position;
            var rotation = actor.Rotation;
            var layer = ResolveArenaProjectionLayer(actor.PosRot.Y);
            var activation = WorldState.CurrentTime.AddSeconds(6d);
            Casters.Add(new(Shape, origin, rotation, activation, actorID: actor.InstanceID, shapeDistance: Shape.Distance(origin, rotation),
                arenaProjectionLayer: layer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
    }
}

sealed class LoosefroxInkyjotsStates : StateMachineBuilder
{
    public LoosefroxInkyjotsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurrowSandPillar>()
            .ActivateOnEnter<SandBreath>()
            .ActivateOnEnter<GobspinHeadlops>()
            .ActivateOnEnter<BombToss>()
            .ActivateOnEnter<GobspinHeadlopsDonut>()
            .ActivateOnEnter<GoblinHammer>()
            .ActivateOnEnter<GobbieBombs>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ExplosionCross>()
            .ActivateOnEnter<Earthquake>()
            .Raw.Update = () => AllDeadOrDestroyed(LoosefroxInkyjots.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, PrimaryActorOID = (uint)OID.LoosefroxInkyjots, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14561u, SortOrder = 5)]
public sealed class LoosefroxInkyjots(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, -420f), new ArenaBoundsCircle(22f))
{
    public static readonly uint[] Bosses = [(uint)OID.LoosefroxInkyjots, (uint)OID.ChewchumPopoto];
    private Actor? _loosefrox;
    private Actor? _chewchum;

    public Actor? Loosefrom() => _loosefrox;
    public Actor? Chewchum() => _chewchum;

    protected override void UpdatePreModuleActivation()
    {
        _loosefrox ??= GetActor((uint)OID.LoosefroxInkyjots);
        _chewchum ??= GetActor((uint)OID.ChewchumPopoto);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(_loosefrox);
        Arena.Actor(_chewchum);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.OID == (uint)OID.ChewchumPopoto)
            {
                if (!_loosefrox?.IsDead ?? false)
                {
                    e.Priority = AIHints.Enemy.PriorityUndesirable;
                }
                break;
            }
        }
    }
}

