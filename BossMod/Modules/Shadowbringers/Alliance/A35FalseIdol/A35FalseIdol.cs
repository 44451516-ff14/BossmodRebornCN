namespace BossMod.Shadowbringers.Alliance.A35FalseIdol;

sealed class MadeMagic(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.MadeMagic1, (uint)AID.MadeMagic2], new AOEShapeRect(50f, 15f));
sealed class ScreamingScoreEminence(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ScreamingScoreP1, (uint)AID.ScreamingScoreP2, (uint)AID.Eminence])
{
    public override bool KeepOnPhaseChange => true;
}

sealed class ScatteredMagic(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ScatteredMagic, 4f);
sealed class DarkerNote(BossModule module) : Components.BaitAwayCast(module, (uint)AID.DarkerNote, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class HeavyArms1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyArms1, new AOEShapeRect(44f, 50f));
sealed class HeavyArms2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavyArms2, new AOEShapeRect(100f, 6f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948, SortOrder = 6)]
public sealed class A35FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(24.5f))
{
    public static readonly WPos ArenaCenter = new(-700f, -700f);
    public Actor? BossBossP2;
    public static readonly ArenaBoundsSquare ArenaP2 = new(25f);
    public static readonly Square[] BaseSquare = [new(ArenaCenter, 25f)];
    private static readonly Polygon distortion = new(ArenaCenter, 6f, 20);
    private static readonly Rectangle redgirls = new(new(-700f, -723.5f), 25f, 1.5f);
    public static readonly ArenaBoundsCustom DistortionArena = new(BaseSquare, [distortion]);
    public static readonly ArenaBoundsCustom RedGirlsArena = new(BaseSquare, [redgirls]);
    public static readonly ArenaBoundsCustom RedGirlsDistortionArena = new(BaseSquare, [redgirls, distortion]);

    protected override void UpdateModule()
    {
        BossBossP2 ??= GetActor((uint)OID.BossP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(BossBossP2);
    }
}
