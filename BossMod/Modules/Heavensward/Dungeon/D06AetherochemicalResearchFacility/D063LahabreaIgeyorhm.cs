namespace BossMod.Heavensward.Dungeon.D06AetherochemicalResearchFacility.D063LahabreaIgeyorhm;

public enum OID : uint
{
    Boss = 0x3DA4, // R3.5
    Igeyorhm = 0x3DA3, // R3.5
    BurningStar = 0x3DA6, // R1.5
    FrozenStar = 0x3DA5, // R1.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 32818, // Boss/Lahabrea->player, no cast, single-target
    Teleport = 32791, // Boss->location, no cast, single-target, teleport to center

    AetherialDivideIgeyorhm = 32686, // Boss->Lahabrea, no cast, single-target, Igeyorhm donates HP to Lahabrea
    AetherialDivideLahabrea = 32685, // Lahabrea->Boss, no cast, single-target, Lahabrea donates HP to Igeyorhm

    CircleOfIce = 31878, // Boss->self, 3.0s cast, single-target
    CircleOfIceAOE = 31879, // FrozenStar->self, 3.0s cast, range 5-15 donut

    CircleOfIcePrime1 = 31881, // FrozenStar->self, no cast, single-target
    CircleOfIcePrime2 = 31882, // FrozenStar->self, no cast, single-target
    CircleOfIcePrimeAOE = 33019, // Helper->self, 3.0s cast, range 5-40 donut

    DarkFireII = 32687, // Lahabrea->self, 6.0s cast, single-target
    DarkFireIIAOE = 32688, // Helper->player, 6.0s cast, range 6 circle

    EndOfDays = 31892, // Helper->player, no cast, single-target
    EndOfDays1 = 31891, // Lahabrea->self, 5.0s cast, single-target
    EndOfDays2 = 33029, // Helper->self, no cast, range 50 width 8 rect, line stack

    EsotericFusion1 = 31880, // Boss->self, 3.0s cast, single-target
    EsotericFusion2 = 31888, // Lahabrea->self, 3.0s cast, single-target

    FireSphere = 31886, // Lahabrea->self, 3.0s cast, single-target
    FireSphereAOE = 31887, // BurningStar->self, 3.0s cast, range 8 circle

    FireSpherePrime1 = 31889, // BurningStar->self, no cast, single-target
    FireSpherePrime2 = 31890, // BurningStar->self, no cast, single-target
    FireSpherePrimeAOE = 33020, // Helper->self, 2.0s cast, range 16 circle

    GripOfNight = 32790, // Boss->self, 6.0s cast, range 40 150-degree cone

    ShadowFlare = 31885 // Boss/Lahabrea->self, 5.0s cast, range 40 circle
}

public enum TetherID : uint
{
    StarTether = 110 // BurningStar/FrozenStar->BurningStar/FrozenStar
}

class ShadowFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.ShadowFlare);
class GripOfNight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GripOfNight, new AOEShapeCone(40f, 75f.Degrees()));
class DarkFireIIAOE(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DarkFireIIAOE, 6f);
class EndOfDays(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.EndOfDays, (uint)AID.EndOfDays2);

class Stars(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donutSmall = new(5f, 15f), donutBig = new(5f, 40f);
    private static readonly AOEShapeCircle circleSmall = new(8f), circleBig = new(16f);
    private readonly List<AOEInstance> _aoes = new(5);
    private readonly List<Actor> _stars = new(8);

    private bool _tutorialFire, _tutorialIce;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StarTether)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            var targetPos = target.Position;
            var sourcePos = source.Position;
            var midpoint = new WPos((sourcePos.X + targetPos.X) * 0.5f, (sourcePos.Z + targetPos.Z) * 0.5f).Quantized();
            switch (source.OID)
            {
                case (uint)OID.FrozenStar:
                    ActivateAOE(donutSmall, donutBig, midpoint, source, target);
                    break;
                case (uint)OID.BurningStar:
                    ActivateAOE(circleSmall, circleBig, midpoint, source, target);
                    break;
            }
        }
    }

    private void ActivateAOE(AOEShape smallShape, AOEShape bigShape, WPos midpoint, Actor source, Actor target)
    {
        _stars.Remove(source);
        _stars.Remove(target);
        var activation = WorldState.FutureTime(10.6d);
        _aoes.Add(new(bigShape, midpoint, default, activation));
        var hasDonutBig = false;
        var circleBigCount = 0;

        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].Shape == donutBig)
            {
                hasDonutBig = true;
                break;
            }
            else
                ++circleBigCount;
        }

        if (hasDonutBig || circleBigCount == 2)
        {
            var countS = _stars.Count;
            for (var i = 0; i < countS; ++i)
                _aoes.Add(new(smallShape, _stars[i].Position, default, activation));
            _stars.Clear();
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is ((uint)OID.FrozenStar) or ((uint)OID.BurningStar))
        {
            _stars.Add(actor);

            var frozenStarCount = 0;
            var burningStarCount = 0;

            if (_tutorialIce && _tutorialFire)
                return;
            var count = _stars.Count;
            for (var i = 0; i < count; ++i)
            {
                var star = _stars[i];
                if (star.OID == (uint)OID.FrozenStar)
                    ++frozenStarCount;
                else if (star.OID == (uint)OID.BurningStar)
                    ++burningStarCount;
            }

            if (!_tutorialIce && frozenStarCount == 4)
                Tutorial(donutSmall, ref _tutorialIce);
            else if (!_tutorialFire && burningStarCount == 5)
                Tutorial(circleSmall, ref _tutorialFire);
        }
    }

    private void Tutorial(AOEShape shape, ref bool tutorialFlag)
    {
        tutorialFlag = true;
        var activation = WorldState.FutureTime(7.8d);
        var count = _stars.Count;
        for (var i = 0; i < count; ++i)
            _aoes.Add(new(shape, _stars[i].Position, default, activation));
        _stars.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CircleOfIceAOE or (uint)AID.CircleOfIcePrimeAOE or (uint)AID.FireSphereAOE or (uint)AID.FireSpherePrime1)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

class D063LahabreaIgeyorhmStates : StateMachineBuilder
{
    public D063LahabreaIgeyorhmStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShadowFlare>()
            .ActivateOnEnter<GripOfNight>()
            .ActivateOnEnter<Stars>()
            .ActivateOnEnter<EndOfDays>()
            .ActivateOnEnter<DarkFireIIAOE>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 38, NameID = 2143, SortOrder = 10)]
public class D063LahabreaIgeyorhm(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly ArenaBoundsCustom arena = new([new Polygon(new(230f, -181f), 20.26f, 24)], [new Rectangle(new(230f, -160f), 20f, 1.94f)]);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Igeyorhm));
    }
}
