namespace BossMod.Endwalker.TreasureHunt.ShiftingGymnasionAgonon.GymnasiouLeon;

public enum OID : uint
{
    Boss = 0x3D27, //R=5.95
    GymnasiouLeonMikros = 0x3D28, //R=3.5
    GymnasiouLyssa = 0x3D4E, //R=3.75
    GymnasticGarlic = 0x3D51, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticQueen = 0x3D53, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticEggplant = 0x3D50, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticOnion = 0x3D4F, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasticTomato = 0x3D52, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    GymnasiouLampas = 0x3D4D, //R=2.001, bonus loot adds
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/GymnasiouLyssa/GymnasiouLeonMikros->player, no cast, single-target

    InfernoBlast = 32204, // Boss->self, 3.5s cast, range 46 width 20 rect
    Roar = 32201, // Boss->self, 3.0s cast, range 12 circle
    Pounce = 32200, // Boss->player, 5.0s cast, single-target
    MagmaChamberVisual = 32202, // Boss->self, 3.0s cast, single-target
    MagmaChamber = 32203, // Helper->location, 3.0s cast, range 8 circle
    FlareStarVisual = 32815, // Boss->self, 3.0s cast, single-target
    FlareStar = 32816, // Helper->self, 7.0s cast, range 40 circle, AOE with dmg fall off, damage seems to stop falling after about range 10-12
    MarkOfTheBeast = 32205, // GymnasiouLeonMikros->self, 3.0s cast, range 8 120-degree cone

    PluckAndPrune = 32302, // GymnasticEggplant->self, 3.5s cast, range 7 circle
    Pollen = 32305, // GymnasticQueen->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // GymnasticTomato->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // GymnasticGarlic->self, 3.5s cast, range 7 circle
    TearyTwirl = 32301, // GymnasticOnion->self, 3.5s cast, range 7 circle
    HeavySmash = 32317, // GymnasiouLyssa->location, 3.0s cast, range 6 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus add disappear
}

class InfernoBlast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InfernoBlast, new AOEShapeRect(46f, 10f));

sealed class RoarFlareStar(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Roar, (uint)AID.FlareStar], 12f);

class MarkOfTheBeast(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MarkOfTheBeast, new AOEShapeCone(8f, 60f.Degrees()));
class Pounce(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Pounce);
class MagmaChamber(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagmaChamber, 8f);

class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);

class HeavySmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeavySmash, 6f);

class GymnasiouLeonStates : StateMachineBuilder
{
    public GymnasiouLeonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InfernoBlast>()
            .ActivateOnEnter<RoarFlareStar>()
            .ActivateOnEnter<MarkOfTheBeast>()
            .ActivateOnEnter<Pounce>()
            .ActivateOnEnter<MagmaChamber>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<MandragoraAOEs>()
            .Raw.Update = () => AllDeadOrDestroyed(GymnasiouLeon.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 909, NameID = 11997)]
public class GymnasiouLeon(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GymnasticEggplant, (uint)OID.GymnasticGarlic, (uint)OID.GymnasticOnion, (uint)OID.GymnasticTomato,
    (uint)OID.GymnasticQueen, (uint)OID.GymnasiouLyssa, (uint)OID.GymnasiouLampas];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.GymnasiouLeonMikros, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.GymnasiouLeonMikros));
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
                (uint)OID.GymnasticOnion => 7,
                (uint)OID.GymnasticEggplant => 6,
                (uint)OID.GymnasticGarlic => 5,
                (uint)OID.GymnasticTomato => 4,
                (uint)OID.GymnasticQueen or (uint)OID.GymnasiouLyssa => 3,
                (uint)OID.GymnasiouLampas => 2,
                (uint)OID.GymnasiouLeonMikros => 1,
                _ => 0
            };
        }
    }
}
