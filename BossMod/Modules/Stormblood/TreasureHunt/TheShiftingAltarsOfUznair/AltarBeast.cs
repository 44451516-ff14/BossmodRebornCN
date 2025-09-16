namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarBeast;

public enum OID : uint
{
    Boss = 0x2536, //R=4.6
    AltarKeeper = 0x2567, //R=1.25
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Mandragoras/AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6499, // AltarKeeper->player, no cast, single-target

    RustingClaw = 13259, // Boss->self, 3.0s cast, range 8+R 120-degree cone
    WordsOfWoe = 13260, // Boss->self, 5.0s cast, range 45+R width 6 rect
    EyeOfTheFire = 13263, // Boss->self, 5.0s cast, range 50+R circle, gaze, applies hysteria
    TheSpin = 13262, // Boss->self, 4.0s cast, range 50+R circle, damage fall off with distance, estimated safety distance between 10 and 15
    VengefulSoul = 13740, // AltarKeeper->location, 3.0s cast, range 6 circle
    TailDrive = 13261, // Boss->self, 3.0s cast, range 30+R 120-degree cone

    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/Mandragoras->self, no cast, single-target, bonus adds disappear
}

class RustingClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RustingClaw, new AOEShapeCone(12.6f, 60f.Degrees()));
class TailDrive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailDrive, new AOEShapeCone(34.6f, 60f.Degrees()));
class TheSpin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheSpin, 15f);
class WordsOfWoe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WordsOfWoe, new AOEShapeRect(49.6f, 3f));
class VengefulSoul(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VengefulSoul, 6f);
class EyeOfTheFire(BossModule module) : Components.CastGaze(module, (uint)AID.EyeOfTheFire);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class AltarBeastStates : StateMachineBuilder
{
    public AltarBeastStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RustingClaw>()
            .ActivateOnEnter<TailDrive>()
            .ActivateOnEnter<TheSpin>()
            .ActivateOnEnter<WordsOfWoe>()
            .ActivateOnEnter<VengefulSoul>()
            .ActivateOnEnter<EyeOfTheFire>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarBeast.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7588)]
public class AltarBeast(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarKeeper, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarKeeper));
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AltarOnion => 6,
                (uint)OID.AltarEgg => 5,
                (uint)OID.AltarGarlic => 4,
                (uint)OID.AltarTomato => 3,
                (uint)OID.AltarQueen or (uint)OID.AltarMatanga => 2,
                (uint)OID.AltarKeeper => 1,
                _ => 0
            };
        }
    }
}
