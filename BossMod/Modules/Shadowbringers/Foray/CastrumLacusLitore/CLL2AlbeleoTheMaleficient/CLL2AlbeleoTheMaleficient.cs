namespace BossMod.Shadowbringers.Foray.CastrumLacusLitore.CLL2AlbeleoTheMaleficient;

public enum OID : uint
{
    Boss = 0x2F06, // R0.5
    AlbeleosCanisDirus = 0x2F07, // R4.2
    FourthLegionArmoredWeapon = 0x2EDF, // R4.08
    FourthLegionPredator = 0x2EDE, // R2.1
    FourthLegionReaper = 0x2EE1, // R0.5-2.0
    FourthLegionDuplicarius = 0x2EE2, // R0.5
    AlbeleosHrodvitnir = 0x2F0A, // R4.05
    FourthLegionColossus = 0x2EE3, // R2.5
    AlbeleosMonstrosity = 0x2F09 // R4.6
}

public enum AID : uint
{
    AutoAttack1 = 6499, // Boss->player, no cast, single-target
    AutoAttack2 = 21263, // AlbeleosCanisDirus/FourthLegionPredator/FourthLegionDuplicarius/AlbeleosHrodvitnir/FourthLegionColossus/AlbeleosMonstrosity->player, no cast, single-target
    AutoAttack3 = 21250, // FourthLegionArmoredWeapon->player, no cast, single-target

    Ruin = 21437, // Boss->player, 1.0s cast, single-target
    PredatorClaws = 21507, // AlbeleosCanisDirus->player, 5.0s cast, single-target
    DiffractiveLaser = 21503, // FourthLegionArmoredWeapon->location, 3.0s cast, range 5 circle
    MagitekRay = 21502, // FourthLegionPredator->self, 3.6s cast, range 40 width 6 rect
    PhotonStream = 21504, // FourthLegionReaper->player, no cast, single-target
    MagitekCannon = 21505, // FourthLegionReaper->location, 3.6s cast, range 6 circle
    Summon = 21419, // Boss->self, 4.0s cast, single-target
    LunarCry = 21509, // AlbeleosHrodvitnir->self, 5.0s cast, range 40 circle
    GrandSword = 21506, // FourthLegionColossus->self, 6.0s cast, range 27 120-degree cone
    BalefulGaze = 21508, // AlbeleosMonstrosity->self, 4.0s cast, range 35 circle, gaze
    AbyssalCry = 21510 // AlbeleosHrodvitnir->self, 6.0s cast, range 30 circle
}

sealed class DiffractiveLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffractiveLaser, 5f);
sealed class MagitekCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekCannon, 6f);
sealed class MagitekRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, new AOEShapeRect(40f, 3f));
sealed class GrandSword(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandSword, new AOEShapeCone(27f, 60f.Degrees()));
sealed class BalefulGaze(BossModule module) : Components.CastGaze(module, (uint)AID.BalefulGaze, range: 35f);
sealed class AbyssalCry(BossModule module) : Components.CastInterruptHint(module, (uint)AID.AbyssalCry);

sealed class CLL2AlbeleoTheMaleficentStates : StateMachineBuilder
{
    public CLL2AlbeleoTheMaleficentStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<BalefulGaze>()
            .ActivateOnEnter<AbyssalCry>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || module.PrimaryActor.HPMP.CurHP == 1;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CastrumLacusLitore, GroupID = 735, NameID = 9433)]
