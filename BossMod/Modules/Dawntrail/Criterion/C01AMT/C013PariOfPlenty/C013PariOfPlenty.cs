namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

// TODO
//  - Fix hints for witch hunt - currently says "Stack" "Bait" etc at the same time - most likely just disable for now until priority is added
//  - Fix timeline
//  - Clean up enums + add missing ones
//  - FireflightStackSpread - find out who gets the stack
//  - WheelofFableFlight - find out who gets the stack + clean up
//  - Improve angle working out for FireFlightFourLongNight?

class HeatBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.HeatBurst);
class FireOfVictory(BossModule module) : Components.BaitAwayCast(module, (uint)AID.FireOfVictory, 4f, true, true, true, AIHints.PredictedDamageType.Tankbuster);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(PariOfPlentyStates),
    ConfigType = null, // replace null with typeof(PariOfPlentyConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PariOfPlenty,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.VariantCriterion,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1079u,
    NameID = 14274u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PariOfPlenty(WorldState ws, Actor primary) : BossModule(ws, primary, new(-760f, -805f), new ArenaBoundsSquare(20f));