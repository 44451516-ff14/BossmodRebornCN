namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Batraal = 0x4A8D, // R4.6
    CorruptedCrystal = 0x4A8E // R1.0
}

public enum AID : uint
{
    AutoAttack = 870, // Batraal->player, no cast, single-target

    Hellssend = 45580, // Batraal->self, 3.0s cast, single-target, damage buff to self
    GrimHalo = 45579, // Batraal->self, 5.0s cast, range 12 circle
    CorruptedCrystals = 45581, // Batraal->CorruptedCrystal, no cast, single-target
    Desolation = 45582, // Batraal->self, 3.0s cast, range 60 width 6 rect
    AetherialSurge = 45583, // CorruptedCrystal->self, 3.0s cast, range 6 circle
    GrimCleaver = 45577, // Batraal->player, no cast, single-target
    GrimFate = 45578 // Batraal->player, no cast, single-target
}

public enum SID : uint
{
    Invincibility = 4410 // none->Batraal, extra=0x0
}

[SkipLocalsInit]
sealed class Desolation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Desolation, new AOEShapeRect(60f, 3f));

[SkipLocalsInit]
sealed class AetherialSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialSurge, 6f);

[SkipLocalsInit]
sealed class GrimHalo(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrimHalo, 12f);

[SkipLocalsInit]
sealed class D113BatraalStates : StateMachineBuilder
{
    public D113BatraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrimHalo>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<AetherialSurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(D113BatraalStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = null,
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.Batraal,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.RealmReborn,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 13u,
NameID = 1396u,
SortOrder = 3,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D113Batraal : BossModule
{
    public D113Batraal(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D113Batraal(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(76.51f, -204.28f), new(77.81f, -203.07f), new(79.01f, -202.74f), new(79.7f, -202.77f), new(80.92f, -202.12f),
        new(81.48f, -201.9f), new(82.84f, -202.3f), new(83.35f, -202.16f), new(83.72f, -201.82f), new(84.54f, -200.58f),
        new(84.77f, -200.13f), new(85.01f, -199.46f), new(85.13f, -198.75f), new(85.15f, -197.39f), new(86.32f, -194.36f),
        new(86.4f, -193.69f), new(86.23f, -190.42f), new(86.04f, -189.78f), new(84.87f, -189.25f), new(84.32f, -188.94f),
        new(83.81f, -188.54f), new(83.35f, -188.09f), new(82.69f, -187.04f), new(82.94f, -185.79f), new(83.22f, -185.28f),
        new(85.25f, -182.56f), new(86.29f, -181.85f), new(86.73f, -181.41f), new(87.87f, -180.78f), new(89.09f, -180.37f),
        new(91.18f, -180.49f), new(93.11f, -181.23f), new(93.76f, -180.99f), new(94.35f, -180.84f), new(94.8f, -181.24f),
        new(95.35f, -181.6f), new(96.03f, -181.53f), new(97.73f, -179.54f), new(98.69f, -177.83f), new(98.72f, -177.12f),
        new(98.24f, -175.97f), new(98.61f, -174.73f), new(99.54f, -173.87f), new(100.36f, -172.89f), new(100.9f, -172.64f),
        new(101.45f, -172.28f), new(101.91f, -171.84f), new(102.67f, -169.26f), new(102.5f, -168.57f), new(101.83f, -167.28f),
        new(101.77f, -166.68f), new(102.17f, -164.86f), new(103.45f, -163.43f), new(103.79f, -162.85f), new(104.19f, -161.61f),
        new(103.52f, -160.57f), new(103.76f, -159.97f), new(104.55f, -158.37f), new(104.22f, -157.75f), new(103.74f, -157.25f),
        new(103.52f, -156.67f), new(103.37f, -156.08f), new(103.64f, -155.66f), new(104.24f, -155.33f), new(104.43f, -154.68f),
        new(104.22f, -153.3f), new(104.01f, -152.61f), new(103.46f, -152.26f), new(103.05f, -151.77f), new(102.3f, -150.61f),
        new(101.75f, -150.26f), new(101.06f, -150.23f), new(98.5f, -150.96f), new(97.26f, -150.81f), new(96.59f, -150.88f),
        new(96.01f, -151.22f), new(95.49f, -151.71f), new(95f, -152.07f), new(92f, -153.31f), new(89.32f, -153.1f),
        new(88.72f, -153.4f), new(88.19f, -153.76f), new(87.19f, -154.61f), new(86.81f, -155.18f), new(86.32f, -156.97f),
        new(85.99f, -157.42f), new(85.06f, -158.31f), new(84.47f, -158.51f), new(83.16f, -158.72f), new(82.53f, -158.61f),
        new(81.34f, -158.22f), new(80.72f, -158.11f), new(80.36f, -157.68f), new(79.77f, -156.46f), new(79.58f, -155.9f),
        new(79.9f, -154.53f), new(80.1f, -153.92f), new(80.38f, -153.32f), new(79.84f, -150.11f), new(79.54f, -149.55f),
        new(78.42f, -147.91f), new(77.92f, -147.47f), new(76.89f, -146.74f), new(76.31f, -146.42f), new(73.75f, -146.46f),
        new(73.19f, -146.79f), new(71.9f, -146.96f), new(71.25f, -147.15f), new(67.85f, -149.31f), new(67.32f, -149.23f),
        new(65.45f, -148.54f), new(64.04f, -148.58f), new(63.42f, -148.74f), new(62.22f, -149.25f), new(60.93f, -149.63f),
        new(60.32f, -149.67f), new(59.65f, -149.53f), new(59.02f, -149.71f), new(55.96f, -154.01f), new(55.71f, -154.65f),
        new(56.93f, -157.7f), new(57.43f, -158.19f), new(58f, -158.55f), new(58.54f, -158.75f),
        new(58.69f, -159.08f), new(59.82f, -162.25f), new(59.12f, -164.04f), new(59.21f, -165.34f), new(59.9f, -166.49f),
        new(60.35f, -167.03f), new(60.79f, -167.47f), new(61.01f, -168.04f), new(61.17f, -168.69f), new(61.54f, -169.29f),
        new(62.14f, -169.62f), new(63.09f, -171.16f), new(63.79f, -171.34f), new(65.09f, -171.45f), new(65.15f, -172.72f),
        new(65.78f, -172.78f), new(66.4f, -172.98f), new(66.75f, -173.55f), new(66.5f, -174.19f), new(67.11f, -174.2f),
        new(69.01f, -173.94f), new(69.66f, -174f), new(71f, -174.27f), new(71.57f, -174.44f), new(73.19f, -176.39f),
        new(73.47f, -176.91f), new(73.54f, -177.56f), new(73.7f, -178.2f), new(73.69f, -178.84f), new(73.54f, -179.44f),
        new(72.84f, -181.24f), new(71.02f, -183.2f), new(70.26f, -184.22f), new(69.69f, -184.62f), new(69.15f, -184.88f),
        new(68.49f, -184.93f), new(67.94f, -185.36f), new(64.75f, -188.22f), new(64.21f, -188.57f), new(61.64f, -188.75f),
        new(59.99f, -190.08f), new(59.44f, -190.2f), new(58.18f, -190.29f), new(57.56f, -190.56f), new(57.3f, -191.24f),
        new(57.22f, -191.95f), new(56.9f, -192.5f), new(56.47f, -193.06f), new(56.14f, -193.64f), new(56.03f, -194.3f),
        new(56.04f, -194.97f), new(57.16f, -196.63f), new(57.84f, -198.51f), new(57.71f, -199.07f), new(57.44f, -199.65f),
        new(57.53f, -200.32f), new(57.97f, -200.86f), new(59.23f, -201.5f), new(59.89f, -201.69f), new(61.01f, -201.14f),
        new(61.57f, -201.25f), new(62.15f, -201.61f), new(62.44f, -202.1f), new(62.59f, -202.71f), new(62.84f, -203.29f),
        new(64.58f, -203.85f), new(65.7f, -203.07f), new(66.2f, -203.06f), new(68.3f, -203.59f), new(68.88f, -203.86f),
        new(69.39f, -203.97f), new(70.58f, -203.3f), new(71.16f, -203.39f), new(71.86f, -203.59f), new(72.32f, -203.95f),
        new(72.63f, -204.36f), new(73.12f, -204.16f), new(74.23f, -203.58f), new(75.5f, -203.96f), new(76.07f, -204.24f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CorruptedCrystal => 1,
                (uint)OID.Batraal when PrimaryActor.FindStatus((uint)SID.Invincibility) != null => AIHints.Enemy.PriorityInvincible,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.FindStatus((uint)SID.Invincibility) == null)
        {
            Arena.Actor(PrimaryActor);
        }
        Arena.Actors(Enemies((uint)OID.CorruptedCrystal));
    }
}