public sealed class CLL2AlbeleoTheMaleficent(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(87.45f, -409.99f), new(87.45f, -408.35f), new(87.75f, -407.95f),
    new(91.13f, -407.93f), new(91.59f, -408.28f), new(91.59f, -409.59f), new(98.15f, -409.59f), new(98.33f, -409.11f),
    new(98.32f, -408.45f), new(98.60f, -408.00f), new(101.68f, -407.91f), new(102.26f, -407.98f), new(102.58f, -408.43f),
    new(102.90f, -409.01f), new(107.13f, -409.11f), new(107.70f, -408.71f), new(107.95f, -408.16f), new(108.44f, -407.93f),
    new(109.30f, -407.93f), new(109.60f, -406.28f), new(109.58f, -392.52f), new(108.96f, -392.20f), new(108.31f, -392.05f),
    new(107.94f, -391.70f), new(107.94f, -388.74f), new(108.72f, -387.93f), new(93.71f, -387.78f),
    new(93.03f, -387.92f), new(91.74f, -387.98f), new(91.31f, -387.58f), new(91.26f, -370.52f), new(91.66f, -370.21f), new(108.71f, -370.21f),
    new(108.99f, -368.42f), new(108.52f, -368.19f), new(108.00f, -367.86f), new(107.91f, -365.34f), new(98.77f, -365.05f),
    new(97.47f, -365.30f), new(96.85f, -365.22f), new(96.25f, -365.02f), new(95.97f, -348.06f), new(96.34f, -347.67f),
    new(108.73f, -347.67f), new(108.99f, -344.10f), new(108.60f, -343.55f), new(107.92f, -343.29f), new(107.31f, -343.19f),
    new(106.71f, -342.97f), new(104.76f, -342.92f), new(104.22f, -343.31f), new(100.80f, -343.71f), new(99.34f, -343.71f),
    new(95.98f, -343.33f), new(95.48f, -343.15f), new(93.99f, -342.93f), new(91.81f, -344.16f), new(88.83f, -344.27f),
    new(88.16f, -344.19f), new(87.85f, -343.75f), new(72.38f, -343.39f), new(71.76f, -344.41f), new(71.76f, -352.40f),
    new(71.58f, -352.92f), new(70.99f, -352.98f), new(55.06f, -352.97f), new(54.51f, -352.85f), new(54.39f, -352.31f),
    new(54.40f, -343.23f), new(53.89f, -342.95f), new(53.18f, -343.04f), new(52.53f, -343.27f), new(51.66f, -343.30f),
    new(51.18f, -343.78f), new(51.03f, -351.92f), new(51.40f, -353.23f), new(51.40f, -354.65f), new(51.28f, -355.24f),
    new(51.06f, -355.86f), new(51.01f, -356.74f), new(51.20f, -357.49f), new(67.90f, -357.49f), new(68.32f, -357.77f),
    new(68.37f, -374.14f), new(68.32f, -374.86f), new(67.93f, -375.25f), new(51.34f, -375.25f), new(51.03f, -375.86f),
    new(51.38f, -377.14f), new(51.41f, -378.54f), new(51.32f, -379.15f), new(51.06f, -379.72f), new(50.94f, -387.66f),
    new(51.48f, -388.05f), new(51.98f, -388.31f), new(51.87f, -391.99f), new(50.65f, -392.07f), new(50.40f, -394.83f),
    new(50.41f, -407.95f), new(51.61f, -407.95f),
    new(52.06f, -408.22f), new(52.27f, -408.83f), new(56.86f, -409.08f), new(57.37f, -408.56f), new(57.73f, -407.99f),
    new(60.10f, -407.83f), new(60.79f, -407.88f), new(61.40f, -407.99f), new(61.56f, -409.58f), new(68.25f, -409.58f),
    new(68.54f, -408.08f), new(69.94f, -407.95f), new(72.46f, -408.10f),
    new(72.56f, -409.72f), new(75.47f, -409.98f), new(75.83f, -409.39f), new(76.06f, -408.87f), new(83.03f, -408.65f),
    new(83.72f, -408.76f), new(84.09f, -409.14f), new(84.31f, -409.75f), new(87.45f, -409.99f)];
    private static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);
    private static readonly uint[] adds = [(uint)OID.AlbeleosCanisDirus, (uint)OID.AlbeleosHrodvitnir, (uint)OID.AlbeleosMonstrosity, (uint)OID.FourthLegionColossus,
    (uint)OID.FourthLegionDuplicarius, (uint)OID.FourthLegionReaper, (uint)OID.FourthLegionArmoredWeapon];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Boss => 0,
                _ => 1
            };
        }
    }
}
