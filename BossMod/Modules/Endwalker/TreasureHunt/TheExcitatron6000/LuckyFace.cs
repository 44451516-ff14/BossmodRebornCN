namespace BossMod.Endwalker.TreasureHunt.Excitatron6000.LuckyFace;

public enum OID : uint
{
    Boss = 0x377F, // R3.24
    ExcitingQueen = 0x380C, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingTomato = 0x380B, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingGarlic = 0x380A, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingEgg = 0x3809, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    ExcitingOnion = 0x3808, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 27980, // Boss->player, no cast, single-target

    HeartOnFireII = 28001, // BossHelper->location, 3.0s cast, range 6 circle
    FakeHeartOnFireII = 27988, // Boss->self, 3.0s cast, single-target
    HeartOnFireIV = 27981, // Boss->player, 5.0s cast, single-target

    FakeLeftInTheDark1 = 27989, // Boss->self, 4.0s cast, range 20 180-degree cone
    FakeLeftInTheDark2 = 27993, // Boss->self, 4.0s cast, range 20 180-degree cone
    FakeRightInTheDark1 = 27991, // Boss->self, 4.0s cast, range 20 180-degree cone
    FakeRightInTheDark2 = 27995, // Boss->self, 4.0s cast, range 20 180-degree cone
    LeftInTheDark1 = 27990, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    LeftInTheDark2 = 27994, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    RightInTheDark1 = 27992, // BossHelper->self, 4.0s cast, range 20 180-degree cone
    RightInTheDark2 = 27996, // BossHelper->self, 4.0s cast, range 20 180-degree cone

    FakeQuakeMeAway1 = 27999, // Boss->self, 4.0s cast, range 10-20 donut
    FakeQuakeMeAway2 = 28091, // Boss->self, 4.0s cast, range 10-20 donut
    FakeQuakeInYourBoots1 = 27997, // Boss->self, 4.0s cast, range 10 circle
    FakeQuakeInYourBoots2 = 28090, // Boss->self, 4.0s cast, range 10 circle
    QuakeMeAwayDonut = 28000, // BossHelper->self, 4.0s cast, range 10-20 donut
    QuakeMeAwayCircle = 28190, // BossHelper->self, 4.0s cast, range 10 circle
    QuakeInYourBootsCircle = 27998, // BossHelper->self, 4.0s cast, range 10 circle
    QuakeInYourBootsDonut = 28189, // BossHelper->self, 4.0s cast, range 10-20 donut

    MerryGoRound1 = 27983, // Boss->self, 3.0s cast, single-target, boss animation
    MerryGoRound2 = 27986, // Boss->self, no cast, single-target
    MerryGoRound3 = 27984, // Boss->self, 3.0s cast, single-target
    MerryGoRound4 = 27985, // Boss->self, no cast, single-target
    MerryGoRound5 = 28093, // Boss->self, no cast, single-target
    MerryGoRound6 = 27987, // Boss->self, no cast, single-target

    ApplySpreadmarkers = 28045, // Boss->self, no cast, single-target
    HeartOnFireIII = 28002, // BossHelper->player, 5.0s cast, range 6 circle
    TempersFlare = 27982, // Boss->self, 5.0s cast, range 60 circle

    DeathVisual = 28145, // Boss->self, no cast, single-target

    PluckAndPrune = 6449, // ExcitingEgg->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // ExcitingOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // ExcitingTomato->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // ExcitingGarlic->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // ExcitingQueen->self, 3.5s cast, range 6+R circle
    Telega = 9630 // Mandragoras->self, no cast, single-target, mandragoras disappear
}

abstract class InTheDark(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(20f, 90f.Degrees()));
class LeftInTheDark1(BossModule module) : InTheDark(module, (uint)AID.LeftInTheDark1);
class LeftInTheDark2(BossModule module) : InTheDark(module, (uint)AID.LeftInTheDark2);
class RightInTheDark1(BossModule module) : InTheDark(module, (uint)AID.RightInTheDark1);
class RightInTheDark2(BossModule module) : InTheDark(module, (uint)AID.RightInTheDark2);

abstract class QuakeCircle(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 10f);
class QuakeMeAwayCircle(BossModule module) : QuakeCircle(module, (uint)AID.QuakeMeAwayCircle);
class QuakeInYourBootsCircle(BossModule module) : QuakeCircle(module, (uint)AID.QuakeInYourBootsCircle);

abstract class QuakeDonut(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(10f, 20f));
class QuakeInYourBootsDonut(BossModule module) : QuakeDonut(module, (uint)AID.QuakeInYourBootsDonut);
class QuakeMeAwayDonut(BossModule module) : QuakeDonut(module, (uint)AID.QuakeMeAwayDonut);

class HeartOnFireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeartOnFireII, 6f);
class HeartOnFireIV(BossModule module) : Components.SingleTargetCast(module, (uint)AID.HeartOnFireIV);
class HeartOnFireIII(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HeartOnFireIII, 6f);
class TempersFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.TempersFlare);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class LuckyFaceStates : StateMachineBuilder
{
    public LuckyFaceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LeftInTheDark1>()
            .ActivateOnEnter<LeftInTheDark2>()
            .ActivateOnEnter<RightInTheDark1>()
            .ActivateOnEnter<RightInTheDark2>()
            .ActivateOnEnter<QuakeInYourBootsCircle>()
            .ActivateOnEnter<QuakeInYourBootsDonut>()
            .ActivateOnEnter<QuakeMeAwayCircle>()
            .ActivateOnEnter<QuakeMeAwayDonut>()
            .ActivateOnEnter<TempersFlare>()
            .ActivateOnEnter<HeartOnFireII>()
            .ActivateOnEnter<HeartOnFireIII>()
            .ActivateOnEnter<HeartOnFireIV>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(LuckyFace.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 819, NameID = 10831)]
public class LuckyFace(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(default, -460f), 19.5f, 32)], [new Rectangle(new(default, -440f), 20f, 1f)]);
    private static readonly uint[] bonusAdds = [(uint)OID.ExcitingEgg, (uint)OID.ExcitingQueen, (uint)OID.ExcitingOnion, (uint)OID.ExcitingTomato,
    (uint)OID.ExcitingGarlic];
    public static readonly uint[] All = [(uint)OID.Boss, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
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
                (uint)OID.ExcitingOnion => 6,
                (uint)OID.ExcitingEgg => 5,
                (uint)OID.ExcitingGarlic => 4,
                (uint)OID.ExcitingTomato => 3,
                (uint)OID.ExcitingQueen => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
}
