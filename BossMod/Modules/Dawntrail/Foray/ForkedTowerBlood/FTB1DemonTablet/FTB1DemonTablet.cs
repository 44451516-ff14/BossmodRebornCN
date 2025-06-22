namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class DemonicDarkII(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DemonicDarkIIVisual, (uint)AID.DemonicDarkII, 0.8f);
sealed class OccultChisel(BossModule module) : Components.BaitAwayCast(module, (uint)AID.OccultChisel, 5f, tankbuster: true);
sealed class RotationBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rotation1, new AOEShapeCone(37f, 45f.Degrees()));
sealed class RotationSmall(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Rotation2, (uint)AID.Rotation3], new AOEShapeRect(33f, 1.5f));
sealed class Summon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Summon, new AOEShapeRect(36f, 15f));
sealed class DarkDefenses(BossModule module) : Components.Dispel(module, (uint)SID.DarkDefenses);
sealed class SummonedDemons(BossModule module) : Components.AddsMulti(module, [(uint)OID.SummonedArchDemon, (uint)OID.SummonedDemon], 1);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.TheForkedTowerBlood, GroupID = 1018, NameID = 13760, PlanLevel = 100, SortOrder = 2)]
public sealed class FTB1DemonTablet(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(700f, 379f);
    private static readonly Rectangle[] defaultRect = [new Rectangle(ArenaCenter, 15f, 33f)];
    public static readonly ArenaBoundsComplex DefaultArena = new(defaultRect, [new Rectangle(ArenaCenter, 15f, 3.5f)]);
    public static readonly ArenaBoundsComplex RotationArena = new(defaultRect, [new Rectangle(ArenaCenter, 3.5f, 15.5f)]);
    public static readonly ArenaBoundsRect CompleteArena = new(15f, 33f);
}
