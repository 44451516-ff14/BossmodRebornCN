namespace BossMod.RealmReborn.Dungeon.D04Halatali.D043Tangata;

public enum OID : uint
{
    Boss = 0x4645, // R2.08
    Damantus = 0x4646, // R0.2
    Noxius = 0x4647, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    StraightPunch = 40602, // Boss->player, 5.0s cast, single-target

    BurningWard = 40596, // Boss->self, 5.0s cast, single-target
    ScorchedEarth1 = 40597, // Damantus->self, no cast, range 60 circle
    ScorchedEarth2 = 40598, // Noxius->self, no cast, range 60 circle

    PlainPound = 40599, // Boss->self, 5.0s cast, range 10 circle
    Tremblor = 40600, // Helper->self, 8.0s cast, range 10-20 donut
    Earthquake = 40601, // Helper->self, 11.0s cast, range 20-30 donut

    Firewater = 41123 // Boss->location, 2.5s cast, range 3 circle
}

public enum SID : uint
{
    BurningWard = 4175 // Boss->Boss, extra=0x0

}

class StraightPunch(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StraightPunch);
class Firewater(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firewater, 3);

class PlainPound(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PlainPound)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.PlainPound => 0,
                AID.Tremblor => 1,
                AID.Earthquake => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(3));
        }
    }
}

class D043TangataStates : StateMachineBuilder
{
    public D043TangataStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Firewater>()
            .ActivateOnEnter<StraightPunch>()
            .ActivateOnEnter<PlainPound>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7, NameID = 1194)]
public class D043Tangata(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-257.63f, -12.6f), new(-257.26f, -12.03f), new(-254.14f, -9.78f), new(-249.92f, -8.24f), new(-249.31f, -8.36f),
    new(-247.96f, -8.48f), new(-247.3f, -8.27f), new(-245.23f, -6.63f), new(-244.71f, -6.5f), new(-242.06f, -6.54f),
    new(-241.56f, -6.06f), new(-240.49f, -4.36f), new(-240.08f, -4), new(-239.43f, -3.82f), new(-238.84f, -3.73f),
    new(-238.29f, -3.5f), new(-237.65f, -3.31f), new(-236.99f, -3.19f), new(-236.36f, -3.22f), new(-235.93f, -2.71f),
    new(-235.69f, -1.37f), new(-234.48f, 2.36f), new(-233.36f, 5.51f), new(-233.11f, 6.06f), new(-232.58f, 6.38f),
    new(-232.46f, 7.09f), new(-232.45f, 7.74f), new(-232.07f, 8.19f), new(-231.6f, 8.52f), new(-230.92f, 8.56f),
    new(-230.39f, 8.75f), new(-230.03f, 9.27f), new(-228.96f, 22.03f), new(-229.24f, 22.67f), new(-230.6f, 24.97f),
    new(-230.52f, 25.56f), new(-230.36f, 26.22f), new(-230.05f, 26.8f), new(-230.23f, 27.27f), new(-230.56f, 27.84f),
    new(-231.64f, 30.94f), new(-231.94f, 31.55f), new(-232.05f, 32.18f), new(-232.71f, 34.04f), new(-233.04f, 34.65f),
    new(-233.4f, 35.13f), new(-233.52f, 35.76f), new(-234.63f, 37.37f), new(-235.19f, 37.83f), new(-235.82f, 38.18f),
    new(-236.99f, 37.85f), new(-237.52f, 38.11f), new(-237.88f, 38.61f), new(-237.82f, 39.3f), new(-238.01f, 39.98f),
    new(-238.77f, 41.13f), new(-241.57f, 43.41f), new(-242.08f, 43.34f), new(-244.81f, 42.49f), new(-245.39f, 42.57f),
    new(-249.24f, 44.43f), new(-249.53f, 44.97f), new(-249.85f, 46.25f), new(-251.87f, 46.82f), new(-252.55f, 46.93f),
    new(-253.94f, 46.98f), new(-254.6f, 47.09f), new(-255.31f, 47.12f), new(-255.95f, 47.19f), new(-256.52f, 47.51f),
    new(-257.96f, 47.51f), new(-260.72f, 46.93f), new(-261.35f, 46.55f), new(-261.64f, 45.98f), new(-261.97f, 45.43f),
    new(-262.47f, 45.07f), new(-263.79f, 44.61f), new(-265.87f, 44.17f), new(-267.19f, 44.08f), new(-267.8f, 44.34f),
    new(-268.38f, 44.41f), new(-270.77f, 45.97f), new(-271.24f, 46.16f), new(-272.34f, 45.66f), new(-272.74f, 45.36f),
    new(-273.48f, 43.46f), new(-273.99f, 43.37f), new(-274.64f, 43.6f), new(-274.93f, 44.17f), new(-275.27f, 43.72f),
    new(-275.38f, 42.34f), new(-275.98f, 41.37f), new(-276.58f, 41.45f), new(-277.07f, 41.12f), new(-277.2f, 40.51f),
    new(-277.74f, 40.2f), new(-278.27f, 39.83f), new(-278.95f, 39.78f), new(-279.51f, 39.56f), new(-280.13f, 39.42f),
    new(-280.74f, 39.05f), new(-281.07f, 38.66f), new(-281.19f, 38), new(-283.44f, 31), new(-283.75f, 30.55f),
    new(-284.32f, 30.28f), new(-284.83f, 29.83f), new(-284.89f, 28.45f), new(-285.11f, 27.97f), new(-285.74f, 27.84f),
    new(-286.15f, 27.5f), new(-287.6f, 14.1f), new(-287.58f, 13.6f), new(-286.85f, 12.42f), new(-286.41f, 10.92f),
    new(-286.55f, 10.4f), new(-286.24f, 9.97f), new(-284.54f, 6.52f), new(-283.92f, 4.69f), new(-283.82f, 4.03f),
    new(-283.24f, 1.97f), new(-282.75f, 1.06f), new(-282.72f, 0.49f), new(-282.54f, -0.18f), new(-282.2f, -0.56f),
    new(-281.61f, -1), new(-280.98f, -1.35f), new(-279.05f, -2.17f), new(-278.65f, -2.59f), new(-278.72f, -3.24f),
    new(-278.31f, -3.79f), new(-277.77f, -4.2f), new(-277.45f, -4.74f), new(-276.81f, -5.04f), new(-274.76f, -5.48f),
    new(-271.82f, -6.44f), new(-271.23f, -6.93f), new(-270.63f, -8.02f), new(-270.14f, -8.16f), new(-269.44f, -8.26f),
    new(-267.79f, -9.9f), new(-267.2f, -10.25f), new(-265.97f, -10.74f), new(-265.42f, -10.91f), new(-263.31f, -10.91f),
    new(-262.64f, -10.97f), new(-261.51f, -10.56f), new(-260.84f, -10.62f), new(-260.51f, -11.12f), new(-259.92f, -11.35f),
    new(-259.34f, -11.48f), new(-258.44f, -12.46f), new(-257.83f, -12.85f)];
    public static readonly ArenaBoundsComplex arena = new([new PolygonCustom(vertices)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly && x.FindStatus(SID.BurningWard) == null && x.Position.AlmostEqual(Arena.Center, Bounds.Radius)));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        for (var i = 0; i < hints.PotentialTargets.Count; ++i)
        {
            var e = hints.PotentialTargets[i];
            if (e.Actor.FindStatus(SID.BurningWard) != null)
            {
                e.Priority = -1;
                break;
            }
        }
    }
}
