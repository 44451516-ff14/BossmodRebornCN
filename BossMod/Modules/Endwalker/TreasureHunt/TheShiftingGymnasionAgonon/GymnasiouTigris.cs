namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouTigris;

public enum OID : uint
{
    Boss = 0x3D29, //R=5.75
    GymnasiouTigrisMikra = 0x3D2A, //R=3.5
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasiouLyssa = 0x3D4E, //R=3.75
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GymnasiouTigrisMikra/GymnasiouLyssa->player->player, no cast, single-target

    AbsoluteZero = 32208, // Boss->self, 4.0s cast, range 45 90-degree cone
    BlizzardIII = 32209, // Boss->location, 3.0s cast, range 6 circle
    FrumiousJaws = 32206, // Boss->player, 5.0s cast, single-target
    Eyeshine = 32207, // Boss->self, 4.0s cast, range 35 circle
    CatchingClaws = 32210, // GymnasiouTigrisMikra->self, 3.0s cast, range 12 90-degree cone

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // GymnasiouLyssa/Mandragoras->self, no cast, single-target, bonus add disappear
}

class AbsoluteZero(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbsoluteZero, new AOEShapeCone(45f, 45f.Degrees()));
class FrumiousJaws(BossModule module) : Components.SingleTargetCast(module, (uint)AID.FrumiousJaws);
class BlizzardIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlizzardIII, 6f);
class Eyeshine(BossModule module) : Components.CastGaze(module, (uint)AID.Eyeshine);
class CatchingClaws(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CatchingClaws, new AOEShapeCone(12f, 45f.Degrees()));

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouTigrisStates : StateMachineBuilder
{
    public GymnasiouTigrisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbsoluteZero>()
            .ActivateOnEnter<FrumiousJaws>()
            .ActivateOnEnter<BlizzardIII>()
            .ActivateOnEnter<Eyeshine>()
            .ActivateOnEnter<CatchingClaws>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(GymnasiouTigris.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 11999)]
public class GymnasiouTigris(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouTigrisMikra, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouTigrisMikra));
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
                (uint)OID.GymnasiouTigrisMikra => 1,
                _ => 0
            };
        }
    }
}
