namespace BossMod.Dawntrail.Quest.JobQuests.Pictomancer.SomewhereOnlySheKnows.FlightOfTheGriffin;

public enum OID : uint
{
    Boss = 0x4296, // R9.2
    TheBirdOfPrey = 0x4297, // R1.96
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss/TheBirdOfPrey->player, no cast, single-target

    SwoopingFrenzy = 37519, // Boss->location, 4.0s cast, range 12 circle
    Feathercut = 37522, // TheBirdOfPrey->self, 3.0s cast, range 10 width 5 rect
    FrigidPulse = 37520, // Boss->self, 5.0s cast, range 12-60 donut
    EyeOfTheFierce = 37523, // TheBirdOfPrey->self, 5.0s cast, range 40 circle
    FervidPulse = 37521, // Boss->self, 5.0s cast, range 50 width 14 cross
}

class SwoopingFrenzy(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SwoopingFrenzy), 12);
class Feathercut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Feathercut), new AOEShapeRect(10, 2.5f));
class FrigidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FrigidPulse), new AOEShapeDonut(12, 60));
class FervidPulse(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FervidPulse), new AOEShapeCross(50, 7));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));

class FlightOfTheGriffinStates : StateMachineBuilder
{
    public FlightOfTheGriffinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SwoopingFrenzy>()
            .ActivateOnEnter<Feathercut>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<FervidPulse>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .Raw.Update = () => module.Enemies(OID.TheBirdOfPrey).Concat([module.PrimaryActor]).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70395, NameID = 13035)]
