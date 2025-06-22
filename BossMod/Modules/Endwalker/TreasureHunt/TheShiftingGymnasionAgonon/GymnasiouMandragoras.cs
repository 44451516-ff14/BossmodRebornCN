namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMandragoras;

public enum OID : uint
{
    Boss = 0x3D41, //R=2.85
    GymnasiouKorrigan = 0x3D42, //R=0.85
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/GymnasiouKorrigan->player, no cast, single-target

    Ram = 32297, // Boss->player, 5.0s cast, single-target
    LeafDagger = 32299, // Boss->location, 2.5s cast, range 3 circle
    SaibaiMandragora = 32300, // Boss->self, 3.0s cast, single-target, calls adds

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630, // Mandragoras->self, no cast, single-target, bonus add disappear
}

class Ram(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Ram);
class SaibaiMandragora(BossModule module) : Components.CastHint(module, (uint)AID.SaibaiMandragora, "Calls adds");
class LeafDagger(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LeafDagger, 3f);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class GymnasiouMandragorasStates : StateMachineBuilder
{
    public GymnasiouMandragorasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Ram>()
            .ActivateOnEnter<SaibaiMandragora>()
            .ActivateOnEnter<LeafDagger>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(GymnasiouMandragoras.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12022)]
public class GymnasiouMandragoras(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouKorrigan, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouKorrigan));
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
                (uint)OID.GymnasticOnion => 6,
                (uint)OID.GymnasticEggplant => 5,
                (uint)OID.GymnasticGarlic => 4,
                (uint)OID.GymnasticTomato => 3,
                (uint)OID.GymnasticQueen => 2,
                (uint)OID.GymnasiouKorrigan => 1,
                _ => 0
            };
        }
    }
}
