namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouTriton;

public enum OID : uint
{
    Boss = 0x3D30, //R=6.08
    GymnasiouEcheneis = 0x3D31, //R=2.2
    Bubble = 0x3D32, //R=1.0

    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/GymnasiouEcheneis->player, no cast, single-target

    PelagicCleaver = 32230, // Boss->self, 3.5s cast, range 40 60-degree cone
    AquaticLance = 32231, // Boss->self, 4.0s cast, range 13 circle
    FoulWaters = 32229, // Boss->location, 3.0s cast, range 3 circle, AOE + spawns bubble
    Riptide = 32233, // Bubble->self, 1.0s cast, range 5 circle, pulls into bubble, dist 30 between centers
    WateryGrave = 32234, // Bubble->self, no cast, range 2 circle, voidzone, imprisons player until status runs out
    NavalRam = 32232, // GymnasiouEcheneis->player, no cast, single-target
    ProtolithicPuncture = 32228, // Boss->player, 5.0s cast, single-target

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630 // GymnasiouLyssa/Mandragoras->self, no cast, single-target, bonus add disappear
}

class PelagicCleaver(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PelagicCleaver, new AOEShapeCone(40f, 30f.Degrees()));
class FoulWaters(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.FoulWaters, GetVoidzones, 1.4f)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.Bubble);
}
class AquaticLance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaticLance, 13f);
class ProtolithicPuncture(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ProtolithicPuncture);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class GymnasiouTritonStates : StateMachineBuilder
{
    public GymnasiouTritonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PelagicCleaver>()
            .ActivateOnEnter<FoulWaters>()
            .ActivateOnEnter<AquaticLance>()
            .ActivateOnEnter<ProtolithicPuncture>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(GymnasiouTriton.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12006)]
public class GymnasiouTriton(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouEcheneis, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouEcheneis));
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
                (uint)OID.GymnasticOnion => 6,
                (uint)OID.GymnasticEggplant => 5,
                (uint)OID.GymnasticGarlic => 4,
                (uint)OID.GymnasticTomato => 3,
                (uint)OID.GymnasticQueen => 2,
                (uint)OID.GymnasiouEcheneis => 1,
                _ => 0
            };
        }
    }
}
