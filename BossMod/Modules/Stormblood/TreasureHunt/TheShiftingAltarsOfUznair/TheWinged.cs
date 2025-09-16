namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.TheWinged;

public enum OID : uint
{
    Boss = 0x253D, //R=3.36
    FeatherOfTheWinged = 0x253E, //R=0.5
    AltarQueen = 0x254A, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    AltarGarlic = 0x2548, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    AltarTomato = 0x2549, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    AltarOnion = 0x2546, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    AltarEgg = 0x2547, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // BonusAdds->player, no cast, single-target

    Filoplumes = 13376, // Boss->self, 3.0s cast, range 8+R width 4 rect
    Wingbeat = 13377, // Boss->self, 3.0s cast, range 40+R 60-degree cone, knockback 20 away from source
    FeatherSquallVisual = 13378, // Boss->self, 3.0s cast, single-target
    FeatherSquall = 13379, // BossHelper->location, 3.0s cast, range 6 circle
    Sideslip = 13380, // Boss->self, 3.5s cast, range 50+R circle
    Pinion = 13381, // Featherofthewinged->self, 3.0s cast, range 40+R width 3 rect

    Pollen = 6452, // AltarQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // AltarOnion->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // AltarTomato->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // AltarEgg->self, 3.5s cast, range 6+R circle
    PungentPirouette = 6450, // AltarGarlic->self, 3.5s cast, range 6+R circle
    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/Mandragoras/GoldWhisker->self, no cast, single-target, bonus adds disappear
}

class Filoplumes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Filoplumes, new AOEShapeRect(11.36f, 2f));
class Wingbeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wingbeat, new AOEShapeCone(43.36f, 30f.Degrees()));
class WingbeatKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Wingbeat, 20f, false, 1, new AOEShapeCone(43.36f, 30.Degrees()), stopAtWall: true);
class FeatherSquall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FeatherSquall, 6f);
class Pinion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pinion, new AOEShapeRect(40.5f, 1.5f));
class Sideslip(BossModule module) : Components.RaidwideCast(module, (uint)AID.Sideslip);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class TheWingedStates : StateMachineBuilder
{
    public TheWingedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Filoplumes>()
            .ActivateOnEnter<Wingbeat>()
            .ActivateOnEnter<WingbeatKB>()
            .ActivateOnEnter<FeatherSquall>()
            .ActivateOnEnter<Sideslip>()
            .ActivateOnEnter<Pinion>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(TheWinged.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7595)]
public class TheWinged(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.AltarEgg, (uint)OID.AltarGarlic, (uint)OID.AltarOnion, (uint)OID.AltarTomato,
    (uint)OID.AltarQueen, (uint)OID.GoldWhisker, (uint)OID.AltarMatanga];
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
                (uint)OID.AltarOnion => 6,
                (uint)OID.AltarEgg => 5,
                (uint)OID.AltarGarlic => 4,
                (uint)OID.AltarTomato => 3,
                (uint)OID.AltarQueen or (uint)OID.GoldWhisker => 2,
                (uint)OID.AltarMatanga => 1,
                _ => 0
            };
        }
    }
}
