namespace BossMod.Stormblood.Dungeon.D02ShisuiOfTheVioletTides.D023ShisuiYohi;

public enum OID : uint
{
    Boss = 0x1B0F, // R3.85
    Churn = 0x1B11, // R1.0
    NaishiNoKami = 0x1B10, // R3.0
    NaishiNoJo = 0x1C87, // R1.5
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    FoulNail = 8071, // Boss->players, no cast, single-target
    ThickFog = 8064, // Boss->self, 5.0s cast, range 40 circle
    WaterSplashVisual = 8067, // Helper->self, no cast, single-target
    BlackTide = 8065, // Boss->self, no cast, range 13 circle
    BubbleBurst = 8068, // Churn->self, no cast, range 40 circle, failed to kill bubble in time
    MadStare = 8066, // Boss->self, 5.0s cast, range 40 circle, gaze
    BiteAndRun1 = 8069, // NaishiNoKami->player, 30.0s cast, width 5 rect charge
    BiteAndRun2 = 8070 // NaishiNoJo->player, 15.0s cast, width 5 rect charge
}

class BlackTide(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(13f);
    private Actor? helper;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts >= 10 && helper != null) // I have seen logs with upto 18 water splashes, not sure how the amount is decided, need more logs to hopefully see a pattern
            return new AOEInstance[1] { new(circle, helper.Position.Quantized(), default, activation) };
        return [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WaterSplashVisual)
        {
            ++NumCasts;
            helper = caster;
            activation = WorldState.FutureTime(4f); // seen activation times from 4-6s
        }
        else if (spell.Action.ID == (uint)AID.BlackTide)
            NumCasts = 0;
    }
}

class BiteAndRun1(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.BiteAndRun1, 2.5f);
class BiteAndRun2(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.BiteAndRun2, 2.5f);
class MadStare(BossModule module) : Components.CastGaze(module, (uint)AID.MadStare);
class ThickFog(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThickFog);

class D023ShisuiYohiStates : StateMachineBuilder
{
    public D023ShisuiYohiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BlackTide>()
            .ActivateOnEnter<BiteAndRun1>()
            .ActivateOnEnter<BiteAndRun2>()
            .ActivateOnEnter<MadStare>()
            .ActivateOnEnter<ThickFog>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 235, NameID = 6243)]
public class D023ShisuiYohi(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(2.79f, -452.34f), new(4.19f, -452.35f), new(4.76f, -452.15f), new(10.37f, -449.83f), new(10.9f, -449.57f),
    new(16.48f, -443.99f), new(16.9f, -443.55f), new(19.45f, -437.37f), new(19.6f, -435.43f), new(19.58f, -428.49f),
    new(16.99f, -422.23f), new(16.64f, -421.71f), new(12.82f, -417.88f), new(12.4f, -417.49f), new(10.77f, -415.88f),
    new(6.6f, -414.16f), new(-6.31f, -414.07f), new(-6.89f, -414.27f), new(-10.64f, -415.82f), new(-11.2f, -416.27f),
    new(-16.59f, -421.66f), new(-16.93f, -422.06f), new(-19.43f, -428.1f), new(-19.6f, -436.96f), new(-19.42f, -437.47f),
    new(-17.08f, -443.11f), new(-16.81f, -443.66f), new(-10.96f, -449.52f), new(-10.52f, -449.78f), new(-4.32f, -452.34f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Churn));
        Arena.Actors(Enemies((uint)OID.NaishiNoKami));
        Arena.Actors(Enemies((uint)OID.NaishiNoJo));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.NaishiNoKami or (uint)OID.NaishiNoJo => 2,
                (uint)OID.Churn => 1,
                _ => 0
            };
        }
    }
}