public class FlightOfTheGriffin(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(3.6f, -294.59f), new(3.61f, -292.54f), new(3.92f, -292.06f), new(4.04f, -291.54f), new(5.6f, -291.42f),
    new(6.24f, -291.41f), new(6.77f, -291.43f), new(7.07f, -291.83f), new(7.09f, -292.34f), new(8.36f, -292.65f),
    new(8.67f, -291.14f), new(8.96f, -290.68f), new(11.42f, -290.66f), new(11.84f, -290.95f), new(11.84f, -292.12f),
    new(21.6f, -292.43f), new(21.52f, -291.9f), new(21.74f, -291.43f), new(22.08f, -291.03f), new(22.08f, -289.91f),
    new(21.67f, -289.61f), new(21.41f, -286.83f), new(21.71f, -286.42f), new(23.09f, -286.4f), new(23.09f, -279.58f),
    new(21.81f, -279.59f), new(21.42f, -279.26f), new(21.45f, -276.68f), new(23.1f, -276.4f), new(23.1f, -274.13f),
    new(23.08f, -269.59f), new(22.03f, -269.59f), new(21.52f, -269.47f), new(21.42f, -268.98f), new(21.42f, -266.78f),
    new(21.77f, -266.42f), new(22.94f, -266.42f), new(23.07f, -259.59f), new(21.84f, -259.59f), new(21.42f, -259.28f),
    new(21.43f, -256.71f), new(23.08f, -256.42f), new(23.08f, -249.58f), new(21.75f, -249.58f), new(21.42f, -244.44f),
    new(20.56f, -244.44f), new(20.12f, -244.17f), new(19.97f, -243.68f), new(20.29f, -243.29f), new(22.16f, -241.45f),
    new(22.66f, -241.46f), new(23, -241.08f), new(23.44f, -237.48f), new(23.13f, -237.08f), new(22.22f, -236.59f),
    new(20.23f, -234.63f), new(19.95f, -234.21f), new(20.25f, -233.71f), new(21.65f, -233.56f), new(21.65f, -230.02f),
    new(21.54f, -229.53f), new(21.63f, -229.01f), new(22.04f, -228.67f), new(22.49f, -228.38f), new(22.59f, -221.23f),
    new(22.4f, -220.76f), new(21.93f, -220.56f), new(21.65f, -218.18f), new(21.66f, -217.67f), new(22.13f, -217.4f),
    new(22.54f, -217.09f), new(22.59f, -209.54f), new(22.28f, -209.14f), new(21.79f, -208.97f), new(21.65f, -208.28f),
    new(21.3f, -207.91f), new(19.22f, -207.76f), new(13.63f, -207.76f), new(12.65f, -207.79f), new(12.06f, -208.83f),
    new(9.58f, -208.84f), new(9.18f, -208.54f), new(9.16f, -207.96f), new(8.75f, -207.55f), new(7.05f, -207.6f),
    new(7.05f, -208.11f), new(7.01f, -208.62f), new(6.53f, -208.82f), new(4.44f, -208.82f), new(3.96f, -208.65f),
    new(3.89f, -208.14f), new(3.59f, -207.74f), new(3.56f, -207.24f), new(3.56f, -206.73f), new(-3.63f, -206.46f),
    new(-3.63f, -207.5f), new(-3.87f, -207.95f), new(-3.95f, -208.46f), new(-4.31f, -208.83f), new(-6.39f, -208.83f),
    new(-6.92f, -208.78f), new(-7.14f, -208.31f), new(-7.14f, -207.81f), new(-8.14f, -207.58f), new(-8.8f, -207.6f),
    new(-9.17f, -208.01f), new(-9.17f, -208.52f), new(-11.63f, -208.85f), new(-12.15f, -208.77f), new(-12.34f, -208.29f),
    new(-12.64f, -207.87f), new(-20.95f, -207.82f), new(-21.41f, -208.12f), new(-21.39f, -208.64f), new(-21.69f, -209.07f),
    new(-22.11f, -217.11f), new(-21.51f, -217.41f), new(-21.17f, -217.8f), new(-21.17f, -219.9f), new(-21.23f, -220.41f),
    new(-21.74f, -220.59f), new(-22.15f, -220.99f), new(-22.11f, -221.51f), new(-22.11f, -228.36f), new(-21.59f, -228.66f),
    new(-21.17f, -228.99f), new(-21.17f, -234.07f), new(-21.38f, -234.54f), new(-22.08f, -234.58f), new(-22.09f, -235.1f),
    new(-22.07f, -235.63f), new(-22.11f, -236.14f), new(-22.81f, -236.4f), new(-22.83f, -241.33f), new(-22.37f, -241.61f),
    new(-22.09f, -242.08f), new(-22.1f, -243.11f), new(-21.69f, -243.4f), new(-21.41f, -243.88f), new(-21.4f, -248.09f),
    new(-21.26f, -248.58f), new(-21.38f, -249.11f), new(-21.69f, -249.58f), new(-23.09f, -249.59f), new(-23.1f, -256.41f),
    new(-21.87f, -256.41f), new(-21.44f, -256.68f), new(-21.44f, -259.31f), new(-23.07f, -259.59f), new(-23.09f, -266.42f),
    new(-22.02f, -266.41f), new(-21.51f, -266.56f), new(-21.42f, -267.07f), new(-21.42f, -269.13f), new(-21.75f, -269.58f),
    new(-23.1f, -269.6f), new(-23.1f, -276.42f), new(-22.03f, -276.42f), new(-21.54f, -276.53f), new(-21.42f, -277.02f),
    new(-21.4f, -279.09f), new(-21.66f, -279.54f), new(-22.57f, -279.59f), new(-23.09f, -279.6f), new(-23.1f, -285.98f),
    new(-21.82f, -286.41f), new(-21.42f, -286.71f), new(-21.42f, -288.99f), new(-21.53f, -289.48f), new(-21.91f, -289.82f),
    new(-22.08f, -291.11f), new(-21.71f, -291.46f), new(-21.58f, -292.42f), new(-12.21f, -292.43f), new(-11.8f, -292.02f),
    new(-11.86f, -291.5f), new(-11.83f, -291), new(-9.14f, -290.67f), new(-8.68f, -290.95f), new(-8.67f, -292.28f),
    new(-8.3f, -292.66f), new(-7.07f, -292.66f), new(-7.07f, -292.13f), new(-7.01f, -291.61f), new(-5.11f, -291.43f),
    new(-4.58f, -291.43f), new(-4.08f, -291.51f), new(-3.91f, -291.99f), new(-3.67f, -292.43f), new(-3.58f, -292.94f),
    new(-3.58f, -294.47f)];

    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.TheBirdOfPrey).Concat([PrimaryActor]));
    }
}
