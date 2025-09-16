namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarArachne;

public enum OID : uint
{
    Boss = 0x253B, //R=7.0
    EarthquakeHelper1 = 0x253C, //R=0.5
    EarthquakeHelper2 = 0x2565, //R=0.5
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target

    DarkSpike = 13342, // Boss->player, 3.0s cast, single-target
    SilkenSpray = 13455, // Boss->self, 2.5s cast, range 17+R 60-degree cone
    FrondAffeared = 13784, // Boss->self, 3.0s cast, range 60 circle, gaze, applies hysteria
    Implosion = 13343, // Boss->self, 4.0s cast, range 50+R circle

    Earthquake1 = 13346, // EarthquakeHelper1/EarthquakeHelper2->self, 3.5s cast, range 10+R circle
    Earthquake2 = 13345, // EarthquakeHelper1/EarthquakeHelper2->self, 3.5s cast, range 10-20 donut
    Earthquake3 = 13344, // EarthquakeHelper1/EarthquakeHelper2->self, 3.5s cast, range 20-30 donut

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

class DarkSpike(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.DarkSpike);
class FrondAffeared(BossModule module) : Components.CastGaze(module, (uint)AID.FrondAffeared);
class SilkenSpray(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SilkenSpray, new AOEShapeCone(24f, 30f.Degrees()));
class Implosion(BossModule module) : Components.RaidwideCast(module, (uint)AID.Implosion);
class Earthquake1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthquake1, 10.5f);
class Earthquake2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthquake2, new AOEShapeDonut(10f, 20f));
class Earthquake3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthquake3, new AOEShapeDonut(20f, 30f));

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class AltarArachneStates : StateMachineBuilder
{
    public AltarArachneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkSpike>()
            .ActivateOnEnter<FrondAffeared>()
            .ActivateOnEnter<SilkenSpray>()
            .ActivateOnEnter<Implosion>()
            .ActivateOnEnter<Earthquake1>()
            .ActivateOnEnter<Earthquake2>()
            .ActivateOnEnter<Earthquake3>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarArachne.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7623)]
public class AltarArachne(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GoldWhisker, (uint)OID.AltarMatanga];
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
                (uint)OID.GoldWhisker => 2,
                (uint)OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
