namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouStyphnolobion;

public enum OID : uint
{
    Boss = 0x3D37, //R=5.3
    GymnasiouHippogryph = 0x3D38, //R=2.53
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
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // GymnasiouHippogryph->player, no cast, single-target

    EarthQuakerVisual1 = 32199, // Boss->self, no cast, single-target
    EarthQuakerVisual2 = 32247, // Boss->self, 3.5s cast, single-target
    EarthQuaker1 = 32248, // Helper->self, 4.0s cast, range 10 circle
    EarthQuaker2 = 32249, // Helper->self, 6.0s cast, range 10-20 donut
    Rake = 32245, // Boss->player, 5.0s cast, single-target
    EarthShakerVisual = 32250, // Boss->self, 3.5s cast, single-target
    EarthShaker = 32251, // Helper->players, 4.0s cast, range 60 30-degree cone
    StoneIII = 32252, // Boss->self, 2.5s cast, single-target
    StoneIII2 = 32253, // Helper->location, 3.0s cast, range 6 circle
    BeakSnap = 32254, // GymnasiouHippogryph->player, no cast, single-target
    Tiiimbeeer = 32246, // Boss->self, 5.0s cast, range 50 circle

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // Mandragoras/GymnasiouLyssa->self, no cast, single-target, bonus add disappear
}

class Rake(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Rake);
class Tiiimbeeer(BossModule module) : Components.RaidwideCast(module, (uint)AID.Tiiimbeeer);
class StoneIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.StoneIII2, 6f);
class EarthShaker(BossModule module) : Components.BaitAwayCast(module, (uint)AID.EarthShaker, new AOEShapeCone(60f, 15f.Degrees()), endsOnCastEvent: true);

class EarthQuaker(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EarthQuakerVisual2)
            AddSequence(WPos.ClampToGrid(Arena.Center), Module.CastFinishAt(spell, 0.5f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.EarthQuaker1 => 0,
                (uint)AID.EarthQuaker2 => 1,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouStyphnolobionStates : StateMachineBuilder
{
    public GymnasiouStyphnolobionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rake>()
            .ActivateOnEnter<Tiiimbeeer>()
            .ActivateOnEnter<StoneIII>()
            .ActivateOnEnter<EarthShaker>()
            .ActivateOnEnter<EarthQuaker>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<HeavySmash>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(GymnasiouStyphnolobion.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12012)]
public class GymnasiouStyphnolobion(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouHippogryph, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouHippogryph));
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
                (uint)OID.GymnasticQueen or (uint)OID.GymnasiouLyssa => 2,
                (uint)OID.GymnasiouHippogryph => 1,
                _ => 0
            };
        }
    }
}
