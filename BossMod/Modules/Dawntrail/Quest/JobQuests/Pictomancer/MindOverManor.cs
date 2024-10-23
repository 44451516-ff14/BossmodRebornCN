namespace BossMod.Dawntrail.Quest.JobQuests.Pictomancer.MindOverManor;

public enum OID : uint
{
    Boss = 0x4292, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    GyratingGlare = 37511, // Boss->self, 7.0s cast, range 40 circle
    RubbleRouse = 37512, // Boss->location, 4.0s cast, range 10 circle
    RockAndRefrainVisual = 37514, // Boss->location, no cast, range 10 circle
    RockAndRefrain1 = 37513, // Boss->location, 12.0s cast, range 10 circle
    RockAndRefrain2 = 37515, // Helper->location, 12.8s cast, range 10 circle
    JitteringGlare = 37516, // Boss->self, 4.0s cast, range 40 30-degree cone
}

class JitteringGlare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.JitteringGlare), new AOEShapeCone(40, 15.Degrees()));
class GyratingGlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.GyratingGlare));

abstract class Rubble(BossModule module, AID aid) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(aid), 10);
class RubbleRouse(BossModule module) : Rubble(module, AID.RubbleRouse);
class RockAndRefrain1(BossModule module) : Rubble(module, AID.RockAndRefrain1);
class RockAndRefrain2(BossModule module) : Rubble(module, AID.RockAndRefrain2);

class MindOverManorStates : StateMachineBuilder
{
    public MindOverManorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<JitteringGlare>()
            .ActivateOnEnter<GyratingGlare>()
            .ActivateOnEnter<RubbleRouse>()
            .ActivateOnEnter<RockAndRefrain1>()
            .ActivateOnEnter<RockAndRefrain2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70391, NameID = 13032)]
public class MindOverManor(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(20.21f, -19.11f), new(20.63f, -18.79f), new(21.03f, -18.42f), new(21.19f, -17.93f), new(21.11f, -17.43f),
    new(21.41f, -16.46f), new(21.63f, -15.98f), new(21.9f, -15.54f), new(22.23f, -15.16f), new(22.59f, -13.61f),
    new(22.76f, -13.13f), new(23.24f, -12.84f), new(23.49f, -11.32f), new(23.58f, -7.69f), new(23.59f, 4.78f),
    new(23.57f, 6.47f), new(23.18f, 6.8f), new(22.92f, 7.28f), new(22.83f, 10.93f), new(23, 11.97f),
    new(23.37f, 12.36f), new(23.88f, 12.49f), new(31.12f, 12.49f), new(31.4f, 17.74f), new(31.15f, 18.21f),
    new(30.69f, 18.47f), new(30.36f, 18.87f), new(29.92f, 19.14f), new(25.27f, 18.52f), new(22.93f, 18.49f),
    new(22.52f, 18.16f), new(22.44f, 17.65f), new(22.43f, 17.14f), new(22.46f, 16.6f), new(22.09f, 16.2f),
    new(21.72f, 15.85f), new(21.2f, 15.77f), new(20.69f, 15.86f), new(20.18f, 16), new(18.16f, 16.06f),
    new(17.66f, 16.26f), new(17.22f, 16.63f), new(8.99f, 16.44f), new(8.45f, 16.47f), new(6.92f, 16.38f),
    new(6.42f, 16.25f), new(5.88f, 16.16f), new(2.34f, 15.91f), new(1.97f, 16.26f), new(1.49f, 16.44f),
    new(0.94f, 16.34f), new(0.63f, 15.88f), new(0.27f, 15.55f), new(-0.74f, 15.35f), new(-1.79f, 15.26f),
    new(-2.29f, 15.24f), new(-2.68f, 15.6f), new(-3.2f, 15.81f), new(-5.29f, 16.05f), new(-5.81f, 16.04f),
    new(-6.33f, 15.96f), new(-6.84f, 15.85f), new(-8.86f, 15.6f), new(-9.91f, 15.57f), new(-10.37f, 15.79f),
    new(-10.47f, 16.8f), new(-10.45f, 17.32f), new(-10.92f, 17.51f), new(-12.51f, 17.67f), new(-18.75f, 19.3f),
    new(-19.29f, 19.38f), new(-19.85f, 19.4f), new(-21.59f, 19.4f), new(-22.01f, 19.09f), new(-22.37f, 18.7f),
    new(-23.28f, 18.46f), new(-23.37f, 17.94f), new(-23.39f, 17.43f), new(-23.4f, 14.88f), new(-23.4f, 13.17f),
    new(-23.27f, 12.68f), new(-22.84f, 12.41f), new(-22.54f, 11.96f), new(-22.12f, 11.64f), new(-21.81f, 11.22f),
    new(-21.77f, 10.69f), new(-21.78f, 9.6f), new(-21.82f, 9.09f), new(-21.74f, 8.55f), new(-21.78f, 8.04f),
    new(-21.93f, 7.56f), new(-22.31f, 7.22f), new(-22.81f, 7.26f), new(-23.4f, 7.18f), new(-23.4f, 5.89f),
    new(-22.94f, 5.66f), new(-22.53f, 5.37f), new(-22.49f, 3.92f), new(-23.4f, 3.52f), new(-23.06f, 3.15f),
    new(-23.12f, 2.61f), new(-23.35f, 2.15f), new(-23.64f, 1.73f), new(-23.76f, -1.66f), new(-23.4f, -2.07f),
    new(-23.37f, -2.57f), new(-23.37f, -3.08f), new(-23.03f, -3.47f), new(-22.57f, -3.71f), new(-22.5f, -5.11f),
    new(-22.73f, -5.56f), new(-23.4f, -5.61f), new(-23.4f, -6.56f), new(-23.38f, -7.14f), new(-22.93f, -7.41f),
    new(-22.53f, -7.72f), new(-22.49f, -8.23f), new(-22.49f, -9.83f), new(-22.55f, -10.36f), new(-21.9f, -12.31f),
    new(-21.69f, -12.77f), new(-20.89f, -14.14f), new(-20.67f, -14.6f), new(-20.66f, -15.12f), new(-20.72f, -15.63f),
    new(-20.68f, -16.16f), new(-20.53f, -16.64f), new(-20.06f, -16.84f), new(-9.27f, -16.64f), new(-8.83f, -16.91f),
    new(-8.3f, -16.97f), new(-6.76f, -16.97f), new(-3.62f, -16.6f), new(-2.64f, -16.9f), new(-2.13f, -16.87f),
    new(-2.21f, -16.35f), new(-1.96f, -15.87f), new(-1.64f, -15.46f), new(0.93f, -15.06f), new(6.21f, -15.03f),
    new(6.7f, -15.12f), new(7.14f, -15.4f), new(9.4f, -17.6f), new(9.88f, -17.77f), new(10.93f, -17.75f),
    new(11.73f, -18.5f), new(12.76f, -18.5f), new(13.29f, -18.52f), new(13.82f, -18.49f), new(14.32f, -18.53f),
    new(15.13f, -19.18f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);
}
