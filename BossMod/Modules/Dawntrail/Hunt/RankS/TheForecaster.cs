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

sealed class FloodConditions(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FloodConditions, 6f);
sealed class GaleForceWinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GaleForceWinds, new AOEShapeRect(40f, 20f));
sealed class Hyperelectricity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hyperelectricity, 10f);
sealed class WildfireConditions(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WildfireConditions, new AOEShapeDonut(5f, 40f));
sealed class BlizzardConditions(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlizzardConditions, new AOEShapeCross(40f, 2.5f));

sealed class ForecastClimateChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40f, 20f);
    private static readonly AOEShapeDonut donut = new(5f, 40f);
    private static readonly AOEShapeCircle circle = new(10f);
    private static readonly AOEShapeCross cross = new(40f, 2.5f);
    private readonly List<AOEInstance> _aoes = new(3);
    private AOEShape[] shapes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var aoe0Shape = aoes[0].Shape;
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == 0)
            {
                if (count > 1)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else if ((aoe0Shape == circle || aoe0Shape == donut) && (aoe.Shape == donut || aoe.Shape == circle))
                aoe.Risky = false;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Forecast1:
                shapes = [circle, rect, donut];
                break;
            case (uint)AID.Forecast2:
                shapes = [rect, donut, circle];
                break;
            case (uint)AID.Forecast3:
                shapes = [donut, circle, cross];
                break;
            case (uint)AID.Forecast4:
                shapes = [cross, circle, rect];
                break;
            case (uint)AID.ClimateChange1:
                AdjustForClimateChange(rect, cross);
                break;
            case (uint)AID.ClimateChange2:
                AdjustForClimateChange(cross, donut);
                break;
            case (uint)AID.ClimateChange3:
                AdjustForClimateChange(circle, rect);
                break;
            case (uint)AID.ClimateChange4:
                AdjustForClimateChange(donut, circle);
                break;
            case (uint)AID.WeatherChannelFirstCircle:
            case (uint)AID.WeatherChannelFirstCross:
            case (uint)AID.WeatherChannelFirstDonut:
            case (uint)AID.WeatherChannelFirstRect:
                if (shapes.Length == 0)  // if people join fight late, shapes might be empty
                    return;
                AddAOE(shapes[0]);
                AddAOE(shapes[1], 3.1f);
                AddAOE(shapes[2], 6.1f);
                shapes = [];
                break;
        }
        void AdjustForClimateChange(AOEShape shapeOld, AOEShape shapeNew)
        {
            if (shapes.Length == 0) // if people join fight late, shapes might be empty
                return;
            for (var i = 0; i < 3; ++i)
            {
                if (shapes[i] == shapeOld)
                {
                    shapes[i] = shapeNew;
                    return;
                }
            }
        }
        void AddAOE(AOEShape shape, float delay = default) => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, delay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.WeatherChannelFirstCircle:
                case (uint)AID.WeatherChannelFirstCross:
                case (uint)AID.WeatherChannelFirstDonut:
                case (uint)AID.WeatherChannelFirstRect:
                case (uint)AID.WeatherChannelRestCircle:
                case (uint)AID.WeatherChannelRestDonut:
                case (uint)AID.WeatherChannelRestRect:
                case (uint)AID.WeatherChannelRestCross:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

sealed class TheForecasterStates : StateMachineBuilder
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
public sealed class TheForecaster(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
