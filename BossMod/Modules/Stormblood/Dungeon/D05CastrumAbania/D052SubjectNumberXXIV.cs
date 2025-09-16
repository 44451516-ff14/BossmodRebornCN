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

class SparkingCurrent(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(20f, 3f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SparkingCurrentMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.MainTargetID)!, rect, WorldState.FutureTime(5d)));
        else if (spell.Action.ID == (uint)AID.SparkingCurrent)
            CurrentBaits.Clear();
    }
}

class ThunderII(BossModule module) : Components.CastTowers(module, (uint)AID.ThunderII, 5f);
class FireII(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.FireII, 5f, 4, 4);
class BlizzardII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.BlizzardII, 5f);
class IceGrid(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IceGrid, new AOEShapeRect(40f, 2f), 10);
class Triflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Triflame, new AOEShapeCone(60f, 30f.Degrees()), 3);
class ElementalOverload1(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalOverload1);
class ElementalOverload2(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalOverload2);
class ElementalOverload3(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalOverload3);
class ElementalOverload4(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalOverload4);
class ElementalOverload5(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalOverload5);
class ElementalOverload6(BossModule module) : Components.RaidwideCast(module, (uint)AID.ElementalOverload6);

class D052SubjectNumberXXIVStates : StateMachineBuilder
{
    public D052SubjectNumberXXIVStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SparkingCurrent>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<IceGrid>()
            .ActivateOnEnter<Triflame>()
            .ActivateOnEnter<ElementalOverload1>()
            .ActivateOnEnter<ElementalOverload2>()
            .ActivateOnEnter<ElementalOverload3>()
            .ActivateOnEnter<ElementalOverload4>()
            .ActivateOnEnter<ElementalOverload5>()
            .ActivateOnEnter<ElementalOverload6>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<BlizzardII>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 242, NameID = 12392)]
public class D052SubjectNumberXXIV(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Circle(new(10.5f, 186.5f), 19.55f)], [new Rectangle(new(11, 207), 20, 1.5f), new Rectangle(new(30, 187), 1.1f, 20f)]);
}
