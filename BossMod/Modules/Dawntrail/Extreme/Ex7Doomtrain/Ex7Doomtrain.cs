namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

[SkipLocalsInit]
sealed class UnlimitedExpress(BossModule module) : Components.RaidwideCast(module, (uint)AID.UnlimitedExpress);
[SkipLocalsInit]
sealed class ElectrayLong(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Electray1, (uint)AID.Electray4], new AOEShapeRect(25f, 2.5f));
[SkipLocalsInit]
sealed class ElectrayMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray2, new AOEShapeRect(20f, 2.5f));
[SkipLocalsInit]
sealed class ElectrayShort(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electray3, new AOEShapeRect(5f, 2.5f));
[SkipLocalsInit]
sealed class LightningBurst(BossModule module) : Components.BaitAwayIcon(module, 5f, (uint)IconID.LightningBurst, (uint)AID.LightningBurst, 5.6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
StatesType = typeof(Ex7DoomtrainStates),
ConfigType = null, // replace null with typeof(DoomtrainConfig) if applicable
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.Doomtrain,
Contributors = "Malediktus, Xaenalt",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.Extreme,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1077u,
NameID = 14284u,
SortOrder = 1,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Ex7Doomtrain : BossModule
{
    public Ex7Doomtrain(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private Ex7Doomtrain(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Rectangle(new(100f, 100f), 10.5f, 15f)],
        [new Square(new(102.5f, 97.5f), 2.01f), new Square(new(97.5f, 107.5f), 2.01f)], AdjustForHitboxInwards: true);
        return (arena.Center, arena);
    }
}
