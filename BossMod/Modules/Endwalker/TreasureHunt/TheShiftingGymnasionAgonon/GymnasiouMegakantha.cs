namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouMegakantha;

public enum OID : uint
{
    Boss = 0x3D33, //R=6.0
    GymnasiouAkantha = 0x3D35, //R=1.76 
    GymnasiouSinapi = 0x3D36, //R=1.56
    GymnasiouLyssa = 0x3D4E, //R=3.75
    GymnasiouLampas = 0x3D4D, //R=2.001
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // GymnasiouLyssa->player, no cast, single-target
    AutoAttack2 = 872, // Boss/GymnasiouSinapi/GymnasiouAkantha->player, no cast, single-target

    OdiousAtmosphereComboStart = 32199, // Boss->self, no cast, single-target
    OdiousAtmosphere = 32241, // Boss->self, 4.0s cast, single-target
    OdiousAtmosphere1 = 32242, // Helper->self, 5.3s cast, range 40 180-degree cone
    OdiousAtmosphere2 = 33015, // Helper->self, 5.3s cast, range 40 180-degree cone
    OdiousAtmosphere3 = 33016, // Helper->self, 3.0s cast, range 40 180-degree cone
    SludgeBomb = 32239, // Boss->self, 3.0s cast, single-target
    SludgeBomb2 = 32240, // Helper->location, 3.0s cast, range 8 circle
    RustlingWind = 32244, // GymnasiouSinapi->self, 3.0s cast, range 15 width 4 rect
    AcidMist = 32243, // GymnasiouAkantha->self, 2.5s cast, range 6 circle
    VineWhip = 32238, // Boss->player, 5.0s cast, single-target
    OdiousAir = 32237, // Boss->self, 3.0s cast, range 12 120-degree cone

    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    Telega = 9630 // Mandragoras/Lyssa/Lampas->self, no cast, single-target, bonus add disappear
}

class SludgeBomb(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SludgeBomb2, 8f);
class RustlingWind(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RustlingWind, new AOEShapeRect(15f, 2f));
class AcidMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AcidMist, 6f);
class OdiousAir(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OdiousAir, new AOEShapeCone(12f, 60f.Degrees()));
class VineWhip(BossModule module) : Components.SingleTargetCast(module, (uint)AID.VineWhip);

class OdiousAtmosphere(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(40f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(ref _aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OdiousAtmosphere1)
            _aoe = new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.OdiousAtmosphere1:
            case (uint)AID.OdiousAtmosphere2:
            case (uint)AID.OdiousAtmosphere3:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouMegakanthaStates : StateMachineBuilder
{
    public GymnasiouMegakanthaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SludgeBomb>()
            .ActivateOnEnter<RustlingWind>()
            .ActivateOnEnter<VineWhip>()
            .ActivateOnEnter<OdiousAir>()
            .ActivateOnEnter<OdiousAtmosphere>()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () =>
            {
                var enemies = module.Enemies(GymnasiouMegakantha.All);
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

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 12009)]
public class GymnasiouMegakantha(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
    private static readonly uint[] rest = [(uint)OID.Boss, (uint)OID.GymnasiouSinapi, (uint)OID.GymnasiouAkantha];
    public static readonly uint[] All = [.. rest, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(rest));
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
                (uint)OID.GymnasticOnion => 7,
                (uint)OID.GymnasticEggplant => 6,
                (uint)OID.GymnasticGarlic => 5,
                (uint)OID.GymnasticTomato => 4,
                (uint)OID.GymnasticQueen or (uint)OID.GymnasiouLampas => 3,
                (uint)OID.GymnasiouLyssa => 2,
                (uint)OID.GymnasiouAkantha or (uint)OID.GymnasiouSinapi => 1,
                _ => 0
            };
        }
    }
}
