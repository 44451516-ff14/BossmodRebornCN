// namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D123ThundergustGriffin;

// public enum OID : uint
// {
//     ThundergustGriffin = 0x4A65, // R6.9
//     BallLightning = 0x4A66, // R2.0
//     ShockStorm = 0x4A67, // R1.0
//     Helper = 0x233C
// }

// public enum AID : uint
// {
//     AutoAttack = 45307, // ThundergustGriffin->player, no cast, single-target

//     Thunderspark = 45291, // ThundergustGriffin->self, 5.0s cast, range 60 circle, raidwide
//     Teleport1 = 46852, // ThundergustGriffin->location, no cast, single-target
//     Teleport2 = 45294, // ThundergustGriffin->location, no cast, single-target

//     HighVolts = 45292, // ThundergustGriffin->self, 4.0s cast, single-target
//     LightningBolt1 = 45293, // Helper->self, 5.0s cast, range 5 circle
//     LightningBolt2 = 46856, // Helper->player, 5.0s cast, range 5 circle

//     _Spell_ = 46944, // Helper->self, 4.3s cast, range 16 width 3 rect
//     _Spell_1 = 46943, // Helper->self, 4.3s cast, range 20 width 3 rect
//     _Spell_2 = 46945, // Helper->self, 4.3s cast, range 16 width 3 rect
//     ThunderingRoar = 45295, // ThundergustGriffin->self, 5.0s cast, single-target
//     Thunderbolt1 = 45296, // Helper->self, 5.5s cast, range 92 width 6 rect
//     Thunderbolt2 = 45297, // Helper->self, 5.5s cast, range 92 width 6 rect
//     Thunderbolt3 = 45298, // Helper->self, 5.5s cast, range 92 width 6 rect
//     GoldenTalons = 45305, // ThundergustGriffin->player, 5.0s cast, single-target
//     FulgurousFall = 45301, // ThundergustGriffin->location, 6.0s cast, single-target
//     Rush = 45302, // Helper->self, 6.0s cast, range 40 width 10 rect
//     ElectrifyingFlight = 47022, // Helper->self, 6.2s cast, range 50 width 40 rect
//     ElectrifyingFlightTeleport = 45300, // ShockStorm->location, 0.5s cast, single-target
//     ElectrogeneticForce = 45304, // Helper->self, 9.6s cast, range 40 width 18 rect
//     StormSurge = 45299, // ThundergustGriffin->self, 3.0s cast, range 50 width 10 rect
// }

// sealed class D123ThundergustGriffinStates : StateMachineBuilder
// {
//     public D123ThundergustGriffinStates(BossModule module) : base(module)
//     {
//         TrivialPhase();
//     }
// }

// [ModuleInfo(BossModuleInfo.Maturity.WIP,
// StatesType = typeof(D123ThundergustGriffinStates),
// ConfigType = null,
// ObjectIDType = typeof(OID),
// ActionIDType = typeof(AID),
// StatusIDType = null, // replace null with typeof(SID) if applicable
// TetherIDType = null, // replace null with typeof(TetherID) if applicable
// IconIDType = null, // replace null with typeof(IconID) if applicable
// PrimaryActorOID = (uint)OID.ThundergustGriffin,
// Contributors = "",
// Expansion = BossModuleInfo.Expansion.Dawntrail,
// Category = BossModuleInfo.Category.Dungeon,
// GroupType = BossModuleInfo.GroupType.CFC,
// GroupID = 1064u,
// NameID = 14288u,
// SortOrder = 3,
// PlanLevel = 0)]
// [SkipLocalsInit]
// public sealed class D123ThundergustGriffin : BossModule
// {
//     public D123ThundergustGriffin(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

//     private D123ThundergustGriffin(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

//     private static (WPos center, ArenaBoundsCustom arena) BuildArena()
//     {
//         var arena = new ArenaBoundsCustom([new Polygon(new(281f, -620f), 20f, 64)], [new Rectangle(new(281f, -590f), 8f, 1.25f)]);
//         return (arena.Center, arena); // after arena transition radius 20
//     }
// }
