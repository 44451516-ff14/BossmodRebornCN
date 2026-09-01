namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(FTME4IndexStates),
    ConfigType = null, // replace null with typeof(FTME1TwoHeadedAevisConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
    IconIDType = typeof(IconID), // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Index,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.TheForkedTowerMagic,
    GroupID = 1114u,
    NameID = 14717u,
    SortOrder = 4,
    PlanLevel = 100)]
[SkipLocalsInit]
public sealed class FTME4Index(WorldState ws, Actor primary) : BossModule(ws, primary, InitialCenter, InitialBounds)
{
    // pulled from normal mode; check replay if arena any different in EX
    // points using material id 0x00007004
    private static readonly WPos[] _arenaInitialPos = [
        new(7.50198f, -615.00610f),new(7.49990f, -600.00012f),new(-7.50010f, -600.00012f),new(-7.50079f, -600.00067f),
        new(-7.50276f, -615.00580f),new(-15.00425f, -628.00012f),new(-27.99880f, -635.50494f),new(-20.49879f, -648.49530f),
        new(-7.50275f, -640.99445f),new(7.50200f, -640.99408f),new(20.49863f, -648.49530f),new(27.99863f, -635.50494f),
        new(15.00408f, -628.00012f),new(15.00408f, -628.00012f)];

    private static readonly WPos[] _arenaFullPos = [
        new(27.99862f, -620.49530f),new(20.49862f, -607.50494f),new(7.50198f, -615.00610f),new(7.49990f, -600.00012f),
        new(-7.50010f, -600.00012f),new(-7.50079f, -600.00067f),new(-7.50276f, -615.00580f),new(-20.49881f, -607.50494f),
        new(-27.99881f, -620.49530f),new(-15.00425f, -628.00012f),new(-27.99880f, -635.50494f),new(-20.49879f, -648.49530f),
        new(-7.50275f, -640.99445f),new(-7.50076f, -656.00049f),new(0.73911f, -656.00031f),new(7.49962f, -656.00043f),
        new(7.49992f, -656.00012f),new(7.50200f, -640.99408f),new(20.49863f, -648.49530f),new(27.99863f, -635.50494f),
        new(15.00408f, -628.00012f),new(15.00408f, -628.00012f)];

    //private static readonly WPos[] _innerHexPos = [new(-2.88752f, -623.00104f), new(0.62856f, -623.00043f), new(2.88607f, -623.00067f), new(5.77356f, -628.00012f), new(2.88633f, -633.00024f), new(-2.88692f, -633.00024f), new(-5.77374f, -628.00012f)];
    private static readonly WPos[] _innerHexPos = [new(-3f, -623f), new(3f, -623f), new(6f, -628f), new(3f, -633f), new(-3f, -633f), new(-6f, -628f)];

    private static readonly PolygonCustom[] _arenaInitial = [new(_arenaInitialPos)];
    private static readonly PolygonCustom[] _arenaFull = [new(_arenaFullPos)];
    private static readonly PolygonCustom[] _innerHex = [new(_innerHexPos)];

    public static WPos InitialCenter = new(0f, -624.25f);
    public static readonly ArenaBoundsCustom InitialBounds = new(_arenaInitial, _innerHex, Offset: -1f);

    public static WPos OmniElementsCenter = new(0f, -628f);
    public static readonly ArenaBoundsCustom OmniElementsBounds = new(_arenaFull, _innerHex, Offset: -1f);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 28f);
}
