namespace BossMod.Shadowbringers.Foray.Skirmish.PyromancerSupreme;

public enum OID : uint
{
    Boss = 0x2EF7, // R0.5
    Helper = 0x2EA1,
    FourthLegionVanguard = 0x2E36, // R2.1
    ForthLegionAvenger = 0x2E3B, // R2.2
    ForthLegionGunship = 0x2E3F, // R3.6
    BozjanBiast = 0x2E44, // R2.7
    Smok = 0x2E4C, // R4.6
}

public enum AID : uint
{
    LevitationVisual1 = 21017, // Boss->self, no cast, single-target
    LevitationVisual2 = 21015, // Boss->self, no cast, single-target
    LevitationVisual3 = 21016, // Boss->self, no cast, single-target
    Fire = 20938, // Boss->player, no cast, single-target

    PyreticEruption = 20929, // Helper->location, 3.0s cast, range 8 circle
    TridirectionalFlameVisual = 20933, // Boss->self, 3.0s cast, single-target
    TridirectionalFlame = 20927, // Helper->self, 3.0s cast, range 60 width 8 rect
    Pyroscatter = 20930, // Helper->location, 3.0s cast, range 8 circle
    PyroburstVisual = 20937, // Boss->self, 4.0s cast, single-target
    Pyroburst = 20931, // Helper->self, 4.0s cast, range 10 circle
    GrandCrossflameVisual = 20934, // Boss->self, 4.5s cast, single-target
    GrandCrossflame = 20928, // Helper->self, 4.5s cast, range 40 width 18 cross
    Stun = 21020, // Helper->player, no cast, single-target
    FirestarterVisual = 20932, // Boss->player, 8.0s cast, single-target
    Firestarter = 20926, // Helper->location, 8.0s cast, range 6-40 donut
    ThermalShock = 21022, // Helper->self, 7.0s cast, range 30 circle, proximity AOE

    AutoAttack = 21263, // ForthLegionGunship->player, no cast, single-target
    GarleanFire = 21246, // ForthLegionGunship->location, 3.0s cast, range 5 circle
    SnowsOfBozja = 21274, // Smok->location, 2.5s cast, range 5 circle
}

sealed class PyreticEruptionPyroscatter(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PyreticEruption, (uint)AID.Pyroscatter], 8f);
sealed class TridirectionalFlame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TridirectionalFlame, new AOEShapeRect(60f, 4f));
sealed class Pyroburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pyroburst, 10f);
sealed class GrandCrossflame(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossflame, new AOEShapeCross(40f, 9f));
sealed class Firestarter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Firestarter, new AOEShapeDonut(6f, 40f));
sealed class GarleanFireSnowsOfBozja(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GarleanFire, (uint)AID.SnowsOfBozja], 5f);
sealed class ThermalShock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermalShock, 20f);

sealed class PyromancerSupremeStates : StateMachineBuilder
{
    public PyromancerSupremeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PyreticEruptionPyroscatter>()
            .ActivateOnEnter<TridirectionalFlame>()
            .ActivateOnEnter<Pyroburst>()
            .ActivateOnEnter<GrandCrossflame>()
            .ActivateOnEnter<Firestarter>()
            .ActivateOnEnter<GarleanFireSnowsOfBozja>()
            .DeactivateOnEnter<ThermalShock>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || !module.PrimaryActor.IsTargetable;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.ForayFATE, GroupID = 735, NameID = 1616)]
public sealed class PyromancerSupreme : SimpleBossModule
{
    public PyromancerSupreme(WorldState ws, Actor primary) : base(ws, primary)
    {
        ActivateComponent<ThermalShock>();
    }

    private static readonly uint[] adds = [(uint)OID.FourthLegionVanguard, (uint)OID.ForthLegionAvenger, (uint)OID.ForthLegionGunship, (uint)OID.BozjanBiast,
    (uint)OID.Smok];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(adds));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 1,
                _ when e.Actor.InCombat => 0,
                _ => AIHints.Enemy.PriorityUndesirable
            };
        }
    }
}
