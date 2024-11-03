namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D103RukshsDheem;

public enum OID : uint
{
    Boss = 0x2CFF, // R4.0
    QueensHarpooner = 0x2D01, // R1.56
    DepthGrip = 0x2D00, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/QueensHarpooner->player, no cast, single-target
    SwiftShift = 19331, // Boss->location, no cast, single-target, teleport

    Bonebreaker = 19340, // Boss->player, 4.0s cast, single-target, tankbuster

    SeabedCeremonyVisual = 19323, // Boss->self, 4.0s cast, single-target
    SeabedCeremony = 19324, // Helper->self, 4.0s cast, range 60 circle

    DepthGrip = 19332, // Boss->self, 4.0s cast, single-target
    Arise = 19333, // DepthGrip->self, no cast, single-target
    WavebreakerVisual1 = 19334, // DepthGrip->self, no cast, single-target
    WavebreakerVisual2 = 19335, // DepthGrip->self, no cast, single-target
    Wavebreaker1 = 13268, // Helper->self, no cast, range 36 width 8 rect
    Wavebreaker2 = 13269, // Helper->self, no cast, range 21 width 10 rect

    FallingWaterVisual = 19325, // Boss->self, 5.0s cast, single-target, spread
    FallingWater = 19326, // Helper->player, 5.0s cast, range 8 circle

    RisingTide = 19339, // Boss->self, 3.0s cast, range 50 width 6 cross

    Meatshield = 19338, // QueensHarpooner->Boss, no cast, single-target
    CoralTrident = 19337, // QueensHarpooner->self, 5.0s cast, range 6 90-degree cone
    Seafoam = 19336, // QueensHarpooner->self, 7.0s cast, range 60 circle

    FlyingFountVisual = 19327, // Boss->self, 5.0s cast, single-target, stack
    FlyingFount = 19328, // Helper->player, 5.0s cast, range 6 circle

    CommandCurrentVisual = 19329, // Boss->self, 4.9s cast, single-target
    CommandCurrent = 19330 // Helper->self, 5.0s cast, range 40 30-degree cone
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    private static readonly WDir offset = new(0, 8);
    private static readonly Func<WPos, float> stayInBounds = p =>
        Math.Max(ShapeDistance.InvertedCircle(D103RukshsDheem.ArenaCenter + offset, 3)(p),
            ShapeDistance.InvertedCircle(D103RukshsDheem.ArenaCenter - offset, 3)(p));

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            switch (index)
            {
                case 0x17:
                    Arena.Bounds = D103RukshsDheem.NarrowBounds;
                    break;
                case 0x18:
                    Arena.Bounds = D103RukshsDheem.SplitBounds;
                    break;
            }
        }
        else if (state == 0x00080004 && index is 0x017 or 0x18)
            Arena.Bounds = D103RukshsDheem.DefaultBounds;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Arena.Bounds != D103RukshsDheem.DefaultBounds && !Arena.InBounds(actor.Position))
            hints.AddForbiddenZone(stayInBounds);
    }
}

class Wavebreaker(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect rectNarrow = new(36, 4);
    private static readonly AOEShapeRect rectWide = new(21, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
            yield break;
        var count = _aoes.Count;
        if (Arena.Bounds == D103RukshsDheem.SplitBounds)
            for (var i = 0; i < count; i++)
            {
                if (i == 0)
                    yield return _aoes[i] with { Color = Colors.Danger };
                else if (i > 0)
                    yield return _aoes[i];
            }
        else if (Arena.Bounds == D103RukshsDheem.DefaultBounds)
            for (var i = 0; i < count; i++)
                yield return _aoes[i];
        else
            for (var i = 0; i < Math.Clamp(count, 0, 4); i++)
            {
                if (_aoes[i].Rotation == _aoes[0].Rotation)
                    if (i == 0)
                        yield return _aoes[i] with { Color = Colors.Danger };
                    else if (i > 0)
                        yield return _aoes[i];
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Arise:
                AddAOEs(caster);
                break;
            case AID.Wavebreaker1:
            case AID.Wavebreaker2:
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                break;
        }
    }

    private void AddAOEs(Actor caster)
    {
        DateTime activation;
        var shape = rectNarrow;

        if (Arena.Bounds == D103RukshsDheem.NarrowBounds)
            activation = WorldState.FutureTime(_aoes.Count > 3 ? 11.6f : 8);
        else if (Arena.Bounds == D103RukshsDheem.SplitBounds)
            activation = WorldState.FutureTime(7.6f);
        else
        {
            activation = WorldState.FutureTime(9.8f);
            shape = rectWide;
        }
        _aoes.Add(new(shape, caster.Position, caster.Rotation, activation));
    }
}

