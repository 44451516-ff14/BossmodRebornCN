namespace BossMod.RealmReborn.Dungeon.D06Haukke.D062ManorSteward;

public enum OID : uint
{
    // Major Steward
    Boss = 0x112,

    // Major Jester
    ManorJester = 0x111
}

public enum AID : uint
{
    // Major Steward
    AutoAttack = 870, // Boss->player, no cast
    HellSlash = 341, // Boss->player, no cast, single target
    SoulDrain = 860, // Boss->self, 4.0s cast, range 9 circle aoe

    // Major Jester
    IceSpikes = 859, // Boss->player, 3.0s cast, single target
    Blizzard = 967 // Boss->player, 1.0s cast, single target
}
class SoulDrain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SoulDrain, new AOEShapeCircle(9f));

class D062ManorStewardStates : StateMachineBuilder
{
    public D062ManorStewardStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SoulDrain>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 6, NameID = 424)]
public class D062ManorSteward(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] shape = [new(23.47f, -16.88f), new(23.57f, -14.91f), new(23.59f, 10.51f), new(23.47f, 11.24f), new(23.28f, 11.88f),
    new(23.74f, 12.41f), new(31.40f, 12.50f), new(31.40f, 17.69f), new(31.22f, 18.41f), new(30.70f, 18.47f),
    new(30.40f, 18.89f), new(30.40f, 19.39f), new(25.75f, 19.39f), new(25.29f, 18.50f), new(22.97f, 18.49f),
    new(22.44f, 19.24f), new(22.37f, 18.62f), new(22.50f, 17.99f), new(22.44f, 17.41f), new(22.23f, 16.83f),
    new(21.82f, 16.31f), new(21.28f, 15.95f), new(20.67f, 15.83f), new(20.08f, 15.88f), new(19.50f, 16.09f),
    new(18.97f, 16.51f), new(18.62f, 17.01f), new(18.51f, 17.61f), new(18.54f, 18.24f), new(18.77f, 18.85f),
    new(18.88f, 19.40f), new(13.56f, 19.37f), new(13.45f, 18.69f), new(11.25f, 18.50f), new(10.68f, 18.56f),
    new(10.50f, 19.28f), new(9.94f, 19.40f), new(9.78f, 18.68f), new(9.94f, 17.29f), new(9.88f, 16.72f),
    new(9.39f, 16.56f), new(6.62f, 16.22f), new(6.04f, 16.34f), new(5.41f, 16.23f), new(2.71f, 16.35f),
    new(2.16f, 16.30f), new(2.13f, 16.81f), new(2.15f, 17.45f), new(2.06f, 18.03f), new(2.17f, 18.65f),
    new(2.46f, 19.27f), new(1.72f, 19.38f), new(1.65f, 18.82f), new(1.14f, 18.49f), new(-0.15f, 18.38f),
    new(-0.75f, 18.43f), new(-1.34f, 18.57f), new(-1.51f, 19.40f), new(-3.87f, 19.39f), new(-4.50f, 19.16f),
    new(-4.61f, 18.51f), new(-4.64f, 17.19f), new(-4.51f, 15.85f), new(-4.76f, 15.33f), new(-5.44f, 15.16f),
    new(-10.20f, 15.33f), new(-10.49f, 15.82f), new(-10.42f, 18.67f), new(-10.91f, 18.50f), new(-13.11f, 18.49f),
    new(-13.51f, 18.97f), new(-22.05f, 19.40f), new(-22.36f, 18.83f), new(-22.70f, 18.46f), new(-23.40f, 18.44f),
    new(-23.40f, 10.79f), new(-22.98f, 10.51f), new(-21.89f, 10.16f), new(-21.79f, 9.64f), new(-21.96f, 7.63f),
    new(-22.44f, 7.23f), new(-23.07f, 7.34f), new(-23.39f, 6.78f), new(-23.39f, 5.59f), new(-22.85f, 5.58f),
    new(-22.50f, 4.17f), new(-22.66f, 3.62f), new(-23.14f, 3.27f), new(-23.14f, -3.46f), new(-22.56f, -3.72f),
    new(-22.50f, -5.14f), new(-22.94f, -5.59f), new(-23.40f, -6.75f), new(-23.29f, -7.47f), new(-22.65f, -7.58f),
    new(-22.50f, -9.19f), new(-22.50f, -10.06f), new(-22.74f, -10.50f), new(-23.40f, -10.73f), new(-23.40f, -18.31f),
    new(-22.83f, -18.35f), new(-22.42f, -18.74f), new(-22.24f, -19.40f), new(-13.85f, -19.39f), new(-13.48f, -18.78f),
    new(-13.09f, -18.47f), new(-11.59f, -18.47f), new(-11.23f, -17.90f), new(-11.06f, -17.36f), new(-10.67f, -16.92f),
    new(-10.12f, -16.71f), new(-9.53f, -16.74f), new(-9.37f, -19.25f), new(-8.77f, -19.24f), new(-8.36f, -18.78f),
    new(-6.35f, -18.65f), new(-6.17f, -18.16f), new(-6.28f, -16.82f), new(-5.86f, -16.54f), new(-3.05f, -16.22f),
    new(-2.50f, -16.25f), new(-2.30f, -16.74f), new(-2.08f, -18.65f), new(1.01f, -17.23f), new(1.12f, -15.39f),
    new(1.70f, -15.13f), new(5.91f, -15.34f), new(6.54f, -15.30f), new(6.90f, -15.67f), new(6.80f, -17.54f)];
    // Centroid of the polygon is at: (0.980f, 0.260f)

    public static readonly ArenaBoundsComplex arena = new([new PolygonCustom(shape)]);
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ManorJester => 2,
                (uint)OID.Boss => 1,
                _ => 0,
            };
        }
    }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ManorJester));
    }
}
