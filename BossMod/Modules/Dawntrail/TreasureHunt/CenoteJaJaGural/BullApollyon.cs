namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.BullApollyon;

public enum OID : uint
{
    Boss = 0x4305, // R7
    TuraliOnion = 0x4300, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliEggplant = 0x4301, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliGarlic = 0x4302, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliTomato = 0x4303, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    TuligoraQueen = 0x4304, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    UolonOfFortune = 0x42FF, // R3.5
    AlpacaOfFortune = 0x42FE, // R1.8
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Teleport = 38334, // Boss->location, no cast, single-target

    Blade = 38261, // Boss->player, 5s cast, single-target tankbuster

    PyreburstVisual = 38263, // Helper->self, no cast, range 60 circle visual
    Pyreburst = 38262, // Boss->self, 5s cast, single-target raidwide

    FlameBladeVisual = 38248, // Boss->self, no cast, single-target
    FlameBlade1 = 38249, // Boss->self, 4s cast, range 40 width 10 rect
    FlameBlade2 = 38250, // Helper->self, 11s cast, range 40 width 10 rect
    FlameBlade3 = 38251, // Helper->self, 2.5s cast, range 40 width 5 rect

    BlazingBreathVisual = 38257, // Boss->self, 2.3+0.7s cast, single-target visual
    BlazingBreath = 38258, // Helper->player, 3s cast, range 44 width 10 rect

    CrossfireBlade1 = 38253, // Boss->self, 4s cast, range 20 width 10 cross
    CrossfireBlade2 = 38254, // Helper->self, 11s cast, range 20 width 10 cross
    CrossfireBlade3 = 38255, // Helper->self, 2.5s cast, range 40 width 5 rect

    BlazingBlastVisual = 38259, // Boss->self, 3s cast, single-target visual
    BlazingBlast = 38260, // Helper->location, 3s cast, range 6 circle

    Inhale = 38280, // UolonOfFortune->self, 0.5s cast, range 27 120-degree cone
    Spin = 38279, // UolonOfFortune->self, 3.0s cast, range 11 circle
    Scoop = 38278, // UolonOfFortune->self, 4.0s cast, range 15 120-degree cone
    RottenSpores = 38277, // UolonOfFortune->location, 3.0s cast, range 6 circle
    TearyTwirl = 32301, // TuraliOnion->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // TuraliTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // TuraliGarlic->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // TuraliEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // TuligoraQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

sealed class Blade(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Blade);
sealed class Pyreburst(BossModule module) : Components.RaidwideCast(module, (uint)AID.Pyreburst);

sealed class FlameBlade1and2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.FlameBlade1, (uint)AID.FlameBlade2], new AOEShapeRect(40f, 5f));
sealed class CrossfireBlade3FlameBlade3(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrossfireBlade3, (uint)AID.FlameBlade3], new AOEShapeRect(40f, 2.5f));
sealed class CrossfireBlade1and2(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.CrossfireBlade1, (uint)AID.CrossfireBlade2], new AOEShapeCross(20f, 5f));

sealed class BlazingBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingBreath, new AOEShapeRect(44f, 5f));
sealed class BlazingBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlazingBlast, 6f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class RottenSpores(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenSpores, 6f);
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class BullApollyonStates : StateMachineBuilder
{
    public BullApollyonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<Pyreburst>()
            .ActivateOnEnter<FlameBlade1and2>()
            .ActivateOnEnter<CrossfireBlade3FlameBlade3>()
            .ActivateOnEnter<CrossfireBlade1and2>()
            .ActivateOnEnter<BlazingBreath>()
            .ActivateOnEnter<BlazingBlast>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<RottenSpores>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(BullApollyon.All);
                var count = enemies.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (!enemies[i].IsDeadOrDestroyed)
                        return false;
                }
                return true;
            };
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Kismet, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 13247)]
public sealed class BullApollyon(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.TuraliEggplant, (uint)OID.TuraliTomato, (uint)OID.TuligoraQueen, (uint)OID.TuraliGarlic,
    (uint)OID.TuraliOnion, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.TuraliOnion => 6,
                (uint)OID.TuraliEggplant => 5,
                (uint)OID.TuraliGarlic => 4,
                (uint)OID.TuraliTomato => 3,
                (uint)OID.TuligoraQueen or (uint)OID.AlpacaOfFortune => 2,
                (uint)OID.UolonOfFortune => 1,
                _ => 0
            };
        }
    }
}
