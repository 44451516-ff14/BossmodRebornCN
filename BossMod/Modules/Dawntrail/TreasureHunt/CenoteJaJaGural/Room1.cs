namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.Room1;

public enum OID : uint
{
    Boss = 0x1EBAA4, // R0.5
    CenoteWoodsman = 0x4310, // R1.9
    CenotePitcherWeed = 0x4311, // R2.4
    CenoteNetzach = 0x4312, // R3.18
    CenoteSilverLobo = 0x4307, // R4.0
    CenoteGedan = 0x4308, // R2.2
    CenoteBandercoeurl = 0x4309, // R2.5
    CenoteTohtson = 0x430A, // R1.75
    Cenotekeeper = 0x430B, // R1.7
    CenoteTohsoq = 0x430C, // R2.0
    CenoteTomaton = 0x430D, // R2.04
    CenoteTomatuxmool = 0x430E, // R2.04
    CenoteNecrosis = 0x430F, // R2.88

    UolonOfFortune = 0x42FF, // R3.5
    AlpacaOfFortune = 0x42FE, // R1.8
    TuraliOnion = 0x4300, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliEggplant = 0x4301, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliGarlic = 0x4302, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    TuraliTomato = 0x4303, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    TuligoraQueen = 0x4304, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
}

public enum AID : uint
{
    AutoAttack1 = 872, // UolonOfFortune/CenoteTomaton/CenoteTomatuxmool/CenoteTohtson/Cenotekeeper/CenoteGedan/CenoteWoodsman/CenoteNetzach->player, no cast, single-target
    AutoAttack2 = 870, // CenoteTohsoq/CenoteSilverLobo/CenoteBandercoeurl/CenotePitcherWeed->player, no cast, single-target
    AutoAttack3 = 871, // CenoteNecrosis->player, no cast, single-target

    NepenthicPlunge = 38291, // CenotePitcherWeed->self, 3.0s cast, range 10 90-degree cone
    CreepingHush = 38292, // CenoteNetzach->self, 3.0s cast, range 12 120-degree cone
    Ovation = 38290, // CenoteWoodsman->self, 3.0s cast, range 14 width 4 rect
    BestialFire = 38283, // CenoteBandercoeurl->location, 3.0s cast, range 5 circle
    HeadButt = 38284, // CenoteTohtson->self, 2.5s cast, range 6 120-degree cone
    AetherialBlast = 38287, // CenoteTohsoq->self, 3.0s cast, range 20 width 4 rect
    IsleDrop = 38285, // Cenotekeeper->location, 3.0s cast, range 6 circle
    Envenomate = 38281, // CenoteSilverLobo->players, 4.0s cast, width 3 rect charge
    SyrupSpout = 38288, // CenoteTomatuxmool/CenoteTomaton->self, 3.0s cast, range 10 120-degree cone
    SpinningAttack = 38289, // CenoteNecrosis->self, 3.0s cast, range 10 width 4 rect
    NeckRip = 38282, // CenoteGedan->player, no cast, single-target

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

sealed class NepenthicPlunge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.NepenthicPlunge, new AOEShapeCone(10f, 45f.Degrees()));
sealed class CreepingHush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CreepingHush, new AOEShapeCone(12f, 60f.Degrees()));
sealed class Ovation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Ovation, new AOEShapeRect(14f, 2f));
sealed class BestialFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BestialFire, 5f);
sealed class HeadButt(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeadButt, new AOEShapeCone(6f, 60f.Degrees()));
sealed class AetherialBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialBlast, new AOEShapeRect(20f, 2f));
sealed class Envenomate(BossModule module) : Components.BaitAwayChargeCast(module, (uint)AID.Envenomate, 1.5f);
sealed class SyrupSpout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SyrupSpout, new AOEShapeCone(10f, 60f.Degrees()));
sealed class SpinningAttack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningAttack, new AOEShapeRect(10f, 2f));
sealed class IsleDropRottenSpores(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.IsleDrop, (uint)AID.RottenSpores], 6f);

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class Room1States : StateMachineBuilder
{
    public Room1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NepenthicPlunge>()
            .ActivateOnEnter<CreepingHush>()
            .ActivateOnEnter<Ovation>()
            .ActivateOnEnter<BestialFire>()
            .ActivateOnEnter<HeadButt>()
            .ActivateOnEnter<AetherialBlast>()
            .ActivateOnEnter<IsleDropRottenSpores>()
            .ActivateOnEnter<Envenomate>()
            .ActivateOnEnter<SyrupSpout>()
            .ActivateOnEnter<SpinningAttack>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.EventState == 7;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 5057)]
public sealed class Room1(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    private static readonly WPos arenaCenter = new(default, 377f);
    private static readonly Angle a135 = 135.Degrees();
    private static readonly WDir dir135 = a135.ToDirection(), dirM135 = (-a135).ToDirection();
    public static readonly ArenaBoundsCustom ArenaBounds = new([new Polygon(arenaCenter, 19.5f * CosPI.Pi36th, 36), new Rectangle(arenaCenter + 8.65f * dir135, 20f, 6.15f, -a135),
    new Rectangle(arenaCenter + 8.65f * dirM135, 20f, 6.15f, a135), new Rectangle(arenaCenter + 12f * dir135, 20f, 4.3f, -a135), new Rectangle(arenaCenter + 12f * dirM135, 20f, 4.3f, a135),
    new Rectangle(arenaCenter + 14.3f * dir135, 20f, 3.5f, -a135), new Rectangle(arenaCenter + 14.3f * dirM135, 20f, 3.5f, a135)], [new Rectangle(new(default, 397f), 20f, 1.4f)]);
    private static readonly uint[] bonusAdds = [(uint)OID.TuligoraQueen, (uint)OID.TuraliTomato, (uint)OID.TuraliOnion, (uint)OID.TuraliEggplant,
    (uint)OID.TuraliGarlic, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    private static readonly uint[] trash = [(uint)OID.CenoteNetzach, (uint)OID.CenotePitcherWeed, (uint)OID.CenoteWoodsman, (uint)OID.CenoteSilverLobo, (uint)OID.CenoteGedan,
    (uint)OID.CenoteBandercoeurl, (uint)OID.CenoteTohsoq, (uint)OID.Cenotekeeper, (uint)OID.CenoteTohtson, (uint)OID.CenoteTomatuxmool, (uint)OID.CenoteTomaton, (uint)OID.CenoteNecrosis];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        var m = this;
        Arena.Actors(m, trash);
        Arena.Actors(m, bonusAdds, Colors.Vulnerable);
    }

    protected override bool CheckPull() => Enemies(trash).Count != 0;

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
