namespace BossMod.Dawntrail.TreasureHunt.CenoteJaJaGural.Room4;

public enum OID : uint
{
    Boss = 0x1EBAA7, // R0.5
    CenoteQeziigural = 0x4325, // R3.0
    CenoteTohsoq = 0x430C, // R2.0
    CenoteMegamaguey = 0x4327, // R3.2
    CenoteTyaitya = 0x4328, // R2.0
    CenoteTulichu = 0x4326, // R3.6
    CenoteTomaton = 0x430D, // R2.04
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
    AutoAttack1 = 870, // CenoteMegamaguey/CenoteTyaitya/CenoteQeziigural/CenoteTohsoq->player, no cast, single-target
    AutoAttack2 = 872, // UolonOfFortune/CenoteTulichu/CenoteTomaton->player, no cast, single-target
    AutoAttack3 = 871, // CenoteNecrosis->player, no cast, single-target

    Lumisphere = 38286, // CenoteQeziigural->location, 3.0s cast, range 6 circle
    AetherialBlast = 38287, // CenoteTohsoq/CenoteQeziigural->self, 3.0s cast, range 20 width 4 rect
    SerratedSpin = 38317, // CenoteMegamaguey->self, 3.0s cast, range 8 circle
    RootsOfAtopy = 38314, // CenoteTulichu->location, 3.0s cast, range 6 circle
    SyrupSpout = 38288, // CenoteTomaton->self, 3.0s cast, range 10 120-degree cone
    SpinningAttack = 38289, // CenoteNecrosis->self, 3.0s cast, range 10 width 4 rect
    RightWingblade = 38318, // CenoteTyaitya->self, 3.0s cast, range 8 180-degree cone
    LeftWingblade = 38319, // CenoteTyaitya->self, 3.0s cast, range 8 180-degree cone
    Tornado = 38316, // CenoteMegamaguey->location, 3.0s cast, range 6 circle
    OdiousAir = 38315, // CenoteTulichu->self, 3.0s cast, range 12 120-degree cone

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

sealed class LumisphereTornadoRootsOfAtopyRottenSpores(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Lumisphere, (uint)AID.Tornado, (uint)AID.RootsOfAtopy, (uint)AID.RottenSpores], 6f);
sealed class AetherialBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialBlast, new AOEShapeRect(20f, 2f));
sealed class SerratedSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SerratedSpin, 8f);
sealed class SyrupSpout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SyrupSpout, new AOEShapeCone(10f, 60f.Degrees()));
sealed class SpinningAttack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinningAttack, new AOEShapeRect(10f, 2f));
sealed class OdiousAir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OdiousAir, new AOEShapeCone(12f, 60f.Degrees()));
sealed class Wingblade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftWingblade, (uint)AID.RightWingblade], new AOEShapeCone(8f, 90f.Degrees()));

sealed class Spin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Spin, 11f);
sealed class Scoop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Scoop, new AOEShapeCone(15f, 60f.Degrees()));
sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

sealed class Room4States : StateMachineBuilder
{
    public Room4States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LumisphereTornadoRootsOfAtopyRottenSpores>()
            .ActivateOnEnter<AetherialBlast>()
            .ActivateOnEnter<SerratedSpin>()
            .ActivateOnEnter<Wingblade>()
            .ActivateOnEnter<SyrupSpout>()
            .ActivateOnEnter<SpinningAttack>()
            .ActivateOnEnter<OdiousAir>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<Scoop>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.EventState == 7u;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 993, NameID = 5057)]
public sealed class Room4(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaBounds.Center, ArenaBounds)
{
    private static readonly WPos arenaCenter = new(-197f, -145f);
    private static readonly Angle a135 = 135.Degrees();
    private static readonly WDir dir135 = a135.ToDirection(), dirM135 = (-a135).ToDirection();
    public static readonly ArenaBoundsComplex ArenaBounds = new([new Polygon(arenaCenter, 19.5f * CosPI.Pi36th, 36), new Rectangle(arenaCenter + 8.65f * dir135, 20f, 6.15f, -a135),
    new Rectangle(arenaCenter + 8.65f * dirM135, 20f, 6.15f, a135), new Rectangle(arenaCenter + 12f * dir135, 20f, 4.3f, -a135), new Rectangle(arenaCenter + 12f * dirM135, 20f, 4.3f, a135),
    new Rectangle(arenaCenter + 14.3f * dir135, 20f, 3.5f, -a135), new Rectangle(arenaCenter + 14.3f * dirM135, 20f, 3.5f, a135)], [new Rectangle(new(-197f, -125f), 20f, 1.7f)]);
    private static readonly uint[] bonusAdds = [(uint)OID.TuligoraQueen, (uint)OID.TuraliTomato, (uint)OID.TuraliOnion, (uint)OID.TuraliEggplant,
    (uint)OID.TuraliGarlic, (uint)OID.UolonOfFortune, (uint)OID.AlpacaOfFortune];
    private static readonly uint[] trash = [(uint)OID.CenoteQeziigural, (uint)OID.CenoteTohsoq, (uint)OID.CenoteMegamaguey, (uint)OID.CenoteTyaitya, (uint)OID.CenoteTulichu,
    (uint)OID.CenoteTomaton, (uint)OID.CenoteNecrosis];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(trash));
        Arena.Actors(Enemies(bonusAdds), Colors.Vulnerable);
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
