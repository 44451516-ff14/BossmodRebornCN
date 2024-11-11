namespace BossMod.Dawntrail.Hunt.RankS.TheForecaster;

public enum OID : uint
{
    Boss = 0x4397 // R6.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    GaleForceWinds = 38534, // Boss->self, 4.0s cast, range 40 width 40 rect
    BlizzardConditions = 38535, // Boss->self, 4.0s cast, range 40 width 5 cross
    Hyperelectricity = 38533, // Boss->self, 4.0s cast, range 10 circle
    WildfireConditions = 38532, // Boss->self, 4.0s cast, range 5-40 donut

    Forecast1 = 38521, // Boss->self, 7.0s cast, single-target (Hyperelectricity, Gale-force winds, Wildfires)
    Forecast2 = 38523, // Boss->self, 7.0s cast, single-target (Gale-force winds, Wildfires, Hyperelectricity)
    Forecast3 = 38520, // Boss->self, 7.0s cast, single-target (Wildfires, Hyperelectricity, Blizzards)
    Forecast4 = 38522, // Boss->self, 7.0s cast, single-target (Blizzard, Hyperelectricity, Gale-force winds)

    WeatherChannelFirstCircle = 38526, // Boss->self, 5.0s cast, range 10 circle
    WeatherChannelFirstRect = 38528, // Boss->self, 5.0s cast, range 40 width 40 rect
    WeatherChannelFirstDonut = 38524, // Boss->self, 5.0s cast, range 5-40 donut
    WeatherChannelFirstCross = 38530, // Boss->self, 5.0s cast, range 40 width 5 cross
    WeatherChannelRestCircle = 38527, // Boss->self, no cast, range 10 circle
    WeatherChannelRestRect = 38529, // Boss->self, no cast, range 40 width 40 rect
    WeatherChannelRestDonut = 38525, // Boss->self, no cast, range 5-40 donut
    WeatherChannelRestCross = 38531, // Boss->self, no cast, range 40 width 5 cross
    FloodConditions = 38536, // Boss->location, 3.0s cast, range 6 circle
    ClimateChange1 = 39128, // Boss->self, 3.0s cast, single-target (Blizzard replaces Gale-force winds)
    ClimateChange2 = 39125, // Boss->self, 3.0s cast, single-target (Wildfire replaces Blizzard)
    ClimateChange3 = 39127, // Boss->self, 3.0s cast, single-target (Gale-force winds replace Hyperelectricity)
    ClimateChange4 = 39126, // Boss->self, 3.0s cast, single-target (Hyperelectricity replaces Wildfire)

    ClimateStatusEffects = 38537, // Boss->self, no cast, single-target, boss resets its status effects
    ClimateChangeStatusEffect = 39133 // Boss->self, no cast, single-target
}

class FloodConditions(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FloodConditions), 6);
class GaleForceWinds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GaleForceWinds), new AOEShapeRect(40, 20));
class Hyperelectricity(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hyperelectricity), new AOEShapeCircle(10));
class WildfireConditions(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WildfireConditions), new AOEShapeDonut(5, 40));
class BlizzardConditions(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlizzardConditions), new AOEShapeCross(40, 2.5f));

class ForecastClimateChange(BossModule module) : Components.GenericAOEs(module)
{
    private enum Forecast { None, HGW, GWH, WHB, BHG }
    private Forecast currentForecast;
    private enum ClimateChange { None, G2B, B2W, H2G, W2H }
    private ClimateChange currentClimateChange;
    private static readonly AOEShapeRect rect = new(40, 20);
    private static readonly AOEShapeDonut donut = new(5, 40);
    private static readonly AOEShapeCircle circle = new(10);
    private static readonly AOEShapeCross cross = new(40, 2.5f);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly HashSet<AID> castEnd = [AID.WeatherChannelFirstCircle, AID.WeatherChannelFirstCross, AID.WeatherChannelFirstDonut,
    AID.WeatherChannelFirstRect, AID.WeatherChannelRestCircle, AID.WeatherChannelRestDonut, AID.WeatherChannelRestRect, AID.WeatherChannelRestCross];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 0)
            yield return _aoes[0] with { Color = Colors.Danger };
        if (count > 1)
            yield return _aoes[1] with { Risky = false };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Forecast1:
                currentForecast = Forecast.HGW;
                break;
            case AID.Forecast2:
                currentForecast = Forecast.GWH;
                break;
            case AID.Forecast3:
                currentForecast = Forecast.WHB;
                break;
            case AID.Forecast4:
                currentForecast = Forecast.BHG;
                break;
            case AID.ClimateChange1:
                currentClimateChange = ClimateChange.G2B;
                break;
            case AID.ClimateChange2:
                currentClimateChange = ClimateChange.B2W;
                break;
            case AID.ClimateChange3:
                currentClimateChange = ClimateChange.H2G;
                break;
            case AID.ClimateChange4:
                currentClimateChange = ClimateChange.W2H;
                break;
            case AID.WeatherChannelFirstCircle:
            case AID.WeatherChannelFirstCross:
            case AID.WeatherChannelFirstDonut:
            case AID.WeatherChannelFirstRect:
                AddAOEs(spell);
                break;
        }
    }

    private void AddAOEs(ActorCastInfo spell)
    {
        var position = Module.PrimaryActor.Position;
        AOEShape[] shapes =
        [
            AdjustShape(GetShapeForForecast(currentForecast, 0)),
            AdjustShape(GetShapeForForecast(currentForecast, 1)),
            AdjustShape(GetShapeForForecast(currentForecast, 2)),
        ];

        _aoes.Add(new(shapes[0], position, spell.Rotation, Module.CastFinishAt(spell)));
        _aoes.Add(new(shapes[1], position, spell.Rotation, Module.CastFinishAt(spell, 3.1f)));
        _aoes.Add(new(shapes[2], position, spell.Rotation, Module.CastFinishAt(spell, 6.1f)));
        currentClimateChange = ClimateChange.None;
    }

    private static AOEShape GetShapeForForecast(Forecast forecast, int index)
    {
        AOEShape[] hgwShapes = [circle, rect, donut];
        AOEShape[] gwhShapes = [rect, donut, circle];
        AOEShape[] whbShapes = [donut, circle, cross];
        AOEShape[] bhgShapes = [cross, circle, rect];

        return forecast switch
        {
            Forecast.HGW => hgwShapes[index],
            Forecast.GWH => gwhShapes[index],
            Forecast.WHB => whbShapes[index],
            Forecast.BHG => bhgShapes[index],
            _ => cross
        };
    }

    private AOEShape AdjustShape(AOEShape shape)
    {
        return shape switch
        {
            AOEShapeRect when currentClimateChange == ClimateChange.G2B => cross,
            AOEShapeCross when currentClimateChange == ClimateChange.B2W => donut,
            AOEShapeCircle when currentClimateChange == ClimateChange.H2G => rect,
            AOEShapeDonut when currentClimateChange == ClimateChange.W2H => circle,
            _ => shape
        };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class TheForecasterStates : StateMachineBuilder
{
    public TheForecasterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForecastClimateChange>()
            .ActivateOnEnter<WildfireConditions>()
            .ActivateOnEnter<BlizzardConditions>()
            .ActivateOnEnter<Hyperelectricity>()
            .ActivateOnEnter<FloodConditions>()
            .ActivateOnEnter<GaleForceWinds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.S, NameID = 13437)]
public class TheForecaster(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
