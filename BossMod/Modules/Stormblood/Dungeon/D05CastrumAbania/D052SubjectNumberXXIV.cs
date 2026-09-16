namespace BossMod.Stormblood.Dungeon.D05CastrumAbania.D052SubjectNumberXXIV;

public enum OID : uint
{
    Boss = 0x3F3B, // R3.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target

    ElementalOverload1 = 33450, // Boss->self, 5.0s cast, range 60 circle
    ElementalOverload2 = 33452, // Boss->self, 5.0s cast, range 60 circle
    ElementalOverload3 = 33451, // Boss->self, 5.0s cast, range 60 circle
    ElementalOverload4 = 33453, // Boss->self, 5.0s cast, range 60 circle
    ElementalOverload5 = 33448, // Boss->self, 5.0s cast, range 60 circle
    ElementalOverload6 = 33449, // Boss->self, 5.0s cast, range 60 circle

    DiscreteMagickTowersVisual = 33748, // Boss->self, 5.0+0,5s cast, single-target
    ThunderII = 33464, // Helper->self, 5.5s cast, range 5 circle
    Electrify = 33465, // Helper->self, no cast, range 60 circle, tower fail

    DiscreteMagickBaitVisual = 33749, // Boss->self, 4.5+0,5s cast, single-target
    SparkingCurrentMarker = 33467, // Helper->player, no cast, single-target
    SparkingCurrent = 33466, // Helper->self, no cast, range 20 width 6 rect

    SerialMagicks1 = 33750, // Boss->self, 5.0+0,5s cast, single-target
    SerialMagicks2 = 33747, // Boss->self, 3.5+0,5s cast, single-target
    SerialMagicks3 = 33456, // Boss->self, 3.5+0,5s cast, single-target
    SystemError = 33459, // Boss->self, no cast, single-target

    DiscreteMagickTriflameVisual = 33457, // Boss->self, 3.5+0,5s cast, single-target
    Triflame = 33463, // Helper->self, 4.0s cast, range 60 60-degree cone

    DiscreteMagickStackVisual = 33458, // Boss->self, 4.5+0,5s cast, single-target
    FireII = 33462, // Helper->player, 5.0s cast, range 5 circle

    DiscreteMagickIceGridVisual = 33454, // Boss->self, 3.5+0,5s cast, single-target
    IceGrid = 33460, // Helper->self, 4.0s cast, range 40 width 4 rect

    DiscreteMagickSpreadVisual = 33455, // Boss->self, 4.5+0,5s cast, single-target
    BlizzardII = 33461, // Helper->player, 5.0s cast, range 5 circle
}

sealed class SparkingCurrent(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly AOEShapeRect rect = new(20f, 3f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.SparkingCurrentMarker)
        {
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.MainTargetID)!, rect, WorldState.FutureTime(5d)));
        }
        else if (id == (uint)AID.SparkingCurrent)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class ThunderII(BossModule module) : Components.CastTowers(module, (uint)AID.ThunderII, 5f);
sealed class FireII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FireII, 5f, 4, 4);
sealed class BlizzardII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BlizzardII, 5f);
sealed class IceGrid(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceGrid, new AOEShapeRect(40f, 2f), 10);
sealed class Triflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Triflame, new AOEShapeCone(60f, 30f.Degrees()), 3);
sealed class ElementalOverload(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ElementalOverload1, (uint)AID.ElementalOverload2, (uint)AID.ElementalOverload3,
    (uint)AID.ElementalOverload4, (uint)AID.ElementalOverload5, (uint)AID.ElementalOverload6]);

sealed class D052SubjectNumberXXIVStates : StateMachineBuilder
{
    public D052SubjectNumberXXIVStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SparkingCurrent>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<IceGrid>()
            .ActivateOnEnter<Triflame>()
            .ActivateOnEnter<ElementalOverload>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<BlizzardII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242u, NameID = 12392u)]
public sealed class D052SubjectNumberXXIV : BossModule
{
    public D052SubjectNumberXXIV(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D052SubjectNumberXXIV(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Circle(new(10.5f, 186.5f), 19.55f)], [new Rectangle(new(11f, 207f), 20f, 1.5f), new Rectangle(new(30f, 187f), 1.1f, 20f)]);
        return (arena.Center, arena);
    }
}
