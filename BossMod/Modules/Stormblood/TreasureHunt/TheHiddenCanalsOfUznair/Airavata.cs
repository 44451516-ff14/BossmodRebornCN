namespace BossMod.Stormblood.TreasureHunt.HiddenCalansOfUznair.Airavata;

public enum OID : uint
{
    Boss = 0x1FCC, // R4.75
    CanalAnila = 0x1F10, // R1.92
    CanalAnala = 0x1F0C, // R1.6
    CanalLightningHomunculus = 0x1F13, // R1.6
    CanalWindHomunculus = 0x1F11, // R1.6
    CanalFireHomunculus = 0x1F0D, // R1.6
    CanalIceHomunculus = 0x1F0F, // R1.6
    GoldenApa = 0x1FEC, // R3.12
    GoldenDhara = 0x1FEB, // R1.95

    CanalQueen = 0x1FD1, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    CanalEgg = 0x1FCE, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    CanalTomato = 0x1FD0, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    CanalGarlic = 0x1FCF, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    CanalOnion = 0x1FCD, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards

    NamazuStickywhisker = 0x2063, // R0.54
    Abharamu = 0x2064, // R3.42
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // CanalAnala->player, no cast, single-target
    AutoAttack2 = 872, // Boss/Abharamu/GoldenDhara->player, no cast, single-target

    SpinBoss = 9892, // Boss->self, 3.5s cast, range 20 120-degree cone
    HurlBoss = 9891, // Boss->location, 3.0s cast, range 6 circle
    Buffet = 9893, // Boss->player, 3.0s cast, single-target, tankbuster
    BarbarousScream = 9894, // Boss->self, 3.0s cast, range 14 circle

    Blizzard = 967, // CanalIceHomunculus->player, 1.0s cast, single-target
    Fire = 966, // CanalFireHomunculus->player, 1.0s cast, single-target
    Aero = 969, // CanalWindHomunculus/CanalAnila->player, 1.0s cast, single-target
    Thunder = 968, // CanalLightningHomunculus->player, 1.0s cast, single-target
    Water = 971, // GoldenApa->player, 1.0s cast, single-target
    RingOfFire = 8629, // CanalAnala->location, 3.0s cast, range 5 circle
    DoubleSmash = 618, // GoldenDhara->self, 3.0s cast, range 6+R 120-degree cone
    Abacinate = 8628, // CanalAnala->players, no cast, single-target
    AncientAero = 8727, // CanalAnila->self, 3.0s cast, range 12+R width 4 rect
    StraightPunch = 8659, // GoldenDhara->player, no cast, single-target
    StoneII = 8660, // GoldenDhara->player, 4.0s cast, single-target

    AbharamuActivate = 9636, // Abharamu->self, no cast, single-target
    Spin = 8599, // Abharamu->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // Abharamu->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // Abharamu->location, 3.0s cast, range 6 circle

    PungentPirouette = 6450, // CanalGarlic->self, 3.5s cast, range 6+R circle
    PluckAndPrune = 6449, // CanalEgg->self, 3.5s cast, range 6+R circle
    HeirloomScream = 6451, // CanalTomato->self, 3.5s cast, range 6+R circle
    Pollen = 6452, // CanalQueen->self, 3.5s cast, range 6+R circle
    TearyTwirl = 6448, // CanalOnion->self, 3.5s cast, range 6+R circle

    Telega = 9630 // Mandragoras/Abharamu/NamazuStickywhisker->self, no cast, single-target, bonus adds disappear
}

class Hurl(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HurlBoss, (uint)AID.Hurl], 6f);
class SpinBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SpinBoss, new AOEShapeCone(20f, 60f.Degrees()));
class BarbarousScream(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BarbarousScream, 14f);
class Buffet(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Buffet);

class DoubleSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoubleSmash, new AOEShapeCone(7.95f, 60f.Degrees()));
class AncientAero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AncientAero, new AOEShapeRect(13.6f, 2f));
class RingOfFire(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RingOfFire, 5f);
class StoneII(BossModule module) : Components.SingleTargetCast(module, (uint)AID.StoneII);

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.Abharamu]);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 6.84f);

class AiravataStates : StateMachineBuilder
{
    public AiravataStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<SpinBoss>()
            .ActivateOnEnter<BarbarousScream>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(Airavata.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 276, NameID = 6853)]
public class Airavata(WorldState ws, Actor primary) : FinalRoomArena(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.CanalEgg, (uint)OID.CanalGarlic, (uint)OID.CanalOnion, (uint)OID.CanalTomato,
    (uint)OID.CanalQueen, (uint)OID.Abharamu, (uint)OID.NamazuStickywhisker];
    private static readonly uint[] rest = [(uint)OID.Boss, (uint)OID.CanalLightningHomunculus, (uint)OID.CanalIceHomunculus, (uint)OID.CanalFireHomunculus, (uint)OID.CanalWindHomunculus,
    (uint)OID.CanalAnala, (uint)OID.CanalAnila, (uint)OID.GoldenApa, (uint)OID.GoldenDhara];
    public static readonly uint[] All = [.. rest, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, rest);
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
                (uint)OID.CanalOnion => 7,
                (uint)OID.CanalEgg => 6,
                (uint)OID.CanalGarlic => 5,
                (uint)OID.CanalTomato => 4,
                (uint)OID.CanalQueen or (uint)OID.NamazuStickywhisker => 3,
                (uint)OID.Abharamu => 2,
                (uint)OID.CanalFireHomunculus or (uint)OID.CanalIceHomunculus or (uint)OID.CanalWindHomunculus or (uint)OID.CanalLightningHomunculus or
                (uint)OID.CanalAnala or (uint)OID.CanalAnila or (uint)OID.GoldenApa or (uint)OID.GoldenDhara => 1,
                _ => 0
            };
        }
    }
}