class Drains(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> activeDrains = [], solvedDrains = [];
    private const int X = 11;
    private const float SideLength = 1.25f;
    private static readonly WPos[] drainPositions = new WPos[8];
    private const string Hint = "Block a drain!";
    private DateTime activation;

    static Drains()
    {
        int[] xPositions = [-X, X];
        var zStart = -465;
        var zStep = 10;
        var index = 0;

        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 4; j++)
                drainPositions[index++] = new(xPositions[i], zStart + j * zStep);
    }

    private static readonly AOEShapeRect square = new(SideLength, SideLength, SideLength);
    private const byte i0x0F = 0x0F;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < activeDrains.Count; ++i)
            yield return new(square, activeDrains[i], Color: Colors.SafeFromAOE, Risky: false);
        for (var i = 0; i < solvedDrains.Count; ++i)
        {
            var drain = solvedDrains[i];
            yield return new(square, drain, Color: IsBlockingDrain(actor, drain) ? Colors.SafeFromAOE : Colors.AOE, Risky: false);
        }
    }

    public static bool IsBlockingDrain(Actor actor, WPos pos) => actor.Position.InRect(pos, new Angle(), SideLength, SideLength, SideLength);

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 0x00020001 enabled, 0x00080004 temp disabled, 0x00200004 perm disabled disabled
        // 0x0F - -11, -465 to 0x12 - -11, -435
        // 0x13 - 11, -465 to 0x16 - 11, -435
        if (index is < i0x0F or > 0x16)
            return;

        var positionIndex = index - i0x0F;
        var drainPosition = drainPositions[positionIndex];

        switch (state)
        {
            case 0x00020001:
                activeDrains.Add(drainPosition);
                solvedDrains.Remove(drainPosition);
                if (activation == default)
                    activation = WorldState.FutureTime(12); // 16s if all 8 drains are active
                break;

            case 0x00080004:
                solvedDrains.Add(drainPosition);
                activeDrains.Remove(drainPosition);
                break;

            case 0x00200004:
                activeDrains.Clear();
                solvedDrains.Clear();
                activation = default;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var forbidden = new List<Func<WPos, float>>();
        for (var i = 0; i < activeDrains.Count; ++i)
            forbidden.Add(ShapeDistance.InvertedRect(activeDrains[i], new Angle(), SideLength, SideLength, SideLength));
        for (var i = 0; i < solvedDrains.Count; ++i)
        {
            var drain = solvedDrains[i];
            if (IsBlockingDrain(actor, drain))
            {
                forbidden.Add(ShapeDistance.InvertedRect(drain, new Angle(), SideLength, SideLength, SideLength));
                break; // can only block one drain at a time, no point to keep checking
            }
        }
        if (forbidden.Count > 0)
            hints.AddForbiddenZone(p => forbidden.Max(f => f(p)), activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor).Any();
        if (!aoes)
            return;
        if (ActiveAOEs(slot, actor).Any(c => c.Check(actor.Position)))
            hints.Add(Hint, false);
        else
            hints.Add(Hint);
    }
}

class SeabedCeremony(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SeabedCeremony));
class Seafoam(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Seafoam));
class Bonebreaker(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.Bonebreaker));
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FallingWater), 8);
class FlyingFount(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.FlyingFount), 6);
class CommandCurrent(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CommandCurrent), new AOEShapeCone(40, 15.Degrees()));
class CoralTrident(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CoralTrident), new AOEShapeCone(6, 45.Degrees()));
class RisingTide(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RisingTide), new AOEShapeCross(50, 3));

class D103RukshsDheemStates : StateMachineBuilder
{
    public D103RukshsDheemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<SeabedCeremony>()
            .ActivateOnEnter<Seafoam>()
            .ActivateOnEnter<Bonebreaker>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<FlyingFount>()
            .ActivateOnEnter<CommandCurrent>()
            .ActivateOnEnter<CoralTrident>()
            .ActivateOnEnter<RisingTide>()
            .ActivateOnEnter<Wavebreaker>()
            .ActivateOnEnter<Drains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9264)]
public class D103RukshsDheem(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultBounds)
{
    public static readonly WPos ArenaCenter = new(-0, -450);
    private const float X = 15.5f;
    private const float Z = 19.5f;
    public static readonly ArenaBoundsRect DefaultBounds = new(X, Z);
    public static readonly ArenaBoundsRect NarrowBounds = new(X - 7.5f, Z);
    public static readonly ArenaBoundsComplex SplitBounds = new([new Rectangle(ArenaCenter, X, Z)], [new Rectangle(ArenaCenter, X, 4)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.QueensHarpooner).Concat([PrimaryActor]));
    }
}
