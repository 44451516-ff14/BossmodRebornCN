namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D052Rhitahtyn;

public enum OID : uint
{
    Boss = 0x346B, // R=9.0
    Crystal = 0x35DA, // R1.0
    TargetMarker1 = 0x346C, // R1.0
    TargetMarker2 = 0x346D, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 25688, // Boss->location, no cast, single-target

    AnvilOfTartarus = 25686, // Boss->player, 5.0s cast, single-target, tankbuster
    Impact = 25679, // Helper->self, 4.0s cast, range 14 width 40 rect

    ShieldSkewer = 25680, // Boss->location, 11.0s cast, range 40 width 14 rect
    ShrapnelShellVisual = 25682, // Boss->self, 3.0s cast, single-target
    ShrapnelShellAOE = 25684, // Helper->location, 3.5s cast, range 5 circle
    TargetMarkerVisual = 25683, // 346C/346D->self, no cast, single-target

    TartareanImpact = 25685, // Boss->self, 5.0s cast, range 60 circle, raidwide
    TartareanSpark = 25687, // Boss->self, 3.0s cast, range 40 width 6 rect

    Vexillatio = 25678 // Boss->self, 4.0s cast, single-target
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public bool Safespots;
    private DateTime activation;
    public static readonly WPos ArenaCenter = new(11, 144);
    public static readonly ArenaBoundsSquare DefaultBounds = new(19.5f);
    private static readonly WPos[] positions = [new(19.5f, 152), new(2.5f, 152), new(2.5f, 136),
    new(19.5f, 136)];
    private static readonly Rectangle[] squares = positions.Select(pos => new Rectangle(pos, 2, 1.5f)).ToArray();
    private static readonly Rectangle[] rect = [new(ArenaCenter, 6.5f, 19.5f)];
    private readonly List<Shape> union = [];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index == 0x00)
            UpdateArena();
        else if (state == 0x00200010)
        {
            if (index == 0x01) // 0x01 and 0x04 always are a pair
                union.AddRange([squares[0], squares[2]]);
            else if (index == 0x02) // 0x02 and 0x03 always are a pair
                union.AddRange([squares[1], squares[3]]);

            Safespots = true;
            activation = WorldState.FutureTime(7.3f);
            UpdateArena();
        }
        else if (state == 0x00080004)
        {
            switch (index)
            {
                case 0x00:
                    union.Clear();
                    Arena.Bounds = DefaultBounds;
                    break;
                case 0x01:
                case 0x02:
                    Safespots = false;
                    break;
            }
        }
    }

    private void UpdateArena()
    {
        Arena.Bounds = new ArenaBoundsComplex([.. rect, .. union]);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        if (Safespots) // force AI to move to a safespot before it becomes available
        {
            foreach (var shape in union)
                if (shape is Rectangle rectangle)
                    forbidden.Add(ShapeDistance.InvertedCircle(rectangle.Center, 5));
        }
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), activation);
    }
}

class ShieldSkewer(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShieldSkewer), new AOEShapeRect(40, 7))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation,
    Module.CastFinishAt(c.CastInfo), Color == 0 ? Colors.AOE : Color, !Module.FindComponent<ArenaChanges>()!.Safespots));
}

