namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D102Beatrice;

public enum OID : uint
{
    Boss = 0x396D, // R=4.95
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 29820, // Boss->location, no cast, single-target

    BeatificScorn1 = 29811, // Boss->self, 4.0s cast, single-target
    BeatificScorn2 = 29812, // Boss->self, no cast, single-target
    BeatificScorn3 = 29813, // Boss->self, 4.0s cast, single-target
    BeatificScorn4 = 29814, // Boss->self, no cast, single-target
    BeatificScorn5 = 29815, // Boss->self, no cast, single-target
    BeatificScorn6 = 29816, // Boss->self, no cast, single-target
    BeatificScornEnd = 29819, // Boss->self, no cast, single-target
    BeatificScornAOE = 29817, // Helper->self, 10.0s cast, range 9 circle

    DeathForeseen1 = 29821, // Helper->self, 5.0s cast, range 40 circle, gaze
    DeathForeseen2 = 29828, // Helper->self, 8.0s cast, range 40 circle, gaze

    EyeOfTroia = 29818, // Boss->self, 4.0s cast, range 40 circle, raidwide

    Hush = 29824, // Boss->player, 5.0s cast, single-target, tankbuster
    VoidNail = 29823, // Helper->player, 5.0s cast, range 6 circle, spread

    Voidshaker = 29822, // Boss->self, 5.0s cast, range 20 120-degree cone
    ToricVoidVisual1 = 29829, // Boss->self, 4.0s cast, single-target
    ToricVoidVisual2 = 31206, // Boss->self, no cast, single-target
    ToricVoid = 31207, // Helper->self, 4.0s cast, range 10-20 donut
    Antipressure = 31208, // Helper->player, 7.0s cast, range 6 circle, stack
}

public enum SID : uint
{
    Doom = 3364 // Helper->player, extra=0x0
}

sealed class BeatificScorn5(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BeatificScornAOE, 9f, 8);

sealed class DeathForeseen1(BossModule module) : Components.CastGaze(module, (uint)AID.DeathForeseen1);
sealed class DeathForeseen2(BossModule module) : Components.CastGaze(module, (uint)AID.DeathForeseen2, maxCasts: 2);

sealed class Voidshaker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Voidshaker, new AOEShapeCone(20f, 60f.Degrees()));

sealed class VoidNail(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VoidNail, 6f);
sealed class Hush(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Hush);
sealed class EyeOfTroia(BossModule module) : Components.RaidwideCast(module, (uint)AID.EyeOfTroia);
sealed class ToricVoid(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ToricVoid, new AOEShapeDonut(10f, 20f));
sealed class Antipressure(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Antipressure, 6f, 4, 4);
sealed class Doom(BossModule module) : Components.CleansableDebuff(module, (uint)SID.Doom);

sealed class D102BeatriceStates : StateMachineBuilder
{
    public D102BeatriceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<BeatificScorn5>()
            .ActivateOnEnter<DeathForeseen1>()
            .ActivateOnEnter<DeathForeseen2>()
            .ActivateOnEnter<Voidshaker>()
            .ActivateOnEnter<VoidNail>()
            .ActivateOnEnter<Hush>()
            .ActivateOnEnter<EyeOfTroia>()
            .ActivateOnEnter<ToricVoid>()
            .ActivateOnEnter<Antipressure>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869, NameID = 11384, SortOrder = 5)]
public sealed class D102Beatrice(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(default, -148f), 19.5f * CosPI.Pi32th, 32)], [new Rectangle(new(default, -127.63f), 20f, 1.25f),
    new Rectangle(new(default, -168.32f), 20f, 1.25f)]);
}
