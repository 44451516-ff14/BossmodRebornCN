namespace BossMod.Dawntrail.Quest.JobQuests.Pictomancer.SomewhereOnlySheKnows.TheWingedSteed;

public enum OID : uint
{
    Boss = 0x4293, // R1.3
    SonOfTheKingdom1 = 0x4295, // R0.75
    SonOfTheKingdom2 = 0x4294, // R0.75
}

public enum AID : uint
{
    AutoAttack1 = 6499, // Boss->player, no cast, single-target
    AutoAttack2 = 6498, // SonOfTheKingdom1->player, no cast, single-target
    AutoAttack3 = 6497, // SonOfTheKingdom2->player, no cast, single-target

    BurningBright = 37517, // Boss->self, 3.0s cast, range 47 width 6 rect
    Nicker = 37518 // Boss->self, 4.0s cast, range 12 circle
}

class BurningBright(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningBright), new AOEShapeRect(47, 3));
class Nicker(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Nicker), new AOEShapeCircle(12));

class TheWingedSteedStates : StateMachineBuilder
{
    public TheWingedSteedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BurningBright>()
            .ActivateOnEnter<Nicker>()
            .Raw.Update = () => module.Enemies(OID.SonOfTheKingdom1).Concat([module.PrimaryActor]).Concat(module.Enemies(OID.SonOfTheKingdom2)).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70395, NameID = 13033)]
public class TheWingedSteed(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(33.04f, -231.66f), new(33.08f, -231.15f), new(33.25f, -230.68f), new(34.25f, -230.59f), new(34.72f, -230.42f),
    new(34.92f, -229.82f), new(35.25f, -229.42f), new(37.25f, -229.41f), new(37.76f, -229.42f), new(38.09f, -230.23f),
    new(38.5f, -230.64f), new(46.47f, -230.6f), new(46.94f, -230.21f), new(46.93f, -229.7f), new(48.55f, -229.27f),
    new(49.56f, -229.4f), new(50.02f, -229.6f), new(50.09f, -230.24f), new(58.54f, -230.59f), new(58.87f, -230.18f),
    new(58.94f, -229.66f), new(60.51f, -229.4f), new(61, -229.25f), new(61.5f, -229.36f), new(61.98f, -229.54f),
    new(62.09f, -230.22f), new(62.46f, -230.59f), new(70.54f, -230.59f), new(70.76f, -230.14f), new(70.91f, -229.64f),
    new(71.59f, -229.42f), new(73.47f, -229.4f), new(73.97f, -229.52f), new(74.09f, -230.23f), new(74.45f, -230.59f),
    new(82.61f, -230.59f), new(82.96f, -230.18f), new(82.95f, -229.65f), new(83.7f, -229.41f), new(84.09f, -229.02f),
    new(84.09f, -220.95f), new(83.71f, -220.59f), new(83.2f, -220.57f), new(82.91f, -217.82f), new(83.22f, -217.42f),
    new(83.76f, -217.35f), new(84.09f, -212.72f), new(84.09f, -210.29f), new(84.08f, -209.47f), new(83.22f, -208.19f),
    new(83.14f, -207.7f), new(82.44f, -207.53f), new(82.14f, -207.95f), new(81.65f, -207.86f), new(81.24f, -207.56f),
    new(76.11f, -207.29f), new(74.34f, -207.51f), new(74.08f, -207.99f), new(73.97f, -208.48f), new(73.45f, -208.59f),
    new(71.38f, -208.6f), new(70.94f, -208.34f), new(70.88f, -207.74f), new(62.94f, -207.4f), new(62.4f, -207.48f),
    new(62.09f, -208.16f), new(59.42f, -208.58f), new(58.95f, -208.37f), new(58.88f, -207.75f), new(50.48f, -207.4f),
    new(50.11f, -207.77f), new(50.07f, -208.31f), new(48.34f, -208.59f), new(47.86f, -208.74f), new(47.34f, -208.65f),
    new(46.93f, -208.33f), new(46.91f, -207.77f), new(46.5f, -207.36f), new(38.44f, -207.41f), new(38.16f, -207.84f),
    new(38.08f, -208.39f), new(37.15f, -208.6f), new(35.52f, -208.6f), new(35.03f, -208.47f), new(34.91f, -207.98f),
    new(34.62f, -207.57f), new(33.49f, -207.41f), new(33.09f, -206.65f), new(27.9f, -206.42f), new(27.95f, -206.95f),
    new(27.66f, -207.38f), new(27.11f, -207.45f), new(26.4f, -207.65f), new(26.03f, -208.61f), new(25.42f, -208.85f),
    new(25.42f, -210.45f), new(25.71f, -210.85f), new(26.21f, -210.94f), new(26.48f, -212.73f), new(26.67f, -213.21f),
    new(26.61f, -213.71f), new(26.23f, -214.06f), new(25.71f, -214.1f), new(25.4f, -222.79f), new(25.8f, -223.09f),
    new(26.31f, -223.23f), new(26.48f, -225.91f), new(26.22f, -226.34f), new(25.68f, -226.43f), new(25.43f, -228.82f),
    new(25.8f, -229.2f), new(26.32f, -229.19f), new(26.58f, -230.24f), new(26.95f, -230.62f), new(27.47f, -230.59f),
    new(27.9f, -230.94f), new(27.9f, -232.19f)];
    private static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.SonOfTheKingdom1).Concat([PrimaryActor]).Concat(Enemies(OID.SonOfTheKingdom2)));
    }
}