class Shrapnel(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(6);
    private static readonly Dictionary<string, WPos[]> shrapnelPositions = new()
    {
        ["SWNE"] = [ new(-6.027f, 153.979f), new(7.98f, 160.998f), new(20.981f, 126.97f), new(7.98f, 146.99f), new(27.97f, 140.978f),
                                new(20.981f, 140.978f), new(27.97f, 133.99f), new(13.992f, 140.978f), new(20.981f, 133.99f), new(0.992f, 146.99f),
                                new(0.992f, 153.979f), new(-6.027f, 160.998f), new(13.992f, 126.97f), new(13.992f, 133.99f), new(0.992f, 160.998f),
                                new(-6.027f, 146.99f), new(7.98f, 153.979f), new(27.97f, 126.97f) ],
        ["NWSE"] = [ new(20.981f, 160.998f), new(13.992f, 153.979f), new(-6.027f, 140.978f), new(13.992f, 160.998f), new(-6.027f, 126.97f),
                                new(7.98f, 126.97f), new(0.992f, 133.99f), new(7.98f, 133.99f), new(-6.027f, 133.99f), new(27.97f, 153.979f),
                                new(13.992f, 146.99f), new(20.981f, 146.99f), new(0.992f, 126.97f), new(0.992f, 140.978f), new(27.97f, 160.998f),
                                new(27.97f, 146.99f), new(20.981f, 153.979f), new(7.98f, 140.978f) ],
        ["W"] = [ new(-6.027f, 160.998f), new(0.992f, 153.979f), new(7.98f, 133.99f), new(-6.027f, 146.99f), new(-6.027f, 140.978f),
                             new(0.992f, 133.99f), new(-6.027f, 133.99f), new(7.98f, 126.97f), new(0.992f, 140.978f), new(7.98f, 153.979f),
                             new(-6.027f, 153.979f), new(0.992f, 146.99f), new(0.992f, 126.97f), new(-6.027f, 126.97f), new(0.992f, 160.998f),
                             new(7.98f, 160.998f), new(7.98f, 146.99f), new(7.98f, 140.978f) ],
        ["E"] = [ new(13.992f, 160.998f), new(20.981f, 146.99f), new(20.981f, 140.978f), new(27.97f, 146.99f), new(27.97f, 140.978f),
                             new(20.981f, 133.99f), new(13.992f, 126.97f), new(27.97f, 126.97f), new(27.97f, 133.99f), new(13.992f, 146.99f),
                             new(27.97f, 153.979f), new(20.981f, 153.979f), new(20.981f, 126.97f), new(13.992f, 133.99f), new(13.992f, 153.979f),
                             new(27.97f, 160.998f), new(20.981f, 160.998f), new(13.992f, 140.978f) ]
    };
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void Update()
    {
        if (_aoes.Count == 0)
        {
            var target1SWNE = Module.Enemies(OID.TargetMarker1).FirstOrDefault(x => x.Position.Z < 144 && x.Rotation >= 0.1f.Degrees());
            var target1NWSE = Module.Enemies(OID.TargetMarker1).FirstOrDefault(x => x.Position.Z < 144 && x.Rotation <= -0.1f.Degrees());
            var target2W = Module.Enemies(OID.TargetMarker2).FirstOrDefault(x => x.Position.X < 11);
            var target2E = Module.Enemies(OID.TargetMarker2).FirstOrDefault(x => x.Position.X > 11);
            if (target1SWNE != null)
                AddAOEs("SWNE");
            else if (target1NWSE != null)
                AddAOEs("NWSE");
            else if (target2W != null)
                AddAOEs("W");
            else if (target2E != null)
                AddAOEs("E");
        }
    }

    private void AddAOEs(string direction)
    {
        foreach (var position in shrapnelPositions[direction])
            _aoes.Add(new(circle, position, default, WorldState.FutureTime(8)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID == AID.ShrapnelShellAOE)
            _aoes.RemoveAll(x => x.Origin.AlmostEqual(spell.LocXZ, 1));
    }
}

class Impact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Impact), new AOEShapeRect(14, 20));
class TartareanSpark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TartareanSpark), new AOEShapeRect(40, 3));
class AnvilOfTartarus(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.AnvilOfTartarus));
class TartareanImpact(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.TartareanImpact));

class D052RhitahtynStates : StateMachineBuilder
{
    public D052RhitahtynStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<ShieldSkewer>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<Shrapnel>()
            .ActivateOnEnter<TartareanSpark>()
            .ActivateOnEnter<AnvilOfTartarus>()
            .ActivateOnEnter<TartareanImpact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10292)]
public class D052Rhitahtyn(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.DefaultBounds);
