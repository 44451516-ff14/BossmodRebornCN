namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D122Nixie;

public enum OID : uint
{
    Boss = 0x307F, // R2.4
    Icicle = 0x3081, // R1.0
    UnfinishedNixie = 0x3080, // R1.2
    Geyser = 0x1EB0C7,
    CloudPlatform = 0x1EA1A1, // R0.5s-2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 22932, // Boss->player, no cast, single-target
    Teleport = 22933, // Boss->location, no cast, single-target

    CrashSmash = 22927, // Boss->self, 3.0s cast, single-target
    CrackVisual = 23481, // Icicle->self, 5.0s cast, single-target
    Crack = 22928, // Icicle->self, no cast, range 80 width 3 rect

    ShowerPower = 22929, // Boss->self, 3.0s cast, single-target
    Gurgle = 22930, // Helper->self, no cast, range 60 width 10 rect

    PitterPatter = 22920, // Boss->self, 3.0s cast, single-target
    Sploosh = 22926, // Helper->self, no cast, range 6 circle, geysirs, no dmg, just throwing player around
    FallDamage = 22934, // Helper->player, no cast, single-target

    SinginInTheRain = 22921, // UnfinishedNixie->self, 40.0s cast, single-target
    SeaShantyVisual = 22922, // Boss->self, no cast, single-target
    SeaShanty = 22924, // Helper->self, no cast, ???
    SeaShantyEnrage = 22923, // Helper->self, no cast, ???

    SplishSplash = 22925, // Boss->self, 3.0s cast, single-target
    Sputter = 22931 // Helper->player, 5.0s cast, range 6 circle, spread
}

public enum TetherID : uint
{
    Tankbuster = 8, // Icicle->player
    Gurgle = 3 // Boss->Helper
}

class Gurgle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60, 5);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly Angle[] angles = [89.999f.Degrees(), -90.004f.Degrees()];
    private static readonly Dictionary<byte, (WPos, Angle)> aoePositions = new()
    {
        { 0x13, (new(-20, -165), angles[0]) },
        { 0x14, (new(-20, -155), angles[0]) },
        { 0x15, (new(-20, -145), angles[0]) },
        { 0x16, (new(-20, -135), angles[0]) },
        { 0x17, (new(20, -165), angles[1]) },
        { 0x18, (new(20, -155), angles[1]) },
        { 0x19, (new(20, -145), angles[1]) },
        { 0x1A, (new(20, -135), angles[1]) }
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && aoePositions.TryGetValue(index, out var value))
        {
            _aoes.Add(new(rect, value.Item1, value.Item2, WorldState.FutureTime(9)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Gurgle)
            _aoes.Clear();
    }
}

class Crack(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(80, 1.5f);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tankbuster)
            CurrentBaits.Add(new(source, WorldState.Actors.Find(tether.Target)!, rect, WorldState.FutureTime(5.4f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Crack)
            CurrentBaits.RemoveAll(x => x.Source == caster);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("4x Tankbuster cleave");
    }
}

class GeysersCloudPlatform(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6);
    private readonly List<AOEInstance> _aoes = [];
    private bool active;
    private const string RiskHint = "Go to correct geyser!";
    private const string StayHint = "Wait for erruption!";

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Arena.Bounds == D122Nixie.DefaultArena)
        {
            var closestGeysir = _aoes.MinBy(a => (a.Origin - D122Nixie.CloudCenter).LengthSq());
            foreach (var a in _aoes)
            {
                var safeGeysir = active && a == closestGeysir;
                yield return a with { Shape = safeGeysir ? circle with { InvertForbiddenZone = true } : circle, Color = safeGeysir ? Colors.SafeFromAOE : Colors.AOE };
            }
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x12)
        {
            if (state == 0x00020001)
                active = true;
            else if (state == 0x00080004)
                active = false;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Geyser)
            _aoes.Add(new(circle, actor.Position, default, WorldState.FutureTime(3.9f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Sploosh)
            _aoes.RemoveAll(x => x.Origin == caster.Position);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (D122Nixie.Cloud.Contains(pc.Position - D122Nixie.CloudCenter))
        {
            Arena.Bounds = D122Nixie.Cloud;
            Arena.Center = D122Nixie.CloudCenter;
        }
        else
        {
            Arena.Bounds = D122Nixie.DefaultArena;
            Arena.Center = D122Nixie.ArenaCenter;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeAOEs = ActiveAOEs(slot, actor).ToList();
        if (activeAOEs.Any(c => c.Color == Colors.SafeFromAOE && !c.Check(actor.Position)))
            hints.Add(RiskHint);
        else if (activeAOEs.Any(c => c.Color == Colors.SafeFromAOE && c.Check(actor.Position)))
            hints.Add(StayHint, false);
        else
            base.AddHints(slot, actor, hints);
    }
}

class Sputter(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Sputter), 6);

class D122NixieStates : StateMachineBuilder
{
    public D122NixieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gurgle>()
            .ActivateOnEnter<Crack>()
            .ActivateOnEnter<Sputter>()
            .ActivateOnEnter<GeysersCloudPlatform>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746, NameID = 9738)]
public class D122Nixie(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(0, -150);
    public static readonly WPos CloudCenter = new(0, -175);
    public static readonly ArenaBoundsRect Cloud = new(9.5f, 5.5f);
    public static readonly ArenaBoundsSquare DefaultArena = new(19.5f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.UnfinishedNixie).Concat([PrimaryActor]));
    }
}
